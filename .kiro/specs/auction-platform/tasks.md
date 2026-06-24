# Implementation Plan: Plataforma de Subastas (MVP)

## Overview

Plan de implementación incremental para la Plataforma de Subastas. Se construye primero la infraestructura del backend (dominio, datos, autenticación), luego la lógica de negocio (subastas, pujas, monedero), después la capa de tiempo real (SignalR) y finalmente el frontend React/TypeScript. Cada tarea referencia requisitos específicos para trazabilidad completa.

## Tasks

- [x] 1. Configuración del proyecto y estructura base
  - [x] 1.1 Crear la solución .NET y proyectos por capa
    - Crear solución `AuctionPlatform.sln` con los proyectos: `AuctionPlatform.Api`, `AuctionPlatform.Application`, `AuctionPlatform.Domain`, `AuctionPlatform.Infrastructure`, `AuctionPlatform.Workers`
    - Agregar proyecto de tests `AuctionPlatform.Tests` con xUnit y FsCheck
    - Configurar dependencias entre proyectos según la arquitectura por capas
    - _Requirements: 8.1, 8.5_

  - [x] 1.2 Definir entidades del dominio y enumeraciones
    - Crear clases de dominio: `Usuario`, `Rol`, `Subasta`, `Puja`, `Monedero`, `MovimientoMonedero`, `Imagen`, `Notificacion`
    - Crear enumeraciones: `EstadoSubasta` (Programada, Activa, Cerrada, Inactiva), `TipoMovimiento` (Acreditacion, Retiro, Reserva, Liberacion)
    - Crear excepciones de dominio: `DomainException`, `NotFoundException`, `ConflictException`, `ForbiddenException`, `ValidationException`
    - _Requirements: 1.1, 2.1, 4.6, 5.1_

  - [x] 1.3 Configurar EF Core, DbContext y Fluent API
    - Crear `AuctionDbContext` con DbSets para todas las entidades
    - Crear configuraciones Fluent API (`IEntityTypeConfiguration<T>`) para cada entidad con tipos de datos, índices y restricciones según el diseño
    - Configurar `rowversion` en Subastas para control de concurrencia optimista
    - Configurar índice compuesto en Pujas (id_subasta, monto, fecha_hora)
    - Configurar índice único en Monederos (id_usuario) y Usuarios (email)
    - _Requirements: 2.1, 4.6, 4.7, 5.1_

  - [x] 1.4 Crear la migración inicial y seed de datos
    - Generar la primera migración de EF Core con el esquema completo
    - Crear seed para roles (`Administrador`, `Comprador`) y usuario administrador inicial
    - _Requirements: 1.2_

  - [x] 1.5 Configurar middleware global, estructura de respuesta API y Swagger
    - Implementar `ApiResponse<T>` record con campos `data`, `error`, `meta`
    - Implementar `ExceptionHandlingMiddleware` que mapea excepciones de dominio a códigos HTTP
    - Configurar serializador JSON para fechas ISO 8601 UTC (`JsonSerializerOptions`)
    - Configurar Swagger/OpenAPI 3.0 en `/api/docs`
    - Configurar rate limiting (100 req/min por token JWT) con `System.Threading.RateLimiting`
    - _Requirements: 8.2, 8.3, 8.4, 8.6_

  - [ ]* 1.6 Write property tests for API response structure and date format
    - **Property 22: Toda respuesta de la API sigue la estructura { data, error, meta }**
    - **Property 23: Todas las fechas en respuestas de la API están en formato ISO 8601 UTC**
    - **Validates: Requirements 8.3, 8.6**

