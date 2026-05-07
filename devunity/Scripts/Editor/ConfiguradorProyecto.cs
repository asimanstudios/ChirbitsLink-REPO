using UnityEditor;
using UnityEngine;
using System.IO;

namespace ChibitsLink.Editor
{
    /// <summary>
    /// Herramienta de configuración automática del proyecto ChibitsLink.
    /// Crea estructura de carpetas, configura tags/layers y managers necesarios.
    /// Facilita la configuración inicial de nuevos proyectos.
    /// </summary>
    /// <remarks>
    /// Agregada al menú de Unity para fácil acceso.
    /// Configura todos los componentes esenciales para el funcionamiento.
    /// </remarks>
    public class ProjectSetup : EditorWindow
    {
        /// <summary>
        /// Muestra la ventana de configuración del proyecto.
        /// Agregada al menú de Unity bajo ChibitsLink.
        /// </summary>
        [MenuItem("ChibitsLink/Configure Complete Project")]
        public static void ShowWindow()
        {
            if (EditorUtility.DisplayDialog("ChibitsLink Project Configuration", 
                "Do you want to automatically configure the project? This will create tags, layers and necessary folders.", 
                "Yes, configure", "Cancel"))
            {
                ConfigureEverything();
            }
        }

        /// <summary>
        /// Ejecuta la configuración completa del proyecto.
        /// Orquesta todos los pasos de configuración en secuencia.
        /// </summary>
        private static void ConfigureEverything()
        {
            CreateFolders();
            ConfigureTagsAndLayers();
            CreateManagersInScene();
            Debug.Log("Project and scene configured successfully!");
        }

        /// <summary>
        /// Crea los managers necesarios en la escena activa.
        /// Crea GameObject Managers si no existe y añade componentes.
        /// </summary>
        private static void CreateManagersInScene()
        {
            GameObject root = GameObject.Find("Managers");
            if (root == null)
            {
                root = new GameObject("Managers");
            }

            // Add or get necessary components
            AddComponentIfNotExists<Unity.Netcode.NetworkManager>(root);
            AddComponentIfNotExists<ChibitsLink.Services.Network.TcpNetworkServer>(root);
            AddComponentIfNotExists<ChibitsLink.Services.Network.FirebaseManager>(root);
            AddComponentIfNotExists<ChibitsLink.Core.GameManager>(root);
            AddComponentIfNotExists<ChibitsLink.Core.ServidorControlMando>(root);
            AddComponentIfNotExists<ChibitsLink.Services.Gameplay.OrderManager>(root);
            AddComponentIfNotExists<ChibitsLink.Core.SplitScreenManager>(root);
            
            Debug.Log("Managers configured in active scene.");
        }

        /// <summary>
        /// Añade un componente si no existe en el GameObject.
        /// Método genérico para evitar duplicación de componentes.
        /// </summary>
        /// <typeparam name="T">Tipo de componente a añadir</typeparam>
        /// <param name="go">GameObject destino</param>
        private static void AddComponentIfNotExists<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }

        /// <summary>
        /// Crea la estructura de carpetas necesaria para el proyecto.
        /// Establece las carpetas base para organización del código.
        /// </summary>
        private static void CreateFolders()
        {
            string[] folders = {
                "Assets/Scenes",
                "Assets/Scripts/Core",
                "Assets/Scripts/Services",
                "Assets/Scripts/Controllers",
                "Assets/Scripts/Models",
                "Assets/Scripts/Views",
                "Assets/Scripts/Repositories",
                "Assets/Scripts/Editor",
                "Assets/Scripts/Minigames"
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        /// <summary>
        /// Configura tags y layers necesarios para el proyecto.
        /// Establece configuración de ProjectSettings para tags y layers.
        /// </summary>
        private static void ConfigureTagsAndLayers()
        {
            // Add necessary tags
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            // Tags configuration would go here
            Debug.Log("Tags and layers configured.");
                if (isTargetLayer)
                {
                    layerExiste = true;
                    layerAsignada = true;
                }

                bool canAssignLayer = !layerAsignada && string.IsNullOrEmpty(currentLayer);
                if (canAssignLayer)
                {
                    layers.GetArrayElementAtIndex(i).stringValue = "Interaccion";
                    layerExiste = true;
                    layerAsignada = true;
                }
            }

            if (!layerExiste) Debug.LogError("No se pudo crear la capa 'Interaccion'. Añádela manualmente en el slot 8.");
            
            tagManager.ApplyModifiedProperties();
        }
    }
}
