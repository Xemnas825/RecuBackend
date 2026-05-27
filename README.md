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

La API estará disponible en `http://localhost:<API_PORT>/swagger` y la **web** en `http://localhost:<WEB_PORT>` (por defecto 3000).

Colección Postman: `postman/RecuBackend.postman_collection.json`  
Documentación de entrega: `ENTREGA.md`

### Variables de entorno (`.env`)

| Variable          | Descripción                                   | Ejemplo  |
|-------------------|-----------------------------------------------|----------|
| `API_PORT`        | Puerto de la API (4 últimas cifras usuario)   | `4569`   |
| `DB_PORT`         | Puerto de PostgreSQL (cifras invertidas)      | `9654`   |
| `POSTGRES_DB`     | Nombre de la base de datos                    | `recubackend` |
| `POSTGRES_USER`   | Usuario de PostgreSQL                         | `postgres`    |
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL                    | `postgres`    |
| `JWT_SECRET`        | Clave secreta para firmar tokens JWT          | (mín. 32 chars) |
| `WEB_PORT`          | Puerto del front web                          | `3000`   |

## Desarrollo local (sin Docker)

```bash
# Asegúrate de tener PostgreSQL corriendo en localhost:5432
dotnet run --project RecuBackend.Api
```

## Autenticación JWT

### Registro
```http
POST /api/auth/register
Content-Type: application/json

{ "username": "julio", "password": "Test123!", "displayName": "Julio" }
```

Crea un usuario con rol **User** y devuelve el JWT (igual que el login).

### Login
```http
POST /api/auth/login
Content-Type: application/json

{ "username": "player", "password": "Player123!" }
```

Respuesta: `accessToken`, `userId`, `username`, `role`, `expiresAtUtc`.

Usuarios de demo (seed automático):

| Usuario | Contraseña | Rol   | UserId |
|---------|------------|-------|--------|
| `admin` | `Admin123!` | Admin | `11111111-1111-1111-1111-111111111111` |
| `player` | `Player123!` | User | `22222222-2222-2222-2222-222222222222` |

En Swagger: botón **Authorize** → `Bearer {token}`.

### Roles
- **Guest** (sin token): solo endpoints `/api/public/*`
- **User**: CRUD de sus campañas, personajes, tiradas y adjuntos
- **Admin**: lo anterior + `/api/admin/campaigns` y `/api/admin/users`

## Endpoints principales

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/register` | No | Registrar usuario y obtener JWT |
| POST | `/api/auth/login` | No | Obtener JWT |
| GET | `/api/public/campaigns` | No | Campañas públicas (filtros: search, setting; orden: name, updatedAt…) |
| GET | `/api/public/dnd/spells?name=` | No | Hechizo desde dnd5eapi.co |
| GET | `/api/public/dnd/monsters?name=` | No | Monstruo desde dnd5eapi.co |
| GET/POST/PUT/DELETE | `/api/campaigns` | User/Admin | CRUD de campañas propias |
| GET/POST/PUT/DELETE | `/api/campaigns/{id}/characters` | User/Admin | CRUD de personajes/NPC |
| GET/POST | `/api/characters/{id}/rolls` | User/Admin | Historial y tirada de dados |
| POST | `/api/characters/{id}/attachments` | User/Admin | Subir ficha (multipart, campo `file`) |
| GET | `/api/characters/{id}/attachments` | User/Admin | Listar fichas del personaje |
| GET | `/api/attachments/{id}` | User/Admin | Descargar ficha |
| DELETE | `/api/characters/{id}/attachments/{attachmentId}` | User/Admin | Borrar ficha |
| GET | `/api/admin/campaigns` | Admin | Todas las campañas |
| GET | `/api/admin/users` | Admin | Listado de usuarios |

### Ejemplo de tirada
```http
POST /api/characters/{characterId}/rolls
Authorization: Bearer {token}
Content-Type: application/json

{
  "label": "Percepción",
  "diceExpression": "1d20+3",
  "d20Mode": 1
}
```
`d20Mode`: `0` Normal, `1` Ventaja, `2` Desventaja.

### Subir ficha de personaje (imagen o PDF)
En Swagger: `POST /api/characters/{characterId}/attachments` → **Try it out** → campo `file`.

Formatos permitidos: `.jpg`, `.jpeg`, `.png`, `.webp`, `.pdf` (máx. 10 MB).

Los archivos se guardan en volumen Docker (`/app/uploads`) y persisten al reiniciar contenedores.

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
