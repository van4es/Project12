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

Сервис фиксирует заявки пользователей (автор, категория, место, описание, приоритет) и автоматически рассчитывает крайний срок исполнения по **SLA**.

## Функции

- **POST** создание заявки с расчётом `slaDueAt` (зависит от `category` и `priority`).
- **GET** получение списка заявок.
- (Учебная версия) Хранение в памяти, без БД.

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

json
Копировать код
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

bash
Копировать код

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

bash
Копировать код
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

bash
Копировать код
Project1/
├─ Controllers/
│ └─ TicketsController.cs
├─ Data/ # InMemoryTicketRepository (опционально)
├─ Contracts/ # DTO/модели запросов и ответов
├─ Models/ # Доменные модели (Ticket и пр.)
├─ Program.cs
└─ amvera.yml
