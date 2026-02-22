# ChirbitsLink - Guía de Configuración Rápida (10 min)

## 🎮 Configuración en Unity (Lado del Juego)

### Paso 1: Importar Scripts
Copia los archivos de `dev/ChibitsLink/external/` a tu proyecto Unity:
- `TcpServer.cs`
- `PlayerManager.cs`
- `LobbyManager.cs`

### Paso 2: Configurar TcpServer
1. Crea un GameObject vacío llamado **"NetworkManager"**
2. Agrega el componente `TcpServer`
3. Arrastra el **PlayerManager** al campo `Player Manager`
4. **Configuración:**
   - `Port`: 11000 (por defecto)

### Paso 3: Configurar PlayerManager
1. En el mismo GameObject o crea uno nuevo **"PlayerManager"**
2. Agrega el componente `PlayerManager`
3. **Spawn Points:** Crea 4 Empty GameObjects (posiciones P1, P2, P3, P4) y arrástralos a la lista
4. **Character Prefabs:** Arrastra tus prefabs de personajes y configura:
   - `Character Id`: "VALIENTE", "MAGA", "PICARO", "BRUJO"
   - `Prefab`: Tu GameObject de personaje

### Paso 4: Configuración de Red
```
IP del servidor: 192.168.x.x  (tu IP local)
Puerto: 11000
```

---

## 📱 Configuración de la App Móvil

### Datos requeridos en Firebase
Colección: `parties`
```
json
{
  "id": "party_123",
  "name": "Mi Sala",
  "roomCode": "ABC123",
  "playerIds": [1, 2, 3, 4],
  "maxPlayers": 4
}
```

Colección: `users`
```
json
{
  "id": "user_123",
  "username": "Jugador1",
  "selectedCharacterId": "VALIENTE"
}
```

### Configuración de Conexión
En la app, el usuario debe configurar:
- **IP del servidor:** IP de la PC donde corre Unity
- **Puerto:** 11000

---

## 🔄 Protocolo de Comunicación

### Mensajes de la App → Juego
| Comando | Formato | Descripción |
|---------|---------|-------------|
| UNIRSE | `SYNC_CHAR\|roomCode\|userId\|charId` | Unirse con personaje |
| SYNC | `SYNC_CHAR\|roomCode\|userId\|nuevoCharId` | Cambiar personaje |
| CHECK | `CHECK_ROOM\|roomCode` | Verificar sala |
| READY | `PLAYER_READY\|roomCode\|userId` | Jugador listo |

### Respuestas del Juego → App
| Respuesta | Descripción |
|-----------|-------------|
| `OK\|JOIN\|playerNumber` | Unión exitosa (1-4) |
| `ERROR\|FULL` | Sala llena |
| `ERROR\|NOT_FOUND` | Sala no existe |
| `ERROR\|INVALID` | Datos inválidos |

---

## ⚡ Configuración en 10 Minutos

### Minuto 1-2: Copiar archivos
```
bash
cp dev/ChibitsLink/external/*.cs TU_PROYECTO_UNITY/Assets/Scripts/Network/
```

### Minuto 3-4: Setup TcpServer
- Crear GameObject "NetworkManager"
- Agregar TcpServer.cs
- Asignar PlayerManager

### Minuto 5-6: Setup PlayerManager
- Crear 4 spawn points en la escena
- Configurar prefabs de personajes
- Asignar en inspector

### Minuto 7-8: Firebase
- Crear colección "parties"
- Configurar reglas de Firestore

### Minuto 9-10: Prueba
- Ejecutar Unity
- Conectar app al mismo WiFi
- Probar conexión

---

## 🔧 Solución de Problemas

### "No puedo conectar"
1. Verifica que estén en la misma red WiFi
2. Desactiva el firewall de Windows o permite el puerto 11000
3. Verifica la IP en la app (ipconfig en CMD)

### "La sala no existe"
1. Verifica que la colección "parties" existe en Firebase
2. El código debe ser de 6 caracteres

### "Máximo de jugadores"
- El límite es 4 jugadores por sala
- Espera a que alguien abandone

---

## 📋 Lista de Verificación Rápida

- [ ] Scripts copiados a Unity
- [ ] TcpServer configurado en escena
- [ ] PlayerManager con 4 spawn points
- [ ] 4 prefabs de personajes asignados
- [ ] Firebase con colección "parties"
- [ ] Misma red WiFi
- [ ] Puerto 11000 abierto
