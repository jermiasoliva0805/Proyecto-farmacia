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

En Azure App Service, usa el formato `Smtp__` (doble guion bajo) para variables de entorno jerárquicas:

| Variable | Valor |
|----------|-------|
| `Smtp__Host` | `smtp.gmail.com` |
| `Smtp__Port` | `587` |
| `Smtp__User` | `tu-email@gmail.com` |
| `Smtp__Password` | `tu-app-password` |
| `Smtp__EnableSsl` | `true` |

⚠️ **IMPORTANTE**: 
- **Las 5 variables SMTP son obligatorias (Host, Port, User, Password, EnableSsl)**. Si falta cualquiera (especialmente `Smtp__Password`), los emails **no se enviarán**.
- Usa `Smtp__` (doble guion bajo), NO `SMTP_` (guion bajo simple)
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

## Verificación rápida

Para confirmar que todo está configurado correctamente:
1. Ve a **App Service → Log stream**
2. Crea un pedido o realiza cualquier acción que envíe email
3. Busca mensajes como:
   ```
   [SMTP Config] Host=smtp.gmail.com, Port=587, EnableSsl=True
   [SMTP Config] User=✅ configurado, Password=✅ configurado
   [EmailSender] Enviando email a cliente@example.com via smtp.gmail.com:587
   [EmailSender] ✅ Email enviado exitosamente
   ```

Si ves `❌ NO configurado` junto a `User` o `Password`, significa que esa variable `Smtp__*` no se cargó. Verifica:
- Que las variables estén con el formato `Smtp__Host` (doble guion bajo), NO `SMTP_HOST`
- Que estén **todas** configuradas en Azure Portal en Application Settings (Host, Port, User, Password, EnableSsl)
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
   - El código en `Program.cs` prioriza: variables de entorno > appsettings.json > valores por defecto

## Resolución de problemas

### Los emails no se envían
- Revisa los logs en `App Service → Log stream` al arrancar la app
- Busca la línea `[SMTP Config] User=... Password=...` — cualquier campo con `❌ NO configurado` es la causa
- Verifica que **las 5 variables** estén definidas: `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Password`, `Smtp__EnableSsl`
- Confirma que el email del cliente (campo `Mail` en la base de datos) no esté vacío
- Usa el formato `Smtp__*` (doble guion bajo), NO `SMTP_*`

### Log: "❌ NO configurado (Smtp__User)"
- La variable `Smtp__User` NO está en Azure Portal o tiene valor vacío
- Agrégala con el valor de tu email de Gmail
- Recarga la app: `az webapp restart -g <resource-group> -n <app-name>`

### Log: "❌ NO configurado (Smtp__Password)"
- La variable `Smtp__Password` NO está en Azure Portal ← **causa más frecuente**
- Para Gmail, crea una App Password en https://myaccount.google.com/apppasswords
- Agrégala en Azure Portal como `Smtp__Password` con la contraseña de 16 caracteres
- Recarga la app: `az webapp restart -g <resource-group> -n <app-name>`

### Log: "❌ NO configurado" en Host
- Las variables `Smtp__*` no se cargaron
- Verifica que uses `Smtp__Host` (NO `SMTP_HOST`)
- En Azure Portal → Configuration → Application Settings
- Confirma que estén TODAS las variables Smtp__*
- Recarga la app después de guardar

### Error SMTP: "535 5.7.8 Username and Password not accepted"
- La contraseña de Gmail es incorrecta
- Crea una nueva App Password en https://myaccount.google.com/apppasswords
- NO uses tu contraseña regular de Gmail
- Copia la contraseña de 16 caracteres y pégala en `Smtp__Password`
