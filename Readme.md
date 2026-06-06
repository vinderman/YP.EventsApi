### Запуск проекта
1. В директории YP.EventsApi.Web/Properties создайте файл launchSettings.json со следующим содержимым
    ```
    {
      "$schema": "http://json.schemastore.org/launchsettings.json",
      "profiles": {
        "http": {
          "commandName": "Project",
          "dotnetRunMessages": true,
          "launchBrowser": true,
          "launchUrl": "swagger",
          "applicationUrl": "http://localhost:5133",
          "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
          }
        },
      }
    }
    ```
2. Последовательно запустите команды:

    `dotnet build`

    `dotnet run --project YP.EventsApi.Web`
3. Откройте браузер по <a href="http://localhost:5133/swagger/index.html">адресу</a> 

### Запуск тестов

В решении два тестовых проекта:

| Проект | Назначение |
|--------|------------|
| `Yp.EventsApi.Tests` | Юнит-тесты сервисов (зависимости подменяются через Moq) |
| `Yp.EventsApi.IntegrationTests` | Интеграционные тесты репозиториев и сервисов с реальной PostgreSQL |

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

Проект `Yp.EventsApi.IntegrationTests` проверяет работу с БД через EF Core и репозитории (`EventRepository`, `BookingRepository`), а также сценарии сервисного слоя (например, резервирование мест).

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

### Миграции EF Core

Миграции хранятся в проекте `Yp.EventsApi.DataAccess` (каталог `Migrations/`). Контекст — `AppDbContext`.

При запуске веб-приложения (`YP.EventsApi.Web`) миграции применяются автоматически (`db.Database.Migrate()` в `Program.cs`). Для локальной разработки нужен доступный экземпляр PostgreSQL; строка подключения задаётся в `YP.EventsApi.Web/appsettings.Development.json` (`ConnectionStrings:DefaultConnection`).

Для работы с миграциями из CLI нужен глобальный инструмент:

```bash
dotnet tool install --global dotnet-ef
```

**Создать новую миграцию** (из корня репозитория):

```bash
dotnet ef migrations add <ИмяМиграции> \
  --project Yp.EventsApi.DataAccess \
  --startup-project YP.EventsApi.Web
```

Пример:

```bash
dotnet ef migrations add AddEventVenue \
  --project Yp.EventsApi.DataAccess \
  --startup-project YP.EventsApi.Web
```

**Применить миграции к базе** (без запуска приложения):

```bash
dotnet ef database update \
  --project Yp.EventsApi.DataAccess \
  --startup-project YP.EventsApi.Web
```

**Откатить последнюю миграцию** (удалить файлы миграции из проекта):

```bash
dotnet ef migrations remove \
  --project Yp.EventsApi.DataAccess \
  --startup-project YP.EventsApi.Web
```

**Сгенерировать SQL-скрипт** (без применения к БД):

```bash
dotnet ef migrations script \
  --project Yp.EventsApi.DataAccess \
  --startup-project YP.EventsApi.Web \
  --output migrations.sql
```

Пакет `Microsoft.EntityFrameworkCore.Design` подключён к `YP.EventsApi.Web` — поэтому в командах `dotnet ef` в качестве startup-проекта указывается веб-приложение, а проект с миграциями — `Yp.EventsApi.DataAccess`.

### Поиск событий
Для поиска событий используется метод GET /events.
В качестве аргументов принимает query-параметры, описанные классом EventFilter

### Модель события (Event)
В ответах API событие (модель `EventDto`) содержит поля про вместимость:

| Поле | Тип | Описание |
|------|-----|----------|
| `TotalSeats` | `int` | Общее количество мест на событии (вместимость). Задаётся при создании/обновлении события. |
| `AvailableSeats` | `int` | Текущее количество доступных мест. Инициализируется значением `TotalSeats` и уменьшается при бронировании, увеличивается при отклонении (rejected) бронирования. |

### Бронирования
Через HTTP доступны следующие операции:

| Метод | Путь | Описание |
|-------|------|----------|
| `POST` | `/events/{id}/book` | Создать бронирование для события с идентификатором `id`. Сначала проверяется существование события и наличие доступных мест. Ответ **202 Accepted**; в теле — `BookingDto`. Заголовок `Location` указывает на ресурс бронирования (`GET /bookings/{bookingId}`). Если событие не найдено — **404**. Если для события нет доступных мест — **409 Conflict**. |
| `GET` | `/bookings/{id}` | Получить бронирование по идентификатору. Ответ **200 OK** с `BookingDto`. Если бронирование не найдено — **404**. |

Модель ответа `BookingDto` содержит поля `Id`, `EventId`, `Status` (перечисление `BookingStatus`: `Pending`, `Confirmed`, `Rejected`).

Дополнительно в `IBookingService` реализованы методы `GetBookingsByStatusAsync` и `UpdateBookingStatusAsync` — они не выставлены как отдельные HTTP-эндпоинты и используются фоновой обработкой (`BookingProcessorBackgroundService`) для поиска ожидающих бронирований и смены их статуса.

### Синхронизация и защита от гонок (овербукинга)
В проекте используется `SemaphoreSlim` (семафор) как примитив синхронизации для сериализации критических секций в конкурентных сценариях.

- **Где используется**:
  - В `BookingService.CreateBookingAsync(...)` — вокруг логики «зарезервировать место + добавить бронирование в память».
  - В `BookingProcessorBackgroundService` — вокруг операций отклонения бронирования (чтобы безопасно менять состояние бронирований/мест при параллельной обработке нескольких заявок).
- **Зачем нужно**:
  - Защищает *in-memory* коллекции и общий счётчик `AvailableSeats` от гонок при одновременных запросах.
  - Гарантирует, что две параллельные попытки забронировать последнее место не «пройдут» одновременно (то есть предотвращает овербукинг).

### Пример сценария с овербукингом
Пусть есть событие с `TotalSeats = 1` и `AvailableSeats = 1`. Два клиента почти одновременно вызывают бронирование.

1) Клиент A и клиент B отправляют `POST /events/{id}/book` параллельно.

2) Ожидаемый результат:
- Один запрос получит **202 Accepted** и создаст бронирование (места станут `AvailableSeats = 0`).
- Второй запрос получит **409 Conflict** с ошибкой в формате `ProblemDetails` (причина: «нет доступных мест»).

### Формат ошибок
При работе с API все ошибки имеют формат ProblemDetails
Расширенная информация об ошибках, если их несколько, находится в поле errors