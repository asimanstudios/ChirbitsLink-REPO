# Documentación de Scripts - ChirbitsLink

## Tabla de Contenidos
- [Dependencias Externas](#dependencias-externas)
- [Controllers](#controllers)
- [Core](#core)
- [Services](#services)
- [Minigames](#minigames)
- [Editor](#editor)
- [Models](#models)
- [Views](#views)
- [Repositories](#repositories)
- [Utils](#utils)

---

## Dependencias Externas

### Firebase
- **Documentación**: [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)
- **Paquetes**: `Firebase`, `Firebase.Firestore`, `Firebase.Extensions`
- **Uso**: Base de datos en tiempo real para almacenamiento de partidas, usuarios y estadísticas

### Unity Netcode for GameObjects
- **Documentación**: [Unity Netcode Documentation](https://docs-multiplayer.unity3d.com/)
- **Paquete**: `Unity.Netcode`
- **Uso**: Sistema de networking multijugador sincronizado

---

## Controllers

### LegacyPlayerController
```csharp
/// <summary>
/// Controlador legacy para el movimiento del jugador con soporte para input móvil y teclado.
/// Maneja física básica de movimiento, salto e interacciones con objetos del mundo.
/// </summary>
public class LegacyPlayerController : MonoBehaviour, PlayerManager.IChibitsController
```

#### Métodos Principales:
- **ProcessJoystick(float x, float y)**
  - **Entrada**: Coordenadas x, y del joystick (-1 a 1)
  - **Salida**: void
  - **Función**: Procesa input de joystick móvil para movimiento del jugador
  - **Influencia**: Actualiza el vector de movimiento del PlayerModel

- **ProcessButton(string buttonId, string state)**
  - **Entrada**: ID del botón y estado ("pressed"/"released")
  - **Salida**: void
  - **Función**: Maneja eventos de botones móviles (salto, interacción)
  - **Influencia**: Activa acciones específicas según el botón presionado

- **TryInteract()**
  - **Entrada**: Ninguna
  - **Salida**: void
  - **Función**: Realiza raycast hacia adelante para detectar objetos interactivos
  - **Influencia**: Ejecuta el método Interact() de objetos que implementen IInteractable

---

### PlayerAnimationController
```csharp
/// <summary>
/// Controlador de animaciones del jugador basado en su estado de movimiento y acciones.
/// Sincroniza animaciones con el modelo del jugador y eventos de juego.
/// </summary>
```

### PlayerAudioController
```csharp
/// <summary>
/// Controlador de efectos de audio del jugador.
/// Reproduce sonidos de pasos, salto, interacción y otros eventos del jugador.
/// </summary>
```

### PlayerCombatController
```csharp
/// <summary>
/// Controlador de acciones de combate del jugador.
/// Maneja ataques, defensa y habilidades especiales durante minijuegos.
/// </summary>
```

---

## Core

### GameManager
```csharp
/// <summary>
/// Gestor principal del ciclo de vida del juego.
/// Controla estados de juego, temporización y gestión de sesiones de jugadores.
/// </summary>
public class GameManager : MonoBehaviour
```

#### Métodos Principales:
- **StartGame()**
  - **Entrada**: Ninguna
  - **Salida**: void
  - **Función**: Inicia la transición de Waiting a Preparing y luego a Playing
  - **Influencia**: Cambia el estado global del juego y activa temporizadores

- **EndGame()**
  - **Entrada**: Ninguna
  - **Salida**: void
  - **Función**: Finaliza la partida actual y pasa a estado Finished
  - **Influencia**: Detiene temporizadores y procesa resultados

- **CanStartGame()**
  - **Entrada**: Ninguna
  - **Salida**: bool
  - **Función**: Verifica si se cumplen las condiciones para iniciar partida
  - **Influencia**: Utilizado por UI para habilitar/deshabilitar botón de inicio

#### Estados del Juego:
- **Waiting**: Esperando jugadores
- **Preparing**: Tiempo de preparación
- **Playing**: Juego activo
- **Finished**: Juego terminado

### GestorDePantallaDividida
```csharp
/// <summary>
/// Gestor de configuración de pantalla dividida para multijugador local.
/// Ajusta cámaras y viewport según la cantidad de jugadores conectados.
/// </summary>
```

#### Métodos Principales:
- **ConfigurarPantalla(int cantidadJugadores)**
  - **Entrada**: Número de jugadores (1-4)
  - **Salida**: void
  - **Función**: Configura la disposición de cámaras para pantalla dividida
  - **Influencia**: Modifica el viewport de todas las cámaras de jugadores

---

## Services

### FirebaseManager
```csharp
/// <summary>
/// Gestor de conexión y operaciones con Firebase Firestore.
/// Maneja persistencia de datos de usuarios, partidas y estadísticas en la nube.
/// </summary>
/// <seealso href="https://firebase.google.com/docs/firestore">Firebase Firestore Documentation</seealso>
public class FirebaseManager : MonoBehaviour
```

#### Métodos Principales:
- **UpdateGameSession(string host, int players, int points)**
  - **Entrada**: Host (string), Players (int), Points (int)
  - **Salida**: void (async)
  - **Función**: Actualiza los datos de una sesión de juego activa
  - **Influencia**: Modifica documentos en Firebase Firestore

- **FinalizePartyScoresAsync(string roomCode, Dictionary<string, int> finalScores)**
  - **Entrada**: Código de sala y diccionario de puntuaciones finales
  - **Salida**: Task
  - **Función**: Procesa y guarda las puntuaciones finales de una partida
  - **Influencia**: Actualiza estadísticas de usuarios e historial de partidas

#### Repositories Accesibles:
- **PartyRepository**: Gestión de partidos y salas
- **UserRepository**: Datos de usuarios y estadísticas
- **SessionRepository**: Sesiones de juego activas

### NetworkConnectionManager
```csharp
/// <summary>
/// Gestor de conexión de red utilizando Unity Netcode.
/// Proporciona interfaz GUI para iniciar como Host, Client o Server.
/// </summary>
/// <seealso href="https://docs-multiplayer.unity3d.com/">Unity Netcode Documentation</seealso>
public class NetworkManager : MonoBehaviour
```

#### Métodos Principales:
- **OnGUI()**
  - **Entrada**: Ninguna (método Unity)
  - **Salida**: void
  - **Función**: Muestra interfaz para conexión de red y estado actual
  - **Influencia**: Permite al usuario iniciar/detener conexiones de red

---

## Minigames

### BombTagGameManager
```csharp
/// <summary>
/// Gestor específico del minijuego BombTag.
/// Controla la lógica del juego de etiquetas con bombas y puntuación.
/// </summary>
```

### BombTagPhysics
```csharp
/// <summary>
/// Sistema de física para el minijuego BombTag.
/// Maneja colisiones, explosiones y efectos físicos de las bombas.
/// </summary>
```

### BombTagScoring
```csharp
/// <summary>
/// Sistema de puntuación para BombTag.
/// Calcula y gestiona los puntos de jugadores durante el minijuego.
/// </summary>
```

---

## Editor

### AutoScriptReassigner
```csharp
/// <summary>
/// Herramienta de editor para reasignar automáticamente scripts a GameObjects.
/// Utiliza reflection y análisis de componentes para restaurar referencias perdidas.
/// </summary>
```

### ConfiguradorProyecto
```csharp
/// <summary>
/// Configurador de proyecto para establecer settings iniciales del proyecto.
/// Configura layers, tags y configuraciones de compilación.
/// </summary>
```

---

## Models

### PlayerModel
```csharp
/// <summary>
/// Modelo de datos que representa el estado del jugador.
/// Almacena input de movimiento, estado físico y variables de juego.
/// </summary>
```

### MovementModel
```csharp
/// <summary>
/// Modelo específico para cálculos de movimiento del jugador.
/// Contiene vectores, velocidades y parámetros físicos.
/// </summary>
```

---

## Views

### CameraController
```csharp
/// <summary>
/// Controlador de cámara que sigue al jugador.
/// Implementa seguimiento suave con configuración de distancia y ángulo.
/// </summary>
```

### BombTagUI
```csharp
/// <summary>
/// Interfaz de usuario para el minijuego BombTag.
/// Muestra temporizador, puntuaciones y estado del juego.
/// </summary>
```

---

## Repositories

### UserRepository
```csharp
/// <summary>
/// Repositorio para operaciones CRUD de usuarios en Firebase.
/// Maneja perfiles, estadísticas e historial de partidas.
/// </summary>
```

### PartyRepository
```csharp
/// <summary>
/// Repositorio para gestión de partidos y salas.
/// Crea, actualiza y consulta datos de partidas multijugador.
/// </summary>
```

---

## Temas y Comentarios de Documentación

### Estilo de Comentarios

#### Comentarios de Clase
```csharp
/// <summary>
/// Breve descripción de la clase en una línea.
/// Descripción más detallada del propósito y responsabilidades de la clase.
/// Incluye información sobre patrones de diseño utilizados y dependencias clave.
/// </summary>
/// <remarks>
/// Notas adicionales sobre implementación, consideraciones de rendimiento
/// o detalles importantes sobre el ciclo de vida del objeto.
/// </remarks>
/// <seealso href="URL_RELEVANTE">Documentación Externa</seealso>
public class NombreClase : MonoBehaviour
```

#### Comentarios de Método
```csharp
/// <summary>
/// Descripción concisa de lo que hace el método.
/// </summary>
/// <param name="parametro1">Descripción del primer parámetro y su propósito</param>
/// <param name="parametro2">Descripción del segundo parámetro</param>
/// <returns>Descripción del valor retornado y su significado</returns>
/// <exception cref="System.Exception">Cuándo se lanza esta excepción</exception>
/// <example>
/// Ejemplo de uso:
/// <code>
/// var resultado = NombreMetodo(valor1, valor2);
/// </code>
/// </example>
public TipoRetorno NombreMetodo(TipoParam1 parametro1, TipoParam2 parametro2)
```

#### Comentarios de Propiedad
```csharp
/// <summary>
/// Descripción de la propiedad y su propósito en el sistema.
/// </summary>
/// <value>Descripción del valor que representa</value>
public TipoPropiedad NombrePropiedad { get; set; }
```

### Temas de Documentación por Categoría

#### 1. Controllers
- **Propósito**: Control de flujo y coordinación entre componentes
- **Patrones**: MVP, MVC, o Controller específico del dominio
- **Comentarios clave**: Eventos manejados, dependencias inyectadas, ciclo de vida

#### 2. Services
- **Propósito**: Lógica de negocio y acceso a recursos externos
- **Patrones**: Singleton, Factory, Repository
- **Comentarios clave**: Configuración, manejo de errores, asíncronía

#### 3. Models
- **Propósito**: Estructura de datos y estado de la aplicación
- **Patrones**: Data Transfer Object, Value Object
- **Comentarios clave**: Validación, serialización, inmutabilidad

#### 4. Views/UI
- **Propósito**: Presentación de datos y interacción del usuario
- **Patrones**: Observer, Command
- **Comentarios clave**: Eventos de UI, binding de datos, accesibilidad

### Estándares de Calidad

#### Nivel 1: Básico
- `<summary>` para todas las clases públicas
- `<param>` y `<returns>` para métodos públicos
- Sin comentarios internos

#### Nivel 2: Estándar
- Todo lo del Nivel 1
- `<remarks>` para detalles de implementación
- `<exception>` para excepciones conocidas
- Comentarios inline para lógica compleja

#### Nivel 3: Completo
- Todo lo del Nivel 2
- `<example>` para métodos complejos
- `<seealso>` para documentación relacionada
- Comentarios de rendimiento y consideraciones de threading

### Palabras Clave para Comentarios

#### Verbos de Acción
- **Procesa**: Transforma datos de entrada a salida
- **Valida**: Verifica condiciones o restricciones
- **Inicializa**: Prepara el estado inicial
- **Actualiza**: Modifica estado existente
- **Calcula**: Realiza operaciones matemáticas o lógicas
- **Notifica**: Dispara eventos o comunica cambios

#### Sustantivos de Estado
- **Estado**: Condición actual del objeto
- **Configuración**: Parámetros ajustables
- **Recurso**: Elemento externo utilizado
- **Dependencia**: Componente requerido
- **Resultado**: Salida de una operación

---

## Convenciones de Documentación

### Etiquetas XML Utilizadas:
- `<summary>`: Descripción general de la clase/método
- `<param>`: Descripción de parámetros de entrada
- `<returns>`: Descripción del valor de retorno
- `<seealso>`: Enlaces a documentación externa
- `<remarks>`: Información adicional y notas de implementación

### Formato de Métodos:
```
- **NombreMetodo(tipo parametro)**
  - **Entrada**: Descripción de parámetros
  - **Salida**: Tipo y descripción de retorno
  - **Función**: Qué hace el método
  - **Influencia**: Cómo afecta al sistema
```

---

## Notas de Implementación

1. **Singleton Pattern**: La mayoría de managers usan patrón singleton con `Instance`
2. **Event-Driven**: Sistema basado en eventos para comunicación entre componentes
3. **Async/Await**: Operaciones de Firebase usan programación asíncrona
4. **Unity Lifecycle**: Métodos siguen convenciones de Unity (Awake, Update, FixedUpdate)

## Enlaces Útiles

- [Unity Documentation](https://docs.unity3d.com/)
- [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup)
- [Unity Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
- [C# XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
