# YP.EventsApi

Распределённая система для управления событиями и бронированиями мест. Три независимых микросервиса на **.NET 8** и **PostgreSQL** (EF Core), асинхронное взаимодействие через **Apache Kafka**.

## Состав системы

| Сервис | Каталог | Назначение | HTTP-порт | База данных | Порт БД |
|--------|---------|------------|-----------|-------------|---------|
| **Users** | `src/Users` | Регистрация пользователей, выдача JWT | 5139 | PostgreSQL `users` | 5432 |
| **Events** | `src/Events` | CRUD событий, резервирование мест | 5133 | PostgreSQL `events` | 5434 |
| **Bookings** | `src/Bookings` | Создание и отмена бронирований | 5140 | PostgreSQL `bookings` | 5433 |

Каждый сервис — отдельное решение со слоями Domain, Application, Infrastructure и Presentation. Общие типы сообщений, исключения и настройки — в `src/Shared`.

Дополнительная инфраструктура (описана в `docker-compose.yml`):

| Компонент | Порт | Назначение |
|-----------|------|------------|
| Apache Kafka | 9092 | Брокер сообщений |
| Kafka UI | 8080 | Веб-интерфейс для просмотра топиков |

## Поток данных BookingConfirmed

При создании бронирования сервисы обмениваются событием через топик Kafka `confirm-booking`.

### Кто публикует

**bookings-api** — при вызове `POST /bookings` сервис `BookingService` создаёт бронирование со статусом `Pending` в БД `bookings` и публикует сообщение `BookingConfirmed` через `CreateBookingProducer` в топик `confirm-booking`.

Формат сообщения (`src/Shared/Messages/BookingConfirmed.cs`):

| Поле | Тип | Описание |
|------|-----|----------|
| `EventId` | `Guid` | Идентификатор события |
| `BookingId` | `Guid` | Идентификатор бронирования |
| `UserId` | `Guid` | Идентификатор пользователя |

Ключ сообщения в Kafka — `EventId` (все бронирования одного события попадают в одну партицию).

### Кто подписан

**events-api** — фоновый сервис `ConfirmBookingConsumer` подписан на топик `confirm-booking` в consumer-группе `events-service`.

### Что происходит при получении

1. Сообщение десериализуется в `BookingConfirmed`.
2. Вызывается `EventService.TryReserveSeats(eventId, 1)` — транзакция с блокировкой строки события (`SELECT FOR UPDATE`) и уменьшением `AvailableSeats`.
3. Офсет коммитится вручную (семантика **at-least-once**):

| Ситуация | Действие |
|----------|----------|
| Место успешно зарезервировано | Офсет сохраняется |
| `NoAvailableSeatsException` — нет мест | Сообщение считается обработанным, офсет сохраняется |
| `EntityNotFoundException` — событие не найдено | Сообщение считается обработанным, офсет сохраняется |
| Ошибка десериализации JSON | Сообщение пропускается, офсет сохраняется |
| Прочая техническая ошибка | Офсет **не** коммитится — сообщение будет доставлено повторно |

## Запуск

### Вариант 1: Docker Compose (рекомендуется)

Из корня репозитория:

```bash
docker compose up --build
```

Поднимаются Kafka (с автосозданием топика `confirm-booking`), Kafka UI, три экземпляра PostgreSQL и три API-сервиса. После старта:

