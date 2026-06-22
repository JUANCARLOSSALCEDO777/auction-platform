# Documentación de Código

## Comentarios XML obligatorios

Todo código C# generado debe incluir comentarios de documentación XML (`///`) en los siguientes elementos:

- **Clases públicas**: `<summary>` describiendo el propósito de la clase.
- **Métodos públicos**: `<summary>`, `<param>` para cada parámetro, `<returns>` si devuelve valor, `<exception>` si lanza excepciones controladas.
- **Interfaces**: `<summary>` en la interfaz y en cada método.
- **Propiedades públicas** (en DTOs y entidades): `<summary>` breve si el nombre no es autoexplicativo.
- **Enumeraciones**: `<summary>` en el enum y en cada valor si su significado no es obvio por el nombre.

## Comentarios inline en configuraciones

Las clases de configuración (Fluent API, middleware, Program.cs) deben incluir comentarios `//` explicando:

- Qué hace cada bloque de configuración
- Por qué se eligió un valor específico (ej: `// max 254 chars según RFC 5321`)
- Relaciones entre entidades y el tipo de delete behavior elegido

## Idioma

Todos los comentarios deben estar en **español**, coherente con el idioma del proyecto.

## Ejemplo de formato esperado

```csharp
/// <summary>
/// Servicio de autenticación que maneja registro, login y gestión de tokens JWT.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario con rol Comprador y monedero inicial en cero.
    /// </summary>
    /// <param name="dto">Datos de registro: nombre, email y contraseña.</param>
    /// <returns>Token JWT y datos de sesión.</returns>
    /// <exception cref="ConflictException">Si el email ya está registrado.</exception>
    /// <exception cref="ValidationException">Si los datos no pasan validación.</exception>
    Task<AuthResult> RegisterAsync(RegisterDto dto);
}
```

## Regla general

Si un bloque de código requiere más de 10 segundos para entender su propósito al leerlo por primera vez, necesita un comentario.
