# ⚽ La Masia — Sistema de Reservas

Sistema completo de reservas online para el complejo de fútbol La Masia, Rosario.

---

## 🔗 Links (completar después del deploy)

| | URL |
|---|---|
| 👥 **Página de usuarios** | `https://TU-APP.up.railway.app` |
| ⚙️ **Panel admin** | `https://TU-APP.up.railway.app/admin.html` |
| 📡 **API (Swagger)** | `https://TU-APP.up.railway.app/swagger` |

---

## 🔐 Credenciales Admin

```
Usuario:    admin
Contraseña: lamasia2024
```

> ⚠️ Cambialas en admin.html antes de subir (buscar ADMIN_USER y ADMIN_PASS)

---

## 🚀 Deploy en Railway — paso a paso

### Paso 1 — Subir a GitHub

```bash
git init
git add .
git commit -m "La Masia - primer deploy"
git remote add origin https://github.com/TU-USUARIO/lamasia.git
git push -u origin main
```

### Paso 2 — Crear proyecto en Railway

1. railway.app → New Project → Deploy from GitHub repo
2. Seleccionar el repo → Railway detecta el Dockerfile automáticamente ✅

### Paso 3 — Volumen para la base de datos

1. Railway → Add Service → Volume
2. Mount path: /app/data
3. Las reservas quedan guardadas aunque Railway reinicie

### Paso 4 — Dominio público

1. Railway → tu servicio → Settings → Networking → Generate Domain
2. URL tipo: lamasia.up.railway.app

### Paso 5 — Verificar

```
✅ https://lamasia.up.railway.app             → usuarios
✅ https://lamasia.up.railway.app/admin.html  → admin
✅ https://lamasia.up.railway.app/swagger     → API docs
```

---

## 💻 Desarrollo local

```bash
dotnet run --project API
# Backend en http://localhost:5000
# Abrir frontend/index.html y frontend/admin.html en el navegador
```

---

## 📁 Estructura

```
FutbolComplejo/
├── Domain/Entities/          Cancha, Usuario, Turno, ListaEspera
├── Application/Services/     Lógica de negocio
├── Infrastructure/           DbContext, Repositorios
├── API/Controllers/          Endpoints REST
├── frontend/
│   ├── index.html            Página de usuarios
│   └── admin.html            Panel admin
├── Dockerfile
├── railway.json
└── FutbolComplejo.csproj
```

---

## 🏟️ Canchas

| Cancha | Tipo | Precio/hora |
|--------|------|-------------|
| Cancha 1 | Fútbol 7 | $45.000 |
| Cancha 2 | Fútbol 5 | $35.000 |
| Cancha 3 | Fútbol 5 | $30.000 |

Horarios: Lun-Vie 16:00→01:00 · Sáb-Dom 16:00→02:00

---

## 📡 API Principal

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | /api/canchas | Lista canchas |
| GET | /api/reservas/disponibilidad/{id}?fecha= | Slots disponibles |
| POST | /api/reservas | Crear reserva |
| DELETE | /api/reservas/{id} | Cancelar reserva |
| POST | /api/listaespera | Lista de espera |

---

*Desarrollado por Matias Bueno — linkedin.com/in/matias-daniel-bueno*
