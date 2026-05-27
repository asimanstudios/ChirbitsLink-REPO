using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ChibiCocina.Editor
{
    /// <summary>
    /// Ventana de editor para buscar scripts faltantes en GameObjects.
    /// Identifica objetos que tienen referencias rotas a scripts.
    /// </summary>
    /// <remarks>
    /// Escanea la escena actual en busca de GameObjects con scripts faltantes.
    /// Permite seleccionar y navegar a los objetos afectados.
    /// Esencial para mantenimiento y limpieza del proyecto.
    /// </remarks>
    public class ScriptReferenceFinder : EditorWindow
    {
        /// <summary>
        /// Muestra la ventana de búsqueda de scripts faltantes.
        /// Accesible desde el menú de Unity.
        /// </summary>
        [MenuItem("ChibiCocina/Herramientas/Buscar Scripts Faltantes")]
        public static void ShowWindow()
        {
            GetWindow<ScriptReferenceFinder>("Scripts Faltantes");
        }
        
        /// <summary>Posición del scroll</summary>
        private Vector2 scrollPosition;
        /// <summary>GameObjects con scripts faltantes</summary>
        private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
        /// <summary>Indica si se ha realizado búsqueda</summary>
        private bool hasSearched = false;
        
        /// <summary>
        /// Dibuja la interfaz de usuario de la ventana.
        /// Muestra botones de búsqueda y resultados.
        /// </summary>
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
            catch (InvalidOperationException ex)
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
            Component[] components;
            bool hasMissingScript;
            
            foreach (GameObject obj in allObjects)
            {
                components = obj.GetComponents<Component>();
                hasMissingScript = false;
                
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        hasMissingScript = true;
                    }
                }
                
                if (hasMissingScript)
                {
                    objectsWithMissingScripts.Add(obj);
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
            string possibleScript;
            bool alreadyShown = false;
            
            foreach (Component component in components)
            {
                if (component == null && !alreadyShown)
                {
                    // Intentar adivinar qué script era basado en el nombre del GameObject
                    possibleScript = GetPossibleScriptName(obj.name);
                    alreadyShown = true;
                    
                    EditorUtility.DisplayDialog(
                        "Sugerencia de Script",
                        $"Para el GameObject '{obj.name}'\n\nPosible script necesario:\n{possibleScript}\n\nBusca en la nueva estructura de carpetas:\n- Controllers/Player/\n- Controllers/Gameplay/\n- Services/Network/\n- Core/Managers/\n- Core/Systems/",
                        "OK"
                    );
                }
            }
        }
        
        private string GetPossibleScriptName(string gameObjectName)
        {
            string name = gameObjectName.ToLower();
            string result = "Revisa la guía UNITY_SCRIPT_REASSIGNMENT_GUIDE.md";
            
            if (name.Contains("jugador") || name.Contains("player") || name.Contains("personaje"))
                result = "Movimiento.cs o PlayerController.cs (Controllers/Player/)";
            else if (name.Contains("manager") || name.Contains("gestor"))
                result = "DebugManager.cs (Core/Systems/) o PlayerManager.cs (Core/Managers/)";
            else if (name.Contains("tcp") || name.Contains("server") || name.Contains("red"))
                result = "TcpServer.cs (Services/Network/) o NetworkService.cs (Services/Network/)";
            else if (name.Contains("moneda") || name.Contains("coin"))
                result = "Moneda.cs (Controllers/Gameplay/)";
            else if (name.Contains("bomba") || name.Contains("tag"))
                result = "BombaTag.cs (Controllers/Gameplay/)";
            
            return result;
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
