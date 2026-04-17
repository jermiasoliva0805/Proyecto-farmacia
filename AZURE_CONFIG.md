# Configuración en Azure App Service

## Variables de Entorno Necesarias

Configure las siguientes variables de entorno en Azure App Service. Estos valores reemplazan automáticamente los valores por defecto en `appsettings.json`.

### 1. Base de Datos

El código acepta **dos formas** de configurar la cadena de conexión (se usa la primera que encuentre):

**Opción A (recomendada): Application setting**
```
DATABASE_CONNECTION_STRING
```

**Opción B: Connection string de Azure (sección "Connection strings" del portal)**
```
Nombre: DefaultConnection  (tipo: SQLAzure o Custom)
```

Ejemplo para Azure SQL Database:
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=FarmaciaDB;Persist Security Info=False;User ID=your-user;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 2. JWT Token

El código acepta **dos formas** (se usa la primera que encuentre):

**Opción A (recomendada): Application setting**
```
JWT_TOKEN
```

**Opción B: Application setting jerárquico**
```
AppSettings__Token
```

Generar una clave segura de al menos 64 caracteres. Ejemplo:
```
your-super-secret-key-minimum-64-characters-long-for-jwt-authentication
```

### 3. Frontend URL

```
FRONTEND_URL
```
Ejemplo: 
```
https://tu-dominio-frontend.com
```

### 4. SMTP (Configuración de Emails)

En Azure App Service, usa el formato `Smtp__` (doble guion bajo) para variables de entorno jerárquicas:

| Variable | Valor |
|----------|-------|
| `Smtp__Host` | `smtp.gmail.com` |
| `Smtp__Port` | `587` |
| `Smtp__User` | `tu-email@gmail.com` |
| `Smtp__Password` | `tu-app-password` |
| `Smtp__EnableSsl` | `true` |

⚠️ **IMPORTANTE**: 
- Usa `Smtp__` (doble guion bajo), NO `SMTP_HOST` ni `SMTP_USER` (formato incorrecto)
- Para Gmail, NO uses tu contraseña regular. Usa una **contraseña de aplicación (App Password)**:
  1. Ve a https://myaccount.google.com/apppasswords
  2. Selecciona "Mail" y "Windows Computer"
  3. Copia la contraseña de 16 caracteres que genera
  4. Pega en `Smtp__Password`

## Pasos en Azure Portal

1. Ir a **App Service → Configuration → Application Settings**
2. Click en **New application setting** para cada variable
3. Ingresa el nombre y valor
4. Click **Save** 
5. La app se reiniciará automáticamente con las nuevas variables

## Habilitar Application Logging (ver logs en tiempo real)

Para ver los logs de la app (incluido `[SMTP Config]`, `[EmailSender]`) en **Log Stream**:

1. Ir a **App Service → App Service Logs**
2. En **Application Logging (File System)** → seleccionar **On** (nivel: Verbose o Information)
3. Click **Save**
4. Ir a **App Service → Log stream** para ver los logs en tiempo real

> Sin este paso, el Log Stream solo muestra logs de Docker/Kudu pero no el stdout de la aplicación.

## Verificación rápida

Para confirmar que todo está configurado correctamente:
1. Ve a **App Service → Log stream**
2. Crea un pedido o realiza cualquier acción que envíe email
3. Busca mensajes como:
   ```
   [DB Config] ✅ ConnectionString configurada correctamente.
   [SMTP Config] Host: smtp.gmail.com, Port: 587, EnableSsl: True
   [EmailSender] Enviando email a cliente@example.com via smtp.gmail.com:587
   [EmailSender] ✅ Email enviado exitosamente a cliente@example.com
   ```

Si ves `⚠️ ADVERTENCIA: Usuario SMTP no configurado`, significa que las variables `Smtp__*` no se cargaron. Verifica:
- Que las variables estén con el formato `Smtp__Host` (doble guion bajo), NO `SMTP_HOST`
- Que estén todas configuradas en Azure Portal en Application Settings
- Reinicia la app: `az webapp restart -g <resource-group> -n <app-name>`

