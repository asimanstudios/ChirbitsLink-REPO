# Diagramas - ChibitsLink

Documentación visual de la arquitectura, flujos y estructura de datos de la aplicación.

---

## 1. Diagrama de Clases

Muestra las entidades principales y sus relaciones.

```mermaid
classDiagram
    direction TB

    class User {
        +string Id
        +string RealName
        +string Username
        +string SelectedCharacterId
        +int Level
        +List~string~ GameHistory
    }

    class Character {
        +string Id
        +string Name
        +string ImageUrl
        +string Description
    }

    class Game {
        +int Id
        +string Name
        +GameType Type
        +string Description
    }

    class Party {
        +string Id
        +string Name
        +string RoomCode
        +List~int~ PlayerIds
    }

    class PartyProgress {
        +string Id
        +string PartyId
        +int? WinnerId
        +Dictionary~int,int~ PlayerScores
        +DateTime CompletedAt
    }

    class LobbyHistory {
        +string Id
        +string RoomCode
        +string UserId
        +string CharacterId
        +bool Won
        +DateTime Timestamp
    }

    class GameType {
        <<enumeration>>
        Soccer
        Jump
        Accelerometer
        Kitchen
    }

    User --> Character : selectedCharacter
    User --> LobbyHistory : historial
    Party --> PartyProgress : progreso
    Party --> User : jugadores
    Game --> GameType : tipo
```

---

## 2. Diagrama Entidad-Relación (Firestore)

Representa las colecciones de Cloud Firestore y sus relaciones lógicas.

```mermaid
erDiagram
    USERS {
        string id PK
        string realName
        string username
        string selectedCharacterId FK
        int level
        list~string~ gameHistory
    }

    PERSONAJES {
        string id PK
        string name
        string imageUrl
        string description
    }

    GAMES {
        int id PK
        string name
        string type
        string description
    }

    PARTIES {
        string id PK
        string name
        string roomCode
        list~int~ playerIds
    }

    PARTY_PROGRESS {
        string id PK
        string partyId FK
        int winnerId FK
        map playerScores
        datetime completedAt
    }

    LOBBYS {
        string id PK
        string roomCode FK
        string userId FK
        string characterId FK
        bool won
        datetime timestamp
    }

    USERS ||--o{ LOBBYS : "participa en"
    USERS }o--|| PERSONAJES : "selecciona"
    PARTIES ||--|| PARTY_PROGRESS : "tiene progreso"
    PARTIES }o--o{ USERS : "contiene jugadores"
    LOBBYS }o--|| PARTIES : "pertenece a"
```

---

## 3. Diagrama de Flujo: Inicio de Sesión

```mermaid
graph TD
    A([Inicio]) --> B[Introduce email y contraseña]
    B --> C{¿Campos vacíos?}
    C -- Sí --> D[Mostrar alerta de campos vacíos]
    D --> B
    C -- No --> E[Llamar AccountService.Login]
    E --> F{¿Sesión activa guardada?}
    F -- Sí --> G[Cargar usuario de Firestore]
    G --> H[Navegar a MainMenuPage]
    F -- No --> I[Autenticar en Firebase Auth]
    I --> J{¿Éxito?}
    J -- No --> K[Mostrar error de credenciales]
    K --> B
    J -- Sí --> L[Guardar sesión en SecureStorage]
    L --> H
```

---

## 4. Diagrama de Flujo: Unirse a una Sala

```mermaid
graph TD
    A([Inicio]) --> B[Introducir código de 6 dígitos]
    B --> C{¿Código válido?}
    C -- No --> D[Mostrar error de formato]
    D --> B
    C -- Sí --> E[Llamar GameService.ValidateLobbyAsync]
    E --> F[Consultar colección 'parties' en Firestore]
    F --> G{¿Sala encontrada?}
    G -- No --> H[Mostrar 'Sala no existe']
    H --> B
    G -- Sí --> I[Navegar a LobbyPage con código]
    I --> J[Seleccionar personaje y marcar Listo]
    J --> K[Navegar a ControllerPage]
    K --> L([Fin])
```

---

## 5. Diagrama de Secuencia: Flujo de Registro

