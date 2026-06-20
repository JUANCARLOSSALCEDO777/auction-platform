# Requirements Document

## Introduction

**Plataforma de Subastas — MVP**

Aplicación web para subastas de artículos construida con React (frontend), ASP.NET Core 8 (backend REST API), Entity Framework Core sobre SQL Server, SignalR para tiempo real y autenticación JWT.

El sistema permite a los usuarios registrarse, cargar su monedero electrónico y participar en subastas activas. Los administradores gestionan subastas, usuarios y saldos de monedero. Las subastas se inician y finalizan automáticamente según sus fechas configuradas.

**Decisiones de alcance del MVP:**
- El saldo del monedero es gestionado exclusivamente por el Administrador (acreditación y retiro manual). La carga de saldo por el propio usuario mediante tarjeta de crédito/débito queda fuera del MVP y se implementará en una fase futura como extensión del Requisito 5.

**Stack tecnológico del MVP:**
- Frontend: React
- Backend: ASP.NET Core 8 (REST API)
- ORM: Entity Framework Core
- Base de datos: SQL Server
- Tiempo real: SignalR
- Autenticación: JWT

---

## Glossary

### Términos del dominio

- **Usuario**: Persona registrada en la Plataforma con rol `Comprador` o `Administrador`. Entidad `Usuarios` en BD.
- **Comprador**: Usuario con rol que puede ver el catálogo, cargar su monedero y realizar pujas.
- **Administrador**: Usuario con rol que gestiona subastas, usuarios y saldos de monedero.
- **Rol**: Nivel de privilegio asignado a un Usuario. Valores: `Administrador`, `Comprador`. Entidad `Roles` en BD.
- **Subasta**: Artículo publicado para puja con fechas de inicio/fin y ciclo de vida automático. Estados: `Programada` (creada, esperando fecha de inicio), `Activa` (en curso, acepta pujas), `Cerrada` (finalizada automáticamente al llegar fecha_fin), `Inactiva` (desactivada manualmente por el Administrador). Al cerrarse, almacena `id_usuario_ganador` (FK nullable a `Usuarios`) con el id del Comprador que realizó la puja ganadora, o `null` si no hubo pujas. Entidad `Subastas` en BD.
- **Puja**: Oferta económica realizada por un Comprador sobre una Subasta activa. Entidad `Pujas` en BD.
- **Puja_Mínima**: Importe mínimo de la primera puja. Debe ser mayor que 0,00.
- **Incremento_Mínimo**: Diferencia mínima configurable por el Administrador que debe superar cada nueva Puja respecto a la puja más alta vigente.
- **Precio_Inicial**: Importe base de la Subasta, equivalente a la Puja_Mínima para la primera oferta.
- **Monedero**: Saldo virtual 1:1 con cada Usuario. Entidad `Monederos` en BD.
- **MovimientoMonedero**: Registro de una operación del Monedero: tipo (`acreditacion`, `retiro`, `reserva`, `liberacion`), monto, fecha, usuario y administrador que lo originó. Entidad `MovimientosMonedero` en BD.
- **Saldo_Disponible**: Saldo del Monedero menos las reservas comprometidas en pujas activas.
- **Saldo_Comprometido**: Porción del saldo reservada por una puja activa pendiente de resolución.
- **Imagen**: Foto asociada a una Subasta. Entidad `Imágenes` en BD. Relación N:1 con `Subastas`.
- **Notificación**: Mensaje en tiempo real entregado vía SignalR al cliente. Entidad `Notificaciones` en BD.
- **Hub SignalR**: Endpoint de SignalR que emite eventos de puja y estado de subasta a todos los clientes conectados.

### Entidades de base de datos

