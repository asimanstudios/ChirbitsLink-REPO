using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Este script debe integrarse en Unity para gestionar la creación de lobbys y sincronización con la App.
    /// </summary>
    public class LobbyManager
    {
        private readonly IFirestore _firestore;
        private const string LOBBY_COLLECTION = "parties";
        private static readonly Random _random = new Random();

        public LobbyManager(IFirestore firestore)
        {
            _firestore = firestore;
        }

        /// <summary>
        /// Genera un código de lobby único y lo registra en la base de datos.
        /// </summary>
        public async Task<Party> CreateNewLobbyAsync(string lobbyName)
        {
            string roomCode = GenerateRoomCode();
            
            // Asegurar unicidad (simplemente reintentando una vez para el ejemplo)
            if (await IsCodeTaken(roomCode)) roomCode = GenerateRoomCode();

            var newParty = new Party
            {
                Id = Guid.NewGuid().ToString(),
                Name = lobbyName,
                RoomCode = roomCode,
                PlayerIds = new List<int>()
            };

            await _firestore.Collection(LOBBY_COLLECTION)
                .Document(roomCode) // Usamos roomCode como ID para búsqueda rápida
                .SetAsync(newParty);

            return newParty;
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> IsCodeTaken(string code)
        {
            var doc = await _firestore.Collection(LOBBY_COLLECTION).Document(code).GetAsync();
            return doc.Exists;
        }

        /// <summary>
        /// Finaliza un lobby y lo elimina de las disponibles.
        /// </summary>
        public async Task CloseLobbyAsync(string roomCode)
        {
            await _firestore.Collection(LOBBY_COLLECTION).Document(roomCode).DeleteAsync();
        }

        /// <summary>
        /// Actualiza la lista de jugadores en tiempo real para que la App lo vea.
        /// </summary>
        public async Task UpdateParticipantsAsync(string roomCode, List<int> updatedPlayerIds)
        {
            await _firestore.Collection(LOBBY_COLLECTION)
                .Document(roomCode)
                .UpdateAsync(new { PlayerIds = updatedPlayerIds });
        }
    }
}