```mermaid
sequenceDiagram
    actor Usuario
    participant RegisterPage
    participant AccountController
    participant AccountService
    participant FirebaseAuth
    participant Database

    Usuario->>RegisterPage: Rellena formulario y pulsa Registrar
    RegisterPage->>AccountController: Register(nombre, usuario, email, pass)
    AccountController->>AccountService: RegisterAsync(...)
    AccountService->>FirebaseAuth: CreateUserWithEmailAndPasswordAsync(email, pass)
    FirebaseAuth-->>AccountService: UserCredential
    AccountService->>Database: SaveUser(newUser)
    Database-->>AccountService: OK
    AccountService->>Database: InitializeCharactersAsync()
    Database-->>AccountService: OK
    AccountService->>AccountService: SaveSession(uid)
    AccountService-->>AccountController: (Success: true, null)
    AccountController-->>RegisterPage: Navegar a MainMenuPage
    RegisterPage-->>Usuario: Pantalla principal
```

---

## 6. Diagrama de Secuencia: Flujo del Mando

```mermaid
sequenceDiagram
    actor Jugador
    participant ControllerPage
    participant ControllerController
    participant ControllerService
    participant Connection
    participant UnityServer

    Jugador->>ControllerPage: Mueve Joystick
    ControllerPage->>ControllerController: HandleJoystickMoved(x, y)
    ControllerController->>ControllerService: SendJoystickMove(x, y)
    ControllerService->>ControllerService: Serializar JSON {type, x, y, userId}
    ControllerService->>Connection: SendMessageAsync(json)
    Connection->>UnityServer: TCP/WebSocket frame
    UnityServer-->>Connection: ACK / Latency ping
    Connection-->>ControllerPage: LatencyUpdated (ms)
    ControllerPage-->>Jugador: Actualiza etiqueta Ping
```

---

## 7. Resumen de Casos de Uso

```mermaid
graph LR
    Jugador((Usuario))

    subgraph ChibitsLink
        UC1(["UC-01: Registrarse"])
        UC2(["UC-02: Iniciar Sesión"])
        UC3(["UC-03: Escoger Personaje"])
        UC4(["UC-04: Unirse a Sala"])
        UC5(["UC-05: Controlar Juego"])
        UC6(["UC-06: Consultar Historial"])
        UC7(["UC-07: Configurar Perfil"])
    end

    Jugador -.usa.-> UC1
    Jugador -.usa.-> UC2
    Jugador -.usa.-> UC3
    Jugador -.usa.-> UC4
    Jugador -.usa.-> UC5
    Jugador -.usa.-> UC6
    Jugador -.usa.-> UC7
```

---

