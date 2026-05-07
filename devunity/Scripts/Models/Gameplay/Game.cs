using Firebase.Firestore;
using System;

namespace ChibitsLink.Models
{
    /// <summary>
    /// Modelo de datos para juegos/minijuegos.
    /// Representa información básica de un juego para Firestore.
    /// </summary>
    /// <remarks>
    /// Utilizado para almacenar metadatos de juegos en Firebase.
    /// Compatible con serialización de Unity y Firebase Firestore.
    /// </remarks>
    [FirestoreData]
    [Serializable]
    public class Game
    {
        /// <summary>ID único del juego</summary>
        [FirestoreProperty]
        public string Id { get; set; }

        /// <summary>Nombre del juego</summary>
        [FirestoreProperty]
        public string Name { get; set; }

        /// <summary>Descripción del juego</summary>
        [FirestoreProperty]
        public string Description { get; set; }

        /// <summary>URL de la imagen del juego</summary>
        [FirestoreProperty]
        public string ImageUrl { get; set; }
    }
}
