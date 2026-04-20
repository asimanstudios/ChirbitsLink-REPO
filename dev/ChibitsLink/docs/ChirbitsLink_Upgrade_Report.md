# 🏰 Reporte de Actualización: Chirbits Link

Este documento detalla todas las mejoras, correcciones y procedimientos aplicados para estabilizar el sistema de salas, el historial de batallas y la progresión de niveles.

## 🚀 Resumen de Cambios

### 1. Sistema de Salas y Navegación
- **Flujo de Retorno Automatizado**: Se ha sincronizado Unity y MAUI para que, al acabar un minijuego, la App vuelva automáticamente a la zona de "Marcar como Listo".
- **Solución al Bug "000000"**: Ahora la App preserva el `RoomCode` durante toda la sesión. Al volver del juego, la App ya no muestra ceros, sino el código real de la sala.
- **Estado "CLOSED" vs Deletción**: Las salas ya no se borran de Firestore al cerrar el juego; se marcan como `CLOSED`. Esto permite que el registro persista para el historial.

### 2. Historial de Batallas (Relatos de Batalla)
- **Unificación de Modelos**: Se ha creado el modelo `PartyHistory` para estandarizar la terminología.
- **Carga de Datos Real**: Se ha corregido la consulta en la App para que cargue las participaciones desde la colección correcta, solucionando el error de "No se pudo cargar el historial".
- **Feedback Visual**: Se ha añadido un indicador de carga (`ActivityIndicator`) para que el usuario sepa que los datos se están recuperando.

### 3. Sistema de Experiencia y Niveles
- **Dificultad Incremental**: Se ha implementado una curva de niveles más realista y desafiante:
  - **Nivel 2**: 10,000 XP.
  - **Nivel 3**: 25,000 XP (incremento de +15k).
  - **Nivel 4**: 45,000 XP (incremento de +20k).
  - **Puntos de Sesión**: Los puntos ganados en cada minijuego se suman correctamente al XP total del usuario al finalizar.

---

## 🛠️ Soluciones a Problemas Detectados

| Problema | Solución Aplicada |
| :--- | :--- |
| **Error `WhereNotEqualsTo`** | El plugin de Firestore en MAUI no soportaba este operador. Se cambió por una búsqueda simple y filtrado en memoria C#. |
| **Error `QueryProperty`** | Se movió el atributo del campo a la declaración de la clase, cumpliendo con la sintaxis de .NET MAUI. |
| **Keystore no Encontrado** | Se generó un nuevo almacén de claves (`chirbits.keystore`) para permitir la firma manual del APK sin dependencias externas. |
| **Código de Sala 000000** | Se implementó el paso de parámetros por QueryProperty en las rutas de navegación de Shell. |

---

## 💻 Lista de Comandos Finales

Usa este flujo cada vez que quieras desplegar una nueva versión de la App:

### 1. Compilación
```powershell
dotnet publish -f net8.0-android -c Release -p:EmbedAssembliesIntoApk=true -p:AndroidSdkDirectory="C:\Users\adris\AppData\Local\Android\Sdk" -r android-arm64 --no-self-contained
```

### 2. Alineación (Optimization)
```powershell
& "C:\Users\adris\AppData\Local\Android\Sdk\build-tools\34.0.0\zipalign.exe" -f -v 4 bin/Release/net8.0-android/android-arm64/publish/com.companyname.chibitslink-Signed.apk ChirbitsLink_Final.apk
```

### 3. Firma (Utilizando chirbits.keystore)
```powershell
& "C:\Users\adris\AppData\Local\Android\Sdk\build-tools\34.0.0\apksigner.bat" sign --ks chirbits.keystore --ks-pass pass:chirbits --out ChirbitsLink_Final.apk ChirbitsLink_Final.apk
```

### 4. Instalación
```powershell
& "C:\Users\adris\AppData\Local\Android\Sdk\platform-tools\adb.exe" install -r ChirbitsLink_Final.apk
```

---

> [!TIP]
> **Recordatorio**: Para que los cambios de historial y niveles surtan efecto completo, asegúrate de que tanto Unity como la App instalada tengan aplicadas estas últimas versiones del código.

---
**¡Todo el sistema está ahora sincronizado y listo para la acción!** ⚔️🔝
