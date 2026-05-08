using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;
using ChibitsLink.Models;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de base de datos de partidos (parties).
    /// Maneja todas las operaciones CRUD para parties sin lógica de negocio.
    /// Implementa patrón Repository para acceso a datos de Firebase Firestore.
    /// </summary>
    /// <remarks>
    /// Se centra únicamente en operaciones de base de datos.
    /// No contiene lógica de negocio, solo validaciones y acceso a datos.
    /// Maneja excepciones específicas para cada tipo de operación.
    /// </remarks>
    /// <seealso cref="https://firebase.google.com/docs/firestore">
    /// Documentación de Firebase Firestore
    /// </seealso>
    public class PartyRepository
    {
        /// <summary>Instancia de base de datos Firebase Firestore</summary>
        private readonly FirebaseFirestore _database;
        /// <summary>Nombre de la colección de parties</summary>
        private readonly string _collectionName = "parties";
        /// <summary>Indica si el repositorio está inicializado</summary>
        private bool _isInitialized;
        
        // Constants
        /// <summary>Nombre de la colección de parties</summary>
        private const string PARTIES_COLLECTION = "parties";
        /// <summary>Estado de lobby para parties activas</summary>
        private const string LOBBY_STATE = "LOBBY";
        /// <summary>Estado cerrado para parties finalizadas</summary>
        private const string CLOSED_STATE = "CLOSED";

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de parties.
        /// </summary>
        /// <param name="database">Instancia de Firebase Firestore</param>
        /// <exception cref="ArgumentNullException">Si database es null</exception>
        public PartyRepository(FirebaseFirestore database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _isInitialized = true;
        }

        /// <summary>
        /// Crea un nuevo party en la base de datos.
        /// </summary>
        /// <param name="party">Party a crear</param>
        /// <returns>Room code del party creado</returns>
        /// <exception cref="RepositoryNotInitializedException">Si el repositorio no está inicializado</exception>
        /// <exception cref="InvalidPartyException">Si el party no es válido</exception>
        /// <exception cref="PartyCreationException">Si falla la creación en Firebase</exception>
        public async Task<string> CreatePartyAsync(Party party)
        {
            ValidateRepositoryInitialized();
            ValidatePartyForCreation(party);
            
            try
            {
                var docRef = _database.Collection(_collectionName).Document(party.RoomCode);
                await docRef.SetAsync(party);
                return party.RoomCode;
            }
            catch (FirebaseException ex)
            {
                throw new PartyCreationException($"Failed to create party {party.RoomCode}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error creating party {party.RoomCode}", ex);
            }
        }
        
        /// <summary>
        /// Valida que el repositorio esté inicializado.
        /// </summary>
        /// <exception cref="RepositoryNotInitializedException">Si no está inicializado</exception>
        private void ValidateRepositoryInitialized()
        {
            if (!_isInitialized)
            {
                throw new RepositoryNotInitializedException("PartyRepository not initialized");
            }
        }
        
        /// <summary>
        /// Valida un party para creación.
        /// </summary>
        /// <param name="party">Party a validar</param>
        /// <exception cref="ArgumentNullException">Si party es null</exception>
        /// <exception cref="InvalidPartyException">Si el room code es inválido</exception>
        private void ValidatePartyForCreation(Party party)
        {
            if (party == null)
            {
                throw new ArgumentNullException(nameof(party));
            }

            if (string.IsNullOrEmpty(party.RoomCode))
            {
                throw new InvalidPartyException("Party room code cannot be null or empty");
            }
        }

        /// <summary>
        /// Obtiene un party por su room code.
        /// </summary>
        /// <param name="roomCode">Room code del party a buscar</param>
        /// <returns>Party encontrado</returns>
        /// <exception cref="PartyNotFoundException">Si el party no existe</exception>
        /// <exception cref="PartyRetrievalException">Si falla la recuperación</exception>
        public async Task<Party> GetPartyAsync(string roomCode)
        {
            ValidateRepositoryInitialized();
            ValidateRoomCode(roomCode);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(roomCode);
                var snapshot = await docRef.GetSnapshotAsync();
                
                if (!snapshot.Exists)
                    throw new PartyNotFoundException($"Party {roomCode} not found");

                return snapshot.ConvertTo<Party>();
            }
            catch (PartyNotFoundException ex)
            {
                throw new PartyRetrievalException($"Party {roomCode} not found", ex);
            }
            catch (FirebaseException ex)
            {
                throw new PartyRetrievalException($"Failed to retrieve party {roomCode}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error retrieving party {roomCode}", ex);
            }
        }
        
        /// <summary>
        /// Valida un room code.
        /// </summary>
        /// <param name="roomCode">Room code a validar</param>
        /// <exception cref="ArgumentNullException">Si room code es null o vacío</exception>
        private void ValidateRoomCode(string roomCode)
        {
            if (string.IsNullOrEmpty(roomCode))
            {
                throw new ArgumentNullException(nameof(roomCode));
            }
        }

        /// <summary>
        /// Actualiza un party con los campos especificados.
        /// </summary>
        /// <param name="roomCode">Room code del party a actualizar</param>
        /// <param name="updates">Diccionario de campos a actualizar</param>
        /// <exception cref="PartyUpdateException">Si falla la actualización</exception>
        public async Task UpdatePartyAsync(string roomCode, Dictionary<string, object> updates)
        {
            ValidateRepositoryInitialized();
            ValidateRoomCode(roomCode);
            ValidateUpdates(updates);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(roomCode);
                await docRef.UpdateAsync(updates);
            }
            catch (FirebaseException ex)
            {
                throw new PartyUpdateException($"Failed to update party {roomCode}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error updating party {roomCode}", ex);
            }
        }
        
        /// <summary>
        /// Valida los datos de actualización.
        /// </summary>
        /// <param name="updates">Actualizaciones a validar</param>
        /// <exception cref="ArgumentException">Si updates es null o vacío</exception>
        private void ValidateUpdates(Dictionary<string, object> updates)
        {
            if (updates == null || updates.Count == 0)
            {
                throw new ArgumentException("Updates cannot be null or empty", nameof(updates));
            }
        }

        /// <summary>
        /// Cierra un party y guarda los datos finales.
        /// </summary>
        /// <param name="roomCode">Room code del party a cerrar</param>
        /// <param name="finalData">Datos finales del party</param>
        /// <exception cref="PartyUpdateException">Si falla el cierre</exception>
        public async Task ClosePartyAsync(string roomCode, Party finalData)
        {
            ValidateRepositoryInitialized();
            ValidateRoomCode(roomCode);
            ValidateFinalData(finalData);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(roomCode);
                var updates = PreparePartyCloseUpdates(finalData);
                await docRef.UpdateAsync(updates);
            }
            catch (FirebaseException ex)
            {
                throw new PartyUpdateException($"Failed to close party {roomCode}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error closing party {roomCode}", ex);
            }
        }
        
        /// <summary>
        /// Valida los datos finales de un party.
        /// </summary>
        /// <param name="finalData">Datos finales a validar</param>
        /// <exception cref="ArgumentNullException">Si finalData es null</exception>
        private void ValidateFinalData(Party finalData)
        {
            if (finalData == null)
            {
                throw new ArgumentNullException(nameof(finalData));
            }
        }
        
        /// <summary>
        /// Prepara las actualizaciones para cerrar un party.
        /// </summary>
        /// <param name="finalData">Datos finales del party</param>
        /// <returns>Diccionario con las actualizaciones</returns>
        private Dictionary<string, object> PreparePartyCloseUpdates(Party finalData)
        {
            return new Dictionary<string, object>
            {
                { "GameState", CLOSED_STATE },
                { "ParticipantNames", finalData.ParticipantNames ?? new Dictionary<string, string>() },
                { "ParticipantCharacters", finalData.ParticipantCharacters ?? new Dictionary<string, string>() },
                { "ParticipantLevels", finalData.ParticipantLevels ?? new Dictionary<string, int>() },
                { "PlayerScores", finalData.PlayerScores ?? new Dictionary<string, int>() },
                { "PlayedGames", finalData.PlayedGames ?? new List<string>() },
                { "ClosedAt", Timestamp.GetCurrentTimestamp() }
            };
        }

        /// <summary>
        /// Elimina un party de la base de datos.
        /// </summary>
        /// <param name="roomCode">Room code del party a eliminar</param>
        /// <exception cref="PartyDeletionException">Si falla la eliminación</exception>
        public async Task DeletePartyAsync(string roomCode)
        {
            ValidateRepositoryInitialized();
            ValidateRoomCode(roomCode);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(roomCode);
                await docRef.DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                throw new PartyDeletionException($"Failed to delete party {roomCode}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error deleting party {roomCode}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los parties activos (en estado LOBBY).
        /// </summary>
        /// <returns>Lista de parties activos ordenados por fecha de creación</returns>
        /// <exception cref="PartyRetrievalException">Si falla la recuperación</exception>
        public async Task<List<Party>> GetActivePartiesAsync()
        {
            ValidateRepositoryInitialized();

            try
            {
                var query = _database.Collection(_collectionName)
                    .WhereEqualTo("GameState", LOBBY_STATE)
                    .OrderByDescending("CreatedAt");

                var snapshot = await query.GetSnapshotAsync();
                var parties = new List<Party>();

                foreach (var document in snapshot.Documents)
                {
                    parties.Add(document.ConvertTo<Party>());
                }

                return parties;
            }
            catch (FirebaseException ex)
            {
                throw new PartyRetrievalException("Failed to retrieve active parties", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Unexpected error retrieving active parties", ex);
            }
        }
    }
}
