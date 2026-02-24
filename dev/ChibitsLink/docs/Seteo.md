# Guía de Configuración - ChibitsLink

Guía completa para poner en marcha el ecosistema ChibitsLink: **Firebase**, **App MAUI** y **Servidor Unity**.

---

## 1. Requisitos Previos

| Herramienta | Versión mínima | Notas |
|---|---|---|
| .NET SDK | 9.0 | `dotnet --version` |
| MAUI Workload | incluída en .NET 9 | `dotnet workload install maui` |
| JetBrains Rider / Visual Studio | 2024+ | Con soporte .NET MAUI |
| Unity | 2022.3 LTS o superior | Con módulo Android Build |
| Android SDK | API 21+ | Emulador o dispositivo físico |
| Cuenta Firebase | — | [console.firebase.google.com](https://console.firebase.google.com/) |

---

## 2. Configuración de Firebase

### 2.1 Crear el Proyecto

1. Accede a [Firebase Console](https://console.firebase.google.com/) y haz clic en **Agregar Proyecto**.
2. Dale un nombre (p. ej. `chibitslink-prod`).
3. Habilita **Google Analytics** si lo deseas (opcional).

### 2.2 Activar Servicios

| Servicio | Ruta en Firebase Console | Configuración |
|---|---|---|
| **Authentication** | Build → Authentication → Get started | Habilita el proveedor **Email/Contraseña** |
| **Cloud Firestore** | Build → Firestore Database → Create database | Modo **Producción** → elige una región cercana |

### 2.3 Reglas de Firestore recomendadas

```firestore-rules
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Perfil de usuario: solo el propio usuario puede leer/escribir
    match /users/{userId} {
      allow read, write: if request.auth.uid == userId;
    }
    // Salas (Parties): lectura para todos los registrados, escritura solo Unity
    match /parties/{code} {
      allow read: if request.auth != null;
      allow write: if false; 
    }
    // Historial de Salas: el usuario puede ver su historial
    match /lobbys/{id} {
      allow read: if request.auth != null && (resource == null || resource.data.UserId == request.auth.uid);
      allow write: if request.auth != null;
    }
    // Personajes: Lectura pública para usuarios autenticados
    match /personajes/{id} {
      allow read: if request.auth != null;
      allow write: if false;
    }
  }
}
```


### 2.4 Descargar Archivos de Configuración

1. En Firebase Console → **Configuración del proyecto** → **Tus apps** → añade app **Android**.
2. Usa el paquete `com.companyname.chibitslink`.
3. Descarga `google-services.json`.
4. Colócalo en:

```
ChibitsLink/
  Platforms/
    Android/
      google-services.json   ← aquí
```

---

## 3. Inicialización de Datos en Firestore

La primera vez que un usuario se registra, la app llama automáticamente a `Database.InitializeCharactersAsync()`.
Esto crea la colección `personajes` con los personajes base si está vacía.

**IDs de personaje definidos en la app** (deben coincidir exactamente con los de Unity):

| Id en Firestore | Nombre visible |
|---|---|
| `VALIENTE` | Valiente |
| `MAGA` | Maga |
| `PICARO` | Pícaro |

> [!IMPORTANT]
> Si añades personajes nuevos en Unity, añade el mismo `characterId` en `Database.InitializeCharactersAsync()` para que la app los reconozca.

---

## 4. Configuración de la Aplicación MAUI

### 4.1 Abrir y Restaurar

```powershell
# En la carpeta del proyecto
dotnet restore
```

O usa Rider / Visual Studio → **Restore NuGet Packages**.

### 4.2 Dependencias Clave (ya en ChibitsLink.csproj)

| Paquete | Función |
|---|---|
| `Plugin.CloudFirestore` | Acceso a Firestore |
| `Plugin.FirebaseAuth` | Autenticación Firebase |
| `Plugin.BLE` | Conexión Bluetooth |
| `Newtonsoft.Json` | Serialización JSON de inputs del mando |

### 4.3 Configurar IP del Servidor Unity en la App

Desde **SettingsPage** dentro de la app, el usuario puede cambiar la IP y el puerto del servidor Unity en tiempo real.
Los valores se guardan en las preferencias del dispositivo con estas claves:

| Clave (`Preferences`) | Valor por defecto |
|---|---|
| `pref_server_ip` | `127.0.0.1` |
| `pref_server_port` | `11000` |

> [!TIP]
> Para pruebas en red local, usa la **IP local** del PC que ejecuta Unity (p. ej. `192.168.1.XX`), no `localhost`.

### 4.4 Ejecutar en Android

1. Conecta un dispositivo físico con **Depuración USB** activa, o lanza un emulador Android (API 21+).
2. Selecciona el dispositivo en el desplegable de Rider y pulsa **Ejecutar**.

---

## 5. Integración con Unity — Scripts Externos

Los scripts de la carpeta `external/` deben copiarse a tu proyecto Unity para habilitar la comunicación con la app.

### 5.1 Estructura de Scripts

```
external/
  TcpServer.cs-l      →  CopyTo: Assets/Scripts/Network/TcpServer.cs
  PlayerManager.cs-l  →  CopyTo: Assets/Scripts/Network/PlayerManager.cs
  LobbyManager.cs-l   →  CopyTo: Assets/Scripts/Network/LobbyManager.cs
```

> Renombra los archivos quitando el sufijo `.cs-l` al copiarlos.

---

### 5.2 TcpServer.cs — Servidor TCP de Unity

**Responsabilidad:** Escucha al puerto `11000`, acepta clientes (la app ChibitsLink), y además delega mensajes al `PlayerManager`.

**Configuración en el Inspector de Unity:**

| Campo | Valor |
|---|---|
| `port` | `11000` (cambiar si hay conflicto) |
| `playerManager` | Arrastra el GameObject que tenga `PlayerManager` |

**Protocolo de mensajes recibidos desde la App:**

| Tipo de mensaje | Formato | Acción |
|---|---|---|
| Sincronizar personaje | `SYNC_CHAR\|{userId}\|{charId}` | Llama a `PlayerManager.HandlePlayerJoin` |
| Input del mando | `{"type":"joystick","x":0.5,"y":-0.3,"userId":"..."}` | Llama a `PlayerManager.HandleControllerInput` |

**Cómo añadir a la escena:**
1. Crea un **GameObject vacío** → nómbralo `NetworkManager`.
2. Añade el componente `TcpServer`.
3. Añade el componente `PlayerManager` al mismo GameObject.
4. En el Inspector de `TcpServer`, arrastra `NetworkManager` al campo `Player Manager`.

---

### 5.3 PlayerManager.cs — Gestión de Jugadores

**Responsabilidad:** Spawnea, actualiza y destruye los prefabs de personajes en función de los jugadores conectados.

**Configuración en el Inspector de Unity:**

| Campo | Descripción |
|---|---|
| `Spawn Points` | Lista de `Transform` — posición de aparición de P1, P2, P3… |
| `Character Prefabs` | Lista de `CharacterPrefabMap` — mapeo de `characterId` → Prefab |

**CharacterPrefabMap — ejemplo de relleno:**

| `characterId` | `prefab` |
|---|---|
| `VALIENTE` | Arrastra el prefab del Valiente |
| `MAGA` | Arrastra el prefab de la Maga |
| `PICARO` | Arrastra el prefab del Pícaro |

> [!IMPORTANT]
> Los `characterId` deben ser exactamente iguales a los definidos en Firestore (sensible a mayúsculas).

**Para conectar el input al movimiento real del personaje**, edita `HandleControllerInput`:

```csharp
public void HandleControllerInput(string userId, string inputData)
{
    if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
    {
        // Parsea el JSON y mueve el personaje
        var input = JsonUtility.FromJson<ControllerInput>(inputData);
        playerObj.GetComponent<PlayerController>().ProcessInput(input);
    }
}

[Serializable]
public class ControllerInput
{
    public string type;   // "joystick" o "button"
    public float x;
    public float y;
    public string buttonId;
    public string userId;
}
```

---

### 5.4 LobbyManager.cs — Gestión de Salas vía Firestore

**Responsabilidad:** Crea y cierra las salas en la colección `parties` de Firestore, que la App valida al unirse.

**Dependencia:** Requiere el **SDK de Firebase para Unity** instalado.

**Instalación del SDK Firebase en Unity:**
1. Descarga [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).
2. Importa el paquete `FirebaseFirestore.unitypackage` vía **Assets → Import Package → Custom Package**.
3. Coloca el archivo `google-services.json` también en `Assets/` del proyecto Unity.

**Uso típico en la escena del juego:**

```csharp
// En el script de inicio del nivel:
var firestore = FirebaseFirestore.DefaultInstance;
var lobbyManager = new LobbyManager(firestore);

// Al iniciar partida → crea sala y obtén el código para mostrarlo en pantalla
Party lobby = await lobbyManager.CreateNewLobbyAsync("Partida Épica");
Debug.Log($"Código de sala: {lobby.RoomCode}"); // Muéstralo en la UI

// Al terminar la partida → elimina la sala
await lobbyManager.CloseLobbyAsync(lobby.RoomCode);
```

---

## 6. Flujo de Conexión Completo

```
USUARIO                    APP MAUI                   UNITY SERVER
   |                          |                             |
   |--- Introduce código ----> |                             |
   |                          |-- ValidateLobbyAsync ------> Firestore
   |                          |<- Sala existe -------------- Firestore
   |                          |                             |
   |--- Pulsa "Listo" -------> |                             |
   |                          |-- ConnectTcpAsync(IP:11000) -> TcpServer.AcceptClient
   |                          |-- SYNC_CHAR|userId|charId --> PlayerManager.HandlePlayerJoin
   |                          |                             |--- Spawn prefab del personaje
   |                          |                             |
   |--- Mueve joystick ------> |                             |
   |                          |-- {"type":"joystick",...} --> PlayerManager.HandleControllerInput
   |                          |                             |--- Mueve el personaje en Unity
   |                          |<-- ACK/ping ---------------- TcpServer
   |<-- Latencia actualizada---|                             |
```

---

## 7. Checklist de Verificación

- [ ] `google-services.json` en `Platforms/Android/`
- [ ] `google-services.json` en `Assets/` del proyecto Unity
- [ ] Firebase Authentication → proveedor Email/Contraseña habilitado
- [ ] Firestore creado en modo Producción con las reglas del punto 2.3
- [ ] `TcpServer.cs`, `PlayerManager.cs` y `LobbyManager.cs` importados en Unity
- [ ] `NetworkManager` con `TcpServer` + `PlayerManager` en la escena Unity
- [ ] Prefabs de personajes asignados en `PlayerManager` con IDs correctos
- [ ] IP del PC con Unity configurada en SettingsPage de la app (o `127.0.0.1` si es el mismo dispositivo)
- [ ] Puerto `11000` abierto (o modificado en `TcpServer.port` e igual en `pref_server_port`)
- [ ] `InitializeCharactersAsync()` ejecutado al menos una vez (ocurre en el primer registro)