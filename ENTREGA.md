# Documentación de entrega – RecuBackend

## Nombre del proyecto
**RecuBackend – DnD Campaign Companion API**

## Propósito
API REST para partidas de rol (Dungeons & Dragons): gestionar campañas, personajes/NPCs, tiradas de dados con un clic y fichas de personaje (imagen/PDF), con autenticación por usuario.

## Funcionalidades implementadas

### Obligatorias
- API REST con CRUD, códigos HTTP y recursos públicos (`/api/public/*`) y privados filtrados por usuario.
- Modelo EF Core: Campaign, Character, RollLog, FileAttachment, AppUser (relaciones 1-N).
- JWT + roles Admin/User; registro y login.
- Integración API externa [dnd5eapi.co](https://www.dnd5eapi.co): hechizos y monstruos.
- Subida/descarga de fichas en volumen Docker.
- Docker Compose: API + PostgreSQL + Web (sin BBDD adicional en el front).

### Extras
- Gitflow (ramas `feat/*` → `develop`).
- Colección Postman en `postman/RecuBackend.postman_collection.json`.
- Filtros y ordenación (≥2 campos) en campañas y personajes, zona pública y privada.
- Pruebas unitarias (`RecuBackend.Tests`): dados y validación de archivos.
- Front web contenerizado (`RecuFrontend`).

## Decisiones técnicas
| Área | Decisión |
|------|----------|
| Framework | .NET 8 Web API |
| BBDD | PostgreSQL 16 + EF Core 8 |
| Auth | JWT Bearer, BCrypt para contraseñas |
| Archivos | Almacenamiento local en volumen (`/app/uploads`) |
| API externa | HttpClient tipado hacia dnd5eapi.co |
| Front | HTML/JS estático en nginx; CORS abierto en desarrollo |
| Contenedores | Puertos API/DB según usuario San Valero vía `.env` |

## Gitflow
- `main`: releases
- `develop`: integración
- `feat/*`: docker, models-crud, auth-jwt, files, external-api, search-sort, unit-tests, web

## Ejecución
```bash
cp .env.example .env
# Editar API_PORT, DB_PORT, WEB_PORT
docker compose --env-file .env up --build -d
```
- API/Swagger: `http://localhost:<API_PORT>/swagger`
- Web: `http://localhost:<WEB_PORT>`
- Tests: `dotnet test`

## Uso crítico de IA
La IA se usó para acelerar bootstrap (Docker, EF, JWT) y revisión de código. Las decisiones de dominio (DnD, tiradas, NPCs), estructura de ramas, pruebas prioritarias y documentación fueron validadas y ajustadas manualmente.

## Demo rápida (defensa)
1. Registro o login (`player` / `Player123!`).
2. Crear campaña pública.
3. Añadir NPC y tirar `1d20+5`.
4. Buscar hechizo `fireball` en API externa.
5. Subir PDF/imagen de ficha.
6. Mostrar filtros en listado de campañas.
