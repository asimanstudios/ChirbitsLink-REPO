using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ChibitsLink.Utils
{
    /// <summary>
    /// Forma segura de ejecutar código en el thread principal de Unity desde threads secundarios.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Esencial para operaciones asíncronas que necesitan interactuar con la API de Unity.
    /// Utiliza una cola thread-safe para ejecutar acciones en el frame siguiente.
    /// Proporciona tanto síncrono como asíncrono para encolar acciones.
    /// </remarks>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        /// <summary>Cola de acciones a ejecutar (thread-safe)</summary>
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        /// <summary>Instancia singleton</summary>
        private static UnityMainThreadDispatcher _instance = null;

        /// <summary>
        /// Obtiene la instancia del dispatcher.
        /// Crea una si no existe.
        /// </summary>
        /// <returns>Instancia del UnityMainThreadDispatcher</returns>
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();

                if (_instance == null)
                {
                    var obj = new GameObject("UnityMainThreadDispatcher");
                    _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(obj);
                    Debug.Log("[UnityMainThreadDispatcher] Instance created and marked as DontDestroyOnLoad.");
                }
            }
            return _instance;
        }

        /// <summary>
        /// Inicialización del componente.
        /// Establece el patrón Singleton y persistencia.
        /// </summary>
        public void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

            bool isDuplicateInstance = _instance != null && _instance != this;
            if (isDuplicateInstance)
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Ejecuta acciones encoladas cada frame.
        /// Procesa todas las acciones pendientes de forma segura.
        /// </summary>
        public void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    try
                    {
                        _executionQueue.Dequeue().Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UnityMainThreadDispatcher] Error executing action: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Encola una acción para ejecutar en el thread principal.
        /// Thread-safe desde cualquier thread.
        /// </summary>
        /// <param name="action">Acción a ejecutar</param>
        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Encola una acción asíncronamente.
        /// Devuelve una Task que se completa cuando la acción se ejecuta.
        /// </summary>
        /// <param name="action">Acción a ejecutar</param>
        /// <returns>Task que se completa al ejecutar la acción</returns>
        public Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            Enqueue(() => {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    Debug.LogError($"[UnityMainThreadDispatcher] Async error: {ex.Message}");
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Limpia la instancia al destruir el objeto.
        /// </summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
