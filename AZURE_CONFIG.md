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
2. Reiniciá (o esperá el próximo arranque) y buscá estos mensajes de inicio:
   ```
   [SMTP Config] Host: smtp.gmail.com, Port: 587, EnableSsl: True
   [SMTP Config] ✅ Usuario SMTP configurado: tu-email@gmail.com
   ```
3. Luego ejecutá una acción que dispare un email (ver flujo abajo) y buscá:
   ```
   [EmailSender] Enviando email a cliente@example.com via smtp.gmail.com:587
   [EmailSender] ✅ Email enviado exitosamente a cliente@example.com
   ```

Si ves `⚠️ ADVERTENCIA: Usuario SMTP no configurado`, las variables `Smtp__*` no se cargaron. Verifica:
- Que las variables estén con el formato `Smtp__Host` (doble guion bajo), NO `SMTP_HOST`
- Que estén todas configuradas en Azure Portal en Application Settings
- Reiniciá la app: `az webapp restart -g <resource-group> -n <app-name>`

## Flujo de envío de emails (mapa completo)

El sistema envía emails automáticamente en los siguientes eventos. Si un email no llega, usá este mapa para saber exactamente **dónde mirar en el código y en los logs**.

| # | Evento | Quién dispara | Servicio / Método | Email enviado |
|---|--------|--------------|-------------------|---------------|
| 1 | **Pedido creado** | Operario/Admin crea un pedido | `OrderService.CreateOrderAsync` → `EnviarEmailTrackingAsync` | ✅ "Pedido Recibido" (con link de tracking) |
| 2 | **Operario asignado** (estado 1 → 2) | Encargado asigna operario | `OrderStatusService.AsignarOperarioAsync` | ✅ "Preparar pedido" |
| 3 | **Inicio de armado** (estado 2 → 2) | Operario presiona "Comenzar armado" | `OrderStatusService.CambiarEstadoAsync` | ❌ Sin email (intencional) |
| 4 | **Listo para despachar** (estado 2 → 4) | Operario finaliza preparación | `OrderStatusService.CambiarEstadoAsync` | ✅ "Listo para despachar" |
| 5 | **Cadete asignado / Despachando** (estado 4 → 5) | Encargado asigna cadete | `OrderStatusService.AsignarCadeteAsync` | ✅ "Despachando" |
| 6 | **En camino** (estado 5/6/8 → 6) | Cadete actualiza estado | `OrderStatusService.CambiarEstadoAsync` | ✅ "En camino" |
| 7 | **Entregado** (estado 5/6/8 → 7) | Cadete confirma entrega | `OrderStatusService.CambiarEstadoAsync` | ✅ "Entregado" |
| 8 | **Entrega fallida** (1er o 2do intento) (→ 8) | Cadete registra fallo | `OrderStatusService.CambiarEstadoAsync` | ✅ "Entrega fallida" |
| 9 | **Cancelado automáticamente** (3er intento fallido → 9) | Sistema automático | `OrderStatusService.CambiarEstadoAsync` | ✅ "Cancelado automáticamente" |
| 10 | **Cancelado manualmente** (→ 9/10) | Admin u Operario cancela | `OrderStatusService.CancelarPedidoAsync` | ✅ "Cancelado" |

### Condición para que se envíe el email
Antes de intentar enviar el email, el código verifica que el **campo `Mail` del cliente no esté vacío**. Si el cliente no tiene email registrado, el log mostrará:
```
[ADVERTENCIA] El pedido 123 no tiene email de cliente cargado, no se envió notificación.
```

### Archivos clave a revisar
| Archivo | Responsabilidad |
|---------|----------------|
| `Back/Program.cs` | Lee las variables `Smtp__*` al arrancar y las inyecta como `SmtpSettings` |
| `Back/Services/SmtpSettings.cs` | Modelo con las propiedades SMTP |
| `Back/Services/EmailSender.cs` | Construye y envía el email usando `SmtpClient` |
| `Back/Services/OrderService.cs` | Email de confirmación al crear un pedido |
| `Back/Services/OrderStatusService.cs` | Emails en cada cambio de estado |
| `Back/Services/EmailTemplateService.cs` | Genera el HTML del email |

## Variables de Entorno (Alternativa: CLI)

Si preferís usar CLI en lugar del portal:

```bash
az webapp config appsettings set \
  -g farmacia-app \
  -n farmaciaapi \
  --settings \
    DATABASE_CONNECTION_STRING="<your-connection-string>" \
    JWT_TOKEN="<64-char-secret>" \
    FRONTEND_URL="<frontend-url>" \
    Smtp__Host="smtp.gmail.com" \
    Smtp__User="<tu-email-real@gmail.com>" \
    Smtp__Password="<app-password-de-16-chars>" \
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

### Lista de verificación rápida (checklist)

Antes de profundizar, revisá estos puntos en orden:

1. **¿Las variables `Smtp__*` están configuradas en Azure?**
   - Azure Portal → App Service `farmaciaapi` → **Configuración → Configuración de la aplicación**
   - Verificá que existan: `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Password`, `Smtp__EnableSsl`
   - ⚠️ Usá doble guion bajo `__`, NO `_` ni `:`

2. **¿`Smtp__User` tiene un email real?**
   - El valor debe ser el email Gmail real que se usa para enviar (ej. `farmacia@gmail.com`)
   - Si el valor todavía dice algo como `tu-email@gmail.com` o es un placeholder, reemplazalo con el email real

3. **¿`Smtp__Password` es una App Password de Gmail (no la contraseña normal)?**
   - Gmail bloquea el SMTP con contraseña normal desde 2022
   - La App Password tiene **16 caracteres sin espacios** (ej. `abcdabcdabcdabcd`)
   - Generala en https://myaccount.google.com/apppasswords

4. **¿La app se reinició después de guardar las variables?**
   - Azure reinicia la app automáticamente al guardar en Configuración
   - Si no, usá: `az webapp restart -g farmacia-app -n farmaciaapi`

5. **¿El cliente tiene email registrado?**
   - Si el campo `Mail` del cliente está vacío en la base de datos, el email no se envía
   - El log mostrará: `[ADVERTENCIA] El pedido X no tiene email de cliente cargado`

6. **¿Los logs muestran un error de SMTP?**
   - Azure Portal → App Service → **Log stream**
   - Cambiá el estado de un pedido y buscá líneas `[EmailSender]`

### Los emails no se envían
- Revisá los logs en `App Service → Log stream`
- Buscá mensajes de error SMTP
- Verificá que `Smtp__User` y `Smtp__Password` sean correctos
- Confirmá que el email del cliente no está vacío en la base de datos
- Usá el formato `Smtp__*` (doble guion bajo), NO `SMTP_*`

### Error: "Usuario SMTP no configurado"
- La variable `Smtp__User` NO está en Azure Portal (o tiene un valor vacío)
- Agregala con el valor de tu email real
- Recargá la app: `az webapp restart -g farmacia-app -n farmaciaapi`

### Error: "Host SMTP no configurado" o emails silenciosos
- Las variables `Smtp__*` no se cargaron
- Verificá que usés `Smtp__Host` (NO `SMTP_HOST`)
- En Azure Portal → Configuración → Configuración de la aplicación
- Confirmá que estén TODAS las variables `Smtp__*`
- Recargá la app después de guardar

### Error: "535 5.7.8 Username and Password not accepted"
- La contraseña de Gmail es incorrecta o es la contraseña regular (no App Password)
- Creá una nueva App Password en https://myaccount.google.com/apppasswords
- NO uses tu contraseña regular de Gmail
- Copiá la contraseña de 16 caracteres y pegala en `Smtp__Password`
