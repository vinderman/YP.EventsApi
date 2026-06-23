# YP.EventsApi

REST API для управления событиями и бронированиями мест. Проект построен на **.NET 8** и **PostgreSQL** (EF Core).

## Структура решения

| Проект | Назначение |
|--------|------------|
| `Yp.EventsApi.Domain` | Доменные сущности (`Event`, `Booking`), перечисления и доменные исключения |
| `Yp.EventsApi.Application` | Сервисы, интерфейсы репозиториев, модели запросов и фильтров |
| `Yp.EventsApi.Infrastructure` | EF Core (`AppDbContext`), репозитории, миграции, `UnitOfWork` |
| `Yp.EventsApi.Presentation` | ASP.NET Core Web API: контроллеры, middleware, Swagger |
| `Yp.EventsApi.Tests` | Юнит-тесты сервисов (зависимости подменяются через Moq) |
| `Yp.EventsApi.IntegrationTests` | Интеграционные тесты репозиториев и сервисов с реальной PostgreSQL |

## Запуск проекта

**Требования:** .NET 8 SDK, запущенный экземпляр PostgreSQL.

1. Настройте строку подключения в `Yp.EventsApi.Presentation/appsettings.Development.json` (`ConnectionStrings:DefaultConnection`).

2. Соберите и запустите приложение:

```bash
dotnet build
dotnet run --project Yp.EventsApi.Presentation
```

