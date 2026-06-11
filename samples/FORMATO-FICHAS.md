# Formato de fichas y adjuntos

Archivos de ejemplo incluidos en esta carpeta:

| Archivo | Uso |
|---------|-----|
| `ficha-ejemplo.pdf` | PDF listo para subir sin conversión |
| `ficha-ejemplo.html` | Abrir en navegador → Imprimir → Guardar como PDF |

## Formatos aceptados

| Tipo | Extensiones | Content-Type |
|------|-------------|--------------|
| Imagen | `.jpg`, `.jpeg`, `.png`, `.webp` | `image/jpeg`, `image/png`, `image/webp` |
| Documento | `.pdf` | `application/pdf` |

**Tamaño máximo:** 10 MB por archivo.

## Cómo subir una ficha

1. Inicia sesión (cualquier usuario autenticado con acceso al personaje).
2. Abre una campaña y selecciona un personaje.
3. Pestaña **Fichas** → elige archivo → **Subir**.
4. La petición es `multipart/form-data` con el campo **`file`**.

### Ejemplo con curl

```bash
curl -X POST "http://localhost:4569/api/characters/{characterId}/attachments" \
  -H "Authorization: Bearer {token}" \
  -F "file=@samples/ficha-ejemplo.pdf"
```

### Respuesta

```json
{
  "id": "...",
  "characterId": "...",
  "fileName": "ficha-ejemplo.pdf",
  "contentType": "application/pdf",
  "sizeBytes": 916,
  "isImage": false,
  "secureUrl": "https://res.cloudinary.com/.../ficha-ejemplo.pdf",
  "uploadedAtUtc": "..."
}
```

`secureUrl` es el enlace público en Cloudinary para ver o descargar el archivo.

## Almacenamiento (Cloudinary)

Los archivos **no** se guardan en disco del servidor. El flujo es:

1. La API valida MIME, extensión y tamaño.
2. Sube a Cloudinary (`ImageUploadParams` para imágenes, `RawUploadParams` para PDF).
3. Guarda en base de datos: `SecureUrl`, `PublicId` y `ResourceType`.

### Variables de entorno

En `.env` (o `appsettings.json`):

```env
CLOUDINARY_CLOUD_NAME=tu_cloud_name
CLOUDINARY_API_KEY=tu_api_key
CLOUDINARY_API_SECRET=tu_api_secret
```

Obtén las credenciales en [cloudinary.com/console](https://cloudinary.com/console).

Sin estas variables, la subida fallará con un mensaje indicando que Cloudinary no está configurado.

## Contenido recomendado de una ficha

No hay un esquema JSON obligatorio: puede ser una imagen escaneada, un PDF exportado o una foto de la ficha en papel. Para pruebas, basta con un PDF de una página con el nombre del personaje y datos básicos (como los ejemplos de esta carpeta).