| Entidad | Descripción | Requisito(s) |
|---|---|---|
| `Usuarios` | id, nombre, email (único), password_hash, fecha_creacion, activo, id_rol | Req. 1 |
| `Roles` | id, nombre (`Administrador`, `Comprador`) | Req. 1 |
| `Subastas` | id, título, descripción, categoría, precio_inicial, puja_mínima, incremento_mínimo, estado (`Programada`/`Activa`/`Cerrada`/`Inactiva`), fecha_inicio, fecha_fin, id_admin_creador, id_usuario_ganador (nullable, FK a `Usuarios`) | Req. 2, 3 |
| `Pujas` | id, id_subasta, id_usuario, monto, fecha_hora | Req. 4 |
| `Monederos` | id, id_usuario (único), saldo_disponible, saldo_comprometido | Req. 5 |
| `MovimientosMonedero` | id, id_monedero, tipo, monto, fecha, id_usuario_admin (nullable) | Req. 5, 6 |
| `Imágenes` | id, id_subasta, url_almacenamiento, orden | Req. 2 |
| `Notificaciones` | id, id_usuario, tipo, contenido, leido, fecha | Req. 4, 7 |

---

## Requirements

### Requirement 1: Usuarios y Seguridad

**User Story:** Como visitante, quiero registrarme con correo y contraseña y gestionar mi perfil, para acceder a las funcionalidades de la Plataforma según mi rol.

#### Acceptance Criteria

1. WHEN un visitante intenta registrarse, THE Plataforma SHALL validar que el nombre tenga entre 2 y 100 caracteres, el correo electrónico cumpla el formato RFC 5321 y la contraseña tenga entre 8 y 128 caracteres con al menos una mayúscula, una minúscula y un dígito.
2. THE Plataforma SHALL asignar el rol `Comprador` por defecto a todo Usuario registrado, salvo que un Administrador le asigne el rol `Administrador`.
3. THE Plataforma SHALL registrar la fecha y hora UTC de creación de la cuenta en el campo `fecha_creacion` de `Usuarios`.
4. IF el correo electrónico ya existe en `Usuarios`, THEN THE Plataforma SHALL rechazar el registro con HTTP 409 sin revelar si el correo está en uso.
5. WHEN un Usuario envía correo y contraseña válidos en el inicio de sesión, THE Plataforma SHALL retornar un token JWT firmado con expiración de 24 horas que incluya el id de usuario y su rol.
6. IF las credenciales son incorrectas, THEN THE Plataforma SHALL retornar HTTP 401 con mensaje genérico sin indicar qué campo falló.
7. WHEN un Usuario autenticado actualiza su perfil, THE Plataforma SHALL permitir modificar nombre y contraseña; el correo electrónico no podrá ser modificado.
8. WHEN un Administrador bloquea un Usuario, THE Plataforma SHALL establecer `activo = false` en `Usuarios`, rechazar cualquier inicio de sesión posterior de ese Usuario con HTTP 403, e invalidar inmediatamente todos los tokens JWT activos de ese Usuario.
9. THE Plataforma SHALL almacenar contraseñas usando bcrypt con cost ≥ 12.
10. WHEN un cliente envía un token JWT expirado, inválido o ausente a un endpoint protegido, THE API SHALL retornar HTTP 401.

---

### Requirement 2: Gestión de Subastas por el Administrador

**User Story:** Como Administrador, quiero crear, editar y desactivar subastas con múltiples imágenes, para publicar artículos disponibles para puja.

#### Acceptance Criteria

