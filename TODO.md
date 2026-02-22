# Plan de Mejoras - Sistema de Conexión App-Juego Unity

## Información Recopilada

### Estructura Actual del Proyecto:

**Lado Unity (Game):**
- `external/TcpServer.cs` - Servidor TCP que acepta conexiones de la App, maneja SYNC_CHAR
- `external/PlayerManager.cs` - Gestiona jugadores, hace spawn según orden de conexión (P1, P2...)
- `external/LobbyManager.cs` - Crea lobbys con códigos, almacena en Firebase

**Lado App (MAUI):**
- `view/JoinRoomPage.xaml.cs` - Ingresa código de sala pero simula conexión (sin validación real)
- `view/LobbyPage.xaml.cs` - Muestra lobby, selección de personaje, botón listo
- `net/Connection.cs` - Manejo de conexión TCP/WebSocket/Bluetooth
- `service/ControllerService.cs` - Envía input de joystick/botones al juego
- `repository/Database.cs` - Operaciones Firebase incluyendo CheckLobbyExistsAsync

---

## Problemas Identificados:

1. **JoinRoomPage no valida el código de sala** - Solo simula conexión con Task.Delay
2. **No hay verificación de conexión TCP** - No se intenta conectar al juego
3. **No hay límite de 4 jugadores** - No se controla el máximo de usuarios en lobby
4. **El sync de personaje** - Puede no funcionar correctamente entre app y juego
5. **Historial y puntos** - Sistema básico, necesita expansión

---

## Plan de Implementación

### Fase 1: Validación de Conexión en JoinRoom

- [ ] **1.1** Modificar `JoinRoomPage.xaml.cs` para validar código contra Firebase
- [ ] **1.2** Agregar método en `Database.cs` para verificar si la sala acepta más jugadores
- [ ] **1.3** Intentar conexión TCP real al juego y mostrar error si falla
- [ ] **1.4** Mostrar mensajes de error apropiados: "Código incorrecto", "Sin conexión al juego", "Sala llena"

### Fase 2: Sistema de Lobby con Límite de 4 Jugadores

- [ ] **2.1** Modificar `Party` model para incluir `MaxPlayers = 4` y `CurrentPlayers`
- [ ] **2.2** Actualizar `LobbyManager.cs` en Unity para validar límite de jugadores
- [ ] **2.3** Sincronizar conteo de jugadores en Firebase en tiempo real

### Fase 3: Sincronización de Personaje

- [ ] **3.1** Mejorar `TcpServer.cs` para manejar mensajes de cambio de personaje
- [ ] **3.2** Asegurar que `PlayerManager.HandleCharacterSync` funcione correctamente
- [ ] **3.3** Enviar notificación de cambio de personaje desde la app

### Fase 4: Sistema de Historial y Puntos

- [ ] **4.1** Crear modelo `PartyProgress` con puntuación de cada jugador
- [ ] **4.2** Actualizar `Database.cs` para guardar progreso de partida
- [ ] **4.3** Agregar método para registrar fin de partida y actualizar nivel de usuario
- [ ] **4.4** Mejorar historial con filtros por fecha/juego

### Fase 5: Mejoras de UX

- [ ] **5.1** Agregar indicador de conexión en tiempo real
- [ ] **5.2** Mostrar jugadores conectados en la sala
- [ ] **5.3** Validar que todos los jugadores estén listos antes de iniciar

---

## Archivos a Modificar:

1. `dev/ChibitsLink/src/main/cs/view/JoinRoomPage.xaml.cs` - Validación de conexión
2. `dev/ChibitsLink/src/main/cs/repository/Database.cs` - Métodos de verificación de sala
3. `dev/ChibitsLink/src/main/cs/model/Party.cs` - Agregar MaxPlayers, CurrentPlayers
4. `dev/ChibitsLink/external/LobbyManager.cs` - Validar límite de jugadores
5. `dev/ChibitsLink/external/TcpServer.cs` - Mejorar manejo de mensajes
6. `dev/ChibitsLink/external/PlayerManager.cs` - Sincronización de personajes
7. `dev/ChibitsLink/src/main/cs/model/PartyProgress.cs` - Nuevo modelo para puntuación

---

## Pendiente:
- Necesito confirmar si hay más archivos en Unity relacionados con la validación de salas
- Revisar cómo se maneja la conexión TCP desde la app hacia el juego
