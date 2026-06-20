# Stack Tecnológico — Plataforma de Subastas

## Resumen del stack

| Capa | Tecnología |
|---|---|
| Frontend | React (con TypeScript) |
| Backend | ASP.NET Core 8 (REST API) |
| ORM | Entity Framework Core 8 |
| Base de datos | SQL Server |
| Tiempo real | SignalR |
| Autenticación | JWT (JSON Web Tokens) |

---

## Backend — ASP.NET Core 8

- Usar **ASP.NET Core 8** como framework principal del backend.
- Organizar el proyecto en capas: `Controllers` → `Services` → `Repositories`.
- Los controladores solo deben manejar HTTP (validación de entrada, mapeo de respuesta). La lógica de negocio va en los servicios.
- Usar **minimal APIs** solo para endpoints muy simples; preferir controladores con `[ApiController]` para el resto.
- Todas las respuestas deben seguir la estructura: `{ "data": <payload|null>, "error": <mensaje|null>, "meta": <paginación|null> }`.
- Los endpoints se exponen bajo el prefijo `/api/v1/`.
- Usar `ILogger<T>` para logging; no usar `Console.WriteLine`.
- Manejar excepciones con un middleware global de manejo de errores.

## ORM — Entity Framework Core 8

- Usar **EF Core 8** con **SQL Server** como proveedor.
- Configurar las entidades mediante **Fluent API** en clases `IEntityTypeConfiguration<T>`, no con Data Annotations.
- Usar migraciones de EF Core para gestionar el esquema de la base de datos.
- No usar lazy loading; preferir carga explícita (`Include`) o proyecciones con `Select`.
- Las consultas que devuelven listas deben ser paginadas con `Skip`/`Take`.

## Base de datos — SQL Server

- Nombres de tablas en español (según el modelo del dominio): `Usuarios`, `Roles`, `Subastas`, `Pujas`, `Monederos`, `MovimientosMonedero`, `Imágenes`, `Notificaciones`.
- Usar tipos de datos apropiados: `decimal(18,2)` para montos, `datetime2` para fechas UTC, `nvarchar` para texto.
- Toda fecha almacenada debe estar en **UTC**.

## Tiempo real — SignalR

- Usar **ASP.NET Core SignalR** para la comunicación en tiempo real.
- Los clientes se unen a grupos por subasta (`AuctionGroup_{id}`).
- Requerir token JWT válido para unirse a un grupo.
- Eventos principales del hub: `NuevaPuja`, `TickContador`, `PrecioActualizado`, `EstadoSubasta`.
- La latencia máxima aceptable para eventos de puja es de 3 segundos.

## Autenticación — JWT

- Usar tokens **JWT** firmados con algoritmo HS256 o RS256.
- El token debe incluir: `id` del usuario y su `rol`.
- Expiración del token: 24 horas.
- Para invalidar tokens al bloquear un usuario, mantener una lista negra en caché (Redis o memoria distribuida).
- Usar el middleware estándar de ASP.NET Core: `AddAuthentication().AddJwtBearer(...)`.

## Frontend — React

- Usar **React** con **TypeScript**.
- Gestionar el estado con hooks estándar (`useState`, `useContext`, `useReducer`); no introducir Redux salvo que sea necesario.
- Conectar al hub de SignalR usando el paquete `@microsoft/signalr`.
- Las llamadas a la API REST deben centralizarse en servicios/hooks personalizados, no hacerse directamente en los componentes.
- Usar `axios` o `fetch` para las llamadas HTTP.

## Reglas generales

- No introducir librerías o frameworks que no estén listados aquí sin acordarlo primero.
- Preferir las librerías ya incluidas en el ecosistema de .NET y React antes de agregar dependencias externas.
- Todo código nuevo debe compilar sin errores y sin warnings antes de considerarse terminado.
- Las fechas y horas siempre se transmiten en formato **ISO 8601 UTC** entre frontend y backend.
