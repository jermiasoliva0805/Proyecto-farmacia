# Configuración en Azure App Service

## Variables de Entorno Necesarias

Configure las siguientes variables de entorno en Azure App Service. Estos valores reemplazan automáticamente los valores por defecto en `appsettings.json`.

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
Configura las 4 variables SMTP para el envío de emails:

**SMTP_HOST**
```
smtp.gmail.com
```

**SMTP_USER**
Tu email de Gmail:
```
tu-email@gmail.com
```

**SMTP_PASSWORD**
```
tu-app-password
```
⚠️ **IMPORTANTE**: Para Gmail, NO uses tu contraseña regular. Usa una **contraseña de aplicación (App Password)**:
1. Ve a https://myaccount.google.com/apppasswords
2. Selecciona "Mail" y "Windows Computer"
3. Copia la contraseña de 16 caracteres que genera
4. Pega en SMTP_PASSWORD

**SMTP_PORT** (opcional)
```
587
```

**SMTP_ENABLE_SSL** (opcional)
```
true
```

## Pasos en Azure Portal

1. Ir a **App Service → Configuration → Application Settings**
2. Click en **New application setting** para cada variable
3. Ingresa el nombre y valor
4. Click **Save** 
5. La app se reiniciará automáticamente con las nuevas variables

## Verificación rápida

Para confirmar que todo está configurado:
1. Ve a **App Service → Logs**
2. Revisa los logs de deployment
3. Busca mensajes como:
   ```
   [SMTP Config] Host: smtp.gmail.com, Port: 587, User: tu-email@gmail.com, EnableSsl: True
   [EmailSender] Enviando email a cliente@example.com via smtp.gmail.com:587
   [EmailSender] ✅ Email enviado exitosamente
   ```

Si ves `⚠️ ADVERTENCIA: SMTP Host no configurado`, significa que las variables de entorno no se cargaron. Revisa que estén todas configuradas en Azure Portal.

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
    SMTP_HOST="smtp.gmail.com" \
    SMTP_USER="<tu-email@gmail.com>" \
    SMTP_PASSWORD="<app-password>" \
    SMTP_PORT="587" \
    SMTP_ENABLE_SSL="true"
```

## Cómo funciona la configuración

1. **En Desarrollo**: Se usa `appsettings.Development.json` con valores locales
2. **En Azure**: 
   - Se carga `appsettings.json` como base
   - Las variables de entorno reemplazan automáticamente los valores
   - El código en `Program.cs` prioriza: variables de entorno > appsettings.json > valores por defecto

## Resolución de problemas

### Los emails no se envían
- Revisa los logs en `App Service → Log stream`
- Busca mensajes de error SMTP
- Verifica que SMTP_USER y SMTP_PASSWORD sean correctos
- Confirma que el email del usuario (Cliente) no está vacío

### Error: "SMTP Host no configurado"
- Las variables de entorno no se cargaron en Azure
- Recarga la app: `az webapp restart -g <resource-group> -n <app-name>`
- O redeploy desde GitHub

### Error: "535 5.7.8 Username and Password not accepted"
- La contraseña de Gmail es incorrecta
- Crea una nueva app password en https://myaccount.google.com/apppasswords
