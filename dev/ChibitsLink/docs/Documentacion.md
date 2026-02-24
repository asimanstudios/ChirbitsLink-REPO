# Documentación del Código - ChibitsLink

ChibitsLink es una aplicación móvil desarrollada con **.NET MAUI** (Multiplataforma) que convierte un dispositivo Android/iOS en un **mando de videojuego** para partidas en Unity. Los datos de usuario y partidas se persisten en **Google Cloud Firestore** y la autenticación gestiona con **Firebase Auth**.

---

## Arquitectura General

La aplicación sigue una arquitectura en capas similar al patrón **MVC con Repository**:

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   VIEW       │────▶│ CONTROLLER   │────▶│   SERVICE    │────▶│  REPOSITORY  │
│ (XAML/.cs)   │     │ (lógica UI)  │     │ (neg. puro)  │     │ (Firestore)  │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

| Capa           | Carpeta                        | Responsabilidad                                    |
|----------------|--------------------------------|----------------------------------------------------|
| **Modelos**    | `src/main/cs/model`            | Entidades de datos puras (POCO/DTO).               |
| **Repositorio**| `src/main/cs/repository`       | Acceso directo a Firestore (CRUD genérico).        |
| **Servicios**  | `src/main/cs/service`          | Lógica de negocio y validaciones.                  |
| **Controladores** | `src/main/cs/controller`    | Intermediarios entre vistas y servicios.           |
| **Vistas**     | `src/main/cs/view`             | Páginas XAML y su code-behind.                     |
| **Excepciones**| `src/main/cs/exception`        | Jerarquía de errores de dominio.                   |
| **Red**        | `src/main/cs/net`              | Conexión TCP/WebSocket con el servidor Unity.      |
| **Converters** | `src/main/cs/converters`       | Convertidores de valor para bindings XAML.         |

---

## Modelos (`src/main/cs/model`)

### `User.cs`
Representa al jugador registrado en la plataforma.

| Propiedad           | Tipo            | Descripción                                         |
|---------------------|-----------------|-----------------------------------------------------|
| `Id`                | `string`        | UID de Firebase Auth.                               |
| `RealName`          | `string`        | Nombre real del usuario.                            |
| `Username`          | `string`        | Alias público visible en el juego.                  |
| `SelectedCharacterId` | `string`      | ID del personaje actualmente seleccionado.          |
| `Level`             | `int`           | Nivel del jugador, calculado a partir del historial.|
| `GameHistory`       | `List<string>`  | Lista de códigos de sala de partidas jugadas.       |

### `Character.cs`
Representa un héroe seleccionable. Los atributos de combate (ataque, defensa, velocidad) son gestionados **en Unity**, no en la app.

| Propiedad     | Tipo     | Descripción                         |
|---------------|----------|-------------------------------------|
| `Id`          | `string` | Identificador único del personaje.  |
| `Name`        | `string` | Nombre del personaje.               |
| `ImageUrl`    | `string` | Ruta o URL de su imagen.            |
| `Description` | `string` | Texto descriptivo del personaje.    |

### `Game.cs`
Representa un minijuego disponible en la plataforma.

| Propiedad     | Tipo       | Descripción                            |
|---------------|------------|----------------------------------------|
| `Id`          | `int`      | Identificador único del juego.         |
| `Name`        | `string`   | Nombre del juego.                      |
| `Type`        | `GameType` | Tipo de control (joystick, sensor...). |
| `Description` | `string`   | Descripción del juego.                 |

### `GameType.cs` (enum)
Define los tipos de juego soportados:
- `Soccer` — Fútbol simple.
- `Jump` — Plataformas con sensor de soplido.
- `Accelerometer` — Control por acelerómetro.
- `Kitchen` — Cocina con mando estándar.

### `Party.cs`
Representa una sala de juego activa.

| Propiedad    | Tipo          | Descripción                                  |
|--------------|---------------|----------------------------------------------|
| `Id`         | `string`      | Identificador único de la sala.              |
| `Name`       | `string`      | Nombre de la sala.                           |
| `RoomCode`   | `string`      | Código de 6 caracteres para unirse.          |
| `PlayerIds`  | `List<int>`   | Identificadores de los jugadores en la sala. |

### `PartyProgress.cs`
Registra el resultado final de una sala al terminar la partida.

| Propiedad      | Tipo                  | Descripción                                      |
|----------------|-----------------------|--------------------------------------------------|
| `Id`           | `string`              | Identificador del registro.                      |
| `PartyId`      | `string`              | Referencia a la sala (`Party`).                  |
| `WinnerId`     | `int?`                | ID del jugador ganador (nulo si empate).          |
| `PlayerScores` | `Dictionary<int,int>` | Mapa de jugador → puntuación.                    |
| `CompletedAt`  | `DateTime`            | Fecha y hora de finalización de la partida.      |

