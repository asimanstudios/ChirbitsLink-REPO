using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ChibiCocina.Editor
{
    /// <summary>
    /// Herramienta de Unity Editor para auto-reasignar scripts faltantes.
    /// Detecta GameObjects con scripts perdidos y sugiere reemplazos automáticos.
    /// Facilita la recuperación de escenas con referencias rotas.
    /// </summary>
    /// <remarks>
    /// Utiliza análisis de nombres y componentes para determinar scripts necesarios.
    /// Implementa sistema de confianza para evitar asignaciones incorrectas.
    /// </remarks>
    public class AutoScriptReassigner : EditorWindow
    {
        /// <summary>
        /// Muestra la ventana de auto-reasignación de scripts.
        /// Agregada al menú de Unity bajo ChibiCocina/Herramientas.
        /// </summary>
        [MenuItem("ChibiCocina/Herramientas/Auto-Reasignar Scripts")]
        public static void ShowWindow()
        {
            GetWindow<AutoScriptReassigner>("Auto-Reasignar Scripts");
        }
        
        /// <summary>Posición de scroll para la vista detallada</summary>
        private Vector2 scrollPosition;
        /// <summary>Mapa de GameObjects con scripts faltantes</summary>
        private Dictionary<GameObject, List<ScriptMatch>> missingScriptsMap = new Dictionary<GameObject, List<ScriptMatch>>();
        /// <summary>Indica si se ha realizado el escaneo</summary>
        private bool hasScanned = false;
        /// <summary>Total de scripts corregidos</summary>
        private int totalFixed = 0;
        /// <summary>Control de visibilidad de detalles</summary>
        private bool ShowDetails = true;
        
        /// <summary>
        /// Representa una posible coincidencia de script para un GameObject.
        /// Contiene información sobre el script sugerido y nivel de confianza.
        /// </summary>
        private class ScriptMatch
        {
            /// <summary>Nombre del script sugerido</summary>
            public string ScriptName;
            /// <summary>Ruta del archivo del script</summary>
            public string ScriptPath;
            /// <summary>Descripción del propósito del script</summary>
            public string Description;
            /// <summary>Nivel de confianza de la coincidencia (0-1)</summary>
            public float Confidence;
            /// <summary>Indica si puede ser corregido automáticamente</summary>
            public bool IsAutoFixable;
            /// <summary>Razón de la sugerencia</summary>
            public string Reason;
        }
        
        /// <summary>
        /// Define patrones de mapeo para detección automática de scripts.
        /// Utilizado para configurar reglas de coincidencia basadas en nombres.
        /// </summary>
        private struct GameObjectMapping
        {
            /// <summary>Patrón de nombre a buscar</summary>
            public string Pattern;
            /// <summary>Scripts requeridos para este patrón</summary>
            public string[] RequiredScripts;
            /// <summary>Scripts a excluir para este patrón</summary>
            public string[] ExcludedScripts;
            /// <summary>Componentes requeridos para este patrón</summary>
            public string[] RequiredComponents;
        }
        
        /// <summary>
        /// Dibuja la interfaz de usuario de la ventana.
        /// Muestra controles y resultados del análisis de scripts.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("🔧 Auto-Reasignador de Scripts", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.BeginVertical("box");
            GUILayout.Label("Esta herramienta detecta automáticamente qué scripts necesita cada GameObject y los reasigna.", EditorStyles.helpBox);
            GUILayout.Space(5);
            GUILayout.Label("⚠️ Haz backup de tu escena antes de usar esta herramienta", EditorStyles.boldLabel);
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🔍 Escanear GameObjects con Scripts Faltantes", GUILayout.Height(30)))
            {
                ScanForMissingScripts();
            }
            
            if (hasScanned)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Se encontraron {missingScriptsMap.Count} GameObjects con scripts faltantes", EditorStyles.helpBox);
                
                if (missingScriptsMap.Count > 0)
                {
                    GUILayout.Space(10);
                    
                    // Botones de acción
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("🔧 Auto-Reasignar Todo", GUILayout.Height(30)))
                    {
                        AutoReassignAll();
                    }
                    
                    if (GUILayout.Button("📋 Ver Detalles", GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        ShowDetails = !ShowDetails;
                    }
                    GUILayout.EndHorizontal();
                    
                    if (totalFixed > 0)
                    {
                        GUILayout.Label($"✅ {totalFixed} scripts reasignados correctamente", EditorStyles.helpBox);
                    }
                    
                    if (ShowDetails)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("Detalles por GameObject:", EditorStyles.boldLabel);
                        
                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                        
                        foreach (var kvp in missingScriptsMap)
                        {
                            DrawGameObjectCard(kvp.Key, kvp.Value);
                        }
                        
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
        }
        
        private bool ShowDetails = true;
        
        /// <summary>
        /// Escanea todos los GameObjects en busca de scripts faltantes.
        /// Analiza componentes nulos y genera mapa de problemas encontrados.
        /// </summary>
        private void ScanForMissingScripts()
        {
            missingScriptsMap.Clear();
            totalFixed = 0;
            hasScanned = true;
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                var missingScripts = FindMissingScriptsForGameObject(obj);
                if (missingScripts.Count > 0)
                {
                    missingScriptsMap[obj] = missingScripts;
                }
            }
            
            EditorUtility.DisplayDialog(
                "Escaneo Completado", 
                $"Se encontraron {missingScriptsMap.Count} GameObjects con scripts faltantes.\n\nUsa 'Auto-Reasignar Todo' para corregirlos automáticamente.",
                "OK"
            );
        }
        
        /// <summary>
        /// Busca scripts faltantes para un GameObject específico.
        /// Analiza componentes nulos y genera lista de coincidencias.
        /// </summary>
        /// <param name="obj">GameObject a analizar</param>
        /// <returns>Lista de scripts faltantes con sus coincidencias</returns>
        private List<ScriptMatch> FindMissingScriptsForGameObject(GameObject obj)
        {
            var matches = new List<ScriptMatch>();
            Component[] components = obj.GetComponents<Component>();
            
            foreach (Component component in components)
            {
                if (component == null)
                {
                    var possibleScripts = DetectRequiredScripts(obj);
                    matches.AddRange(possibleScripts);
                }
            }
            
            return matches;
        }
        
        /// <summary>
        /// Detecta scripts requeridos basándose en el nombre y componentes del GameObject.
        /// Utiliza heurísticas para determinar qué scripts podrían faltar.
        /// </summary>
        /// <param name="obj">GameObject a analizar</param>
        /// <returns>Lista de coincidencias de scripts ordenadas por confianza</returns>
        private List<ScriptMatch> DetectRequiredScripts(GameObject obj)
        {
            var matches = new List<ScriptMatch>();
            string objName = obj.name.ToLower();
            
            // Detectar por nombre del GameObject
            if (objName.Contains("player") || objName.Contains("jugador") || objName.Contains("personaje"))
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "Movimiento",
                    ScriptPath = "Assets/Scripts/Controllers/Player/Movimiento.cs",
                    Description = "Control de movimiento y física del personaje",
                    Confidence = 0.9f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'player' o 'jugador'"
                });
                
                matches.Add(new ScriptMatch
                {
                    ScriptName = "PlayerController",
                    ScriptPath = "Assets/Scripts/Controllers/Player/PlayerController.cs",
                    Description = "Input de teclado/joystick del jugador",
                    Confidence = 0.9f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'player' o 'jugador'"
                });
            }
            
            if (objName.Contains("manager") || objName.Contains("gestor"))
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "DebugManager",
                    ScriptPath = "Assets/Scripts/Core/Systems/DebugManager.cs",
                    Description = "Panel de debug y herramientas de desarrollo",
                    Confidence = 0.8f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'manager' o 'gestor'"
                });
                
                matches.Add(new ScriptMatch
                {
                    ScriptName = "TcpServer",
                    ScriptPath = "Assets/Scripts/Services/Network/TcpServer.cs",
                    Description = "Servidor TCP para conexiones móviles",
                    Confidence = 0.7f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'manager' - probablemente necesita red"
                });
            }
            
            if (objName.Contains("moneda") || objName.Contains("coin") || objName.Contains("dinero"))
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "Moneda",
                    ScriptPath = "Assets/Scripts/Controllers/Gameplay/Moneda.cs",
                    Description = "Comportamiento de moneda coleccionable",
                    Confidence = 0.95f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'moneda' o 'coin'"
                });
            }
            
            if (objName.Contains("bomba") || objName.Contains("tag") || objName.Contains("explos"))
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "BombaTag",
                    ScriptPath = "Assets/Scripts/Controllers/Gameplay/BombaTag.cs",
                    Description = "Comportamiento de bomba para minijuegos",
                    Confidence = 0.95f,
                    IsAutoFixable = true,
                    Reason = "GameObject contiene 'bomba' o 'tag'"
                });
            }
            
            // Detectar por componentes existentes
            if (obj.GetComponent<CharacterController>() != null)
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "Movimiento",
                    ScriptPath = "Assets/Scripts/Controllers/Player/Movimiento.cs",
                    Description = "Control de movimiento (requiere CharacterController)",
                    Confidence = 0.85f,
                    IsAutoFixable = true,
                    Reason = "GameObject tiene CharacterController"
                });
            }
            
            if (obj.GetComponent<Rigidbody>() != null && !obj.GetComponent<CharacterController>())
            {
                matches.Add(new ScriptMatch
                {
                    ScriptName = "PlayerController",
                    ScriptPath = "Assets/Scripts/Controllers/Player/PlayerController.cs",
                    Description = "Control de jugador (usa Rigidbody)",
                    Confidence = 0.8f,
                    IsAutoFixable = true,
                    Reason = "GameObject tiene Rigidbody pero no CharacterController"
                });
            }
            
            return matches.OrderByDescending(m => m.Confidence).ToList();
        }
        
        /// <summary>
        /// Dibuja una tarjeta para un GameObject específico con sus scripts faltantes.
        /// Muestra información detallada y opciones de corrección.
        /// </summary>
        /// <param name="obj">GameObject a mostrar</param>
        /// <param name="matches">Lista de coincidencias de scripts</param>
        private void DrawGameObjectCard(GameObject obj, List<ScriptMatch> matches)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label($"🎯 {obj.name}", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Seleccionar", GUILayout.Width(80)))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            GUILayout.EndHorizontal();
            
            // Componentes actuales
            var components = obj.GetComponents<Component>().Where(c => c != null).ToArray();
            GUILayout.Label($"Componentes: {string.Join(", ", components.Select(c => c.GetType().Name))}");
            
            // Scripts faltantes
            GUILayout.Space(5);
            GUILayout.Label("Scripts Faltantes:", EditorStyles.boldLabel);
            
            foreach (var match in matches)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Indicador de confianza
                Color originalColor = GUI.backgroundColor;
                if (match.Confidence >= 0.9f)
                    GUI.backgroundColor = Color.green;
                else if (match.Confidence >= 0.7f)
                    GUI.backgroundColor = Color.yellow;
                else
                    GUI.backgroundColor = Color.red;
                
                GUILayout.Label($"• {match.ScriptName}", GUILayout.Width(120));
                
                GUI.backgroundColor = originalColor;
                
                GUILayout.Label(match.Description, EditorStyles.miniLabel);
                
                if (GUILayout.Button("🔧", GUILayout.Width(30)))
                {
                    AddScriptToGameObject(obj, match);
                }
                
                GUILayout.EndHorizontal();
                
                EditorGUI.indentLevel++;
                GUILayout.Label($"📍 {match.ScriptPath}", EditorStyles.miniLabel);
                GUILayout.Label($"💡 {match.Reason} (Confianza: {(match.Confidence * 100):0}%)", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        /// <summary>
        /// Auto-reasigna todos los scripts con alta confianza.
        /// Procesa todos los GameObjects y aplica correcciones automáticas.
        /// </summary>
        private void AutoReassignAll()
        {
            int fixedCount = 0;
            int errorCount = 0;
            GameObject obj;
            List<ScriptMatch> loopMatches;
            IEnumerable<ScriptMatch> highConfidenceMatches;
            
            foreach (var kvp in missingScriptsMap)
            {
                obj = kvp.Key;
                loopMatches = kvp.Value;
                
                // Solo auto-reasignar scripts con alta confianza
                highConfidenceMatches = loopMatches.Where(m => m.Confidence >= 0.8f && m.IsAutoFixable);
                
                foreach (var match in highConfidenceMatches)
                {
                    if (AddScriptToGameObject(obj, match))
                    {
                        fixedCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }
            
            // Actualizar el escaneo
            ScanForMissingScripts();
            
            totalFixed += fixedCount;
            
            EditorUtility.DisplayDialog(
                "Auto-Reasignación Completada",
                $"✅ Scripts reasignados: {fixedCount}\n❌ Errores: {errorCount}\n\nSe recomienda revisar los GameObjects con baja confianza manualmente.",
                "OK"
            );
        }
        
        /// <summary>
        /// Añade un script a un GameObject específico.
        /// Carga el script y lo añade si no existe ya.
        /// </summary>
        /// <param name="obj">GameObject destino</param>
        /// <param name="match">Información del script a añadir</param>
        /// <returns>True si se añadió correctamente</returns>
        private bool AddScriptToGameObject(GameObject obj, ScriptMatch match)
        {
            bool result = false;
            try
            {
                var scriptType = GetScriptType(match.ScriptPath);
                if (scriptType != null)
                {
                    if (obj.GetComponent(scriptType) != null)
                    {
                        Debug.LogWarning($"[AutoScriptReassigner] {obj.name} ya tiene {match.ScriptName}");
                        result = true;
                    }
                    else
                    {
                        obj.AddComponent(scriptType);
                        Debug.Log($"[AutoScriptReassigner] ✅ {match.ScriptName} añadido a {obj.name}");
                        result = true;
                    }
                }
                else
                {
                    Debug.LogError($"[AutoScriptReassigner] No se pudo cargar el script: {match.ScriptPath}");
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"[AutoScriptReassigner] Error añadiendo {match.ScriptName} a {obj.name}: {ex.Message}");
            }
            return result;
        }
        
        /// <summary>
        /// Obtiene el tipo de un script desde su ruta de archivo.
        /// Carga el MonoScript y extrae la clase del componente.
        /// </summary>
        /// <param name="scriptPath">Ruta del archivo del script</param>
        /// <returns>Tipo del script o null si no se encuentra</returns>
        private System.Type GetScriptType(string scriptPath)
        {
            // Cargar el asset del script
            var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (scriptAsset == null)
            {
                // Intentar buscar en todas las carpetas
                string[] guids = AssetDatabase.FindAssets("t:MonoScript");
                string path;
                foreach (string guid in guids)
                {
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(System.IO.Path.GetFileName(scriptPath)) && scriptAsset == null)
                    {
                        scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    }
                }
            }
            
            return scriptAsset?.GetClass();
        }
        
        /// <summary>
        /// Realiza backup de la escena y ejecuta auto-reasignación.
        /// Función combinada para mayor seguridad del usuario.
        /// </summary>
        [MenuItem("ChibiCocina/Herramientas/Backup y Auto-Reasignar")]
        public static void BackupAndAutoReassign()
        {
            if (EditorUtility.DisplayDialog(
                "Backup y Auto-Reasignar",
                "Esta herramienta:\n1. Hará backup de la escena actual\n2. Escaneará scripts faltantes\n3. Auto-reasignará scripts con alta confianza\n\n¿Continuar?",
                "Sí, Continuar", "Cancelar"))
            {
                // Hacer backup
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(currentScene))
                {
                    string backupPath = currentScene.Replace(".unity", "_backup_before_reassign.unity");
                    if (AssetDatabase.CopyAsset(currentScene, backupPath))
                    {
                        Debug.Log($"[AutoScriptReassigner] Backup guardado en: {backupPath}");
                    }
                }
                
                // Abrir la ventana de auto-reasignación
                ShowWindow();
            }
        }
    }
}
