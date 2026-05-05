using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

namespace ChibiCocina.Datos
{
    public class GestorFirebase : MonoBehaviour
    {
        public static GestorFirebase Instancia;
        private FirebaseFirestore db;
        private bool esInicializado = false;

        private void Awake()
        {
            if (Instancia == null) Instancia = this;
            else Destroy(gameObject);

            InicializarFirebase();
        }

        private void InicializarFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(tarea => {
                var estadoDependencia = tarea.Result;
                if (estadoDependencia == DependencyStatus.Available)
                {
                    db = FirebaseFirestore.DefaultInstance;
                    esInicializado = true;
                    Debug.Log("Firebase Firestore inicializado correctamente.");
                }
                else
                {
                    Debug.LogError($"No se pudieron inicializar las dependencias de Firebase: {estadoDependencia}");
                }
            });
        }

        public void RegistrarPedido(string nombreCliente, int puntuacion, string estado, List<string> items)
        {
            if (esInicializado)
            {
                DocumentReference docRef = db.Collection("pedidos").Document();
                Dictionary<string, object> datosPedido = new Dictionary<string, object>
                {
                    { "nombre_cliente", nombreCliente },
                    { "puntuacion", puntuacion },
                    { "estado", estado },
                    { "items", items },
                    { "fecha", Timestamp.GetCurrentTimestamp() }
                };

                docRef.SetAsync(datosPedido).ContinueWithOnMainThread(tarea => {
                    if (tarea.IsCompleted)
                    {
                        Debug.Log($"Pedido registrado en Firestore con ID: {docRef.Id}");
                    }
                    else
                    {
                        Debug.LogError("Error al registrar el pedido en Firestore.");
                    }
                });
            }
            else
            {
                Debug.LogWarning("Firebase no inicializado aún. El pedido no se guardará.");
            }
        }

        public void ActualizarSesion(string host, int jugadores, int puntos)
        {
            if (esInicializado)
            {
                DocumentReference docRef = db.Collection("sesiones_juego").Document(host);
                Dictionary<string, object> datosSesion = new Dictionary<string, object>
                {
                    { "nombre_host", host },
                    { "jugadores_activos", jugadores },
                    { "puntos_totales", puntos },
                    { "ultima_conexion", Timestamp.GetCurrentTimestamp() }
                };

                docRef.SetAsync(datosSesion, SetOptions.MergeAll);
            }
        }
    }
}
