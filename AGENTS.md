# AGENTS.md

## Команды

- `dotnet run` в `AskQuestion.WebApi/` — запуск на `http://localhost:5500` (Swagger на `/swagger`)
- `dotnet build` в корне решения — сборка всех проектов
- `dotnet test` в корне решения — запуск xUnit-тестов `AskQuestion.BLL.Tests`

## Архитектура

.NET 10, решение `ask-question-backend.sln`. Пять проектов:

```
WebApi → BLL → DAL → Core
WebApi → DAL
AskQuestion.BLL.Tests → BLL, DAL, Core
```

| Проект | Назначение |
|--------|-----------|
| `AskQuestion.Core` | Enums (`UserRoles`, `VoteType`, `QuestionStatus`), Constants (`UserStringRoles`) — без зависимостей |
| `AskQuestion.DAL` | EF Core + Npgsql (`10.0.2`), сущности, `DataContext`, миграции. Пакеты: `BCrypt.Net-Next` 4.2.0 |
| `AskQuestion.BLL` | Репозитории (интерфейсы + реализации), DTO, Email-подсистема. Пакеты: `HtmlSanitizer` 9.0.892, `Microsoft.Extensions.Hosting.Abstractions` 10.0.8 |
| `AskQuestion.WebApi` | Controllers, Request/Response модели, Program.cs, Extensions, ActionFilters. Пакеты: `BCrypt.Net-Next` 4.2.0, `SkiaSharp` 3.119.4, `SkiaSharp.NativeAssets.Linux.NoDependencies` 3.119.4, `Swashbuckle.AspNetCore` 10.1.7, `Swashbuckle.AspNetCore.Filters` 10.0.1, `Microsoft.EntityFrameworkCore.Design` 10.0.8, `Microsoft.EntityFrameworkCore.Tools` 10.0.8 |
| `AskQuestion.BLL.Tests` | xUnit-тесты BLL-репозиториев, Email-подсистемы, HtmlSanitizer. Пакеты: `xunit` 2.9.3, `FluentAssertions` 8.10.0, `Microsoft.EntityFrameworkCore.InMemory` 10.0.9, `BCrypt.Net-Next` 4.2.0, `Microsoft.NET.Test.Sdk` 17.14.1, `xunit.runner.visualstudio` 3.1.4, `coverlet.collector` 6.0.4 |

Слой DAL → BLL — **нет**. BLL ссылается на DAL (и Core). WebApi ссылается на BLL и DAL напрямую (для `DataContext` в DI).

## Тесты

Проект `AskQuestion.Tests/AskQuestion.BLL/AskQuestion.BLL.Tests.csproj` (net10.0, xUnit). Тестирует репозитории BLL, Email-подсистему и HtmlSanitizer на изолированном in-memory `DataContext`:

**Репозитории** (каталог `Repositories/`):

| Класс | Что покрывает |
|-------|---------------|
| `UserRepositoryTests` | авторизация, сброс пароля, изменение пароля, soft-delete |
| `QuestionRepositoryTests` | создание, пагинация, голосование, смена статуса, просмотры, популярные вопросы |
| `SpeakerRepositoryTests` | создание, обновление, удаление (soft-delete), порядок спикеров |
| `AreaRepositoryTests` | CRUD, порядок |
| `FaqCategoryRepositoryTests` | CRUD, порядок, записи внутри категории |
| `FaqEntryRepositoryTests` | CRUD, порядок |
| `FeedbackRepositoryTests` | CRUD |
| `QuestionStatusTransitionRepositoryTests` | логирование переходов статусов |

**Email-подсистема** (каталог `Email/`):

| Класс | Что покрывает |
|-------|---------------|
| `EmailSenderTests` | постановка в Channel, чтение из ChannelReader |
| `EmailBackgroundServiceTests` | обработка сообщений из Channel через IEmailClientFactory |
| `EmailTemplateBuilderTests` | генерация HTML-шаблонов писем |
| `SmtpEmailClientTests` | отправка через SmtpClient |
| `SmtpEmailClientFactoryTests` | создание клиента из IOptions<SmtpSettings> |
| `FakeEmailClient` | фейк `IEmailClient` для тестов, не отправляет реальные письма |
| `FakeEmailClientFactory` | фейк `IEmailClientFactory` для тестов |

