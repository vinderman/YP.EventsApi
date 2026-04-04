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
Для запуска тестов воспользуйтесь командой dotnet test

### Поиск событий
Для поиска событий используется метод GET /events.
В качестве аргументов принимает query-параметры, описанные классом EventFilter

### Бронирования
Через HTTP доступны следующие операции:

| Метод | Путь | Описание |
|-------|------|----------|
| `POST` | `/events/{id}/book` | Создать бронирование для события с идентификатором `id`. Сначала проверяется существование события. Ответ **202 Accepted**; в теле — `BookingDto`. Заголовок `Location` указывает на ресурс бронирования (`GET /bookings/{bookingId}`). Если событие не найдено — **404**. |
| `GET` | `/bookings/{id}` | Получить бронирование по идентификатору. Ответ **200 OK** с `BookingDto`. Если бронирование не найдено — **404**. |

Модель ответа `BookingDto` содержит поля `Id`, `EventId`, `Status` (перечисление `BookingStatus`: `Pending`, `Confirmed`, `Rejected`).

Дополнительно в `IBookingService` реализованы методы `GetBookingsByStatusAsync` и `UpdateBookingStatusAsync` — они не выставлены как отдельные HTTP-эндпоинты и используются фоновой обработкой (`BookingProcessorBackgroundService`) для поиска ожидающих бронирований и смены их статуса.

### Формат ошибок
При работе с API все ошибки имеют формат ProblemDetails
Расширенная информация об ошибках, если их несколько, находится в поле errors