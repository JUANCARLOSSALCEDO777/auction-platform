# Checkpoint — Plataforma de Subastas

## Estado actual (a revisar)

Este checkpoint fue creado porque el diseño técnico (`design.md`) está pendiente de revisión y aprobación.

**El `design.md` fue generado automáticamente pero NO ha sido aprobado todavía.**

---

## Qué está aprobado ✅

- `requirements.md` — requisitos completos y acordados
- Stack tecnológico: React, ASP.NET Core 8, EF Core, SQL Server, SignalR, JWT
- Entidades de BD: Usuarios, Roles, Subastas, Pujas, Monederos, MovimientosMonedero, Imágenes, Notificaciones
- Alcance del MVP: monedero gestionado por Admin (sin cobros por tarjeta)

## Qué está aprobado también ✅

- `design.md` — diseño técnico aprobado
  - Arquitectura por capas (`Controllers → Services → Repositories`)
  - Endpoints REST definidos
  - Interfaces de servicios en C#
  - Estrategia de concurrencia con `rowversion`
  - BackgroundService para ciclo de vida de subastas
  - Invalidación de JWT con lista negra en caché
  - 23 propiedades de corrección
  - Estrategia de testing con PBT (FsCheck + fast-check)

## Próximos pasos sugeridos

1. Generar `tasks.md` para comenzar la implementación

## Para retomar desde aquí

Decirle a Kiro: _"retomemos desde el checkpoint, quiero revisar el diseño"_
