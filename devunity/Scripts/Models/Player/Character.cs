using Firebase.Firestore;
using System;

namespace ChibitsLink.Models
{
    /// <summary>
    /// Modelo de datos para personajes del juego.
    /// Representa información básica de un personaje almacenada en Firebase.
    /// </summary>
    /// <remarks>
    /// Utilizado para persistencia de datos de personajes en Firestore.
    /// Compatible con serialización automática de Firebase.
    /// </remarks>
    /// <seealso cref="https://firebase.google.com/docs/firestore/manage-data/add-data">
    /// Documentación de Firebase Firestore
    /// </seealso>
    [FirestoreData]
    [Serializable]
    public class Character
    {
        /// <summary>Identificador único del personaje</summary>
        [FirestoreProperty]
        public string Id { get; set; }

        /// <summary>Nombre del personaje</summary>
        [FirestoreProperty]
        public string Name { get; set; }

        /// <summary>URL de la imagen del personaje</summary>
        [FirestoreProperty]
        public string ImageUrl { get; set; }

        /// <summary>Descripción del personaje</summary>
        [FirestoreProperty]
        public string Description { get; set; }
    }
}
