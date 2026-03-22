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
Для поиск событий используется метод GET /events.
В качестве аргументов принимает query-параметры, описанные классом EventFilter

### Формат ошибок
При работе с API все ошибки имеют формат ProblemDetails
Расширенная информация об ошибках, если их несколько, находится в поле errors