- [ ] 2. Autenticación, usuarios y seguridad
  - [x] 2.1 Implementar servicio JWT y validación de tokens
    - Crear `JwtTokenService` que genera tokens con claims `id` y `rol`, expiración 24h
    - Configurar `AddAuthentication().AddJwtBearer(...)` en Program.cs
    - Implementar middleware de lista negra JWT usando `IDistributedCache` (clave `blacklist:{userId}`, TTL 24h)
    - _Requirements: 1.5, 1.8, 1.10_

  - [ ]* 2.2 Write property tests for JWT generation and token validation
    - **Property 3: JWT de login contiene los claims correctos y expira en 24 horas**
    - **Property 6: Tokens inválidos, expirados o ausentes producen HTTP 401**
    - **Validates: Requirements 1.5, 1.10**

  - [ ] 2.3 Implementar registro de usuario con validaciones
    - Crear `UserValidator` con validaciones: nombre (2-100 chars), email (RFC 5321), contraseña (8-128 chars, mayúscula, minúscula, dígito)
    - Implementar `IAuthService.RegisterAsync`: hash bcrypt (cost ≥ 12), asignar rol Comprador, crear Monedero (saldo 0,00), registrar `fecha_creacion` UTC
    - Manejar email duplicado → HTTP 409 con mensaje genérico
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.9, 5.1_

  - [ ]* 2.4 Write property tests for user registration validation
    - **Property 1: Validación de registro rechaza exactamente los inputs inválidos**
    - **Property 2: Todo usuario registrado recibe rol Comprador y monedero en cero**
    - **Validates: Requirements 1.1, 1.2, 5.1**

  - [ ] 2.5 Implementar login y gestión de perfil
    - Implementar `IAuthService.LoginAsync`: verificar credenciales, retornar JWT; HTTP 401 con mensaje genérico si falla
    - Implementar `UserService.UpdateProfileAsync`: permitir cambio de nombre y contraseña, ignorar campo email
    - Crear `AuthController` (POST /register, POST /login) y `UsersController` (GET /me, PUT /me)
    - _Requirements: 1.5, 1.6, 1.7_

  - [ ]* 2.6 Write property test for profile update immutability
    - **Property 5: Actualización de perfil sólo permite nombre y contraseña, nunca el correo**
    - **Validates: Requirements 1.7**

  - [ ] 2.7 Implementar bloqueo/desbloqueo de usuarios con invalidación de tokens
    - Implementar `IAuthService.BlockUserAsync`: establecer `activo = false`, agregar a lista negra en caché
    - Implementar `IAuthService.UnblockUserAsync`: establecer `activo = true`, remover de lista negra
    - El middleware de validación debe rechazar con HTTP 403 cualquier request de usuario bloqueado
    - _Requirements: 1.8, 6.2_

  - [ ]* 2.8 Write property test for blocked user token rejection
    - **Property 4: Tokens de usuarios bloqueados son rechazados en todos los endpoints protegidos**
    - **Validates: Requirements 1.8, 6.2**

- [ ] 3. Checkpoint - Verificar autenticación y seguridad
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 4. Gestión de subastas (Administrador)
  - [ ] 4.1 Implementar creación y edición de subastas
    - Crear `AuctionValidator`: validar campos obligatorios, precio_inicial > 0, puja_mínima > 0, incremento_mínimo > 0, fecha_fin > fecha_inicio, duración ≥ 1 hora
    - Implementar `IAuctionService.CreateAsync`: asignar estado `Programada` si fecha_inicio es futura, asociar imágenes (JPEG/PNG/WebP, max 10MB, max 10 por subasta)
    - Implementar `IAuctionService.UpdateAsync`: permitir edición solo si estado = `Programada`; rechazar con HTTP 422 si otro estado
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.7_

  - [ ]* 4.2 Write property tests for auction creation and validation
    - **Property 7: Subastas nuevas con fecha_inicio futura inician en estado Programada**
    - **Property 8: Validación de fechas de subasta rechaza duraciones inválidas**
    - **Property 9: Sólo subastas en estado Programada pueden editarse**
    - **Validates: Requirements 2.2, 2.4, 2.5**

  - [ ] 4.3 Implementar desactivación de subastas y controlador de administración
    - Implementar `IAuctionService.DeactivateAsync`: cambiar estado `Activa` → `Inactiva`, cancelar pujas pendientes y liberar saldos comprometidos
    - Crear `AuctionsController` con endpoints: POST crear, PUT editar, POST desactivar
    - Validar autorización: solo rol Administrador
    - _Requirements: 2.5, 2.6, 6.7_

  - [ ] 4.4 Implementar catálogo público y detalle de subasta
    - Implementar `IAuctionService.GetCatalogAsync`: retornar solo subastas en estado `Activa`, paginación (20 por página), filtro por categoría
    - Implementar `IAuctionService.GetDetailAsync`: todos los campos, todas las imágenes, historial de pujas ordenado por monto descendente (desempate por fecha_hora ASC)
    - Crear endpoints GET en `AuctionsController`: catálogo y detalle
    - _Requirements: 3.1, 3.2, 3.3_

  - [ ]* 4.5 Write property tests for catalog and bid history
    - **Property 10: El catálogo sólo contiene subastas en estado Activa**
    - **Property 11: El filtro por categoría produce resultados de esa categoría únicamente**
    - **Property 12: El historial de pujas de una subasta está ordenado por monto descendente**
    - **Validates: Requirements 3.1, 3.2, 3.3, 6.4**

