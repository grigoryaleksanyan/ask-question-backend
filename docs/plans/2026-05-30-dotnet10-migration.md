# .NET 10 Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate the backend from .NET 8 to .NET 10, updating all dependencies to compatible versions.

**Architecture:** Linear migration — update TFM in all csproj, update NuGet packages, refactor code with breaking API changes (SkiaSharp, Swashbuckle), update Dockerfile, verify build.

**Tech Stack:** .NET 10, EF Core 10, Npgsql 10, Swashbuckle 10, SkiaSharp 3.x

---

## Version Map

| Package | Current | Target |
|---------|---------|--------|
| TargetFramework | net8.0 | net10.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.2 | 10.0.2 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.2 | 10.0.8 |
| BCrypt.Net-Next | 4.0.3 | 4.2.0 |
| SkiaSharp | 2.88.7 | 3.119.4 |
| SkiaSharp.NativeAssets.Linux.NoDependencies | 2.88.7 | 3.119.4 |
| Swashbuckle.AspNetCore | 6.5.0 | 10.1.7 |
| Swashbuckle.AspNetCore.Filters | 8.0.1 | 10.0.1 |
| Dockerfile base images | 7.0 | 10.0 |

---

### Task 1: Update TargetFramework in all csproj files

**Files:**
- Modify: `AskQuestion.Core/AskQuestion.Core.csproj:4`
- Modify: `AskQuestion.DAL/AskQuestion.DAL.csproj:4`
- Modify: `AskQuestion.BLL/AskQuestion.BLL.csproj:4`
- Modify: `AskQuestion.WebApi/AskQuestion.WebApi.csproj:4`

- [ ] **Step 1: Update AskQuestion.Core.csproj**

Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`.

- [ ] **Step 2: Update AskQuestion.DAL.csproj**

Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`.

- [ ] **Step 3: Update AskQuestion.BLL.csproj**

Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`.

- [ ] **Step 4: Update AskQuestion.WebApi.csproj**

Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`.

---

### Task 2: Update NuGet package versions

**Files:**
- Modify: `AskQuestion.DAL/AskQuestion.DAL.csproj:18-19`
- Modify: `AskQuestion.WebApi/AskQuestion.WebApi.csproj:12-19`

- [ ] **Step 1: Update AskQuestion.DAL.csproj packages**

```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.2.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.2" />
</ItemGroup>
```

- [ ] **Step 2: Update AskQuestion.WebApi.csproj packages**

```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.2.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.8">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="SkiaSharp" Version="3.119.4" />
  <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.119.4" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
  <PackageReference Include="Swashbuckle.AspNetCore.Filters" Version="10.0.1" />
</ItemGroup>
```

---

### Task 3: Migrate SkiaSharp captcha code from 2.x to 3.x API

SkiaSharp 3.x moved text properties from `SKPaint` to `SKFont`. The `DrawText(string, float, float, SKPaint)` overload is still available but the text-related SKPaint properties (TextSize, Typeface, TextAlign) are obsolete with warnings.

**Files:**
- Modify: `AskQuestion.WebApi/Helpers/GenerateCaptcha.cs`

- [ ] **Step 1: Refactor GetCaptchaBase64 to use SKFont**

Replace the entire `GetCaptchaBase64` method body:

```csharp
public static string GetCaptchaBase64(string captchaText)
{
    int height = 48;
    int width = 160;

    using var bitmap = new SKBitmap(width, height);
    using var font = new SKFont(SKTypeface.Default, 32);
    using var paint = new SKPaint { MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Inner, 4) };

    float textWidth = paint.MeasureText(captchaText, font);
    float x = (width - textWidth) / 2F;
    float y = 35;

    using (var bitmapCanvas = new SKCanvas(bitmap))
    {
        bitmapCanvas.Clear();
        bitmapCanvas.DrawText(captchaText, x, y, font, paint);
    }

    using var resultImage = SKImage.FromBitmap(bitmap);
    using var data = resultImage.Encode(SKEncodedImageFormat.Png, 100);

    return "data:image;base64," + Convert.ToBase64String(data.ToArray());
}
```

