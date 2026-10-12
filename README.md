# dotnet-label-service

REST-сервис на ASP.NET Core для управления музыкальными треками (CRUD). Данные хранятся в
PostgreSQL, доступ — через Entity Framework Core (провайдер Npgsql).

## Стек

| Компонент | Версия / технология |
|---|---|
| Платформа | .NET 10 (`net10.0`), ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| БД | PostgreSQL 16 (`postgres:16-alpine` в контейнере) |
| Провайдер EF | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` |
| Контейнеризация | Docker / Docker Compose, отладка из Visual Studio |

## Структура репозитория

```
dotnet-label-service/
├─ docker-compose.dcproj              # проект Docker Compose (точка входа для F5 в VS)
├─ docker-compose.yml                 # сервисы: dotnet-label-service + db (Postgres)
├─ docker-compose.override.yml        # настройки для локальной отладки (порты, env, тома)
├─ .env                               # POSTGRES_* (в .gitignore, нужен локально)
├─ dotnet-label-service.slnx          # solution
├─ launchSettings.json                # профиль "Docker Compose" для VS
├─ tools/opencode-dev.ps1             # запуск opencode в Developer PowerShell (см. ниже)
└─ dotnet-label-service/
   ├─ Controllers/TracksController.cs # HTTP-эндпоинты
   ├─ Migrations/                     # EF-миграции + snapshot
   ├─ Models/
   │  ├─ Track.cs                     # сущность
   │  └─ CreateTrackRequest.cs        # DTO запроса на создание/обновление
   ├─ Properties/launchSettings.json  # профили http/https/Docker
   ├─ Program.cs                      # bootstrap, DI, автоприменение миграций
   ├─ dotnet_label_serviceContext.cs  # DbContext + конфигурация модели
   ├─ Dockerfile                      # multi-stage сборка
   ├─ appsettings.json                # строка подключения по умолчанию
   ├─ appsettings.Development.json
   └─ dotnet-label-service.http       # примеры HTTP-запросов
```

## Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (для варианта с контейнерами)
- Visual Studio 2026 / VS Code / Rider — по желанию
- `dotnet-ef` для работы с миграциями:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

## Конфигурация

### Строка подключения

Имя строки подключения — `dotnet_label_serviceContext`. Оно читается в `Program.cs` и, если
не найдено, приложение падает с `InvalidOperationException`.

- `appsettings.json` содержит строку для **локального** запуска:
  ```
  server=localhost;username=postgres;database=dotnet_label_serviceContext
  ```
- При запуске через Docker Compose строка **переопределяется** переменной окружения в
  `docker-compose.override.yml`:
  ```
  ConnectionStrings__dotnet_label_serviceContext=server=db;username=${POSTGRES_USER};database=${POSTGRES_DB};password=${POSTGRES_PASSWORD};port=5432
  ```
  Обратите внимание на хост `db` — это имя сервиса Postgres внутри compose-сети, а не `localhost`.

> Префикс `ConnectionStrings__` (двойное подчёркивание) — это способ ASP.NET Core переопределить
> вложенный ключ `ConnectionStrings:dotnet_label_serviceContext` через переменную окружения.

### Переменные окружения (`.env`)

Файл `.env` в корне репозитория — обязательный локальный конфиг для compose:

```
POSTGRES_DB=label
POSTGRES_USER=postgres
POSTGRES_PASSWORD=<пароль>
```

`.env` добавлен в `.gitignore` и в репозиторий не коммитится — заведите его локально.

## Запуск

Миграции применяются автоматически при старте приложения, но **только в окружении Development**
(см. `Program.cs`). В других окружениях применяйте их явно (`dotnet ef database update`).

### Вариант 1. Docker Compose

1. Создайте `.env` (см. выше).
2. Запустите профиль **Docker Compose** (в VS — из `launchSettings.json`, либо из консоли):

   ```powershell
   docker compose up --build
   ```

3. Сервис доступен по адресам:
   - HTTP:  `http://localhost:8080`
   - HTTPS: `https://localhost:8081`

Postgres слушает внутри compose-сети и отдельного порта на хост не публикует. Данные сохраняются
в volume `db_data`.