1. THE Administrador SHALL poder crear una Subasta proporcionando: título, descripción, categoría, precio_inicial (> 0,00), puja_mínima (> 0,00), incremento_mínimo (> 0,00), fecha_inicio, fecha_fin y al menos una imagen.
2. THE Plataforma SHALL validar que fecha_fin es posterior a fecha_inicio y que la duración mínima de la Subasta es de 1 hora; IF la validación falla, THEN THE Plataforma SHALL retornar HTTP 422 con detalle de los campos inválidos.
3. THE Plataforma SHALL aceptar imágenes en formato JPEG, PNG o WebP con tamaño máximo de 10 MB por imagen, hasta un máximo de 10 imágenes por Subasta.
4. THE Plataforma SHALL asignar estado `Programada` a toda Subasta recién creada cuya fecha_inicio sea futura.
5. WHILE una Subasta está en estado `Programada`, THE Administrador SHALL poder editar cualquier campo incluyendo imágenes, precios y fechas; la Plataforma SHALL validar que el Usuario posee rol `Administrador` y que la Subasta está en estado `Programada` antes de permitir la edición.
6. WHILE una Subasta está en estado `Activa`, THE Administrador SHALL poder desactivarla, lo que cambiará su estado a `Inactiva` y cancelará las pujas pendientes.
7. IF algún campo obligatorio está ausente o inválido en la creación o edición, THEN THE Plataforma SHALL retornar HTTP 422 con el detalle de cada campo con error.
8. THE Administrador SHALL poder consultar un listado paginado de todas las Subastas filtrable por estado, con título, estado, número de pujas y puja más alta actual.

---

### Requirement 3: Catálogo y Ciclo de Vida Automático de Subastas

**User Story:** Como Comprador, quiero ver el catálogo de subastas activas y que las subastas arranquen y finalicen automáticamente, para participar sin depender de acciones manuales del Administrador.

#### Acceptance Criteria

1. THE Plataforma SHALL mostrar un catálogo paginado de Subastas en estado `Activa`, donde cada entrada incluye imagen principal, título, categoría, precio_inicial, puja más alta actual (o "Sin pujas") y tiempo restante en formato HH:MM:SS.
2. THE Plataforma SHALL permitir filtrar el catálogo por categoría y mostrar hasta 20 Subastas por página con total de resultados.
3. WHEN un Usuario solicita el detalle de una Subasta, THE Plataforma SHALL mostrar todos los campos de la Subasta, todas sus imágenes y el historial completo de Pujas ordenado de mayor a menor monto.
4. WHEN la fecha_inicio de una Subasta en estado `Programada` es alcanzada, THE Plataforma SHALL cambiar automáticamente su estado a `Activa` sin intervención manual.
5. WHEN la fecha_fin de una Subasta en estado `Activa` es alcanzada, THE Plataforma SHALL cambiar automáticamente su estado a `Cerrada` y determinar la Puja ganadora como la de mayor monto registrada.
6. WHEN una Subasta cambia a estado `Cerrada` con al menos una Puja, THE Plataforma SHALL persistir el `id_usuario_ganador` en la entidad `Subastas` con el id del Comprador cuya Puja fue la ganadora (mayor monto; en caso de empate, la de menor fecha_hora UTC).
7. IF una Subasta en estado `Activa` llega a su fecha_fin sin ninguna Puja, THEN THE Plataforma SHALL cambiar su estado a `Cerrada` y establecer `id_usuario_ganador = null`.
8. THE Plataforma SHALL ejecutar las transiciones automáticas de estado con una tolerancia máxima de 60 segundos respecto a la fecha configurada.

---

### Requirement 4: Pujas

**User Story:** Como Comprador autenticado con saldo suficiente, quiero realizar pujas sobre subastas activas, para intentar ganar el artículo.

#### Acceptance Criteria