## UC-01: Registrarse

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-01: Registrarse"
        UC(["Registrarse"])
        INC1(["Introducir Datos"])
        INC2(["Crear Cuenta Firebase"])
        EXT1(["Escoger Personaje"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario crea una nueva cuenta en la plataforma. | |
| 1. Accede a RegisterPage | |
| 2. Introduce nombre real, nombre de usuario, email y contraseña | |
| 3. Confirma la contraseña | |
| 4. Pulsa Registrar | |
| 5. El sistema crea la cuenta en Firebase Auth | |
| 6. El sistema guarda el perfil en Firestore | |
| 7. Se inicia sesión automáticamente | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| El usuario no tiene cuenta | Cuenta creada en Firebase Auth |
| Conexión a internet activa | Perfil guardado en colección `users` |
| | Sesión activa durante 30 días |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Nombre real | Pantalla principal (MainMenuPage) |
| Nombre de usuario | |
| Email | |
| Contraseña | |
| **TABLAS** | **CLASES** |
| USERS | [RegisterPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/RegisterPage.xaml.cs) |
| | [AccountController.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/controller/AccountController.cs) |
| | [AccountService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/AccountService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |

---

## UC-02: Iniciar Sesión

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-02: Iniciar Sesión"
        UC(["Iniciar Sesión"])
        INC1(["Autenticar en Firebase"])
        INC2(["Comprobar Sesión Guardada"])
        EXT1(["Recuperar Perfil Firestore"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario accede a la aplicación con su cuenta. | |
| 1. Accede a LoginPage | |
| 2. El sistema comprueba si hay sesión guardada válida (30 días) | |
| 3. Si hay sesión, redirige directamente a MainMenuPage | |
| 4. Si no, el usuario introduce email y contraseña | |
| 5. Pulsa Iniciar Sesión | |
| 6. Firebase Auth valida las credenciales | |
| 7. Se carga el perfil desde Firestore | |
| 8. La sesión se guarda en SecureStorage | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| El usuario tiene cuenta registrada | Usuario autenticado en Firebase |
| Conexión a internet activa | Perfil cargado en memoria |
| | Sesión guardada durante 30 días |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Email | Pantalla principal (MainMenuPage) |
| Contraseña | Objeto `User` en memoria |
| **TABLAS** | **CLASES** |
| USERS | [LoginPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/LoginPage.xaml.cs) |
| | [AccountController.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/controller/AccountController.cs) |
| | [AccountService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/AccountService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |

---

## UC-03: Escoger Personaje

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-03: Escoger Personaje"
        UC(["Escoger Personaje"])
        INC1(["Ver Carrusel de Héroes"])
        INC2(["Guardar Selección"])
        EXT1(["Sincronizar con Unity"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario elige el héroe con el que participará en la partida. | |
| 1. Accede a MainMenuPage o LobbyPage | |
| 2. El sistema carga los personajes desde Firestore | |
| 3. Se muestra el carrusel de personajes disponibles | |
| 4. El usuario selecciona un personaje | |
| 5. La UI actualiza el avatar y el nombre del héroe | |
| 6. El sistema guarda `SelectedCharacterId` en Firestore | |
| 7. (Opcional) Si hay conexión TCP activa, se sincroniza con Unity | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| Usuario logado | `SelectedCharacterId` actualizado en Firestore |
| Personajes inicializados en Firestore | Personaje visible en LobbyPage |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Selección del personaje (Id) | `User.SelectedCharacterId` actualizado |
| | Confirmación de héroe elegido |
| **TABLAS** | **CLASES** |
| USERS | [MainMenuPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/MainMenuPage.xaml.cs) |
| PERSONAJES | [LobbyPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/LobbyPage.xaml.cs) |
| | [AccountService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/AccountService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |
| | [Connection.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/net/Connection.cs) |

---

## UC-04: Unirse a una Sala

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-04: Unirse a Sala"
        UC(["Unirse"])
        INC1(["Loguearse"])
        INC2(["Introducir Clave"])
        EXT1(["Cambiar Personaje"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario se une a una sala de juego existente mediante un código de 6 dígitos. | |
| 1. Pincha Unirse desde JoinRoomPage o SelectionPage | |
| 2. Introduce la clave de 6 dígitos | |
| 3. Pulsa Conectar | |
| 4. El sistema valida la clave contra Firestore | |
| 5. La clave es correcta: navega a LobbyPage | |
| 6. (Opcional) Cambio de personaje desde el lobby | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| Usuario logado | Conexión a Lobby establecida |
| Sala creada en Unity | Aparición del jugador en la partida |
| | Acceso a ControllerPage (mando virtual) |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Clave de sala (6 dígitos) | LobbyPage activa |
| NickName de usuario | |
| **TABLAS** | **CLASES** |
| USERS | [JoinRoomPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/JoinRoomPage.xaml.cs) |
| PARTIES | [SelectionPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/SelectionPage.xaml.cs) |
| | [GameService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/GameService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |
| | [Connection.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/net/Connection.cs) |

---

## UC-05: Controlar el Juego por Mando

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-05: Controlar Juego"
        UC(["Controlar Juego"])
        INC1(["Mover Joystick"])
        INC2(["Pulsar Botones"])
        EXT1(["Usar Giroscopio"])
        EXT2(["Ver Latencia"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
    UC -->|extends| EXT2
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario controla su personaje en Unity desde el mando virtual del móvil. | |
| 1. El usuario está en ControllerPage con conexión activa | |
| 2. Mueve el joystick virtual (PanGesture) | |
| 3. El sistema serializa y envía las coordenadas al servidor | |
| 4. Pulsa botones de acción (A, B, X, Y) | |
| 5. (Opcional) Activa el modo giroscópico con el acelerómetro | |
| 6. El servidor Unity procesa los inputs y devuelve ACK/ping | |
| 7. La UI actualiza la etiqueta de latencia en ms | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| Usuario en LobbyPage listo | Inputs enviados al servidor Unity |
| Conexión TCP/WebSocket activa | Personaje controlado en tiempo real |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Posición joystick (x, y) | Evento de movimiento en Unity |
| Id de botón pulsado | Latencia actual (ms) |
| Lectura del acelerómetro | |
| **TABLAS** | **CLASES** |
| — | [ControllerPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/ControllerPage.xaml.cs) |
| | [ControllerController.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/controller/ControllerController.cs) |
| | [ControllerService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/ControllerService.cs) |
| | [Connection.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/net/Connection.cs) |

---

## UC-06: Consultar Historial de Partidas

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-06: Consultar Historial"
        UC(["Consultar Historial"])
        INC1(["Cargar Partidas de Firestore"])
        INC2(["Mostrar Resultado"])
        EXT1(["Ver Detalle de Sala"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario consulta la lista de salas en las que ha participado y sus resultados. | |
| 1. Accede a HistoryPage desde el menú principal | |
| 2. El sistema comprueba la sesión activa | |
| 3. Consulta la colección `lobbys` filtrada por `UserId` | |
| 4. Ordena los registros por `Timestamp` descendente | |
| 5. Muestra la lista con código de sala y resultado (VICTORIA / DERROTA) | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| Usuario logado | Lista de partidas visible en HistoryPage |
| Al menos una partida jugada | |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Id del usuario activo | Lista de `LobbyHistory` |
| | Resultado por partida (VICTORIA/DERROTA) |
| **TABLAS** | **CLASES** |
| LOBBYS | [HistoryPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/HistoryPage.xaml.cs) |
| USERS | [AccountService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/AccountService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |
| | [BooleanToTextConverter.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/converters/BooleanConverters.cs) |

---

## UC-07: Configurar Perfil

```mermaid
graph LR
    Actor((Usuario))

    subgraph "UC-07: Configurar Perfil"
        UC(["Configurar Perfil"])
        INC1(["Editar Datos Básicos"])
        INC2(["Guardar Preferencias de Red"])
        EXT1(["Cambiar Email"])
        EXT2(["Cambiar Contraseña"])
    end

    Actor -.usa.-> UC
    UC -->|includes| INC1
    UC -->|includes| INC2
    UC -->|extends| EXT1
    UC -->|extends| EXT2
```

| | |
|---|---|
| **DESCRIPCIÓN**: El usuario actualiza su información de perfil y los parámetros de red del servidor. | |
| 1. Accede a SettingsPage | |
| 2. El sistema carga datos actuales del usuario y preferencias IP/puerto | |
| 3. El usuario edita nombre real, nombre de usuario, email o contraseña | |
| 4. (Opcional) Modifica IP y puerto del servidor Unity | |
| 5. Pulsa Guardar | |
| 6. El sistema actualiza el perfil en Firestore | |
| 7. (Opcional) Actualiza el email en Firebase Auth | |
| 8. (Opcional) Cambia la contraseña en Firebase Auth | |
| 9. Guarda preferencias de red en `Preferences` del dispositivo | |
| **PRECONDICIONES** | **POSTCONDICIONES** |
| Usuario logado | Datos de perfil actualizados en Firestore |
| | Email/contraseña actualizados en Firebase Auth |
| | Preferencias de red guardadas en dispositivo |
| **DATOS ENTRADA** | **DATOS SALIDA** |
| Nombre real (opcional) | Perfil actualizado |
| Nombre de usuario (opcional) | Cabecera del Shell actualizada |
| Nuevo email (opcional) | |
| Nueva contraseña (opcional) | |
| IP del servidor (opcional) | |
| Puerto del servidor (opcional) | |
| **TABLAS** | **CLASES** |
| USERS | [SettingsPage.xaml.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/view/SettingsPage.xaml.cs) |
| | [AccountService.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/service/AccountService.cs) |
| | [Database.cs](file:///c:/Users/adris/RiderProjects/ChirbitsLink/ChibitsLink/src/main/cs/repository/Database.cs) |