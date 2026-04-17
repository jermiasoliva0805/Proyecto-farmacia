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
2. Reiniciá (o esperá el próximo arranque) y buscá estos mensajes de inicio:
   ```
   [DB Config] ✅ ConnectionString configurada correctamente.
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
   - Para variables jerárquicas (ej. `Smtp:Host`) se usa doble guion bajo (`Smtp__Host`) en Azure
   - Para `DATABASE_CONNECTION_STRING` y `JWT_TOKEN`, el código hace la resolución manualmente

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


### ContainerTimeout / La app no arranca

El error `ContainerTimeout` aparece cuando Azure no puede verificar que la app esté respondiendo:

1. **Verificar que `DATABASE_CONNECTION_STRING` esté configurada** — Si no está, la app intenta conectar a SQL Server local (falla), y el arranque se demora hasta 30 segundos.
2. **Verificar que WEBSITES_PORT o ASPNETCORE_URLS estén configurados** — Si no, la app escucha en el puerto 8080 por defecto (el código lo hace automáticamente).
3. **Endpoint de health check**: La app expone `/health` que devuelve `200 OK` y sirve como verificación de que está corriendo.
4. **Revisar logs**: Habilitar Application Logging (ver sección arriba) y verificar el Log Stream.

### Los emails no se envían
- Revisá los logs en `App Service → Log stream` (debe estar habilitado Application Logging)
- Buscá mensajes de error SMTP
- Verificá que `Smtp__User` y `Smtp__Password` sean correctos
- Confirmá que el email del cliente (campo `Mail` en la tabla de clientes) no está vacío
- Usá el formato `Smtp__*` (doble guion bajo), NO `SMTP_*`

### No se ven logs de la app en Log Stream

Solo aparecen logs de Docker/Kudu por defecto. Para ver logs de la aplicación:
1. **App Service → App Service Logs → Application Logging (File System) → On**
2. Click Save y esperar el reinicio
3. Volver a **Log stream** — ahora verás `[SMTP Config]`, `[EmailSender]`, etc.

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

### Error: DB / Migraciones
- Si ves `[DB Config] ⚠️ ADVERTENCIA: ConnectionString no configurada`, agrega `DATABASE_CONNECTION_STRING` en Application Settings de Azure
- Verifica que la cadena de conexión apunte a Azure SQL, no a SQL Server local
