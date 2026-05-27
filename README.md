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