- [ ] 5. Monedero electrónico
  - [ ] 5.1 Implementar servicio de monedero (acreditación, retiro, reserva, liberación)
    - Implementar `IWalletService.CreditAsync`: incrementar saldo_disponible, registrar MovimientoMonedero tipo `acreditacion` con id_usuario_admin
    - Implementar `IWalletService.WithdrawAsync`: validar saldo_disponible ≥ monto, decrementar, registrar MovimientoMonedero tipo `retiro`; HTTP 422 si insuficiente
    - Implementar `IWalletService.ReserveFundsAsync`: decrementar saldo_disponible, incrementar saldo_comprometido, registrar MovimientoMonedero tipo `reserva`
    - Implementar `IWalletService.ReleaseFundsAsync`: decrementar saldo_comprometido, incrementar saldo_disponible, registrar MovimientoMonedero tipo `liberacion`
    - Implementar `IWalletService.GetWalletAsync`: retornar saldos + historial últimos 90 días ordenado descendente por fecha
    - _Requirements: 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8_

  - [ ]* 5.2 Write property tests for wallet invariants
    - **Property 17: Los saldos del monedero se actualizan correctamente en acreditación, retiro y reserva**
    - **Property 18: Todo MovimientoMonedero contiene todos los campos requeridos**
    - **Validates: Requirements 5.2, 5.3, 5.4, 5.5, 5.6, 5.7**

  - [ ] 5.3 Crear WalletController y endpoints de administración de monedero
    - Crear `WalletController`: GET /wallet (saldo + historial), POST /wallet/credit (Admin), POST /wallet/withdraw (Admin)
    - Crear endpoint GET /admin/users/{id}/wallet para historial por usuario (Admin)
    - Validar autorización por rol según endpoint
    - _Requirements: 5.8, 5.9, 6.5, 6.6_

- [ ] 6. Sistema de pujas
  - [ ] 6.1 Implementar servicio de pujas con control de concurrencia
    - Implementar `IBidService.PlaceBidAsync` con transacción serializable y validación de rowversion:
      1. Verificar subasta en estado `Activa`
      2. Validar monto vs puja_mínima (primera puja) o monto > max_actual + incremento_mínimo (posterior)
      3. Verificar saldo_disponible ≥ monto
      4. Insertar puja atómicamente, reservar saldo (llamar WalletService.ReserveFundsAsync)
      5. Liberar saldo de la puja anterior del mismo pujador o del pujador superado (WalletService.ReleaseFundsAsync)
    - Manejar `DbUpdateConcurrencyException` → HTTP 409
    - Manejar pujas simultáneas con mismo monto → aceptar la de menor fecha_hora, rechazar otra con HTTP 409
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.5a, 4.6, 4.7_

  - [ ]* 6.2 Write property tests for bid validation
    - **Property 14: Primera puja requiere monto ≥ puja_mínima**
    - **Property 15: Pujas posteriores requieren superar la mayor en al menos el incremento_mínimo**
    - **Property 16: Puja rechazada si saldo_disponible es insuficiente**
    - **Validates: Requirements 4.2, 4.3, 4.5**

  - [ ] 6.3 Crear BidsController y endpoint de historial
    - Crear `BidsController`: POST /bids (Comprador), GET /bids/auction/{id} (Admin)
    - Implementar `IBidService.GetBidHistoryAsync` con paginación
    - _Requirements: 4.1, 4.8, 6.4_

- [ ] 7. Checkpoint - Verificar lógica de negocio backend
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Ciclo de vida automático de subastas (BackgroundService)
  - [ ] 8.1 Implementar AuctionLifecycleBackgroundService
    - Crear `AuctionLifecycleBackgroundService` que hereda de `BackgroundService`
    - Loop cada 30 segundos: consultar subastas con fecha_inicio ≤ now en estado `Programada` → cambiar a `Activa`
    - Loop cada 30 segundos: consultar subastas con fecha_fin ≤ now en estado `Activa` → cambiar a `Cerrada`
    - Al cerrar: determinar ganador (mayor monto, desempate por menor fecha_hora), persistir `id_usuario_ganador`
    - Al cerrar: liberar saldo_comprometido de todos los pujadores no ganadores
    - Registrar servicio con `AddHostedService<AuctionLifecycleBackgroundService>`
    - _Requirements: 3.4, 3.5, 3.6, 3.7, 3.8, 4.9_

  - [ ]* 8.2 Write property test for winner determination
    - **Property 13: El ganador al cerrar una subasta es el pujador de mayor monto (desempate por fecha)**
    - **Validates: Requirements 3.5, 3.6, 4.9**

