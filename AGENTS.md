# AGENTS.md

## Команды

- `dotnet run` в `AskQuestion.WebApi/` — запуск на `http://localhost:5500` (Swagger на `/swagger`)
- `dotnet build` в корне решения — сборка всех проектов
- Тестов нет, тест-проектов нет

## Архитектура

.NET 10, решение `ask-question-backend.sln`. Четыре проекта с линейной зависимостью:

```
WebApi → BLL → DAL → Core
WebApi → DAL
```

| Проект | Назначение |
|--------|-----------|
| `AskQuestion.Core` | Enums (`UserRoles`, `VoteType`, `QuestionStatus`), Constants (`UserStringRoles`) — без зависимостей |
| `AskQuestion.DAL` | EF Core + Npgsql, сущности, `DataContext`, миграции |
| `AskQuestion.BLL` | Репозитории (интерфейсы + реализации), DTO, Email-подсистема |
| `AskQuestion.WebApi` | Controllers, Request/Response модели, Program.cs, Extensions, ActionFilters |

Слой DAL → BLL — **нет**. BLL ссылается на DAL (и Core). WebApi ссылается на BLL и DAL напрямую (для `DataContext` в DI).

## База данных

PostgreSQL 16.1 через Npgsql EF Core provider. Контекст: `AskQuestion.DAL.DataContext`.

- Соединение: `appsettings.json` → `ConnectionStrings:PostgreSQL` (host `localhost`, port `5432`, db `AskQuestionDb`, user `postgres`, password `12345`)
- Миграции: `AskQuestion.DAL/Migrations/`. Применяются **автоматически** при старте (`dbContext.Database.Migrate()` в `Program.cs`)
- Seed: **только две роли** (Administrator, Speaker) — через `OnModelCreating`. Администратор больше не сидируется — создаётся через API при первичной настройке
- Расширение PostgreSQL: `uuid-ossp`
- Первичные ключи: `Guid` (через `uuid-ossp`), базовая сущность `BaseEntity` (`Id`, `Created`, `Updated`)

Создание миграции (из корня решения):
```
dotnet ef migrations add <Name> --project AskQuestion.DAL --startup-project AskQuestion.WebApi
```

## Сущности

| Сущность | Наследует | Ключ | Примечания |
|---------|-----------|------|-----------|
| `BaseEntity` | — | `Guid Id` | `Created`, `Updated` — колонки с латиницей (миграция `RenameCyrillicCreatedColumn`) |
| `User` | BaseEntity | Guid | Email (unique index), Password (BCrypt), UserRoleId FK → UserRole |
| `UserDetails` | BaseEntity | Guid | UserId FK → User, FirstName, LastName, Patronymic, Position, AdditionalInfo, IsDeleted. Метод `GetFullName()` (LastName + FirstName + Patronymic) |
| `UserRole` | — | `int UserRoleId` | Не наследует BaseEntity. Seed: 1=Administrator, 2=Speaker |
| `Question` | BaseEntity | Guid | Text, Author, AreaId? FK→Area (SetNull), SpeakerId? FK→User (SetNull), Views, Likes, Dislikes, Status (int), Comment?, Answered? |
| `QuestionStatusTransition` | BaseEntity | Guid | QuestionId FK→Question (Cascade), FromStatus (int), ToStatus (int), ChangedByUserId? FK→User (SetNull) |
| `QuestionVote` | — | Composite `{QuestionId, VisitorId}` | VoteType enum. FK→Question (Cascade) |
| `Area` | BaseEntity | Guid | Title, Order |
| `FaqCategory` | BaseEntity | Guid | Name, Order, Nav: FaqEntries |
| `FaqEntry` | BaseEntity | Guid | FaqCategoryId FK, Question, Answer, Order |
| `Feedback` | BaseEntity | Guid | Username, Email, Theme, Text |

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
| `AuthController` | `api/Auth` | SetupRequired, Setup, Login — анонимно; Logout — Admin+Speaker |
| `QuestionController` | `api/Question` | GetCaptcha, GetAll, GetPopularQuestions, GetById, Like, Dislike, Create — анонимно; ChangeStatus (`{id}/status`), SetComment (`{id}/comment`) — Admin+Speaker (Speaker только для своих вопросов); Update, Delete — Admin |
| `FaqCategoryController` | `api/FaqCategory` | GetAllWithEntries — анонимно; GetAllWithEntriesForAdmin — Admin; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `FaqEntryController` | `api/FaqEntry` | Весь контроллер — Authorize на уровне класса; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `FeedbackController` | `api/Feedback` | Create — анонимно; GetAll, Delete — Admin |
| `AreaController` | `api/Area` | GetAll — анонимно; Create, Update, Delete, SetOrder — Admin |
| `UserController` | `api/User` | GetUserData, ChangePassword — Admin+Speaker |
| `SpeakerController` | `api/Speaker` | GetAllPublic — анонимно; GetAll, GetById, Create, Update, Delete — Admin |
| `DashboardController` | `api/Dashboard` | Summary — Admin |

