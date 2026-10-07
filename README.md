# TaskManager

Prueba técnica: gestor de tareas con backend en .NET y frontend en Angular.

Cada usuario se registra, inicia sesión y maneja solo sus tareas (crear, listar, editar, eliminar, asignar categoría y buscar por título).

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core + SQL Server 2022 (Docker)
- JWT + refresh token
- Angular

## Estructura

```
Backend/TaskManager
  TaskManager.Domain          entidades (User, TaskItem, Category, RefreshToken)
  TaskManager.Application
  TaskManager.Infrastructure  AppDbContext, configuraciones y migraciones
  TaskManager.Api             controllers, Program.cs, docker-compose.yml

Frontend/Taskweb
  src/app
    auth.ts           servicio de auth, interceptor y guard
    task.service.ts   llamadas a la API
    login.ts          login / registro
    tasks.ts/.html    CRUD de tareas
    app.routes.ts     rutas
```

## Cómo levantarlo

**1. Base de datos**

```
cd Backend/TaskManager/TaskManager.Api
docker compose up -d
```

**2. API**

Revisar la cadena de conexión y `Jwt:Key` en `appsettings.json` (la clave debe tener mínimo 32 caracteres).

Aplicar migraciones desde la Package Manager Console (proyecto por defecto: `TaskManager.Infrastructure`):

```
Update-Database
```

Ejecutar con F5. Swagger: https://localhost:7201/swagger

**3. Frontend**

```
cd Frontend/Taskweb
npm install
ng serve
```

Abrir http://localhost:4200

## Endpoints

| Método | Ruta | |
|---|---|---|
| POST | /api/auth/register | registro |
| POST | /api/auth/login | login |
| POST | /api/auth/refresh | renovar token |
| GET | /api/categories | categorías |
| GET | /api/tasks?search= | listar / buscar |
| GET | /api/tasks/{id} | detalle |
| POST | /api/tasks | crear |
| PUT | /api/tasks/{id} | editar |
| DELETE | /api/tasks/{id} | eliminar |

Las rutas de tareas y categorías requieren `Authorization: Bearer <token>`.

Las categorías se cargan con la migración (Trabajo, Personal, Estudios).

## Algunas notas

- Las contraseñas se guardan con BCrypt.
- El access token dura 15 minutos y el refresh token 7 días. En la BD guardo el hash del refresh token, no el valor. Cada vez que se usa, se revoca y se genera uno nuevo.
- El id del usuario se toma del token (claim `sub`), así cada uno solo ve sus tareas. Si alguien pide una tarea que no es suya, la API responde 404.
- Las reglas de la tarea (título de 3 a 100 caracteres, categoría obligatoria) están en la entidad `TaskItem`.
- En Angular el interceptor agrega el token a cada request y, si recibe un 401, intenta el refresh y repite la petición.
- El buscador usa `debounceTime` + `switchMap` para no llamar a la API en cada tecla.
- Las rutas usan lazy loading y `/tasks` está protegida con un guard.

## Pendientes

Por el tiempo de la prueba dejé la lógica en los controllers. Lo siguiente sería:

- Pasar la lógica a servicios en la capa Application.
- Manejo global de errores.
- Tests.
- Refresh token en cookie httpOnly en lugar de localStorage.
- Dockerizar API y frontend.

> Los valores de `appsettings.json` son de ejemplo, no usar en producción.