1. WHILE una Subasta está en estado `Activa`, THE Plataforma SHALL permitir a un Comprador autenticado enviar una Puja.
2. WHEN un Comprador envía una Puja y no existe ninguna Puja previa en la Subasta, THE Plataforma SHALL aceptar la Puja únicamente si el monto es mayor o igual a la puja_mínima de la Subasta.
3. WHEN un Comprador envía una Puja y ya existe al menos una Puja previa, THE Plataforma SHALL aceptar la Puja únicamente si el monto supera la puja más alta actual en al menos el incremento_mínimo configurado.
4. IF el monto de la Puja no cumple los criterios de los criterios 2 o 3, THEN THE Plataforma SHALL rechazar la Puja con HTTP 422 e indicar el monto mínimo requerido.
5. THE Plataforma SHALL verificar que el Saldo_Disponible del Comprador es mayor o igual al monto de la Puja antes de aceptarla; IF el saldo es insuficiente, THEN THE Plataforma SHALL rechazar la Puja con HTTP 422.
5a. WHEN una Puja supera las validaciones de monto (criterios 2 o 3) y saldo (criterio 5), THE Plataforma SHALL aceptarla únicamente si la Subasta sigue en estado `Activa` en el momento de la escritura atómica; IF la Subasta cambió de estado entre la validación y la escritura, THEN THE Plataforma SHALL rechazar la Puja con HTTP 409.
6. WHEN una Puja es aceptada, THE Plataforma SHALL registrarla en `Pujas` con id_usuario, id_subasta, monto y fecha_hora UTC de forma atómica.
7. WHEN dos Pujas simultáneas llegan con el mismo monto, THE Plataforma SHALL aceptar únicamente la de menor fecha_hora UTC y rechazar la otra con HTTP 409.
8. THE Plataforma SHALL mantener el historial completo de todas las Pujas de cada Subasta consultable por el Administrador.
9. WHEN una Subasta finaliza, THE Plataforma SHALL determinar automáticamente la Puja ganadora como la de mayor monto; en caso de empate de monto, se considera ganadora la de menor fecha_hora UTC.

---

### Requirement 5: Monedero Electrónico

**User Story:** Como usuario del sistema, quiero disponer de un monedero electrónico con historial de movimientos, para que el Administrador pueda gestionar mi saldo y el sistema valide mis fondos antes de aceptar pujas.

#### Acceptance Criteria

1. WHEN se crea una cuenta de Usuario, THE Plataforma SHALL crear automáticamente un Monedero con saldo_disponible = 0,00 y saldo_comprometido = 0,00.
2. THE Administrador SHALL poder acreditar saldo a cualquier Usuario registrado y activo especificando el monto (> 0,00); WHEN la acreditación se procesa, THE Plataforma SHALL incrementar el saldo_disponible del Monedero y registrar automáticamente un MovimientoMonedero con tipo `acreditacion`.
3. THE Administrador SHALL poder retirar saldo de cualquier Usuario registrado y activo especificando el monto; IF el monto solicitado supera el saldo_disponible, THEN THE Plataforma SHALL rechazar la operación con HTTP 422.
4. WHEN el Administrador realiza un retiro válido, THE Plataforma SHALL decrementar el saldo_disponible del Monedero y registrar un MovimientoMonedero de tipo `retiro`.
5. THE Plataforma SHALL registrar cada MovimientoMonedero con: fecha UTC, tipo, monto, id del Usuario propietario y (cuando aplique) id del Administrador que originó el movimiento.
6. WHEN un Comprador realiza una Puja aceptada, THE Plataforma SHALL reservar el monto de la Puja incrementando saldo_comprometido y decrementando saldo_disponible de forma atómica.
7. WHEN un Comprador es superado en una Subasta activa por otra Puja, THE Plataforma SHALL liberar el saldo_comprometido de su puja anterior, decrementando saldo_comprometido e incrementando saldo_disponible.
8. WHEN un Usuario consulta su Monedero, THE Plataforma SHALL mostrar saldo_disponible, saldo_comprometido y el historial de MovimientosMonedero de los últimos 90 días ordenado de más reciente a más antiguo.
9. THE Administrador SHALL poder visualizar el historial de MovimientosMonedero de cualquier Usuario desde el panel de administración.

---

### Requirement 6: Panel de Administración

**User Story:** Como Administrador, quiero tener un panel centralizado para gestionar usuarios, subastas y monederos, para operar la plataforma de forma eficiente.

#### Acceptance Criteria

