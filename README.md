# Jellyfin Plugin - AniList + TMDb Fusion

Plugin skeleton that fetches TMDb metadata (in Spanish) and replaces the title with AniList's romaji title.

## Instrucciones

1. Rellenar `manifest.json` si quieres cambiar `guid` o `owner`.
2. Compilar contra los assemblies de Jellyfin Server (añadir referencias a los DLLs del servidor).
3. Instalar el plugin en `/var/lib/jellyfin/plugins/` (o la carpeta de plugins de tu instalación).
4. Reiniciar Jellyfin, abrir la página del plugin en el panel de administración y añadir tu API Key de TMDb.

Este repositorio es un esqueleto — puede necesitar ajustes según la versión de Jellyfin y sus SDKs.
