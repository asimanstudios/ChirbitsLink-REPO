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
        public async Task<Party> CreateNewLobbyAsync(string lobbyName, string hostUserId)
        {
            string roomCode = GenerateRoomCode();
            
            // Asegurar unicidad (simplemente reintentando una vez para el ejemplo)
            if (await IsCodeTaken(roomCode)) roomCode = GenerateRoomCode();

            var newParty = new Party
            {
                Id = Guid.NewGuid().ToString(),
                Name = lobbyName,
                RoomCode = roomCode,
                PlayerIds = new List<string>(),
                MaxPlayers = 4,
                CurrentPlayers = 0,
                HostUserId = hostUserId,
                IsGameStarted = false,
                CreatedAt = DateTime.UtcNow
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
        public async Task UpdateParticipantsAsync(string roomCode, List<string> updatedPlayerIds)
        {
            await _firestore.Collection(LOBBY_COLLECTION)
                .Document(roomCode)
                .UpdateAsync(new { 
                    PlayerIds = updatedPlayerIds,
                    CurrentPlayers = updatedPlayerIds.Count
                });
        }

        /// <summary>
        /// Verifica si un jugador puede unirse a la sala (tiene espacio y no ha comenzado el juego).
        /// </summary>
        public async Task<(bool CanJoin, string? Reason)> CanJoinLobbyAsync(string roomCode)
        {
            var doc = await _firestore.Collection(LOBBY_COLLECTION).Document(roomCode).GetAsync();
            
            if (!doc.Exists)
                return (false, "La sala no existe");
            
            var party = doc.ToObject<Party>();
            if (party == null)
                return (false, "Error al leer los datos de la sala");
                
            if (party.IsGameStarted)
                return (false, "El juego ya ha comenzado");
                
            if (party.CurrentPlayers >= party.MaxPlayers)
                return (false, "La sala está llena (máximo 4 jugadores)");
                
            return (true, null);
        }

        /// <summary>
        /// Marca el juego como iniciado.
        /// </summary>
        public async Task StartGameAsync(string roomCode)
        {
            await _firestore.Collection(LOBBY_COLLECTION)
                .Document(roomCode)
                .UpdateAsync(new { IsGameStarted = true });
        }

        /// <summary>
        /// Finaliza la partida y actualiza el historial.
        /// </summary>
        public async Task FinishGameAsync(string roomCode, Dictionary<string, int> playerScores)
        {
            // Guardar resultado en historial de partidas
            // Por implementar: guardar en colección "game_results"
            await CloseLobbyAsync(roomCode);
        }
    }
}