1. THE Administrador SHALL poder visualizar un listado paginado de todos los Usuarios registrados con nombre, correo, rol, fecha de creación y estado (activo/bloqueado).
2. THE Administrador SHALL poder bloquear o desbloquear un Usuario desde el listado; WHEN un Usuario es bloqueado, THE Plataforma SHALL establecer `activo = false`, impedir su inicio de sesión inmediatamente e invalidar todos sus tokens JWT activos para terminar las sesiones en curso.
3. THE Administrador SHALL poder visualizar todas las Subastas activas con su estado, número de participantes y puja más alta actual.
4. THE Administrador SHALL poder visualizar el historial completo de Pujas de cualquier Subasta ordenado por fecha_hora descendente.
5. THE Administrador SHALL poder visualizar y filtrar los MovimientosMonedero de cualquier Usuario por tipo y rango de fechas.
6. THE Administrador SHALL poder acreditar o retirar saldo del Monedero de cualquier Usuario registrado y activo desde el panel (ver Req. 5 criterios 2–4).
7. THE Plataforma SHALL restringir el acceso a todas las rutas del panel de administración a Usuarios con rol `Administrador`; IF un Usuario con rol `Comprador` intenta acceder, THE Plataforma SHALL retornar HTTP 403.

---

### Requirement 7: Tiempo Real con SignalR

**User Story:** Como Comprador conectado a una subasta, quiero ver las actualizaciones de pujas, precio y contador sin recargar la página, para tener la misma visión del estado de la subasta que todos los participantes.

#### Acceptance Criteria

1. WHEN una Puja es aceptada, THE Hub SignalR SHALL emitir un evento `NuevaPuja` a todos los clientes conectados al grupo de esa Subasta con el nuevo monto más alto, el nombre del pujador y la fecha_hora, con una latencia máxima de 3 segundos desde el registro de la Puja.
2. WHILE una Subasta está en estado `Activa`, THE Hub SignalR SHALL emitir un evento `TickContador` cada segundo a los clientes conectados con el tiempo restante hasta fecha_fin.
3. WHEN el precio de la puja más alta cambia, THE Hub SignalR SHALL emitir un evento `PrecioActualizado` con el nuevo importe a todos los clientes conectados al grupo de esa Subasta.
4. WHEN una Subasta cambia de estado (a `Activa`, `Cerrada` o `Inactiva`), THE Hub SignalR SHALL emitir un evento `EstadoSubasta` a todos los clientes conectados con el nuevo estado y (si aplica) el ganador.
5. THE Plataforma SHALL garantizar que todos los clientes conectados al mismo grupo de Subasta reciban el mismo estado en cada evento, sin divergencias entre clientes.
6. THE Hub SignalR SHALL requerir un token JWT válido para unirse a un grupo de Subasta; IF el token es inválido o ausente, THE Hub SHALL rechazar la conexión.

---

### Requirement 8: API REST para Integración Futura Móvil

**User Story:** Como equipo de desarrollo, quiero que todas las funcionalidades estén expuestas como API REST versionada, para integrar una aplicación móvil en el futuro sin cambios en el backend.

#### Acceptance Criteria

1. THE API SHALL exponer endpoints REST bajo el prefijo `/api/v1/` cubriendo: registro/login, perfil, catálogo, detalle de subasta, pujas, monedero y administración.
2. THE API SHALL publicar documentación OpenAPI 3.0 accesible en `/api/docs` que incluya esquemas de request/response y códigos de error de cada endpoint.
3. FOR ALL respuestas, THE API SHALL retornar JSON con estructura consistente: `{ "data": <payload|null>, "error": <mensaje|null>, "meta": <paginación u otros|null> }`.
4. THE API SHALL implementar rate limiting de 100 peticiones por minuto por token JWT; WHEN se supera el límite, THE API SHALL retornar HTTP 429.
5. THE API SHALL versionar sus endpoints de modo que una futura `/api/v2/` pueda coexistir con `/api/v1/` sin romper clientes existentes.
6. THE API SHALL aceptar y retornar fechas y horas siempre en formato ISO 8601 UTC.
