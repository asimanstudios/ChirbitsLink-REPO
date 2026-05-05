using System;
using Firebase.Firestore;
using System.Collections.Generic;

namespace ChibitsLink.Models
{
    [FirestoreData]
    public class Party
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string RoomCode { get; set; }

        [FirestoreProperty]
        public List<string> PlayerIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public List<string> ReadyPlayerIds { get; set; } = new List<string>();

        [FirestoreProperty]
        public Dictionary<string, int> Votes { get; set; } = new Dictionary<string, int>(); // GameId -> Count

        [FirestoreProperty]
        public string GameState { get; set; } = "LOBBY"; // LOBBY, VOTING, IN_GAME

        [FirestoreProperty]
        public string IpAddress { get; set; }

        [FirestoreProperty]
        public int Port { get; set; }

        [FirestoreProperty]
        public Dictionary<string, int> PlayerScores { get; set; } = new Dictionary<string, int>(); // UserId -> Total Points in Session

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public List<string> PlayedGames { get; set; } = new List<string>();

        [FirestoreProperty]
        public Dictionary<string, string> ParticipantNames { get; set; } = new Dictionary<string, string>();

        [FirestoreProperty]
        public Dictionary<string, string> ParticipantCharacters { get; set; } = new Dictionary<string, string>();

        [FirestoreProperty]
        public Dictionary<string, int> ParticipantLevels { get; set; } = new Dictionary<string, int>();
    }
}
