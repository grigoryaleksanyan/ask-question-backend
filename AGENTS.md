# AGENTS.md

## Команды

- `dotnet run` в `AskQuestion.WebApi/` — запуск на `http://localhost:5500` (Swagger на `/swagger`)
- `dotnet build` в корне решения — сборка всех проектов
- Тестов нет, тест-проектов нет

## Архитектура

.NET 10, решение `ask-question-backend.sln`. Четыре проекта с линейной зависимостью:

```
WebApi → BLL → Core
              ↗
WebApi → DAL → (нет подчинённых)
BLL → DAL
```

| Проект | Назначение |
|--------|-----------|
| `AskQuestion.Core` | Enums (`UserRoles`), Constants (`UserStringRoles`) — без зависимостей |
| `AskQuestion.DAL` | EF Core + Npgsql, сущности, `DataContext`, миграции |
| `AskQuestion.BLL` | Репозитории (интерфейсы + реализации), DTO |
| `AskQuestion.WebApi` | Controllers, Request/Response модели, Program.cs, Extensions |

Слой DAL → BLL — **нет**. BLL ссылается на DAL (и Core). WebApi ссылается на BLL и DAL напрямую (для `DataContext` в DI).

## База данных

PostgreSQL 16.1 через Npgsql EF Core provider. Контекст: `AskQuestion.DAL.DataContext`.

- Соединение: `appsettings.json` → `ConnectionStrings:PostgreSQL` (host `localhost`, port `5432`, db `AskQuestionDb`, user `postgres`, password `12345`)
- Миграции: `AskQuestion.DAL/Migrations/`. Применяются **автоматически** при старте (`dbContext.Database.Migrate()` в `Program.cs`)
- Seed: Admin-пользователь и две роли (Administrator, Speaker) — через `OnModelCreating`
- Расширение PostgreSQL: `uuid-ossp`
- Первичные ключи: `Guid` (через `uuid-ossp`), базовая сущность `BaseEntity` (`Id`, `Сreated`, `Updated`)

Создание миграции (из корня решения):
```
dotnet ef migrations add <Name> --project AskQuestion.DAL --startup-project AskQuestion.WebApi
```

## Авторизация

Cookie-аутентификация (`CookieAuthenticationDefaults.AuthenticationScheme`). Cookie: `.WebApi`. Claims: `Name`, `NameIdentifier`, `Role` (по `UserRoleId`). Срок: 1 день. Роли: 1 = Administrator, 2 = Speaker. Константы ролей — `AskQuestion.Core.Constants.UserStringRoles`.

## Контроллеры

Все маршруты начинаются с `api/`. Response caching отключён глобально на контроллерах.

| Контроллер | Маршрут | Авторизация |
|-----------|---------|-------------|
| `AuthController` | `api/Auth` | Login — анонимно, Logout — Admin+Speaker |
| `QuestionController` | `api/Question` | Create — анонимно (captcha), Update/Delete — Admin |
| `FaqCategoryController` | `api/FaqCategory` | — |
| `FaqEntryController` | `api/FaqEntry` | — |
| `FeedbackController` | `api/Feedback` | — |
| `AreaController` | `api/Area` | — |
| `UserController` | `api/User` | — |

## DI

Регистрация сервисов — в `AskQuestion.WebApi.Extensions.IServiceCollectionExtensions` (internal static). Репозитории регистрируются как Scoped (`AddScoped<I*Repository, *Repository>`).

## Несоответствия и особенности

- **CORS**: `WithOrigins("http://localhost:8080")` — не совпадает с реальным портом frontend (5000)
- **Dockerfile**: обновлён до .NET 10.0 (base/SDK)
- **Опечатка**: `GetCapctha` / `capctha` вместо `GetCaptcha` / `captcha` — в контроллере, хелпере и сессии
- **Сессия**: используется `AddDistributedMemoryCache` + `AddSession` — только для хранения текста капчи
- **Swagger**: доступен только в Development, XML-документация инжектируется (`GenerateDocumentationFile` + `NoWarn(1591)`)
- **Кириллица в колонке БД**: свойство `Created` (латиница) в `BaseEntity` маппится на колонку `Сreated` (кириллица) через `[Column("Сreated")]`. Колонка в БД остаётся с кириллическим именем; JSON-сериализация теперь выдаёт `created` корректно
