using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ChibiCocina.Editor
{
    public class ScriptReferenceFinder : EditorWindow
    {
        [MenuItem("ChibiCocina/Herramientas/Buscar Scripts Faltantes")]
        public static void ShowWindow()
        {
            GetWindow<ScriptReferenceFinder>("Scripts Faltantes");
        }
        
        private Vector2 scrollPosition;
        private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
        private bool hasSearched = false;
        
        private void OnGUI()
        {
            try
            {
                GUILayout.Label("Herramienta para encontrar GameObjects con scripts faltantes", EditorStyles.boldLabel);
                GUILayout.Space(10);
                
                if (GUILayout.Button("Buscar Scripts Faltantes en Escena Actual"))
                {
                    FindMissingScripts();
                }
                
                GUILayout.Space(10);
                
                if (hasSearched)
                {
                    GUILayout.Label($"Se encontraron {objectsWithMissingScripts.Count} GameObjects con scripts faltantes", EditorStyles.helpBox);
                    
                    if (objectsWithMissingScripts.Count > 0)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("GameObjects afectados:", EditorStyles.boldLabel);
                        
                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                        
                        foreach (var obj in objectsWithMissingScripts)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label($"• {obj.name}", GUILayout.Width(200));
                            
                            if (GUILayout.Button("Seleccionar", GUILayout.Width(80)))
                            {
                                Selection.activeGameObject = obj;
                                EditorGUIUtility.PingObject(obj);
                            }
                            
                            if (GUILayout.Button("Buscar Script", GUILayout.Width(100)))
                            {
                                FindPossibleScript(obj);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.EndScrollView();
                        
                        GUILayout.Space(10);
                        if (GUILayout.Button("Limpiar Lista"))
                        {
                            objectsWithMissingScripts.Clear();
                            hasSearched = false;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GUILayout.Label($"Error: {ex.Message}", EditorStyles.helpBox);
                if (GUILayout.Button("Reiniciar Herramienta"))
                {
                    objectsWithMissingScripts = new List<GameObject>();
                    hasSearched = false;
                }
            }
        }
        
        private void FindMissingScripts()
        {
            objectsWithMissingScripts.Clear();
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                Component[] components = obj.GetComponents<Component>();
                
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        objectsWithMissingScripts.Add(obj);
                        break;
                    }
                }
            }
            
            hasSearched = true;
            
            if (objectsWithMissingScripts.Count == 0)
            {
                EditorUtility.DisplayDialog("Resultado", "No se encontraron scripts faltantes en la escena actual.", "OK");
            }
        }
        
        private void FindPossibleScript(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            
            foreach (Component component in components)
            {
                if (component == null)
                {
                    // Intentar adivinar qué script era basado en el nombre del GameObject
                    string possibleScript = GetPossibleScriptName(obj.name);
                    
                    EditorUtility.DisplayDialog(
                        "Sugerencia de Script",
                        $"Para el GameObject '{obj.name}'\n\nPosible script necesario:\n{possibleScript}\n\nBusca en la nueva estructura de carpetas:\n- Controllers/Player/\n- Controllers/Gameplay/\n- Services/Network/\n- Core/Managers/\n- Core/Systems/",
                        "OK"
                    );
                    
                    break;
                }
            }
        }
        
        private string GetPossibleScriptName(string gameObjectName)
        {
            string name = gameObjectName.ToLower();
            
            if (name.Contains("jugador") || name.Contains("player") || name.Contains("personaje"))
                return "Movimiento.cs o PlayerController.cs (Controllers/Player/)";
            
            if (name.Contains("manager") || name.Contains("gestor"))
                return "DebugManager.cs (Core/Systems/) o PlayerManager.cs (Core/Managers/)";
            
            if (name.Contains("tcp") || name.Contains("server") || name.Contains("red"))
                return "TcpServer.cs (Services/Network/) o NetworkService.cs (Services/Network/)";
            
            if (name.Contains("moneda") || name.Contains("coin"))
                return "Moneda.cs (Controllers/Gameplay/)";
            
            if (name.Contains("bomba") || name.Contains("tag"))
                return "BombaTag.cs (Controllers/Gameplay/)";
            
            return "Revisa la guía UNITY_SCRIPT_REASSIGNMENT_GUIDE.md";
        }
        
        [MenuItem("ChibiCocina/Herramientas/Mostrar Guía de Reasignación")]
        public static void ShowGuide()
        {
            string guidePath = "Assets/Scripts/UNITY_SCRIPT_REASSIGNMENT_GUIDE.md";
            
            if (System.IO.File.Exists(guidePath))
            {
                EditorUtility.DisplayDialog(
                    "Guía Disponible",
                    "La guía de reasignación está en:\nAssets/Scripts/UNITY_SCRIPT_REASSIGNMENT_GUIDE.md\n\nÁbrela para ver todas las nuevas ubicaciones de scripts.",
                    "OK"
                );
                
                // Seleccionar el archivo en el Project window
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(guidePath);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Guía No Encontrada",
                    "No se encontró el archivo de guía.\n\nRevisa la documentación en la carpeta Scripts.",
                    "OK"
                );
            }
        }
    }
}
