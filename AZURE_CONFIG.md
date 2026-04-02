# Configuración en Azure App Service

## Variables de Entorno Necesarias

Configure las siguientes variables de entorno en Azure App Service:

### 1. Base de Datos
```
DATABASE_CONNECTION_STRING
```
Ejemplo para Azure SQL Database:
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=FarmaciaDB;Persist Security Info=False;User ID=your-user;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 2. JWT Token
```
JWT_TOKEN
```
Generar una clave segura de al menos 64 caracteres.

### 3. Frontend URL
```
FRONTEND_URL
```
Ejemplo: `https://tu-dominio-frontend.com`

### 4. SMTP (Gmail)
```
SMTP_HOST=smtp.gmail.com
SMTP_USER=tu-email@gmail.com
SMTP_PASSWORD=tu-app-password
```

**Nota para Gmail:** Usa contraseña de aplicación (App Password), no tu contraseña de Gmail.

## Pasos en Azure Portal

1. Ir a **App Service → Configuration → Application Settings**
2. Click en **New application setting** para cada variable
3. Ingresa el nombre y valor
4. Click **Save** y **Continue** cuando se pida reiniciar

## Variables de Entorno (Alternativa: CLI)

```bash
az webapp config appsettings set -g <resource-group> -n <app-name> --settings \
  DATABASE_CONNECTION_STRING="<value>" \
  JWT_TOKEN="<value>" \
  FRONTEND_URL="<value>" \
  SMTP_HOST="smtp.gmail.com" \
  SMTP_USER="<value>" \
  SMTP_PASSWORD="<value>"
```

## Verificación

- El archivo `appsettings.Production.json` ya está configurado para leer estas variables
- El `Dockerfile` apunta a .NET 9.0
- `node_modules` está excluido del repositorio
