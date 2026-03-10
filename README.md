# La Masia — Sistema de Reservas de Canchas · Fullstack

Aplicación web para la gestión de turnos de un complejo de canchas de fútbol en Rosario, Argentina. El sistema consta de una API construida con .NET 10 que gestiona la persistencia en SQLite y un frontend en HTML/Alpine.js para la reserva de canchas en tiempo real y un panel de administración completo.

---

## Tecnologías usadas

- **Lenguaje:** C# y JavaScript
- **Framework Backend:** ASP.NET Core Web API (.NET 10)
- **Arquitectura:** Clean Architecture (Domain / Application / Infrastructure / API)
- **ORM:** Entity Framework Core 9 — Code-First
- **Base de Datos:** SQLite con volumen persistente
- **Frontend:** HTML5 · Alpine.js 3 · CSS puro (sin frameworks, sin build step)
- **Infraestructura:** Docker multi-stage · Railway (CI/CD automático en cada push)
- **Documentación:** Swagger / OpenAPI

---

## Lo que aprendí y apliqué en este proyecto

- **Clean Architecture:** Organicé el proyecto en capas con dependencias que apuntan siempre hacia adentro — `Domain` sin dependencias externas, `Application` con la lógica de negocio, `Infrastructure` con EF Core y repositorios, y `API` como capa HTTP. Esto hace el código testeable e independiente del framework.

- **Autenticación segura:** Implementé el login del panel admin verificado en el servidor contra variables de entorno, evitando que las credenciales queden expuestas en el JavaScript del cliente.

- **Manejo de disponibilidad en tiempo real:** Desarrollé la lógica de slots horarios con bloqueo automático de horarios pasados según la hora local del dispositivo, y soporte para lista de espera cuando un turno está ocupado.

- **Relaciones en base de datos:** Configuré relaciones entre entidades (`Cancha`, `Turno`, `Usuario`, `ListaEspera`) usando navegación con `.Include()` para traer datos relacionados en una sola consulta.

- **Frontend reactivo sin framework pesado:** Integré Alpine.js sobre HTML estático para manejar estado, modales y actualizaciones de UI con menos de 15KB de JavaScript, sin necesidad de build step ni `node_modules`.

- **Docker multi-stage:** Aprendí a separar la imagen de build (~800MB con el SDK) de la imagen de runtime (~220MB), logrando un deploy 4x más liviano en Railway.

- **CI/CD real:** Configuré el deploy automático en Railway — cada `git push` a `main` buildea la imagen Docker y redeploya sin downtime en ~2 minutos.

---

## Endpoints principales

- `GET /api/canchas` — Lista las canchas activas del complejo
- `GET /api/reservas/disponibilidad/{id}?fecha=` — Slots disponibles para una cancha y fecha
- `POST /api/reservas` — Registrar una nueva reserva
- `DELETE /api/reservas/{id}` — Cancelar una reserva existente
- `POST /api/listaespera` — Anotarse en lista de espera para un turno ocupado
- `POST /api/admin/login` — Autenticación del panel de administración

Documentación interactiva disponible en `/swagger`.

---

*Desarrollado por [Matias Bueno](https://www.linkedin.com/in/matias-daniel-bueno/) · Rosario, Santa Fe, Argentina*
