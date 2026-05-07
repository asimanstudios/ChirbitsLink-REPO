using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace ChibiCocina.Editor
{
    /// <summary>
    /// Ventana de editor para inspección y análisis de scripts.
    /// Proporciona información detallada sobre todos los scripts del proyecto.
    /// </summary>
    /// <remarks>
    /// Muestra categorías, componentes requeridos y variables públicas.
    /// Facilita la identificación visual de scripts y su propósito.
    /// Útil para documentación y organización del proyecto.
    /// </remarks>
    public class ScriptInspector : EditorWindow
    {
        /// <summary>
        /// Muestra la ventana del inspector de scripts.
        /// Accesible desde el menú de Unity.
        /// </summary>
        [MenuItem("ChibiCocina/Herramientas/Inspector de Scripts")]
        public static void ShowWindow()
        {
            GetWindow<ScriptInspector>("Inspector de Scripts");
        }
        
        /// <summary>Posición del scroll</summary>
        private Vector2 scrollPosition;
        /// <summary>Información de scripts cargada</summary>
        private Dictionary<string, ScriptInfo> scriptInfos = new Dictionary<string, ScriptInfo>();
        
        /// <summary>
        /// Clase que contiene información sobre un script.
        /// Almacena metadatos para análisis y categorización.
        /// </summary>
        private class ScriptInfo
        {
            /// <summary>Nombre del script</summary>
            public string Name;
            /// <summary>Ruta del archivo</summary>
            public string Path;
            /// <summary>Descripción del script</summary>
            public string Description;
            /// <summary>Componentes requeridos</summary>
            public string[] RequiredComponents;
            /// <summary>Variables públicas</summary>
            public string[] PublicVariables;
            /// <summary>Categoría del script</summary>
            public string Category;
            /// <summary>Es un gestor</summary>
            public bool IsManager;
            /// <summary>Es de red</summary>
            public bool IsNetwork;
            /// <summary>Es de depuración</summary>
            public bool IsDebug;
        }
        
        /// <summary>
        /// Se ejecuta al habilitar la ventana.
        /// Carga la información de todos los scripts.
        /// </summary>
        private void OnEnable()
        {
            LoadScriptInformation();
        }
        
        /// <summary>
        /// Dibuja la interfaz de usuario de la ventana.
        /// Muestra filtros y lista de scripts.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("🔍 Inspector de Scripts - Identificación Visual", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Filtros
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filtrar por:", GUILayout.Width(80));
            
            bool filterAll = GUILayout.Toggle(true, "Todos");
            bool filterPlayer = GUILayout.Toggle(false, "Player");
            bool filterNetwork = GUILayout.Toggle(false, "Red");
            bool filterDebug = GUILayout.Toggle(false, "Debug");
            bool filterManager = GUILayout.Toggle(false, "Manager");
            
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            
            // Búsqueda
            GUILayout.BeginHorizontal();
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            string searchText = EditorGUILayout.TextField("", GUILayout.Width(200));
            if (GUILayout.Button("🔍 Buscar", GUILayout.Width(80)))
            {
                SearchScripts(searchText);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Mostrar scripts
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var scriptsToShow = scriptInfos.Values.Where(s => 
                string.IsNullOrEmpty(searchText) || 
                s.Name.ToLower().Contains(searchText.ToLower()) ||
                s.Description.ToLower().Contains(searchText.ToLower())
            ).ToList();
            
            foreach (var script in scriptsToShow)
            {
                DrawScriptCard(script);
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(10);
            if (GUILayout.Button("📋 Generar Reporte"))
            {
                GenerateReport();
            }
        }
        
        private void DrawScriptCard(ScriptInfo script)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Header con color según categoría
            Color originalColor = GUI.backgroundColor;
            
            if (script.IsNetwork)
                GUI.backgroundColor = Color.cyan;
            else if (script.IsDebug)
                GUI.backgroundColor = Color.yellow;
            else if (script.IsManager)
                GUI.backgroundColor = Color.green;
            else
                GUI.backgroundColor = Color.gray;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"📄 {script.Name}", EditorStyles.boldLabel);
            GUILayout.Label(script.Category, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            
            GUI.backgroundColor = originalColor;
            
            // Path
            EditorGUILayout.LabelField("Ruta:", script.Path, EditorStyles.helpBox);
            
            // Descripción
            EditorGUILayout.LabelField("Función:", script.Description);
            
            // Componentes requeridos
            if (script.RequiredComponents.Length > 0)
            {
                EditorGUILayout.LabelField("Componentes Requeridos:");
                EditorGUI.indentLevel++;
                foreach (var component in script.RequiredComponents)
                {
                    EditorGUILayout.LabelField($"• {component}");
                }
                EditorGUI.indentLevel--;
            }
            
            // Variables públicas clave
            if (script.PublicVariables.Length > 0)
            {
                EditorGUILayout.LabelField("Variables Públicas Clave:");
                EditorGUI.indentLevel++;
                foreach (var variable in script.PublicVariables)
                {
                    EditorGUILayout.LabelField($"• {variable}");
                }
                EditorGUI.indentLevel--;
            }
            
            // Botones de acción
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("📍 Ir al Script", GUILayout.Width(100)))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(script.Path);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            
            if (GUILayout.Button("📋 Copiar Nombre", GUILayout.Width(100)))
            {
                GUIUtility.systemCopyBuffer = script.Name;
                EditorUtility.DisplayDialog("Copiado", $"Nombre '{script.Name}' copiado al portapapeles", "OK");
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        private void LoadScriptInformation()
        {
            scriptInfos.Clear();
            
            // Scripts de Player
            AddScriptInfo("Movimiento", "Assets/Scripts/Controllers/Player/Movimiento.cs",
                "Control de movimiento, salto y física del personaje",
                new[] { "CharacterController", "AudioSource" },
                new[] { "walkSpeed", "runSpeed", "jumpForce", "acceleration" },
                "Player", false, false, false);
                
            AddScriptInfo("PlayerController", "Assets/Scripts/Controllers/Player/PlayerController.cs",
                "Input de teclado/joystick e interacción del jugador",
                new[] { "Rigidbody", "Collider" },
                new[] { "speed", "jumpForce", "groundMask", "groundCheckDistance" },
                "Player", false, false, false);
            
            // Scripts de Red
            AddScriptInfo("TcpServer", "Assets/Scripts/Services/Network/TcpServer.cs",
                "Servidor TCP para conexiones de dispositivos móviles",
                new[] { "MonoBehaviour" },
                new[] { "port", "lobbyUI", "_isRunning" },
                "Red", false, true, false);
                
            AddScriptInfo("NetworkService", "Assets/Scripts/Services/Network/NetworkService.cs",
                "Gestión de conexiones de red y mensajes",
                new[] { "MonoBehaviour" },
                new[] { "Port", "MaxConnections", "ConnectionTimeout" },
                "Red", false, true, false);
            
            // Scripts de Debug
            AddScriptInfo("DebugManager", "Assets/Scripts/Core/Systems/DebugManager.cs",
                "Panel de debug, creación de bots, carga de escenas",
                new[] { "MonoBehaviour" },
                new[] { "isDebugModeActive", "sceneToLoad", "numberOfBotsToAdd" },
                "Debug", false, false, true);
                
            AddScriptInfo("BotService", "Assets/Scripts/Services/Debug/BotService.cs",
                "Creación y gestión de bots para testing",
                new[] { "MonoBehaviour" },
                new[] { "maxBots", "numberOfBotsToAdd", "defaultCharacterIds" },
                "Debug", false, false, true);
            
            // Scripts de Managers
            AddScriptInfo("PlayerManager", "Assets/Scripts/Core/Managers/PlayerManager.cs",
                "Gestión de jugadores en el sistema multijugador",
                new[] { "MonoBehaviour" },
                new[] { "_connectionOrder", "_spawnPoints" },
                "Manager", true, false, false);
                
            AddScriptInfo("LobbyManager", "Assets/Scripts/Core/Managers/LobbyManager.cs",
                "Gestión de lobby y salas de juego",
                new[] { "MonoBehaviour" },
                new[] { "_currentRoomCode", "_sessionScores" },
                "Manager", true, false, false);
            
            // Scripts de Gameplay
            AddScriptInfo("Moneda", "Assets/Scripts/Controllers/Gameplay/Moneda.cs",
                "Comportamiento de monedas coleccionables",
                new[] { "Collider", "Rigidbody" },
                new[] { "valor", "tipoMoneda" },
                "Gameplay", false, false, false);
                
            AddScriptInfo("BombaTag", "Assets/Scripts/Controllers/Gameplay/BombaTag.cs",
                "Comportamiento de bombas para minijuegos",
                new[] { "Collider", "Rigidbody" },
                new[] { "tiempoExplosion", "radioExplosion" },
                "Gameplay", false, false, false);
        }
        
        private void AddScriptInfo(string name, string path, string description, 
            string[] requiredComponents, string[] publicVariables, 
            string category, bool isManager, bool isNetwork, bool isDebug)
        {
            scriptInfos[name] = new ScriptInfo
            {
                Name = name,
                Path = path,
                Description = description,
                RequiredComponents = requiredComponents ?? new string[0],
                PublicVariables = publicVariables ?? new string[0],
                Category = category,
                IsManager = isManager,
                IsNetwork = isNetwork,
                IsDebug = isDebug
            };
        }
        
        private void SearchScripts(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                LoadScriptInformation();
                return;
            }
            
            // La búsqueda se hace automáticamente en OnGUI
        }
        
        private void GenerateReport()
        {
            string report = "# 📋 Reporte de Scripts\n\n";
            report += "## Scripts por Categoría\n\n";
            
            var categories = scriptInfos.Values.GroupBy(s => s.Category);
            
            foreach (var category in categories)
            {
                report += $"### {category.Key}\n\n";
                
                foreach (var script in category)
                {
                    report += $"**{script.Name}**\n";
                    report += $"- Ruta: `{script.Path}`\n";
                    report += $"- Función: {script.Description}\n";
                    
                    if (script.RequiredComponents.Length > 0)
                    {
                        report += $"- Componentes: {string.Join(", ", script.RequiredComponents)}\n";
                    }
                    
                    report += "\n";
                }
                
                report += "\n";
            }
            
            string reportPath = "Assets/Scripts/SCRIPT_REPORT.md";
            System.IO.File.WriteAllText(reportPath, report);
            
            EditorUtility.DisplayDialog(
                "Reporte Generado",
                $"Reporte guardado en: {reportPath}\n\nÁbrelo para ver la lista completa de scripts.",
                "OK"
            );
            
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(reportPath);
        }
    }
}