**Helpers** (каталог `Helpers/`):

| Класс | Что покрывает |
|-------|---------------|
| `HtmlSanitizerServiceTests` | санитизация HTML-контента |
| `RepositoryTestBase` | базовый класс: in-memory `DbContext`, `HtmlSanitizer`, `FakeEmailSender`, `IOptions<SmtpSettings>` |
| `TestDataSeeder` | хелперы для seed пользователей, областей, вопросов и токенов сброса пароля |
| `FakeEmailSender` | фейк `IEmailSender` для тестов, не отправляет реальные письма |

Запуск: `dotnet test` из корня решения. Запуск конкретного класса: `dotnet test --filter "FullyQualifiedName~QuestionRepositoryTests"`.

## База данных

PostgreSQL 16.1 через Npgsql EF Core provider. Контекст: `AskQuestion.DAL.DataContext`.

- Соединение: `appsettings.json` → `ConnectionStrings:PostgreSQL` (host `localhost`, port `5432`, db `AskQuestionDb`, user `postgres`, password `12345`)
- Миграции: `AskQuestion.DAL/Migrations/`. Применяются **автоматически** при старте (`dbContext.Database.Migrate()` в `Program.cs`)
- Seed: **только две роли** (Administrator, Speaker) — через `OnModelCreating`. Администратор больше не сидируется — создаётся через API при первичной настройке
- Расширение PostgreSQL: `uuid-ossp` (подключено, но не используется для генерации ключей)
- Первичные ключи: `Guid`, генерируются на стороне EF Core (`ValueGeneratedOnAdd`). Базовая сущность `BaseEntity` (`Id`, `Created`, `Updated`)

Создание миграции (из корня решения):
```
dotnet ef migrations add <Name> --project AskQuestion.DAL --startup-project AskQuestion.WebApi
```

## Сущности

| Сущность | Наследует | Ключ | Примечания |
|---------|-----------|------|-----------|
| `BaseEntity` | — | `Guid Id` | `Created`, `Updated` — колонки с латиницей (миграция `RenameCyrillicCreatedColumn`) |
| `User` | BaseEntity | Guid | Email (unique index), Password (BCrypt), UserRoleId FK → UserRole |
| `UserDetails` | BaseEntity | Guid | UserId FK → User, FirstName, LastName, Position, IsDeleted, Order. Метод `GetFullName()` (LastName + FirstName) |
| `UserRole` | — | `int UserRoleId` | Не наследует BaseEntity. Seed: 1=Administrator, 2=Speaker |
| `Question` | BaseEntity | Guid | Text, Author, AreaId? FK→Area (SetNull), SpeakerId? FK→User (SetNull), Views, Likes, Dislikes, Status (int), Comment?, Answered? |
| `QuestionStatusTransition` | BaseEntity | Guid | QuestionId FK→Question (Cascade), FromStatus (int), ToStatus (int), ChangedByUserId? FK→User (SetNull) |
| `QuestionVote` | — | Composite `{QuestionId, VisitorId}` | VoteType enum. FK→Question (Cascade) |
| `Area` | BaseEntity | Guid | Title, Order |
| `FaqCategory` | BaseEntity | Guid | Name, Order, Nav: FaqEntries |
| `FaqEntry` | BaseEntity | Guid | FaqCategoryId FK, Question, Answer, Order |
| `Feedback` | BaseEntity | Guid | Username, Email, Theme, Text |
| `PasswordResetToken` | BaseEntity | Guid | UserId FK → User, TokenHash, ExpiresAt, IsUsed. Одноразовый токен сброса пароля (хеш SHA256, TTL 1 час) |

## Enum'ы (Core)

| Enum | Значения |
|------|---------|
| `UserRoles` | Administrator=1, Speaker=2 |
| `VoteType` | Like=0, Dislike=1 |
| `QuestionStatus` | New=0, InFocus=1, Answered=2 |

## Авторизация

