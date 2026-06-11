# RecuBackend – DnD Campaign Manager

API REST en **.NET 8** con frontend web para gestionar campañas de rol (D&D 5e): personajes, NPCs, tiradas de dados, compendio D&D y fichas de personaje (PDF/imágenes) alojadas en **Cloudinary**.

## Contenido del repositorio

| Componente | Descripción |
|------------|-------------|
| `RecuBackend.Api/` | API REST, autenticación JWT, EF Core + PostgreSQL |
| `RecuFrontend/` | Interfaz web (HTML/CSS/JS) servida con nginx |
| `RecuBackend.Tests/` | Tests unitarios |
| `samples/` | Fichas de ejemplo y guía de formatos (`FORMATO-FICHAS.md`) |
| `postman/` | Colección Postman |

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) (desarrollo local)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recomendado)
- Cuenta gratuita en [Cloudinary](https://cloudinary.com/console) (subida de fichas)

---

## Inicio rápido con Docker

```bash
cp .env.example .env
# Edita .env: puertos, JWT_SECRET y credenciales Cloudinary

docker compose --env-file .env up --build -d
```

| Servicio | URL |
|----------|-----|
| **Web** | `http://localhost:<WEB_PORT>` (por defecto `3000`) |
| **API / Swagger** | `http://localhost:<API_PORT>/swagger` (por defecto `4569`) |
| **PostgreSQL** | `localhost:<DB_PORT>` (por defecto `9654`) |

---

## Variables de entorno (`.env`)

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `API_PORT` | Puerto de la API | `4569` |
| `DB_PORT` | Puerto de PostgreSQL | `9654` |
| `WEB_PORT` | Puerto del frontend | `3000` |
| `POSTGRES_DB` | Nombre de la base de datos | `recubackend` |
| `POSTGRES_USER` | Usuario PostgreSQL | `postgres` |
| `POSTGRES_PASSWORD` | Contraseña PostgreSQL | `postgres` |
| `JWT_SECRET` | Clave para firmar JWT (mín. 32 caracteres) | — |
| `CLOUDINARY_CLOUD_NAME` | Cloud name del dashboard Cloudinary | `dxxxxxx` |
| `CLOUDINARY_API_KEY` | API Key de Cloudinary | — |
| `CLOUDINARY_API_SECRET` | API Secret de Cloudinary | — |

> **No subas el `.env` a Git.** Copia las tres variables de Cloudinary desde [cloudinary.com/console](https://cloudinary.com/console) → Dashboard.

Tras cambiar `.env`, reinicia la API:

```bash
docker compose --env-file .env up --build -d
```

### Desarrollo local (sin Docker)

1. PostgreSQL en `localhost:5432`.
2. Configura `CloudinarySettings` en `RecuBackend.Api/appsettings.json` o variables de entorno.
3. Ejecuta:

```bash
dotnet run --project RecuBackend.Api
```

---

## Autenticación

### Login con rol de sesión

Al iniciar sesión (excepto la cuenta `admin`) eliges **Dungeon Master** o **Jugador**. Ese rol va en el JWT y define los permisos de la sesión.

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "player",
  "password": "Player123!",
  "sessionRole": "User"
}
```

`sessionRole`: `"Master"` (DM) o `"User"` (Jugador). La cuenta `admin` siempre recibe rol **Admin** (ignora `sessionRole`).

### Registro

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "julio",
  "password": "Test123!",
  "displayName": "Julio",
  "sessionRole": "User"
}
```

### Respuesta de login / registro

```json
{
  "accessToken": "...",
  "userId": "...",
  "username": "player",
  "role": "User",
  "isAdmin": false,
  "isMaster": false,
  "expiresAtUtc": "..."
}
```

### Consultar sesión actual

```http
GET /api/auth/me
Authorization: Bearer {token}
```

En Swagger: **Authorize** → `Bearer {token}`.

### Usuarios de demo (seed automático)

| Usuario | Contraseña | Cuenta |
|---------|------------|--------|
| `admin` | `Admin123!` | Administrador del sistema (único) |
| `player` | `Player123!` | Usuario normal (elige DM o Jugador al login) |

---

## Roles y permisos

| Rol | Cómo se obtiene | Permisos |
|-----|-----------------|----------|
| **Guest** | Sin token | Solo `/api/public/*` |
| **Master** (DM) | `sessionRole: "Master"` al login | Crear/editar/borrar **sus campañas**; gestionar **todos** los personajes de esas campañas (incl. NPCs) |
| **User** (Jugador) | `sessionRole: "User"` al login | Ver campañas públicas; crear/editar/borrar **solo sus personajes** en campañas **públicas y activas** |
| **Admin** | Cuenta `admin` | Panel de administración: usuarios, moderación de campañas (pública/activa) |

### Flujos típicos

**Dungeon Master**
1. Login → Dungeon Master.
2. Crear campaña (marcar **Pública** si quieres que se unan jugadores).
3. Añadir personajes y NPCs.

**Jugador**
1. Login → Jugador.
2. En **Mis campañas** → **Campañas públicas** (o pestaña Explorar público).
3. Elegir campaña → **Añadir personaje** → crear ficha.

**Administrador**
1. Login con `admin`.
2. Panel ⚙ Admin: usuarios (roles, desactivar) y estado de campañas (pública/activa).

---

## Subida de fichas (Cloudinary)

Los archivos **no** se guardan en disco del servidor. La API sube a Cloudinary y guarda la URL en base de datos.

| Tipo | Extensiones | Máx. |
|------|-------------|------|
| Imagen | `.jpg`, `.jpeg`, `.png`, `.webp` | 10 MB |
| PDF | `.pdf` | 10 MB |

```http
POST /api/characters/{characterId}/attachments
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: (archivo)
```

Ejemplo de prueba: `samples/ficha-ejemplo.pdf`  
Documentación detallada: [`samples/FORMATO-FICHAS.md`](samples/FORMATO-FICHAS.md)

Sin Cloudinary configurado, la subida devuelve error indicando que faltan credenciales.

---

## Endpoints de la API

### Autenticación

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/register` | No | Registrar usuario |
| POST | `/api/auth/login` | No | Obtener JWT |
| GET | `/api/auth/me` | Sí | Rol y datos de la sesión actual |

### Público (sin token)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/public/campaigns` | Campañas públicas activas |
| GET | `/api/public/campaigns/{id}/characters` | Personajes visibles de una campaña pública |
| GET | `/api/public/dnd/spells?name=` | Hechizo (dnd5eapi.co) |
| GET | `/api/public/dnd/monsters?name=` | Monstruo (dnd5eapi.co) |

### Campañas

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/campaigns` | Auth | DM: sus campañas · Jugador: campañas donde tiene personajes |
| GET | `/api/campaigns/explore` | Auth | Campañas públicas activas (para unirse) |
| GET | `/api/campaigns/{id}` | Auth | Detalle (según permisos) |
| POST | `/api/campaigns` | Master | Crear campaña |
| PUT | `/api/campaigns/{id}` | Master | Actualizar campaña propia |
| DELETE | `/api/campaigns/{id}` | Master | Borrar campaña propia |

### Personajes

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/campaigns/{id}/characters` | Auth | Listar personajes (DM: todos · Jugador: solo los suyos) |
| GET | `/api/campaigns/{id}/characters/{charId}` | Auth | Detalle |
| POST | `/api/campaigns/{id}/characters` | Auth | Crear (DM en su campaña · Jugador en pública activa) |
| PUT | `/api/campaigns/{id}/characters/{charId}` | Auth | Actualizar |
| DELETE | `/api/campaigns/{id}/characters/{charId}` | Auth | Borrar |

### Tiradas de dados

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/characters/{id}/rolls` | Auth | Historial |
| POST | `/api/characters/{id}/rolls` | Auth | Nueva tirada |

Ejemplo:

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

`d20Mode`: `0` Normal · `1` Ventaja · `2` Desventaja.

### Adjuntos / fichas

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/characters/{id}/attachments` | Auth | Listar fichas |
| POST | `/api/characters/{id}/attachments` | Auth | Subir (multipart, campo `file`) |
| DELETE | `/api/characters/{id}/attachments/{attachmentId}` | Auth | Borrar |
| GET | `/api/attachments/{id}` | Auth | Descargar / redirigir a Cloudinary |

### Administración (solo Admin)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/admin/campaigns` | Todas las campañas |
| GET | `/api/admin/users` | Todos los usuarios |
| DELETE | `/api/admin/users/{id}` | Desactivar usuario |
| PATCH | `/api/admin/users/{id}/role` | Cambiar rol (`User`, `Master`, `Admin`) |
| PATCH | `/api/admin/campaigns/{id}/status` | Cambiar `isPublic` / `isActive` |

---

## Estructura del proyecto

```
RecuBackend/
├── RecuBackend.Api/
│   ├── Auth/               # Roles JWT, resolución de sesión
│   ├── Cloudinary/         # Subida y borrado en Cloudinary
│   ├── Controllers/
│   ├── Data/               # DbContext, migrations, seeder
│   ├── Dtos/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   └── Program.cs
├── RecuFrontend/           # Web (nginx en Docker)
├── RecuBackend.Tests/
├── samples/                # PDF/HTML de ejemplo para fichas
├── postman/
├── docker-compose.yml
├── .env.example
└── RecuBackend.sln
```

## Tests

```bash
dotnet test RecuBackend.sln
```

## Colección Postman

`postman/RecuBackend.postman_collection.json`
