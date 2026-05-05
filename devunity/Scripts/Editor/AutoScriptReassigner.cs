using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ChibiCocina.Editor
{
    public class AutoScriptReassigner : EditorWindow
    {
        [MenuItem("ChibiCocina/Herramientas/Auto-Reasignar Scripts")]
        public static void ShowWindow()
        {
            GetWindow<AutoScriptReassigner>("Auto-Reasignar Scripts");
        }
        
        private Vector2 scrollPosition;
        private Dictionary<GameObject, List<ScriptMatch>> missingScriptsMap = new Dictionary<GameObject, List<ScriptMatch>>();
        private bool hasScanned = false;
        private int totalFixed = 0;
        
        private class ScriptMatch
        {
            public string ScriptName;
            public string ScriptPath;
            public string Description;
            public float Confidence;
            public bool IsAutoFixable;
            public string Reason;
        }
        
        private struct GameObjectMapping
        {
            public string Pattern;
            public string[] RequiredScripts;
            public string[] ExcludedScripts;
            public string[] RequiredComponents;
        }
        
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
            
            if (!hasScanned) return;
            
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
        
        private bool ShowDetails = true;
        
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
        
        private void AutoReassignAll()
        {
            int fixedCount = 0;
            int errorCount = 0;
            
            foreach (var kvp in missingScriptsMap)
            {
                GameObject obj = kvp.Key;
                var matches = kvp.Value;
                
                // Solo auto-reasignar scripts con alta confianza
                var highConfidenceMatches = matches.Where(m => m.Confidence >= 0.8f && m.IsAutoFixable);
                
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
        
        private bool AddScriptToGameObject(GameObject obj, ScriptMatch match)
        {
            try
            {
                // Cargar el script
                var scriptType = GetScriptType(match.ScriptPath);
                if (scriptType == null)
                {
                    Debug.LogError($"[AutoScriptReassigner] No se pudo cargar el script: {match.ScriptPath}");
                    return false;
                }
                
                // Verificar si ya tiene el script
                if (obj.GetComponent(scriptType) != null)
                {
                    Debug.LogWarning($"[AutoScriptReassigner] {obj.name} ya tiene {match.ScriptName}");
                    return true;
                }
                
                // Añadir el componente
                obj.AddComponent(scriptType);
                
                Debug.Log($"[AutoScriptReassigner] ✅ {match.ScriptName} añadido a {obj.name}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoScriptReassigner] Error añadiendo {match.ScriptName} a {obj.name}: {ex.Message}");
                return false;
            }
        }
        
        private System.Type GetScriptType(string scriptPath)
        {
            // Cargar el asset del script
            var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (scriptAsset == null)
            {
                // Intentar buscar en todas las carpetas
                string[] guids = AssetDatabase.FindAssets("t:MonoScript");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(System.IO.Path.GetFileName(scriptPath)))
                    {
                        scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        break;
                    }
                }
            }
            
            return scriptAsset?.GetClass();
        }
        
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