### `LobbyHistory.cs`
Registro histórico de participación de un usuario en una sala. Persiste en la colección `lobbys`.

| Propiedad     | Tipo       | Descripción                                  |
|---------------|------------|----------------------------------------------|
| `Id`          | `string`   | Identificador único del registro.            |
| `RoomCode`    | `string`   | Código de la sala en que participó.          |
| `UserId`      | `string`   | UID del usuario.                             |
| `CharacterId` | `string`   | Personaje usado en esa partida.              |
| `Won`         | `bool`     | Si el jugador ganó esa partida.              |
| `Timestamp`   | `DateTime` | Momento de participación.                    |

---

## Repositorio (`src/main/cs/repository`)

### `FirebaseConnection.cs`
Provee acceso singleton a las instancias de `IFirestore` y `IAuth`. Gracias al sistema de plugins de MAUI, la inicialización es automática mediante `google-services.json` (Android) o `GoogleService-Info.plist` (iOS).

### `Database.cs`
Centraliza todas las operaciones CRUD contra Cloud Firestore. Expone métodos genéricos y métodos de dominio específicos.

| Método                             | Descripción                                                    |
|------------------------------------|----------------------------------------------------------------|
| `StoreAsync<T>(col, id, data)`     | Guarda o sobreescribe un documento en la colección indicada.   |
| `GetAsync<T>(col, id)`             | Recupera un documento por ID. Retorna `null` si no existe.     |
| `ListAsync<T>(col)`                | Lista todos los documentos de una colección.                   |
| `DeleteAsync(col, id)`             | Elimina un documento por ID.                                   |
| `SaveUser / GetUser / UpdateUser`  | CRUD específico para la colección `users`.                     |
| `SaveGame / GetAvailableGames`     | CRUD específico para la colección `games`.                     |
| `CreateParty / GetParty`           | CRUD específico para la colección `parties`.                   |
| `CheckLobbyExistsAsync(code)`      | Verifica si existe una sala con el código dado.                |
| `JoinLobbyAsync(userId, code)`     | Registra la entrada del usuario a una sala y actualiza historial. |
| `GetUserHistory(userId)`           | Recupera el historial de lobbies de un usuario.               |
| `InitializeCharactersAsync()`      | Siembra los personajes iniciales si la colección está vacía.   |

> Todos los métodos lanzan `DatabaseException` ante errores de Firestore.

---

## Servicios (`src/main/cs/service`)

### `BaseService.cs`
Clase abstracta que provee un `HttpClient` preconfigurado para llamadas a la API REST del servidor. Las subclases heredan `PostAsync<T>` y `SetAuthToken`.

### `AccountService.cs`
Gestiona el ciclo de vida de la sesión del usuario.

| Método                  | Descripción                                                                 |
|-------------------------|-----------------------------------------------------------------------------|
| `IsSessionActiveAsync()`| Comprueba si hay una sesión guardada en `SecureStorage` aún válida.         |
| `Login(email, pass)`    | Autentica contra Firebase, carga el perfil de Firestore y guarda la sesión. |
| `RegisterAsync(...)`    | Crea cuenta en Firebase Auth, guarda perfil en Firestore e inicia sesión.   |
| `UpdateUser(user)`      | Actualiza datos del usuario en Firestore y en la caché en memoria.           |
| `UpdateEmail(newEmail)` | Cambia el email en Firebase Auth.                                           |
| `ChangePassword(pass)`  | Cambia la contraseña en Firebase Auth.                                      |
| `Logout()`              | Cierra sesión en Firebase y limpia datos persistidos.                       |
| `GetCurrentUser()`      | Devuelve el `User` en memoria o `null` si no hay sesión.                    |

La sesión se almacena en `SecureStorage` (UID) y `Preferences` (fecha de expiración a 30 días).

### `GameService.cs`
Lógica de negocio relacionada con las salas de juego.

| Método                          | Descripción                                                     |
|---------------------------------|-----------------------------------------------------------------|
| `GetAvailableGames()`           | Obtiene los juegos disponibles desde Firestore.                 |
| `ValidateLobbyAsync(roomCode)`  | Verifica si existe una sala con el código indicado.             |

### `ControllerService.cs`
Serializa y envía los eventos del mando al servidor Unity via TCP/WebSocket.