Cookie-аутентификация (`CookieAuthenticationDefaults.AuthenticationScheme`). Cookie: `.WebApi`. Claims: `Name` (email), `NameIdentifier` (Guid), `Role` (по `UserRoleId`). Срок: 1 день. Роли: 1 = Administrator, 2 = Speaker. Константы ролей — `AskQuestion.Core.Constants.UserStringRoles`. Авторизация по Email (уникальный, required). Email хранится в `User.Email` (миграция `ReplaceLoginWithEmail` перенесла из `UserDetails.Email`), в `UserDetails` поля Email нет.

## Контроллеры

Все маршруты начинаются с `api/`. Response caching отключён (атрибут `[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]`) на QuestionController, FaqCategoryController, FaqEntryController, FeedbackController.

| Контроллер | Маршрут | Авторизация |
|-----------|---------|-------------|
| `AuthController` | `api/Auth` | SetupRequired, Setup, Login, ForgotPassword, ResetPassword — анонимно; Logout — Admin+Speaker |
| `QuestionController` | `api/Question` | GetCaptcha, GetAll, GetPopularQuestions, GetById, Like, Dislike, Create — анонимно; ChangeStatus (`{id}/status`), SetComment (`{id}/comment`) — Admin+Speaker (Speaker только для своих вопросов); Update, Delete — Admin |
| `FaqCategoryController` | `api/FaqCategory` | GetAllWithEntries — анонимно; GetAllWithEntriesForAdmin — Admin; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `FaqEntryController` | `api/FaqEntry` | Весь контроллер — Authorize на уровне класса; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `FeedbackController` | `api/Feedback` | Create — анонимно; GetAll, Delete — Admin |
| `AreaController` | `api/Area` | GetAll — анонимно; Create, Update, Delete, SetOrder — Admin |
| `UserController` | `api/User` | GetUserData, ChangePassword — Admin+Speaker |
| `SpeakerController` | `api/Speaker` | GetAllPublic — анонимно; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `DashboardController` | `api/Dashboard` | Summary — Admin |

## Ключевые функции

### Первичная настройка администратора (Setup)

Администратор больше не сидируется в БД. При старте без админа — требуется первичная настройка:
- `GET api/Auth/SetupRequired` — возвращает `{ SetupRequired: bool }` (true если админа нет)
- `POST api/Auth/Setup` — создаёт первого администратора (только если админа нет). Принимает `AdminSetupModel` (Email, Password, ConfirmPassword, FirstName, LastName). Автоматически логинит созданного пользователя

Миграция `RemoveAdminSeedData` убрала хардкод-сид админа.

### Сброс пароля

Реализован через сущность `PasswordResetToken` (миграция `AddPasswordResetToken`).
- `POST api/Auth/ForgotPassword` — создаёт токен, отправляет письмо со ссылкой на сброс (токен хранится как SHA256-хеш, TTL 1 час).
- `POST api/Auth/ResetPassword` — проверяет токен и устанавливает новый пароль.
- `UserRepository` содержит `ForgotPasswordAsync` и `ResetPasswordAsync`.

### Email-подсистема

`AskQuestion.BLL/Email/` — отправка email через MailHog (dev) / SMTP (prod):

| Класс | Назначение |
|-------|-----------|
| `IEmailSender` | Интерфейс: `EnqueueAsync(EmailMessage)` и `GetReader()` (возвращает `ChannelReader<EmailMessage>`) |
| `EmailSender` | Реализация через `Channel<EmailMessage>` (producer-consumer, unbounded), Singleton |
| `EmailMessage` | DTO: ToEmail, ToName, Subject, HtmlBody |
| `SmtpSettings` | Конфигурация SMTP: Host, Port, FromEmail, FromName, BaseUrl |
| `IEmailClient` | Интерфейс SMTP-клиента: `SendAsync(MailMessage, CancellationToken)` |
| `SmtpEmailClient` | Реализация `IEmailClient` через `SmtpClient` |
| `IEmailClientFactory` | Фабрика для создания `IEmailClient` |
| `SmtpEmailClientFactory` | Реализация фабрики на основе `IOptions<SmtpSettings>` |
| `EmailBackgroundService` | BackgroundService, читает из Channel и отправляет через `IEmailClientFactory` |
| `EmailTemplateBuilder` | Статический класс с HTML-шаблонами писем |

