# Autenticación y Usuario

## Proyectos incluidos

- `src/PixiEditor.IdentityProvider/` — interfaz abstracta de identidad
- `src/PixiEditor.IdentityProvider.PixiAuth/` — implementación con PixiAuth
- `src/PixiEditor.PixiAuth/` — cliente HTTP para la API de PixiAuth
- `src/PixiEditor/ViewModels/SubViewModels/UserViewModel.cs` — estado del usuario en la UI
- `src/PixiEditor/Views/Auth/` — vistas de autenticación

## Descripción

El sistema de autenticación permite al usuario iniciar sesión para sincronizar preferencias, desbloquear contenido adicional (DLC, packs de pinceles) y asociar su identidad con la aplicación. La implementación actual usa **PixiAuth**, un servicio de autenticación propio del proyecto.

La arquitectura sigue el patrón de proveedor: hay una interfaz abstracta (`IIdentityProvider`) y una implementación concreta (`PixiAuthIdentityProvider`). Esto permite cambiar el proveedor de identidad sin afectar al editor. La plataforma de distribución (Steam, MSStore, Standalone) es la que decide qué proveedor usar.

## Propósito

Separar la lógica de identidad del editor para que:
- Steam pueda usar su propia autenticación
- MSStore y Standalone puedan usar PixiAuth
- Se pueda agregar otro proveedor en el futuro sin tocar el editor

## Puntos de entrada

| Archivo | Qué hace |
|---|---|
| `PixiEditor.IdentityProvider/IIdentityProvider.cs` | Contrato base: login, logout, usuario actual |
| `PixiEditor.IdentityProvider.PixiAuth/PixiAuthIdentityProvider.cs` | Implementación con PixiAuth |
| `PixiEditor.PixiAuth/PixiAuthClient.cs` | Cliente HTTP que llama a la API de PixiAuth |
| `PixiEditor/ViewModels/SubViewModels/UserViewModel.cs` | Estado reactivo del usuario en la UI |

---

## Grupos de funcionalidades

### Interfaz de identidad

**Archivo:** `PixiEditor.IdentityProvider/IIdentityProvider.cs`

Define las operaciones básicas de identidad:
- Login (con credenciales)
- Logout
- Obtener el usuario actual
- Verificar si hay sesión activa
- Obtener productos/licencias del usuario

**Archivo:** `PixiEditor.IdentityProvider/IUser.cs`

Datos del usuario: nombre, avatar, tokens, estado de la sesión.

### Implementación PixiAuth

**Carpeta:** `PixiEditor.IdentityProvider.PixiAuth/`

- `PixiAuthIdentityProvider.cs` — implementa `IIdentityProvider` usando el cliente PixiAuth
- Gestiona tokens, refresh, y persistencia de la sesión

**Carpeta:** `PixiEditor.PixiAuth/`

- `PixiAuthClient.cs` — cliente HTTP que hace llamadas REST a la API de PixiAuth
- `Models/` — DTOs de respuesta de la API (tokens, usuario, productos)
- `Gravatar.cs` — obtiene el avatar del usuario desde Gravatar a partir del email

### UI de autenticación

**Carpeta:** `PixiEditor/Views/Auth/`

- `LoginPopup.axaml` — ventana modal de inicio de sesión
- `LoginForm.axaml` — formulario de credenciales (email + contraseña)
- `UserAvatarToggle.axaml` — botón con el avatar del usuario en la barra de título; muestra un menú con opciones de cuenta

### Estado del usuario en la UI

**Archivo:** `PixiEditor/ViewModels/SubViewModels/UserViewModel.cs`

ViewModel que expone el estado del usuario a toda la UI:
- Si está logueado o no
- Nombre y avatar del usuario
- Productos/licencias disponibles
- Comandos de login y logout

Se registra como singleton en el DI y se accede desde cualquier vista con `{local:MainVM UserViewModel}`.

### Conexión con la plataforma

El proveedor de identidad se obtiene a través de `IPlatform.Current.IdentityProvider`:

- **Standalone**: usa `PixiAuthIdentityProvider` (login con email/contraseña)
- **MSStore**: también usa `PixiAuthIdentityProvider`
- **Steam**: usa la autenticación de Steam (Steamworks.NET)

### Cómo agregar un proveedor de identidad nuevo

1. Crea una clase que implemente `IIdentityProvider`.
2. Si necesitas un cliente HTTP, créalo como servicio separado (como `PixiAuthClient`).
3. Asígnalo a la plataforma correspondiente en su implementación de `IPlatform`.
