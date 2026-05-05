using Firebase.Firestore;
using System.Collections.Generic;

namespace ChibitsLink.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string RealName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SelectedCharacterId { get; set; } = "barbarian";

        [FirestoreProperty]
        public int Level { get; set; } = 1;

        [FirestoreProperty]
        public int Experience { get; set; } = 0;

        [FirestoreProperty]
        public List<string> GameHistory { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> XpClaimedParties { get; set; } = new List<string>();
    }
}