Key changes:
- `SKPaint.Typeface`, `SKPaint.TextAlign`, `SKPaint.TextSize` → moved to `SKFont`
- Text centering: calculate manually via `paint.MeasureText(captchaText, font)` since `SKTextAlign.Center` is obsolete on `SKPaint` and `SKFont` does not have `TextAlign`
- `DrawText(string, float, float, SKFont, SKPaint)` — new overload in SkiaSharp 3.x
- Add `using` for proper disposal of `SKImage` and `SKData`

---

### Task 4: Update Swashbuckle configuration for v10 compatibility

Swashbuckle v10 upgraded to Microsoft.OpenApi 2.x. The `SecurityRequirementsOperationFilter` and `AppendAuthorizeToSummaryOperationFilter` from `Swashbuckle.AspNetCore.Filters` v10 should be compatible, but the `AddSwaggerGen` API may have minor changes.

**Files:**
- Modify: `AskQuestion.WebApi/Extensions/IServiceCollectionExtensions.cs:44-59`

- [ ] **Step 1: Update ConfigureSwagger method**

```csharp
public static void ConfigureSwagger(this IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Ask Question API",
        });

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);

        options.OperationFilter<SecurityRequirementsOperationFilter>();
        options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
    });
}
```

The method body stays the same — verify compilation after package update. If `OpenApiInfo` type changes in Microsoft.OpenApi 2.x, the constructor/signature may need updating.

---

### Task 5: Update CORS configuration in Program.cs

The `app.UseCors(builder => ...)` overload with inline lambda is a legacy pattern. Migrate to named CORS policy registered via `AddCors`.

**Files:**
- Modify: `AskQuestion.WebApi/Program.cs:24,43-46`

- [ ] **Step 1: Replace `builder.Services.AddCors()` with named policy**

Replace:
```csharp
builder.Services.AddCors();
```

With:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5000")
               .AllowCredentials()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
```

- [ ] **Step 2: Replace `app.UseCors(...)` with named policy**

Replace:
```csharp
app.UseCors(builder => builder.WithOrigins("http://localhost:5000")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
```

With:
```csharp
app.UseCors("AllowFrontend");
```

---

### Task 6: Update Dockerfile from .NET 7.0 to .NET 10.0

**Files:**
- Modify: `Dockerfile:3,7`

- [ ] **Step 1: Update base and SDK images**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

---

### Task 7: Build and verify

- [ ] **Step 1: Restore packages**

Run: `dotnet restore` in `ask-question-backend/`
Expected: successful restore

- [ ] **Step 2: Build solution**

Run: `dotnet build` in `ask-question-backend/`
Expected: successful build, zero errors. Warnings about obsolete APIs should be addressed in code already (Task 3).

- [ ] **Step 3: Verify EF migrations are compatible**

Run: `dotnet ef migrations list --project AskQuestion.DAL --startup-project AskQuestion.WebApi` in `ask-question-backend/`
Expected: all migrations listed without errors

---

### Task 8: Update AGENTS.md

**Files:**
- Modify: `AGENTS.md:11`

- [ ] **Step 1: Update .NET version reference**

Change `.NET 8` to `.NET 10` in the architecture section.

---

## Risk Summary

| Risk | Mitigation |
|------|-----------|
| SkiaSharp 3.x API breaking changes | Task 3 rewrites captcha to new SKFont API |
| Swashbuckle v10 Microsoft.OpenApi 2.x | Task 4 verifies compilation; Filters package v10 is compatible |
| EF Core 10 migration compatibility | Existing migrations are forward-compatible (EF Core reads old snapshots) |
| SkiaSharp `MeasureText` overload change | Task 3 uses `paint.MeasureText(text, font)` overload |
| CORS `UseCors` lambda overload may warn | Task 5 migrates to named policy pattern |
