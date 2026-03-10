La Masia — Sistema de Reservas Online

Sistema web full-stack para gestión de turnos de un complejo de canchas de fútbol en Rosario, Argentina.

¿Qué hace?
Reemplaza la gestión manual de turnos por WhatsApp con un sistema web completo. El cliente elige cancha, día y horario desde el celular en menos de 30 segundos — al confirmar, WhatsApp se abre automáticamente con el mensaje de confirmación pre-armado. El encargado tiene un panel de administración con visibilidad del día, bloqueo de horarios y exportación de reservas.

Stack
CapaTecnologíaBackendASP.NET Core 10 · C# · Clean ArchitectureORMEntity Framework Core 9 — Code-FirstBase de datosSQLite con volumen persistenteFrontendHTML5 · Alpine.js 3 · CSS puro (sin frameworks, sin build step)InfraestructuraDocker multi-stage · Railway (CI/CD automático)Documentación APISwagger / OpenAPI

Funcionalidades principales
Página de usuarios

Mapa de canchas con disponibilidad en tiempo real
Horarios pasados bloqueados automáticamente según la hora del dispositivo
Reserva con validación de WhatsApp y confirmación automática al cliente
Lista de espera para turnos ocupados
Responsive de 320px a 4K — 12 breakpoints

Panel de administración

Login autenticado en el servidor (credenciales en variables de entorno, nunca en el JS)
Reservas filtrables por fecha, cancha y estado · KPIs del día
Bloqueo de horario puntual o día completo
Cancelación con notificación automática al cliente por WhatsApp
Exportación CSV y resumen del día por WhatsApp
