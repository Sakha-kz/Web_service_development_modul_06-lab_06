# Модуль 06. Лабораторная работа / СРСП: Task Management API (JWT-аутентификация и Claims)

Лабораторная работа (СРСП) по дисциплине «Разработка веб-сервисов» (CSE5032).  
Кафедра Программной инженерии, Институт Автоматики и информационных технологий.

---

## 🎯 Цель работы
Получить практические навыки настройки аутентификации и авторизации в ASP.NET Core Web API, создания JWT-токенов с клеймами (`Claims`), разграничения доступа к публичным и защищённым методам с помощью атрибута `[Authorize]`, а также извлечения контекстных данных пользователя через `HttpContext.User.Claims`.

---

## 🛠 Технологический стек
- **Платформа:** .NET 9.0 (C# 13)
- **Фреймворк:** ASP.NET Core Web API
- **Аутентификация:** `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Документация и тестирование:** Swagger UI (Swashbuckle 6.6.2) с поддержкой схемы `Bearer`

---

## 📂 Структура проекта

```text
Web_service_development_modul_06-lab_06/
├── .gitignore
├── TaskManagementApi.sln
├── README.md
├── DESIGN_AND_ANSWERS.md
└── src/
    └── TaskManagementApi/
        ├── Controllers/
        │   ├── AuthController.cs     # Аутентификация, валидация и генерация JWT с Claims
        │   └── TasksController.cs    # Эндпоинты задач (публичные GET, защищенные POST/DELETE)
        ├── Models/
        │   ├── LoginRequest.cs       # Модель запроса логина (username, password)
        │   └── TaskItem.cs           # Модель задачи (Id, Title, Description, IsCompleted, CreatedBy)
        ├── appsettings.json          # Конфигурация JWT (Issuer, Audience, Key, Expiry)
        ├── Program.cs                # Настройка DI, JwtBearer, Swagger и Middleware
        └── TaskManagementApi.csproj
```

---

## 🚀 Быстрый запуск

### 1. Сборка проекта
```powershell
cd E:\Web_service_development_RK1\week_6\Web_service_development_modul_06-lab_06
dotnet build
```

### 2. Запуск приложения
```powershell
dotnet run --project src\TaskManagementApi
```
После запуска перейдите по адресу: `http://localhost:5000/swagger`.

---

## 📋 Карта эндпоинтов и прав доступа

| HTTP-метод | Маршрут | Авторизация | Описание |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Анонимный | Вход в систему (`student` / `12345`) и получение JWT |
| `GET` | `/api/tasks` | `[AllowAnonymous]` | Просмотр всех задач без токена |
| `GET` | `/api/tasks/{id}` | `[AllowAnonymous]` | Просмотр задачи по ID без токена |
| `POST` | `/api/tasks` | `[Authorize]` | Создание задачи. Извлекает Claim `Name` и записывает автора |
| `DELETE` | `/api/tasks/{id}` | `[Authorize]` | Удаление задачи авторизованным пользователем |

---

## 🧪 Примеры запросов cURL / HTTP

### 1. Получение токена (POST /api/auth/login)
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{"username": "student", "password": "12345"}'
```
**Пример ответа:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresInMinutes": 60,
  "claims": {
    "name": "student",
    "role": "User"
  }
}
```

### 2. Создание задачи без токена (Отказ 401)
```bash
curl -X POST "http://localhost:5000/api/tasks" \
     -H "Content-Type: application/json" \
     -d '{"title": "Новая задача", "description": "Проверка безопасности", "isCompleted": false}'
```
**Ответ:** `HTTP 401 Unauthorized`

### 3. Создание задачи с Bearer-токеном (Успех 201 Created)
```bash
curl -X POST "http://localhost:5000/api/tasks" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <СКОПИРОВАННЫЙ_ТОКЕН>" \
     -d '{"title": "Завершить отчет", "description": "Сдать отчет по модулю 6", "isCompleted": false}'
```
**Ответ сервера:**
```json
{
  "message": "Задача успешно создана аутентифицированным пользователем!",
  "authorClaimName": "student",
  "authorClaimRole": "User",
  "task": {
    "id": 5,
    "title": "Завершить отчет",
    "description": "Сдать отчет по модулю 6",
    "isCompleted": false,
    "createdBy": "student"
  }
}
```
