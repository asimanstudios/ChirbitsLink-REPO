using UnityEditor;
using UnityEngine;
using System.IO;

namespace ChibiCocina.Editor
{
    public class ConfiguradorProyecto : EditorWindow
    {
        [MenuItem("ChibiCocina/Configurar Proyecto Completo")]
        public static void MostrarVentana()
        {
            if (EditorUtility.DisplayDialog("Configuración de Chibi Cocina", 
                "¿Deseas configurar automáticamente el proyecto? Esto creará tags, layers y carpetas necesarias.", 
                "Sí, configurar", "Cancelar"))
            {
                ConfigurarTodo();
            }
        }

        private static void ConfigurarTodo()
        {
            CrearCarpetas();
            ConfigurarTagsYLayers();
            CrearManagersEnEscena();
            Debug.Log("¡Proyecto y escena configurados satisfactoriamente!");
        }

        private static void CrearManagersEnEscena()
        {
            GameObject root = GameObject.Find("Managers");
            if (root == null)
            {
                root = new GameObject("Managers");
            }

            // Añadir o obtener componentes necesarios
            AñadirComponenteSiNoExiste<Unity.Netcode.NetworkManager>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Nucleo.GestorDeRed>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Datos.GestorFirebase>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Nucleo.GestorDePartida>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Nucleo.ServidorControlMando>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Clientes.GestorDePedidos>(root);
            AñadirComponenteSiNoExiste<ChibiCocina.Nucleo.GestorDePantallaDividida>(root);
            
            Debug.Log("Managers configurados en la escena activa.");
        }

        private static void AñadirComponenteSiNoExiste<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }

        private static void CrearCarpetas()
        {
            string[] carpetas = { "Prefabs", "Modelos", "Materiales", "Escenas", "ScriptableObjects/Ingredientes" };
            foreach (string c in carpetas)
            {
                string ruta = Path.Combine(Application.dataPath, c);
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                    Debug.Log("Carpeta creada: " + c);
                }
            }
            AssetDatabase.Refresh();
        }

        private static void ConfigurarTagsYLayers()
        {
            // Nota: Configurar Tags y Layers programáticamente en Unity requiere manipular TagManager.asset
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            // Añadir Layer "Interaccion"
            SerializedProperty layers = tagManager.FindProperty("layers");
            bool layerExiste = false;
            bool layerAsignada = false;
            for (int i = 8; i < layers.arraySize; i++)
            {
                string currentLayer = layers.GetArrayElementAtIndex(i).stringValue;
                bool isTargetLayer = currentLayer == "Interaccion";
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