### Вариант 2. Локально (`dotnet run`)

1. Поднимите PostgreSQL с параметрами из `appsettings.json` (или поправьте строку подключения под
   свою БД).
2. Запустите:

   ```powershell
   dotnet run --project dotnet-label-service
   ```

3. Профили из `Properties/launchSettings.json`:
   - `http` → `http://localhost:5194`
   - `https` → `https://localhost:7084;http://localhost:5194`

## API

Базовый маршрут контроллера — `api/tracks` (`[Route("api/[controller]")]`, регистр в маршрутах
не важен).

| Метод | Маршрут | Описание | Ответы |
|---|---|---|---|
| `GET` | `/api/tracks` | список всех треков | `200` |
| `GET` | `/api/tracks/{id}` | трек по `id` (Guid) | `200`, `404` |
| `POST` | `/api/tracks` | создать трек | `201` + заголовок `Location` |
| `PUT` | `/api/tracks/{id}` | обновить трек | `204`, `404`, `409` (конкурентность) |
| `DELETE` | `/api/tracks/{id}` | удалить трек | `204`, `404` |

Тело запроса для `POST` и `PUT` — `CreateTrackRequest`:

```json
{
  "title": "Cadillac",
  "artist": "MORGENSHTERN",
  "label": "label1"
}
```

Пример ответа:

```json
{
  "id": "be9512d5-82d7-4b9c-b0bd-48721d23119b",
  "title": "Cadillac",
  "artist": "MORGENSHTERN",
  "label": "label1",
  "createdAt": "2026-09-20T10:59:11.780411Z"
}
```

В окружении Development доступен OpenAPI-документ: `GET /openapi/v1.json`. Swagger UI по умолчанию
не подключён (`MapOpenApi` рендерит только спецификацию).

### Примеры запросов

Файл `dotnet-label-service/dotnet-label-service.http` содержит готовые запросы для
`GET`/`POST`/`DELETE`. Переменная хоста в нём указывает на `https://localhost:8081`
(HTTPS-порт контейнера).

## Модель данных

Сущность `Track` (таблица `Track`):

| Поле | Тип в БД | Примечание |
|---|---|---|
| `Id` | `uuid`, PK | default `gen_random_uuid()` |
| `Title` | `text`, NOT NULL | |
| `Artist` | `text`, NOT NULL | |
| `Label` | `text`, NOT NULL | |
| `CreatedAt` | `timestamp with time zone` | default `NOW()` |
| `Version` | `xid` (системная колонка `xmin`) | токен конкурентности |

Значения по умолчанию `Id`/`CreatedAt` заданы через Fluent API в
`dotnet_label_serviceContext.OnModelCreating`, поэтому попадают в миграции как `defaultValueSql`.

### Конкурентность

Поле `Track.Version` помечено `[Timestamp]` и маппится на системную колонку PostgreSQL `xmin`.
**Отдельная колонка в таблице не создаётся** — это встроенный механизм MVCC в Postgres. При
одновременном изменении одной записи EF бросает `DbUpdateConcurrencyException`, и `PUT` возвращает
`409 Conflict`.

## Миграции EF Core

Миграции лежат в `dotnet-label-service/Migrations` (`InitialCreate` + snapshot).

Добавить новую миграцию (из корня репозитория):

```powershell
dotnet ef migrations add <ИмяМиграции> `
  --project dotnet-label-service/dotnet-label-service.csproj `
  --startup-project dotnet-label-service/dotnet-label-service.csproj
```

> Явно указывайте и `--project`, и `--startup-project`: в корне лежит `docker-compose.dcproj`,
> из-за которого EF иначе не может получить метаданные проекта.

Проверить, что модель и snapshot синхронны:

```powershell
dotnet ef migrations has-pending-model-changes `
  --project dotnet-label-service/dotnet-label-service.csproj `
  --startup-project dotnet-label-service/dotnet-label-service.csproj
```

Применить миграции вручную (например, вне Development или к внешней БД):

```powershell
dotnet ef database update `
  --project dotnet-label-service/dotnet-label-service.csproj `
  --startup-project dotnet-label-service/dotnet-label-service.csproj
```