Шаблоны писем:
1. `BuildNewQuestionNotification` — уведомление спикеру о новом вопросе (вызывается в `QuestionRepository.CreateAsync`)
2. `BuildSpeakerCredentials` — отправка реквизитов доступа спикеру при создании (вызывается в `SpeakerRepository.CreateAsync`)
3. `BuildPasswordResetEmail` — письмо со ссылкой для сброса пароля (вызывается в `UserRepository.ForgotPasswordAsync`)

DI-регистрация: `ConfigureEmail()` в `IServiceCollectionExtensions` — `IOptions<SmtpSettings>`, `IEmailSender` → `EmailSender` (Singleton), `IEmailClientFactory` → `SmtpEmailClientFactory` (Singleton), `EmailBackgroundService` (HostedService).

Конфигурация SMTP в `appsettings.json` → `Smtp`: Host `localhost`, Port `1025`, FromEmail `noreply@askquestion.local`, FromName `Ask Question`, BaseUrl `http://localhost:5000`.

### Голосование за вопросы

Toggle-голосование (анонимное, по VisitorId). Cookie `VisitorId` (HttpOnly, Lax, 365 дней) устанавливается через `EnsureVisitorIdAttribute` (ActionFilter, Scoped). Атрибут установлен **на уровне QuestionController** — VisitorId-кука ставится на все эндпоинты контроллера. Endpoints: `POST {id}/like`, `POST {id}/dislike`. Возвращает `VoteResultDto` с актуальными счётчиками и голосом пользователя. `QuestionViewModel.UserVote` (тип `VoteType?`, `[JsonStringEnumConverter]`) — голос текущего пользователя.

### Пагинация вопросов

`GetAll` принимает `page`, `pageSize`, `status`, `speakerId`, `areaId`, `search`, `sortOrder`. Возвращает `PaginatedResult<QuestionViewModel>` с `Items`, `TotalCount`, `Page`, `PageSize`. `pageSize` ограничен диапазоном 1–50 (`Math.Clamp`), `page` минимум 1.

### Статусы вопросов

`QuestionStatus` enum (New, InFocus, Answered). Поле `Question.Status` (int). Поле `Question.Comment` (string?) — комментарий при смене статуса. Поле `Question.Answered` (DateTimeOffset?) — дата ответа. Смена статуса логируется через `QuestionStatusTransition` (FromStatus, ToStatus, ChangedByUserId).

Валидация переходов: `IsValidTransition(from, to)` — разрешены только переходы New↔InFocus↔Answered (|from - to| == 1). Speaker может менять статус/комментарий только своих вопросов (Forbid() если чужой).

### Популярные вопросы

`GET GetPopularQuestions` — анонимно, возвращает топ-5 вопросов.

### Счётчик просмотров

`IncrementViewsAsync` при `GetById`.

### Дашборд

`GET Summary` — Admin. Параметры: `periodDays` (по умолчанию 30), `speakerId` (опционально). Возвращает `DashboardSummaryDto`:

| Поле | Тип | Описание |
|------|-----|----------|
| TotalQuestions | int | Общее кол-во вопросов |
| AnsweredQuestions | int | Отвеченные |
| UnansweredQuestions | int | Неотвеченные |
| AverageResponseTimeHours | double | Среднее время ответа (0 при отсутствии данных, округляется до 1 знака) |
| TotalFeedback | int | Кол-во обращений обратной связи |
| TotalAreas | int | Кол-во областей |
| QuestionsWithoutSpeaker | int | Вопросы без спикера |
| ByStatus | `List<StatusDistributionDto>` | Распределение по статусам |
| Timeline | TimelinePointDto[] | График по дням (Date, NewCount, AnsweredCount) |
| ByArea | AreaDistributionDto[] | Распределение по областям |
| TopSpeakers | SpeakerProductivityDto[] | Топ спикеров |
| SpeakerAreas | SpeakerAreaDto[] | Спикеры по областям |
| Votes | VotesSummaryDto | Голоса (TotalLikes, TotalDislikes) |

