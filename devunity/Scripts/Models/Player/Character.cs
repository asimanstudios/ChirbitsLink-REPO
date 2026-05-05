using Firebase.Firestore;
using System;

namespace ChibitsLink.Models
{
    [FirestoreData]
    [Serializable]
    public class Character
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }
    }
}