| Сервис | Swagger UI |
|--------|------------|
| Users API | [http://localhost:5139/swagger](http://localhost:5139/swagger) |
| Events API | [http://localhost:5133/swagger](http://localhost:5133/swagger) |
| Bookings API | [http://localhost:5140/swagger](http://localhost:5140/swagger) |
| Kafka UI | [http://localhost:8080](http://localhost:8080) |

Остановка:

```bash
docker compose down
```

Данные БД сохраняются в Docker volumes (`users-db-data`, `events-db-data`, `bookings-db-data`).

### Вариант 2: Локальная разработка

**Требования:** .NET 8 SDK, Docker.

1. Запустите инфраструктуру (Kafka и базы данных):

```bash
docker compose up -d kafka kafka-init kafka-ui users-db events-db bookings-db
```

Либо только Kafka:

```bash
docker compose -f docker-compose.kafka.yml up -d
```

2. Проверьте строки подключения в `appsettings.Development.json` каждого сервиса. Для локального запуска с контейнерами из `docker-compose.yml` используйте порты из таблицы [Состав системы](#состав-системы); для Kafka — `localhost:9092`.

3. Запустите сервисы (в отдельных терминалах):

```bash
dotnet run --project src/Users/Presentation
dotnet run --project src/Events/Presentation
dotnet run --project src/Bookings/Presentation
```

Миграции EF Core применяются автоматически при старте каждого сервиса (`db.Database.Migrate()` в `Program.cs`).

## Аутентификация и авторизация

API использует **JWT Bearer**-аутентификацию. Токен выдаётся после успешного входа и передаётся в заголовке `Authorization: Bearer <token>`.

### Ролевая модель

В системе две роли (`UserRole`):

| Роль | Описание |
|------|----------|
| `User` | Обычный пользователь: просмотр событий, создание бронирований, просмотр и отмена **своих** бронирований |
| `Admin` | Администратор: все права `User`, плюс создание, изменение и удаление событий; доступ к **любым** бронированиям |

Роль хранится в JWT-claim `role` и проверяется атрибутами `[Authorize]` на контроллерах и в сервисном слое.

### Разграничение прав по эндпоинтам

| Метод | Путь | Доступ |
|-------|------|--------|
| `GET` | `/events`, `/events/{id}` | Без аутентификации |
| `POST` | `/users` | Без аутентификации (регистрация) |
| `POST` | `/users/login` | Без аутентификации |
| `POST` | `/events` | Только `Admin` |
| `PUT` | `/events/{id}` | Только `Admin` |
| `DELETE` | `/events/{id}` | Только `Admin` |
| `POST` | `/bookings?eventId={id}` (bookings-api) | Любой аутентифицированный пользователь |
| `GET` | `/bookings/{id}` (bookings-api) | Аутентифицированный пользователь: своя бронь; `Admin` — любая |
| `DELETE` | `/bookings/{id}` (bookings-api) | Владелец брони или `Admin` |

При попытке доступа к чужой брони пользователь с ролью `User` получит **403 Forbidden**. Запросы без токена к защищённым эндпоинтам — **401 Unauthorized**.

### Настройка JWT в конфигурации

Параметры JWT задаются в секции `JwtSettings`:

```json
{
  "JwtSettings": {
    "Secret": "yp-events-api-dev-secret-key-min-32-chars",
    "Issuer": "EventsApi",
    "Audience": "EventsApi",
    "ExpirationMinutes": 15
  }
}
```

| Параметр | Назначение |
|----------|------------|
| `Secret` | Симметричный ключ подписи токена (HMAC-SHA256). Минимум **32 символа** |
| `Issuer` | Издатель токена (`iss`) |
| `Audience` | Аудитория токена (`aud`) |
| `ExpirationMinutes` | Время жизни токена в минутах |

Для локальной разработки значения указаны в `Yp.EventsApi.Presentation/appsettings.Development.json`.

**Продакшн:** не храните `Secret` в репозитории. Задавайте его через переменные окружения, User Secrets или секрет-хранилище (Azure Key Vault, AWS Secrets Manager и т.п.). Используйте криптографически стойкое случайное значение длиной не менее 32 символов, например:

```bash
openssl rand -base64 48
```

Переопределение через переменную окружения:

```bash
export JwtSettings__Secret="<ваш-безопасный-секрет>"
```

### Получение JWT-токена через Swagger

1. Запустите сервисы (см. [Запуск](#запуск)). Для регистрации и входа откройте Swagger **Users API**: [http://localhost:5139/swagger](http://localhost:5139/swagger).

2. **Зарегистрируйте пользователя** (если ещё нет учётной записи): эндпоинт `POST /users` на **users-api**, тело запроса:

   ```json
   {
     "login": "user1",
     "password": "password123",
     "role": "User"
   }
   ```

   Для администратора укажите `"role": "Admin"`.

3. **Выполните вход:** эндпоинт `POST /users/login`, query-параметры `login` и `password`. В ответе **200 OK** придёт строка с JWT-токеном (без префикса `Bearer`).

4. **Авторизуйтесь в Swagger:** нажмите кнопку **Authorize** (замок), введите:

   ```
   Bearer <скопированный-токен>
   ```

   Либо только сам токен — Swagger добавит схему `Bearer` автоматически, если указать значение в формате `Bearer eyJhbGciOi...`.

5. Выполните защищённые запросы (бронирование, просмотр брони, CRUD событий для `Admin`). Токен действует `ExpirationMinutes` минут, после истечения срока повторите вход.

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

Эндпоинты обслуживает **events-api** (`http://localhost:5133`). Большинство операций доступны без аутентификации; создание, изменение и удаление — только для роли `Admin`.

| Метод | Путь | Доступ | Описание |
|-------|------|--------|----------|
| `GET` | `/events` | Публичный | Список событий с фильтрацией и пагинацией. Ответ **200 OK** — `PaginatedResult<EventDto>`. |
| `GET` | `/events/{id}` | Публичный | Событие по идентификатору. Ответ **200 OK** с `EventDto`. Если не найдено — **404**. |
| `POST` | `/events` | `Admin` | Создать событие. Тело — `EventCreateDto`. Ответ **201 Created** с `EventDto` и заголовком `Location`. Ошибки валидации — **400**. |
| `PUT` | `/events/{id}` | `Admin` | Обновить событие. Тело — `EventCreateDto`. Ответ **200 OK** с `EventDto`. Если не найдено — **404**. |
| `DELETE` | `/events/{id}` | `Admin` | Удалить событие. Ответ **204 No Content**. Если не найдено — **404**. |

Бронирование выполняется через **bookings-api** (`POST /bookings?eventId={id}`), см. раздел [API: бронирования](#api-бронирования).

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

Эндпоинты обслуживает **bookings-api** (`http://localhost:5140`). Все запросы требуют JWT-токен (получить на **users-api**). Пользователь с ролью `User` работает только со своими бронированиями; `Admin` имеет доступ ко всем.

| Метод | Путь | Описание |
|-------|------|----------|
| `POST` | `/bookings?eventId={id}` | Создать бронирование. Ответ **202 Accepted** с `BookingDto`. После сохранения в БД публикуется событие `BookingConfirmed` в Kafka; резервирование места выполняет **events-api** (см. [Поток данных BookingConfirmed](#поток-данных-bookingconfirmed)). Если у пользователя 10 активных броней — **409 Conflict**. |
| `GET` | `/bookings/{id}` | Получить бронирование по идентификатору. Ответ **200 OK**. Если не найдено — **404**. Если нет прав — **403 Forbidden**. |
| `DELETE` | `/bookings/{id}` | Отменить бронирование. Ответ **204 No Content**. Если не найдено — **404**. Если нет прав — **403 Forbidden**. |

Модель ответа `BookingDto`:

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Идентификатор бронирования |
| `EventId` | `Guid` | Идентификатор события |
| `Status` | `BookingStatus` | Статус: `Pending`, `Confirmed`, `Rejected` |
| `ProcessedAt` | `DateTime?` | Время подтверждения или отклонения (заполняется фоновой обработкой) |

Дополнительно в `IBookingService` реализованы методы `GetBookingsByStatusAsync`, `ConfirmBookingAsync` и `RejectBookingAsync` — они не выставлены как HTTP-эндпоинты и зарезервированы для дальнейшей обработки статусов бронирования.

## Синхронизация и защита от гонок (овербукинга)

Защита от овербукинга реализована на уровне PostgreSQL и доменной логики:

- **`EventService.TryReserveSeats`** открывает транзакцию, читает строку события с блокировкой `SELECT ... FOR UPDATE` (`GetByIdForUpdateAsync`), вызывает `Event.TryReserveSeats` и сохраняет изменения.
- При отсутствии мест выбрасывается `NoAvailableSeatsException` (**409 Conflict** в API).
- **`EventService.ReleaseSeats`** используется при отклонении бронирования и также работает внутри транзакции с блокировкой строки.

Параллельные запросы на резервирование сериализуются блокировкой строки в БД, а не in-memory примитивами.

### Пример сценария с овербукингом

Пусть есть событие с `TotalSeats = 1` и `AvailableSeats = 1`. Два клиента почти одновременно вызывают бронирование.

1. Клиент A и клиент B отправляют `POST /bookings?eventId={id}` параллельно.
2. Ожидаемый результат:
   - один запрос получит **202 Accepted** и создаст бронирование (`AvailableSeats = 0`);
   - второй запрос получит **409 Conflict** с ошибкой в формате `ProblemDetails` (причина: нет доступных мест).

Аналогичный сценарий покрыт интеграционным тестом `EventServiceReserveSeatsTests.TryReserveSeats_ConcurrentRequests_ShouldPreventOverbooking`.

## Формат ошибок

При работе с API ошибки возвращаются в формате [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807).

- Ошибки валидации (`FluentValidation`) — **400 Bad Request**, тело `ValidationProblemDetails` с полем `errors` (словарь «поле → массив сообщений»).
- Сущность не найдена — **404 Not Found**.
- Недостаточно прав — **403 Forbidden**.
- Нет доступных мест, превышен лимит активных броней — **409 Conflict**.
- Бронирование прошедшего события — **400 Bad Request**.
- Прочие необработанные исключения — **500 Internal Server Error**.
