using Firebase.Firestore;
using System;

namespace ChibitsLink.Models
{
    [FirestoreData]
    [Serializable]
    public class Game
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }
    }
}