| Método                           | Descripción                                                   |
|----------------------------------|---------------------------------------------------------------|
| `SendJoystickMove(x, y)`         | Envía posición del joystick (valores normalizados [-1, 1]).   |
| `SendButtonPress(buttonId)`      | Envía evento de pulsación de botón.                          |
| `SendSensorData(type, value)`    | Envía lectura de sensor (acelerómetro, micrófono, etc.).      |

### `BluetoothService.cs`
Gestiona el escaneo y conexión de dispositivos BLE cercanos (alternativa de conexión al servidor Unity).

### `IOrientationService.cs`
Interfaz para gestionar la orientación de pantalla. Tiene implementación nativa en Android (`Platforms/Android/OrientationService.cs`).

---

## Controladores (`src/main/cs/controller`)

### `AccountController.cs`
Intermediario entre `LoginPage`/`RegisterPage` y `AccountService`. Manejadesfeedback visual (alertas, navegación) que no es responsabilidad del servicio.

### `ControllerController.cs`
Intermediario entre `ControllerPage` y `ControllerService`. Delega los eventos del joystick y botones al servicio.

### `ConexionController.cs`
Gestiona el flujo de conexión TCP/WebSocket al servidor Unity usando las preferencias de IP y puerto configuradas en `SettingsPage`.

### `GameController.cs`
Intermediario entre las páginas de selección y `GameService`. Expone métodos `RecoverAvailableGames` e `IsLobbyValid`.

---

## Vistas (`src/main/cs/view`)

| Página               | Descripción                                                                              |
|----------------------|------------------------------------------------------------------------------------------|
| `IntroPage`          | Pantalla de bienvenida animada. Verifica sesión activa y redirige automáticamente.       |
| `LoginPage`          | Formulario de email y contraseña. Redirige a Main si hay sesión guardada.                |
| `RegisterPage`       | Formulario de registro con validación de campos y confirmación de contraseña.            |
| `MainMenuPage`       | Hub principal: muestra usuario, carrusel de personajes, accesos a mando e historial.     |
| `SelectionPage`      | Selección de juego y método de conexión (Wi-Fi, Bluetooth o QR).                        |
| `JoinRoomPage`       | Introducción del código de sala con validación real contra Firestore.                    |
| `LobbyPage`          | Sala de espera: muestra personaje seleccionado y botón de "Listo" para iniciar.          |
| `ControllerPage`     | Mando virtual con joystick y botones. Soporta modo giroscópico y vibra con feedback.    |
| `HistoryPage`        | Lista cronológica inversa del historial de partidas del usuario.                         |
| `SettingsPage`       | Configuración de perfil (nombre, email, contraseña) y parámetros de red (IP, puerto).   |

---

## Jerarquía de Excepciones (`src/main/cs/exception`)

```
ChibitsLinkException  ← Base de todas las excepciones de dominio
    └── DatabaseException  ← Errores de Firestore (CRUD fallido, sin conexión)
            └── RecordNotFoundException  ← Documento no encontrado en la colección
```

Cada excepción tiene un archivo propio (`ChibitsLinkException.cs`, `DatabaseException.cs`, `RecordNotFoundException.cs`).

---

## Converters (`src/main/cs/converters`)

### `BooleanToTextConverter.cs`
Convierte un valor booleano en texto XAML:
- `true` → `"VICTORIA"`
- `false` → `"DERROTA"`

Utilizado en `HistoryPage.xaml` para mostrar el resultado de cada partida en el historial.

---

## Red (`src/main/cs/net`)

### `Connection.cs`
Gestiona la conexión con el servidor Unity. Soporta múltiples protocolos:
- **TCP**: conexión directa por IP y puerto.
- **WebSocket**: conexión mediante URL `ws://`.
- **Bluetooth**: conexión mediante dispositivo BLE.

Expone el evento `LatencyUpdated` para que `ControllerPage` muestre el ping en tiempo real.

---

## Inyección de Dependencias (`MauiProgram.cs`)

Todos los servicios, repositorios y páginas están registrados en el contenedor de DI de MAUI:

- **Singleton**: `FirebaseConnection`, `Database`, `Connection`, `AccountService`, `BluetoothService`, `ControllerService`, `GameService`, todos los Controladores.
- **Transient**: todas las páginas (cada navegación crea una nueva instancia).

---

## Manejo de Errores

- Las excepciones de Firestore se capturan en `Database.cs` y se re-lanzan como `DatabaseException`.
- Los servicios capturan `DatabaseException` y la re-lanzan para que el controlador/vista decida cómo informar al usuario.
- Las vistas nunca reciben excepciones sin procesar; siempre muestran un `DisplayAlert` con mensaje amigable.
- Los errores inesperados se registran con `System.Diagnostics.Debug.WriteLine` para depuración en desarrollo.