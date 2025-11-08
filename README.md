# FixIt NR API — README (ПР-5)

Учебный **ASP.NET Core Web API** для регистрации заявок на мелкий ремонт/IT-поддержку с вычислением срока **SLA**.  
Проект развернут на **Amvera** и документирован через **Swagger**.

---

## Содержание

- [Назначение](#назначение)
- [Функции](#функции)
- [Архитектура и стек](#архитектура-и-стек)
- [Маршруты API](#маршруты-api)
- [Примеры запросов (cURL)](#примеры-запросов-curl)
- [Swagger (OpenAPI)](#swagger-openapi)
- [Постман-коллекция](#постман-коллекция)
- [Развёртывание на Amvera](#развёртывание-на-amvera)
- [Локальный запуск](#локальный-запуск)
- [XML-комментарии для Swagger](#xml-комментарии-для-swagger)
- [Структура проекта](#структура-проекта)

---

## Назначение

FixIt NR API — учебный веб-сервис для централизованной регистрации заявок на бытовой/ИТ-ремонт в учебной аудитории или офисе.
Сервис принимает заявки от пользователей, стандартизирует состав полей (кто, где, что сломалось, приоритет) и автоматически рассчитывает срок исполнения по SLA — поле slaDueAt, чтобы оператору/преподавателю было понятно, к какой дате должно быть выполнено обращение.

Кому полезен

Пользователи (студенты/сотрудники) — быстро оформить заявку из любой клиентской оболочки (веб/мобайл/чат-бот).

Операторы/преподаватели — видеть поток обращений с автоматическим дедлайном по SLA.

Преподаватель/экзаменатор — проверить корректность API и расчётов через Swagger и Postman.

Цели учебной версии

Показать: проект на ASP.NET Core, базовую доменную модель «Ticket», автоматический расчёт SLA, оформление описаний через XML-комментарии и Swagger.

Обеспечить простую публикацию в сеть (Amvera) и воспроизводимые тесты (Postman).

## Функции

1. Публичные REST-эндпоинты

POST /api/Tickets — создать заявку.
Тело запроса (CreateTicketRequest):

{
"authorName": "Иван",
"category": "IT",
"place": "Ауд. 207",
"description": "Не работает проектор",
"priority": "High"
}

Что делает сервис при создании:

валидирует обязательные поля;

присваивает id (GUID), ставит status = "New";

проставляет createdAt (UTC сейчас);

рассчитывает slaDueAt по правилам ниже;

возвращает 201 Created с объектом Ticket и заголовком Location: /api/Tickets/{id}.

GET /api/Tickets — получить все заявки (может вернуть пустой массив).
Ответ: 200 OK + Ticket[].

GET /api/Tickets/{id} — получить конкретную заявку.
Ответы: 200 OK (если найдена) / 404 Not Found (если нет).

Коды ошибок, которые можно увидеть в ходе тестов:
400 Bad Request — невалидный JSON/пропущены обязательные поля;
404 Not Found — заявка с таким id отсутствует;
405 Method Not Allowed — преднамеренно при запросе неподдерживаемого метода (например, DELETE /api/Tickets).

2. Бизнес-правила SLA (расчёт slaDueAt)

Пусть baseHours — базовые часы по категории, adjust — поправка по приоритету.
Итоговая формула:
due = createdAt + max(6, baseHours + adjust) (часы), минимум всегда 6 ч.

База по категории

Категория baseHours
IT 24
электрика 48
уборка 36
прочее 36

Поправка по приоритету

Приоритет adjust
High −12
Medium 0
Low +12

Примеры

IT + High → 24 − 12 = 12 → slaDueAt ≈ createdAt + 12 ч

электрика + Low → 48 + 12 = 60 → slaDueAt ≈ createdAt + 60 ч

3. Модель данных (возврат сервиса)

Ticket:

{
"id": "uuid",
"authorName": "Иван",
"category": "IT",
"place": "Ауд. 207",
"description": "Не работает проектор",
"priority": "High",
"createdAt": "2025-11-07T12:00:00Z",
"slaDueAt": "2025-11-07T22:00:00Z",
"status": "New"
}

4. Документация и тестирование

Swagger UI всегда доступен по /swagger: описания методов и моделей берутся из XML-комментариев в коде.

Postman-коллекция содержит готовые запросы и тесты:

проверка 201 Created и наличия полей id/createdAt/slaDueAt;

проверка разницы createdAt vs slaDueAt (например, ~12 ч и ~60 ч);

негативные сценарии (400/404/405).

5. Развёртывание / запуск

Amvera: контейнер слушает 8080, конфигурация через amvera.yml; в Program.cs выставлено http://0.0.0.0:8080.

Локально: dotnet build && dotnet run, затем открыть http://localhost:8080/swagger.

## Архитектура и стек

- **ASP.NET Core Web API (.NET 8)**
- **Swagger / Swashbuckle** для авто-документации
- **In-Memory Repository** (без внешней БД)
- Хостинг: **Amvera** (порт **8080**)

---

## Маршруты API

### GET `/api/Tickets`

Возвращает массив заявок.

- **200 OK** →
  ```json
  [
    {
      "id": "uuid",
      "authorName": "Иван",
      "category": "IT",
      "place": "Ауд. 207",
      "description": "Не работает проектор",
      "priority": "High",
      "createdAt": "2025-11-07T12:00:00Z",
      "slaDueAt": "2025-11-07T22:00:00Z",
      "status": "New"
    }
  ]
  GET /api/Tickets/{id}
  Возвращает заявку по идентификатору.
  ```

200 OK | 404 Not Found

POST /api/Tickets
Создаёт заявку и рассчитывает SLA.

Тело запроса:

{
"authorName": "Иван",
"category": "IT",
"place": "Ауд. 207",
"description": "Не работает проектор",
"priority": "High"
}
Ответ 201 Created с объектом Ticket и заголовком Location: /api/Tickets/{id}.

Логика SLA (учебная):

База по категории: IT=24ч, электрика=48ч, уборка=36ч, прочее=36ч

Поправка по приоритету: High −12ч, Low +12ч, Medium 0ч

Минимум: 6ч

## Примеры запросов (cURL)

Замените {{baseUrl}} на ваш домен Amvera, например https://my-fixit.amvera.cloud

# Список заявок

curl -s {{baseUrl}}/api/Tickets

# Создание (IT, High)

curl -s -X POST {{baseUrl}}/api/Tickets \
 -H "Content-Type: application/json" \
 -d '{"authorName":"Иван","category":"IT","place":"Ауд. 207","description":"Не работает проектор","priority":"High"}'

## Swagger (OpenAPI)

Интерфейс: {{baseUrl}}/swagger
В проекте Swagger включён всегда (для удобства проверки на проде).

## Постман-коллекция

Импортируйте этот JSON (в Postman → Import → Raw text), затем установите переменную окружения baseUrl:

{
"info": {
"name": "FixIt NR API — Postman Collection",
"*postman_id": "fixitnr-manual",
"schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
"description": "Коллекция для проверки GET/POST /api/Tickets. Установите переменную окружения baseUrl."
},
"variable": [{ "key": "baseUrl", "value": "https://<ВАШ*ДОМЕН_AMVERA>" }],
"item": [
{ "name": "GET /api/Tickets",
"request": { "method": "GET", "url": { "raw": "{{baseUrl}}/api/Tickets", "host": ["{{baseUrl}}"], "path": ["api","Tickets"] } }
},
{ "name": "POST /api/Tickets (IT, High)",
"request": {
"method": "POST",
"header": [{ "key": "Content-Type", "value": "application/json" }],
"body": { "mode": "raw", "raw": "{\n \"authorName\": \"Иван\",\n \"category\": \"IT\",\n \"place\": \"Ауд. 207\",\n \"description\": \"Не работает проектор\",\n \"priority\": \"High\"\n}" },
"url": { "raw": "{{baseUrl}}/api/Tickets", "host": ["{{baseUrl}}"], "path": ["api","Tickets"] }
}
}
]
}

## Развёртывание на Amvera

В корне репозитория файл amvera.yml (пример для .NET 8):

yaml
Копировать код
meta:
environment: csharp
toolchain:
name: dotnet
version: 8.0
run:
buildFileName: Project1 # имя сборки без .dll (совпадает с названием проекта)
persistenceMount: /data
containerPort: 8080
В Program.cs сервис слушает http://0.0.0.0:8080, Swagger включён.

## Локальный запуск

dotnet build
dotnet run

# открыть: http://localhost:8080/swagger

## XML-комментарии для Swagger

Чтобы в Swagger отображались описания методов/моделей:

В свойствах проекта: Сборка → Выходные данные → Файл документации XML (галочка).
или в .csproj:

<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
В Program.cs подключить XML (динамически):

using System.Reflection;
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
Написать ///-комментарии над методами контроллера и свойствами моделей.

## Структура проекта

Project1/
├─ Controllers/
│ └─ TicketsController.cs
├─ Data/ # InMemoryTicketRepository (опционально)
├─ Contracts/ # DTO/модели запросов и ответов
├─ Models/ # Доменные модели (Ticket и пр.)
├─ Program.cs
└─ amvera.yml
