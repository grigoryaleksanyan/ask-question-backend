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
| `AskQuestion.BLL` | Репозитории (интерфейсы + реализации), DTO |
| `AskQuestion.WebApi` | Controllers, Request/Response модели, Program.cs, Extensions, ActionFilters |

Слой DAL → BLL — **нет**. BLL ссылается на DAL (и Core). WebApi ссылается на BLL и DAL напрямую (для `DataContext` в DI).

## База данных

PostgreSQL 16.1 через Npgsql EF Core provider. Контекст: `AskQuestion.DAL.DataContext`.

- Соединение: `appsettings.json` → `ConnectionStrings:PostgreSQL` (host `localhost`, port `5432`, db `AskQuestionDb`, user `postgres`, password `12345`)
- Миграции: `AskQuestion.DAL/Migrations/`. Применяются **автоматически** при старте (`dbContext.Database.Migrate()` в `Program.cs`)
- Seed: Admin-пользователь + UserDetails + две роли (Administrator, Speaker) — через `OnModelCreating`
- Расширение PostgreSQL: `uuid-ossp`
- Первичные ключи: `Guid` (через `uuid-ossp`), базовая сущность `BaseEntity` (`Id`, `Created`, `Updated`)

Создание миграции (из корня решения):
```
dotnet ef migrations add <Name> --project AskQuestion.DAL --startup-project AskQuestion.WebApi
```

## Сущности

| Сущность | Наследует | Ключ | Примечания |
|---------|-----------|------|-----------|
| `BaseEntity` | — | `Guid Id` | `Created`, `Updated` — колонки теперь с латиницей (миграция `RenameCyrillicCreatedColumn`) |
| `User` | BaseEntity | Guid | Email (unique index), Password (BCrypt), UserRoleId FK → UserRole |
| `UserDetails` | BaseEntity | Guid | UserId FK → User, FirstName, LastName, Patronymic, Position, AdditionalInfo, IsDeleted |
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

Cookie-аутентификация (`CookieAuthenticationDefaults.AuthenticationScheme`). Cookie: `.WebApi`. Claims: `Name` (email), `NameIdentifier` (Guid), `Role` (по `UserRoleId`). Срок: 1 день. Роли: 1 = Administrator, 2 = Speaker. Константы ролей — `AskQuestion.Core.Constants.UserStringRoles`. Авторизация по Email (уникальный, required). Email хранится в `User.Email`, в `UserDetails` поля Email нет.

## Контроллеры

Все маршруты начинаются с `api/`. Response caching отключён глобально на контроллерах.

| Контроллер | Маршрут | Авторизация |
|-----------|---------|-------------|
| `AuthController` | `api/Auth` | Login — анонимно, Logout — Admin+Speaker |
| `QuestionController` | `api/Question` | GetCaptcha, GetAll, GetPopularQuestions, GetById, Like, Dislike, Create — анонимно; ChangeStatus, SetComment — Admin+Speaker; Update, Delete — Admin |
| `FaqCategoryController` | `api/FaqCategory` | GetAllWithEntries — анонимно; GetAllWithEntriesForAdmin — Admin; GetAll, GetById, Create, Update, Delete, SetOrder — Admin |
| `FaqEntryController` | `api/FaqEntry` | Весь контроллер — Admin (authorizate на уровне класса) |
| `FeedbackController` | `api/Feedback` | Create — анонимно; GetAll, Delete — Admin |
| `AreaController` | `api/Area` | GetAll — анонимно; Create, Update, Delete, SetOrder — Admin |
| `UserController` | `api/User` | GetUserData, ChangePassword — Admin+Speaker |
| `SpeakerController` | `api/Speaker` | GetAllPublic — анонимно; GetAll, GetById, Create, Update, Delete — Admin |
| `DashboardController` | `api/Dashboard` | Summary — Admin |

## Ключевые функции

### Голосование за вопросы

Toggle-голосование (анонимное, по VisitorId). Cookie `VisitorId` (HttpOnly, Lax, 365 дней) устанавливается через `EnsureVisitorIdAttribute` (ActionFilter, Scoped). Endpoints: `POST {id}/like`, `POST {id}/dislike`. Возвращает `VoteResultDto` с актуальными счётчиками и голосом пользователя.

### Пагинация вопросов

`GetAll` принимает `page`, `pageSize`, `status`, `speakerId`, `areaId`, `search`, `sortOrder`. Возвращает `PaginatedResult<QuestionViewModel>` с `Items`, `TotalCount`, `Page`, `PageSize`.

### Статусы вопросов

`QuestionStatus` enum (New, InFocus, Answered). Поле `Question.Status` (int). Поле `Question.Comment` (string?) — комментарий при смене статуса. Поле `Question.Answered` (DateTimeOffset?) — дата ответа. Смена статуса логируется через `QuestionStatusTransition` (FromStatus, ToStatus, ChangedByUserId).

### Популярные вопросы

`GET GetPopularQuestions` — анонимно, возвращает топ вопросов.

### Счётчик просмотров

`IncrementViewsAsync` при `GetById`.

### Дашборд

`GET Summary` — Admin. Параметры: `periodDays` (по умолчанию 30), `speakerId` (опционально). Возвращает `DashboardSummaryDto` с `SpeakerProductivityDto`, `SpeakerAreaDto`.

## DI

Регистрация сервисов — в `AskQuestion.WebApi.Extensions.IServiceCollectionExtensions` (internal static). Репозитории регистрируются как Scoped (`AddScoped<I*Repository, *Repository>`).

9 репозиториев: IUserRepository, ISpeakerRepository, IQuestionRepository, IQuestionStatusTransitionRepository, IFaqCategoryRepository, IFaqEntryRepository, IFeedbackRepository, IAreaRepository, IDashboardRepository.

Также: `AddScoped<EnsureVisitorIdAttribute>()`.

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
- **Сессия**: используется `AddDistributedMemoryCache` + `AddSession` — только для хранения текста капчи
- **Swagger**: доступен только в Development, XML-документация инжектируется (`GenerateDocumentationFile` + `NoWarn(1591)`)
- **Dockerfile**: обновлён до .NET 10.0. Копирует `Roboto.ttf` в `/usr/share/fonts/` (для капчи через SkiaSharp)
- **BCrypt.Net-Next**: дублируется в WebApi и DAL
