# UAM Lab Help Desk

## API

Proyecto ASP.NET Core .NET 10 para autenticación, gestión de averías, notificaciones por correo y dashboard administrativo.

### Configuración local

1. Ajustar `Api/ConnectionStrings:DefaultConnection` para SQL Server.
2. Configurar SMTP mediante User Secrets desde la raíz del repositorio:

```powershell
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com" --project Api
dotnet user-secrets set "Smtp:Port" "587" --project Api
dotnet user-secrets set "Smtp:SenderEmail" "CORREO_REMITENTE" --project Api
dotnet user-secrets set "Smtp:SenderName" "UAM Lab Help Desk" --project Api
dotnet user-secrets set "Smtp:Password" "CONTRASENA_DE_APLICACION" --project Api
```

No guardar contraseñas SMTP en `appsettings.json`. Para Gmail debe utilizarse una contraseña de aplicación.

3. Aplicar migraciones y ejecutar:

```powershell
dotnet ef database update --project Api
dotnet run --project Api
```

## Requerimiento 8 — Notificaciones por correo

Se reutiliza la sección `Smtp` del flujo OTP. No existen endpoints nuevos.

Pruebas recomendadas:

1. Crear usuarios activos con rol `Technician` y correos accesibles.
2. Crear un reporte como `Instructor`: todos los técnicos activos reciben correo.
3. Ejecutar `POST /api/FaultReports/AssignFaultReport/{id}` con JWT del técnico: el técnico recibe correo.
4. Ejecutar `PUT /api/FaultReports/UpdateFaultReportStatus/{id}` con estado `Resolved`: el instructor creador recibe correo.
5. Ejecutar `POST /api/FaultReports/CloseFaultReport/{id}`: el instructor recibe correo de cierre.
6. Probar con una configuración SMTP inválida: la operación principal debe conservarse y el error debe aparecer en los logs.

Cada correo incluye reporte, equipo, laboratorio, estado y fecha UTC del evento. Los técnicos inactivos no reciben notificaciones.

## Requerimiento 9 — Dashboard

Todos los endpoints requieren JWT válido y usuario activo con rol `Admin`:

```text
GET /api/Dashboard/GetGeneralSummary
GET /api/Dashboard/GetReportsByLab
GET /api/Dashboard/GetReportsByStatus
GET /api/Dashboard/GetReportsByTechnician
GET /api/Dashboard/GetAverageResolutionTime
```

Flujo de prueba en Swagger:

1. Ejecutar `POST /api/Auth/Login` y `POST /api/Auth/VerifyOtp` con un usuario `Admin`.
2. Autorizar Swagger con el `AccessToken`.
3. Ejecutar los cinco endpoints y contrastar conteos con `FaultReports`, `Equipment`, `Users` y `FaultReportStatusLogs`.
4. Repetir con un usuario `Instructor` o `Technician`; debe responder HTTP 403 y `Success = false`.

El tiempo de resolución usa únicamente el primer log `Resolved` de cada reporte y calcula la diferencia desde `ReportedAtUtc`. Los reportes aún no resueltos quedan excluidos.
