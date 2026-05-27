# RecuBackend – DnD NPC Manager API

API RESTful en **.NET 8** para gestionar campañas de rol (DnD), personajes/NPCs, tiradas de dados y fichas de personaje con imágenes.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Inicio rápido con Docker

```bash
# Copia el fichero de entorno y ajusta los puertos según tu usuario de San Valero
cp .env.example .env

# Levanta API + PostgreSQL
docker compose --env-file .env up --build -d
```

La API estará disponible en `http://localhost:<API_PORT>` y Swagger en `http://localhost:<API_PORT>/swagger`.

### Variables de entorno (`.env`)

| Variable          | Descripción                                   | Ejemplo  |
|-------------------|-----------------------------------------------|----------|
| `API_PORT`        | Puerto de la API (4 últimas cifras usuario)   | `4569`   |
| `DB_PORT`         | Puerto de PostgreSQL (cifras invertidas)      | `9654`   |
| `POSTGRES_DB`     | Nombre de la base de datos                    | `recubackend` |
| `POSTGRES_USER`   | Usuario de PostgreSQL                         | `postgres`    |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL                    | `postgres`    |

## Desarrollo local (sin Docker)

```bash
# Asegúrate de tener PostgreSQL corriendo en localhost:5432
dotnet run --project RecuBackend.Api
```

## API (rama `feat/models-crud`)

### Autenticación temporal
Hasta implementar JWT, envía la cabecera:
```
X-User-Id: 00000000-0000-0000-0000-000000000001
```

### Endpoints principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/public/campaigns` | Campañas públicas (sin auth) |
| GET/POST/PUT/DELETE | `/api/campaigns` | CRUD de campañas del usuario |
| GET/POST/PUT/DELETE | `/api/campaigns/{id}/characters` | CRUD de personajes/NPC |
| GET/POST | `/api/characters/{id}/rolls` | Historial y tirada de dados (`1d20+5`, ventaja/desventaja) |
| GET/POST/DELETE | `/api/characters/{id}/attachments` | Metadatos de fichas (subida real en rama posterior) |

### Ejemplo de tirada
```http
POST /api/characters/{characterId}/rolls
X-User-Id: {tu-guid}
Content-Type: application/json

{
  "label": "Percepción",
  "diceExpression": "1d20+3",
  "d20Mode": 1
}
```
`d20Mode`: `0` Normal, `1` Ventaja, `2` Desventaja.

## Estructura del proyecto

```
RecuBackend/
├── RecuBackend.Api/
│   ├── Controllers/        # Controladores HTTP
│   ├── Data/               # DbContext y Migrations
│   ├── Models/             # Entidades
│   ├── Services/           # Lógica de negocio
│   ├── Dtos/               # Objetos de transferencia
│   ├── Dockerfile
│   └── Program.cs
├── docker-compose.yml
├── .env.example
└── RecuBackend.sln
```