Профили запуска (`http`, `https`) уже описаны в `Yp.EventsApi.Presentation/Properties/launchSettings.json`. По умолчанию API доступен на `http://localhost:5133`, Swagger — на [http://localhost:5133/swagger/index.html](http://localhost:5133/swagger/index.html).

При старте приложения миграции применяются автоматически (`db.Database.Migrate()` в `Program.cs`).

## Запуск тестов

Запуск всех тестов:

```bash
dotnet test
```

Только юнит-тесты:

```bash
dotnet test Yp.EventsApi.Tests
```

Только интеграционные тесты:

```bash
dotnet test Yp.EventsApi.IntegrationTests
```

### Интеграционные тесты и Testcontainers

Проект `Yp.EventsApi.IntegrationTests` проверяет работу с БД через EF Core и репозитории (`EventRepository`, `BookingRepository`), схему миграций, а также сценарии сервисного слоя (резервирование мест, защита от овербукинга).

Для изоляции от локальной базы используется [Testcontainers](https://dotnet.testcontainers.org/): перед прогоном тестов поднимается Docker-контейнер PostgreSQL (`postgres:16-alpine`), к нему применяются миграции, после чего выполняются тесты.

**Требования:**

- установленный и запущенный **Docker** (Docker Desktop или аналог);
- доступ к Docker API из среды, где выполняется `dotnet test`.

**Как устроено:**

- `PostgresFixture` — общая фикстура xUnit (`IAsyncLifetime`): стартует контейнер, создаёт `AppDbContext` и вызывает `Database.MigrateAsync()`.
- `PostgresCollection` — коллекция xUnit (`ICollectionFixture<PostgresFixture>`), чтобы один контейнер использовался всеми тестами в прогоне.
- Тесты помечены `[Collection(nameof(PostgresCollection))]` и получают `PostgresFixture` через конструктор.
- Перед каждым тестом данные очищаются (`DatabaseCleaner`), при необходимости заполняются сидами (`TestDataSeed`).

Если Docker недоступен, интеграционные тесты не запустятся (ошибка при старте контейнера).

## Миграции EF Core

Миграции хранятся в проекте `Yp.EventsApi.Infrastructure` (каталог `Migrations/`). Контекст — `AppDbContext`.

Для работы с миграциями из CLI нужен глобальный инструмент:

```bash
dotnet tool install --global dotnet-ef
```

**Создать новую миграцию** (из корня репозитория):

```bash
dotnet ef migrations add <ИмяМиграции> \
  --project Yp.EventsApi.Infrastructure \
  --startup-project Yp.EventsApi.Presentation
```

Пример:

```bash
dotnet ef migrations add AddEventVenue \
  --project Yp.EventsApi.Infrastructure \
  --startup-project Yp.EventsApi.Presentation
```

**Применить миграции к базе** (без запуска приложения):

```bash
dotnet ef database update \
  --project Yp.EventsApi.Infrastructure \
  --startup-project Yp.EventsApi.Presentation
```

**Откатить последнюю миграцию** (удалить файлы миграции из проекта):

```bash
dotnet ef migrations remove \
  --project Yp.EventsApi.Infrastructure \
  --startup-project Yp.EventsApi.Presentation
```

**Сгенерировать SQL-скрипт** (без применения к БД):

```bash
dotnet ef migrations script \
  --project Yp.EventsApi.Infrastructure \
  --startup-project Yp.EventsApi.Presentation \
  --output migrations.sql
```

Пакет `Microsoft.EntityFrameworkCore.Design` подключён к `Yp.EventsApi.Presentation` — поэтому в командах `dotnet ef` в качестве startup-проекта указывается веб-приложение, а проект с миграциями — `Yp.EventsApi.Infrastructure`.

## API: события

| Метод | Путь | Описание |
|-------|------|----------|
| `GET` | `/events` | Список событий с фильтрацией и пагинацией. Ответ **200 OK** — `PaginatedResult<EventDto>`. |
| `GET` | `/events/{id}` | Событие по идентификатору. Ответ **200 OK** с `EventDto`. Если не найдено — **404**. |
| `POST` | `/events` | Создать событие. Тело — `EventCreateDto`. Ответ **201 Created** с `EventDto` и заголовком `Location`. Ошибки валидации — **400**. |
| `PUT` | `/events/{id}` | Обновить событие. Тело — `EventCreateDto`. Ответ **200 OK** с `EventDto`. Если не найдено — **404**. |
| `DELETE` | `/events/{id}` | Удалить событие. Ответ **204 No Content**. Если не найдено — **404**. |
| `POST` | `/events/{id}/book` | Создать бронирование для события. Ответ **202 Accepted** с `BookingDto`; заголовок `Location` указывает на `GET /bookings/{bookingId}`. Если событие не найдено — **404**. Если нет доступных мест — **409 Conflict**. |

### Поиск и фильтрация событий

Метод `GET /events` принимает query-параметры из класса `EventFilter` (наследует `Pagination`):

| Параметр | Тип | Описание |
|----------|-----|----------|
| `Title` | `string?` | Подстрока в названии (регистронезависимый поиск через `ILIKE`) |
| `From` | `DateTime?` | События, у которых `StartAt >= From` |
| `To` | `DateTime?` | События, у которых `EndAt <= To` |
| `Page` | `int` | Номер страницы (по умолчанию `1`) |
| `PageSize` | `int` | Размер страницы (по умолчанию `10`) |

Ответ содержит поля `Items`, `Total`, `CurrentPage`, `PageSize`.

### Модель события (`EventDto`)

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Идентификатор события |
| `Title` | `string` | Название |
| `Description` | `string?` | Описание |
| `StartAt` | `DateTime` | Дата и время начала |
| `EndAt` | `DateTime` | Дата и время окончания |
| `TotalSeats` | `int` | Общее количество мест (вместимость). Задаётся при создании/обновлении. |
| `AvailableSeats` | `int` | Текущее количество доступных мест. Инициализируется значением `TotalSeats`, уменьшается при бронировании, увеличивается при отклонении бронирования. |

## API: бронирования

| Метод | Путь | Описание |
|-------|------|----------|
| `GET` | `/bookings/{id}` | Получить бронирование по идентификатору. Ответ **200 OK**. Если не найдено — **404**. |

Модель ответа `BookingDto`:

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Идентификатор бронирования |
| `EventId` | `Guid` | Идентификатор события |
| `Status` | `BookingStatus` | Статус: `Pending`, `Confirmed`, `Rejected` |
| `ProcessedAt` | `DateTime?` | Время подтверждения или отклонения (заполняется фоновой обработкой) |

Дополнительно в `IBookingService` реализованы методы `GetBookingsByStatusAsync`, `ConfirmBookingAsync` и `RejectBookingAsync` — они не выставлены как отдельные HTTP-эндпоинты и используются фоновой обработкой (`BookingProcessorBackgroundService`) для поиска ожидающих бронирований и смены их статуса.

Фоновый сервис опрашивает очередь каждые 10 секунд: для каждого бронирования в статусе `Pending` проверяется существование события, затем через ~2 секунды бронирование подтверждается (`Confirmed`) или отклоняется (`Rejected`) с возвратом мест на событие.

## Синхронизация и защита от гонок (овербукинга)

Защита от овербукинга реализована на уровне PostgreSQL и доменной логики:

- **`EventService.TryReserveSeats`** открывает транзакцию, читает строку события с блокировкой `SELECT ... FOR UPDATE` (`GetByIdForUpdateAsync`), вызывает `Event.TryReserveSeats` и сохраняет изменения.
- При отсутствии мест выбрасывается `NoAvailableSeatsException` (**409 Conflict** в API).
- **`EventService.ReleaseSeats`** используется при отклонении бронирования и также работает внутри транзакции с блокировкой строки.

Параллельные запросы на резервирование сериализуются блокировкой строки в БД, а не in-memory примитивами.

### Пример сценария с овербукингом

Пусть есть событие с `TotalSeats = 1` и `AvailableSeats = 1`. Два клиента почти одновременно вызывают бронирование.

1. Клиент A и клиент B отправляют `POST /events/{id}/book` параллельно.
2. Ожидаемый результат:
   - один запрос получит **202 Accepted** и создаст бронирование (`AvailableSeats = 0`);
   - второй запрос получит **409 Conflict** с ошибкой в формате `ProblemDetails` (причина: нет доступных мест).

Аналогичный сценарий покрыт интеграционным тестом `EventServiceReserveSeatsTests.TryReserveSeats_ConcurrentRequests_ShouldPreventOverbooking`.

## Формат ошибок

При работе с API ошибки возвращаются в формате [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807).

- Ошибки валидации (`FluentValidation`) — **400 Bad Request**, тело `ValidationProblemDetails` с полем `errors` (словарь «поле → массив сообщений»).
- Сущность не найдена — **404 Not Found**.
- Нет доступных мест — **409 Conflict**.
- Прочие необработанные исключения — **500 Internal Server Error**.