## Ключевые функции

### Первичная настройка администратора (Setup)

Администратор больше не сидируется в БД. При старте без админа — требуется первичная настройка:
- `GET api/Auth/SetupRequired` — возвращает `{ SetupRequired: bool }` (true если админа нет)
- `POST api/Auth/Setup` — создаёт первого администратора (только если админа нет). Принимает `AdminSetupModel` (Email, Password, ConfirmPassword, FirstName, LastName, Patronymic?). Автоматически логинит созданного пользователя

Миграция `RemoveAdminSeedData` убрала хардкод-сид админа.

### Email-подсистема

`AskQuestion.BLL/Email/` — отправка email через MailHog (dev) / SMTP (prod):

| Класс | Назначение |
|-------|-----------|
| `IEmailSender` | Интерфейс с методом `EnqueueAsync(EmailMessage)` |
| `EmailSender` | Реализация через `Channel<EmailMessage>` (producer-consumer, unbounded), Singleton |
| `EmailMessage` | DTO: ToEmail, ToName, Subject, HtmlBody |
| `SmtpSettings` | Конфигурация SMTP: Host, Port, FromEmail, FromName, BaseUrl |
| `EmailBackgroundService` | BackgroundService, читает из Channel и отправляет через `SmtpClient` |
| `EmailTemplateBuilder` | Статический класс с HTML-шаблонами писем |

Шаблоны писем:
1. `BuildNewQuestionNotification` — уведомление спикеру о новом вопросе (вызывается в `QuestionRepository.CreateAsync`)
2. `BuildSpeakerCredentials` — отправка реквизитов доступа спикеру при создании (вызывается в `SpeakerRepository.CreateAsync`)

DI-регистрация: `ConfigureEmail()` в `IServiceCollectionExtensions` — `IOptions<SmtpSettings>`, `IEmailSender` → `EmailSender` (Singleton), `EmailBackgroundService` (HostedService).

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
| ByStatus | StatusDistributionDto | Распределение по статусам |
| Timeline | TimelinePointDto[] | График по дням (Date, NewCount, AnsweredCount) |
| ByArea | AreaDistributionDto[] | Распределение по областям |
| TopSpeakers | SpeakerProductivityDto[] | Топ спикеров |
| SpeakerAreas | SpeakerAreaDto[] | Спикеры по областям |
| Votes | VotesSummaryDto | Голоса (TotalLikes, TotalDislikes) |

### Мягкое удаление спикеров

`SpeakerRepository.DeleteAsync` не удаляет спикера физически — устанавливает `user.UserDetails.IsDeleted = true`. Удалённые спикеры не могут авторизоваться (`UserRepository.AuthorizeUser` возвращает `null` для soft-deleted пользователей).

### Капча

`GenerateCaptcha` (`AskQuestion.WebApi/Helpers/GenerateCaptcha.cs`) — статический класс, генерирует капчу через SkiaSharp (Base64-изображение). Текст капчи хранится в сессии.

### HtmlSanitizer

`AskQuestion.BLL/Helpers/HtmlSanitizerService.cs` — реализация `IHtmlSanitizerService`, синглтон. Регистрируется через `ConfigureHtmlSanitizer()` в DI. Используется для санитизации HTML-контента.

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
- **BCrypt.Net-Next**: дублируется в WebApi и DAL
- **RuntimeMigrations helper**: файл `Helpers/RuntimeMigrations.cs` существует, но не используется — Program.cs применяет миграции инлайн
- **IQuestionRepository**: находится в `AskQuestion.BLL.Repositories.Interfaces` (согласовано с остальными интерфейсами)
- **SpeakerCreatedDto**: при создании спикера ответ содержит `GeneratedPassword` (сгенерированный пароль)
- **appsettings.Development.json**: существует, переопределяет уровень логирования