- [ ] 9. Panel de administración (API)
  - [ ] 9.1 Implementar AdminController con todos los endpoints
    - GET /admin/users: listado paginado con nombre, correo, rol, fecha_creacion, activo
    - POST /admin/users/{id}/block y POST /admin/users/{id}/unblock
    - GET /admin/auctions: listado paginado con estado, número de participantes, puja más alta
    - GET /admin/auctions/{id}/bids: historial completo por fecha_hora descendente
    - GET /admin/users/{id}/wallet: historial de movimientos filtrable por tipo y rango de fechas
    - Aplicar filtro de autorización `[Authorize(Roles = "Administrador")]` a todos los endpoints
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.7_

  - [ ]* 9.2 Write property test for admin route authorization
    - **Property 19: Las rutas de administración retornan HTTP 403 para usuarios con rol Comprador**
    - **Validates: Requirements 6.7**

- [ ] 10. Tiempo real con SignalR
  - [ ] 10.1 Implementar AuctionHub y emisión de eventos
    - Crear `AuctionHub` con métodos `JoinAuctionGroup` y `LeaveAuctionGroup` (grupos `AuctionGroup_{id}`)
    - Requerir JWT válido para conexión al hub; rechazar si token inválido o usuario bloqueado
    - Emitir evento `NuevaPuja` al aceptar puja (monto, nombre pujador, fecha_hora) con latencia ≤ 3s
    - Emitir evento `PrecioActualizado` con nuevo precio máximo
    - Emitir evento `EstadoSubasta` en cada transición de estado (desde BackgroundService y desactivación manual)
    - Implementar emisión de `TickContador` cada segundo para subastas activas con clientes conectados
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6_

  - [ ]* 10.2 Write property tests for SignalR event emission
    - **Property 20: El hub SignalR emite PrecioActualizado y NuevaPuja ante cualquier puja aceptada**
    - **Property 21: El hub SignalR emite EstadoSubasta ante cualquier cambio de estado**
    - **Validates: Requirements 7.1, 7.3, 7.4**

- [ ] 11. Checkpoint - Verificar backend completo
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 12. Frontend - Configuración y autenticación
  - [ ] 12.1 Crear proyecto React/TypeScript y configuración base
    - Inicializar proyecto React con TypeScript (Vite)
    - Configurar axios con interceptor para estructura `{ data, error, meta }`
    - Configurar interceptor: HTTP 401 → redirigir a login y limpiar token local
    - Configurar cliente SignalR (`@microsoft/signalr`) con reconexión automática
    - Crear contexto de autenticación (`AuthContext`) con almacenamiento de token JWT
    - _Requirements: 8.3, 8.6_

  - [ ] 12.2 Implementar páginas de registro, login y perfil
    - Crear formulario de registro con validación frontend (nombre, email, contraseña según reglas Req 1.1)
    - Crear formulario de login con manejo de errores genéricos
    - Crear página de perfil con edición de nombre y contraseña (email readonly)
    - Implementar routing protegido por rol
    - _Requirements: 1.1, 1.5, 1.6, 1.7_

- [ ] 13. Frontend - Catálogo y subastas
  - [ ] 13.1 Implementar catálogo de subastas activas
    - Crear componente de listado con cards: imagen principal, título, categoría, precio_inicial, puja más alta, tiempo restante (HH:MM:SS)
    - Implementar filtro por categoría y paginación (20 por página)
    - Conectar con SignalR para actualizar precio y tiempo restante en vivo
    - _Requirements: 3.1, 3.2, 7.2, 7.3_

  - [ ] 13.2 Implementar página de detalle de subasta con pujas en tiempo real
    - Mostrar todos los campos, galería de imágenes, historial de pujas (orden descendente por monto)
    - Mostrar contador regresivo en tiempo real (evento `TickContador` vía SignalR)
    - Actualizar precio y lista de pujas en vivo (eventos `NuevaPuja`, `PrecioActualizado`)
    - Mostrar evento `EstadoSubasta` cuando cambia estado (cerrada, inactiva, con ganador)
    - Formulario de nueva puja con validación de monto mínimo
    - _Requirements: 3.3, 4.1, 7.1, 7.2, 7.3, 7.4_

  - [ ] 13.3 Implementar sección de monedero del comprador
    - Mostrar saldo_disponible y saldo_comprometido
    - Mostrar historial de movimientos (últimos 90 días, orden descendente)
    - _Requirements: 5.8_