### Мягкое удаление спикеров

`SpeakerRepository.DeleteAsync` не удаляет спикера физически — устанавливает `user.UserDetails.IsDeleted = true`. Удалённые спикеры не могут авторизоваться (`UserRepository.AuthorizeUser` возвращает `null` для soft-deleted пользователей).

### Порядок спикеров

`UserDetails` содержит поле `Order` (int), добавлено миграцией `AddSpeakerOrder`. `SpeakerController` предоставляет `PUT api/Speaker/SetOrder` для задания порядка спикеров (Admin). `SpeakerDto` и `SpeakerCreatedDto` также содержат `Order`.

### Капча

`GenerateCaptcha` (`AskQuestion.WebApi/Helpers/GenerateCaptcha.cs`) — статический класс, генерирует капчу через SkiaSharp (Base64-изображение). Текст капчи хранится в сессии.

### HtmlSanitizer

`AskQuestion.BLL/Helpers/HtmlSanitizerService.cs` — реализация `IHtmlSanitizerService`, синглтон. Регистрируется через `ConfigureHtmlSanitizer()` в DI. Используется для санитизации HTML-контента в репозиториях: вопросы, FAQ (категории и записи), области, спикеры, первичная настройка администратора.

### Security headers

AuthController.Setup и Login добавляют заголовки `X-Content-Type-Options: nosniff`, `X-Xss-Protection: 1`, `X-Frame-Options: DENY`.

## DI

Регистрация сервисов — в `AskQuestion.WebApi.Extensions.IServiceCollectionExtensions` (internal static). Репозитории регистрируются как Scoped (`AddScoped<I*Repository, *Repository>`).

9 репозиториев: IUserRepository, ISpeakerRepository, IQuestionRepository, IQuestionStatusTransitionRepository, IFaqCategoryRepository, IFaqEntryRepository, IFeedbackRepository, IAreaRepository, IDashboardRepository.

Также: `AddScoped<EnsureVisitorIdAttribute>()`, `ConfigureAuthentication()`, `ConfigureSession()`, `ConfigureSwagger()`, `ConfigureHtmlSanitizer()`, `ConfigureRepositories()`, `ConfigureEmail()`.

## Middleware-порядок (Program.cs)

```
1. dbContext.Database.Migrate()
2. UseSwagger + UseSwaggerUI (Development only)
3. UseCors("AllowFrontend")
4. UseCookiePolicy (Lax, HttpOnly=Always, Secure=None)
5. UseAuthentication()
6. UseAuthorization()
7. UseSession()
8. MapControllers()
```

## Несоответствия и особенности

- **Кириллица в колонке БД (исправлено)**: колонка `Сreated` (кириллица) переименована в `Created` (латиница) миграцией `RenameCyrillicCreatedColumn`
- **Email перенесён в User**: миграция `ReplaceLoginWithEmail` — Email из `UserDetails.Email` → `User.Email`, `User.Login` удалён
- **Сессия**: используется `AddDistributedMemoryCache` + `AddSession` — только для хранения текста капчи. Cookie: `.WebApi.Session`
- **Swagger**: доступен только в Development, XML-документация инжектируется (`GenerateDocumentationFile` + `NoWarn(1591)`)
- **Dockerfile**: обновлён до .NET 10.0. Копирует `Roboto.ttf` в `/usr/share/fonts/` (для капчи через SkiaSharp). `EXPOSE 80`
- **BCrypt.Net-Next**: дублируется в WebApi, DAL и BLL.Tests
- **RuntimeMigrations helper**: файл `Helpers/RuntimeMigrations.cs` существует, но не используется — Program.cs применяет миграции инлайн
- **IQuestionRepository**: находится в `AskQuestion.BLL.Repositories.Interfaces` (согласовано с остальными интерфейсами)
- **SpeakerCreatedDto**: при создании спикера ответ содержит `GeneratedPassword` (сгенерированный пароль)
- **appsettings.Development.json**: существует, содержит настройки логирования (в текущей конфигурации совпадают с `appsettings.json`)
