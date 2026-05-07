using UnityEngine;
using UnityEngine.SceneManagement;
using ChibiCocina.Core.Exceptions;

namespace ChibiCocina.Services
{
    /// <summary>
    /// Servicio para carga asíncrona de escenas.
    /// Proporciona carga segura con manejo de errores.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Previene cargas múltiples simultáneas.
    /// Verifica existencia de escenas antes de cargar.
    /// Proporciona timeout para evitar bloqueos.
    /// </remarks>
    public class SceneLoaderService : MonoBehaviour
    {
        /// <summary>Instancia global del servicio (patrón Singleton)</summary>
        public static SceneLoaderService Instance { get; private set; }
        
        [Header("Configuración de Escenas")]
        /// <summary>Escena por defecto</summary>
        public string defaultScene = "menu";
        /// <summary>Timeout para carga de escenas</summary>
        public float loadingTimeout = 10f;
        
        /// <summary>Indica si hay una escena cargando actualmente</summary>
        private bool isLoading;
        
        /// <summary>
        /// Inicialización del servicio.
        /// Establece el patrón Singleton y persistencia.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Carga una escena de forma asíncrona y segura.
        /// Verifica existencia y previene cargas múltiples.
        /// </summary>
        /// <param name="sceneName">Nombre de la escena a cargar</param>
        /// <exception cref="SceneLoaderException">Si hay errores en la carga</exception>
        public void LoadScene(string sceneName)
        {
            if (!isLoading)
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    throw new SceneLoaderException("El nombre de la escena no puede ser nulo o vacío");
                }
                
                if (SceneExists(sceneName))
                {
                    StartCoroutine(LoadSceneAsync(sceneName));
                }
                else
                {
                    throw new SceneLoaderException($"La escena '{sceneName}' no existe en el build settings");
                }
            }
            else
            {
                Debug.LogWarning("[SceneLoaderService] Ya hay una escena cargando");
            }
        }
        
        private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            float timeoutTimer = 0f;
            
            while (!asyncLoad.isDone)
            {
                timeoutTimer += Time.deltaTime;
                
                bool isTimeout = timeoutTimer >= loadingTimeout;
                if (isTimeout)
                {
                    isLoading = false;
                    throw new SceneLoaderException($"Timeout cargando escena '{sceneName}'");
                }
                
                bool canActivate = asyncLoad.progress >= 0.9f;
                if (canActivate)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            isLoading = false;
            Debug.Log($"[SceneLoaderService] Escena '{sceneName}' cargada correctamente");
        }
        
        public void ReloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
        
        public void LoadMainMenu()
        {
            LoadScene(defaultScene);
        }
        
        private bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneFileName == sceneName)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool IsLoading()
        {
            return isLoading;
        }
        
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