- [ ] 14. Frontend - Panel de administración
  - [ ] 14.1 Implementar panel de gestión de usuarios
    - Listado paginado con nombre, correo, rol, fecha_creacion, estado
    - Botones de bloquear/desbloquear usuario
    - Visualización de historial de monedero por usuario con filtros por tipo y rango de fechas
    - Acciones de acreditar/retirar saldo
    - _Requirements: 6.1, 6.2, 6.5, 6.6_

  - [ ] 14.2 Implementar panel de gestión de subastas
    - Formulario de creación de subasta (todos los campos + upload de imágenes)
    - Formulario de edición (solo si estado = Programada)
    - Listado paginado de subastas con estado, participantes, puja más alta
    - Botón de desactivar subasta activa
    - Historial de pujas por subasta
    - _Requirements: 2.1, 2.5, 2.6, 2.8, 6.3, 6.4_

  - [ ]* 14.3 Write frontend property tests
    - **Property 1 (frontend): Validación de formulario de registro rechaza inputs inválidos** (fast-check sobre `useRegisterForm`)
    - **Property 12 (frontend): Orden historial de pujas en UI siempre descendente por monto** (fast-check sobre `useBidHistory`)
    - **Validates: Requirements 1.1, 3.3**

- [ ] 15. Integración final y wiring
  - [ ] 15.1 Conectar frontend con backend y verificar flujos end-to-end
    - Configurar proxy de desarrollo de Vite hacia la API backend
    - Verificar flujo completo: registro → login → ver catálogo → pujar → ver resultado en tiempo real
    - Verificar flujo admin: crear subasta → esperar activación automática → cerrar → ver ganador
    - Asegurar que todas las rutas de admin protegen con HTTP 403 a Compradores
    - _Requirements: 1.1–1.10, 2.1–2.8, 3.1–3.8, 4.1–4.9, 5.1–5.9, 6.1–6.7, 7.1–7.6, 8.1–8.6_

  - [ ]* 15.2 Write integration tests
    - Tests de integración con TestContainers: transición automática Programada→Activa→Cerrada
    - Test de tolerancia ≤ 60s en transiciones
    - Test de puja concurrente con control de concurrencia (rowversion)
    - Test de rate limiting (HTTP 429 tras 100 req/min)
    - _Requirements: 3.4, 3.5, 3.8, 4.5a, 4.7, 8.4_

- [ ] 16. Final checkpoint - Verificar plataforma completa
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Las tareas marcadas con `*` son opcionales y pueden omitirse para un MVP más rápido
- Cada tarea referencia requisitos específicos para trazabilidad completa
- Los checkpoints aseguran validación incremental antes de avanzar a la siguiente fase
- Los property tests validan propiedades universales de corrección definidas en el diseño (23 propiedades)
- Los tests unitarios validan ejemplos específicos y casos borde
- El backend se implementa primero para validar la lógica de negocio de forma aislada antes de conectar el frontend
- Se usa xUnit + FsCheck para PBT en backend y Vitest + fast-check para PBT en frontend

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2"] },
    { "id": 2, "tasks": ["1.3"] },
    { "id": 3, "tasks": ["1.4", "1.5"] },
    { "id": 4, "tasks": ["1.6", "2.1"] },
    { "id": 5, "tasks": ["2.2", "2.3"] },
    { "id": 6, "tasks": ["2.4", "2.5"] },
    { "id": 7, "tasks": ["2.6", "2.7"] },
    { "id": 8, "tasks": ["2.8", "4.1", "5.1"] },
    { "id": 9, "tasks": ["4.2", "4.3", "5.2", "5.3"] },
    { "id": 10, "tasks": ["4.4", "6.1"] },
    { "id": 11, "tasks": ["4.5", "6.2", "6.3"] },
    { "id": 12, "tasks": ["8.1"] },
    { "id": 13, "tasks": ["8.2", "9.1"] },
    { "id": 14, "tasks": ["9.2", "10.1"] },
    { "id": 15, "tasks": ["10.2", "12.1"] },
    { "id": 16, "tasks": ["12.2", "13.1"] },
    { "id": 17, "tasks": ["13.2", "13.3"] },
    { "id": 18, "tasks": ["14.1", "14.2"] },
    { "id": 19, "tasks": ["14.3", "15.1"] },
    { "id": 20, "tasks": ["15.2"] }
  ]
}
```