## Variables de Entorno (Alternativa: CLI)

Si prefieres usar CLI en lugar del portal:

```bash
az webapp config appsettings set \
  -g <resource-group> \
  -n <app-name> \
  --settings \
    DATABASE_CONNECTION_STRING="<your-connection-string>" \
    JWT_TOKEN="<64-char-secret>" \
    FRONTEND_URL="<frontend-url>" \
    Smtp__Host="smtp.gmail.com" \
    Smtp__User="<tu-email@gmail.com>" \
    Smtp__Password="<app-password>" \
    Smtp__Port="587" \
    Smtp__EnableSsl="true"
```

## Cómo funciona la configuración

1. **En Desarrollo**: Se usa `appsettings.Development.json` con valores locales
2. **En Azure**: 
   - Se carga `appsettings.json` como base
   - Las variables de entorno reemplazan automáticamente los valores
   - Para variables jerárquicas (ej. `Smtp:Host`) se usa doble guion bajo (`Smtp__Host`) en Azure
   - Para `DATABASE_CONNECTION_STRING` y `JWT_TOKEN`, el código hace la resolución manualmente

## Resolución de problemas

### ContainerTimeout / La app no arranca

El error `ContainerTimeout` aparece cuando Azure no puede verificar que la app esté respondiendo:

1. **Verificar que `DATABASE_CONNECTION_STRING` esté configurada** — Si no está, la app intenta conectar a SQL Server local (falla), y el arranque se demora hasta 30 segundos.
2. **Verificar que WEBSITES_PORT o ASPNETCORE_URLS estén configurados** — Si no, la app escucha en el puerto 8080 por defecto (el código lo hace automáticamente).
3. **Endpoint de health check**: La app expone `/health` que devuelve `200 OK` y sirve como verificación de que está corriendo.
4. **Revisar logs**: Habilitar Application Logging (ver sección arriba) y verificar el Log Stream.

### Los emails no se envían

- Revisa los logs en `App Service → Log stream` (debe estar habilitado Application Logging)
- Busca mensajes de error SMTP
- Verifica que `Smtp__User` y `Smtp__Password` sean correctos
- Confirma que el email del cliente (campo `Mail` en la tabla de clientes) no está vacío
- Usa el formato `Smtp__*` (doble guion bajo), NO `SMTP_*`

### No se ven logs de la app en Log Stream

Solo aparecen logs de Docker/Kudu por defecto. Para ver logs de la aplicación:
1. **App Service → App Service Logs → Application Logging (File System) → On**
2. Click Save y esperar el reinicio
3. Volver a **Log stream** — ahora verás `[SMTP Config]`, `[EmailSender]`, etc.

### Error: "Usuario SMTP no configurado"

- La variable `Smtp__User` NO está en Azure Portal
- Agrégala con el valor de tu email
- Recarga la app: `az webapp restart -g <resource-group> -n <app-name>`

### Error: "Host SMTP no configurado" o emails silenciosos

- Las variables `Smtp__*` no se cargaron
- Verifica que uses `Smtp__Host` (NO `SMTP_HOST`)
- En Azure Portal → Configuration → Application Settings
- Confirma que estén TODAS las variables Smtp__*
- Recarga la app después de guardar

### Error: "535 5.7.8 Username and Password not accepted"

- La contraseña de Gmail es incorrecta
- Crea una nueva App Password en https://myaccount.google.com/apppasswords
- NO uses tu contraseña regular de Gmail
- Copia la contraseña de 16 caracteres y pégala en `Smtp__Password`

### Error: DB / Migraciones

- Si ves `[DB Config] ⚠️ ADVERTENCIA: ConnectionString no configurada`, agrega `DATABASE_CONNECTION_STRING` en Application Settings de Azure
- Verifica que la cadena de conexión apunte a Azure SQL, no a SQL Server local
