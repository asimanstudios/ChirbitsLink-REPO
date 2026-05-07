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
    /// Repositorio para operaciones de base de datos de usuarios.
    /// Maneja todas las operaciones CRUD para usuarios sin lógica de negocio.
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
    public class UserRepository
    {
        /// <summary>Instancia de base de datos Firebase Firestore</summary>
        private readonly FirebaseFirestore _database;
        /// <summary>Nombre de la colección de usuarios</summary>
        private readonly string _collectionName = "users";
        /// <summary>Indica si el repositorio está inicializado</summary>
        private bool _isInitialized;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de usuarios.
        /// </summary>
        /// <param name="database">Instancia de Firebase Firestore</param>
        /// <exception cref="ArgumentNullException">Si database es null</exception>
        public UserRepository(FirebaseFirestore database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _isInitialized = true;
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario a buscar</param>
        /// <returns>Usuario encontrado</returns>
        /// <exception cref="UserNotFoundException">Si el usuario no existe</exception>
        /// <exception cref="UserRetrievalException">Si falla la recuperación</exception>
        public async Task<User> GetUserAsync(string userId)
        {
            ValidateRepositoryInitialized();
            ValidateUserId(userId);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();
                
                if (!snapshot.Exists)
                {
                    throw new UserNotFoundException($"User {userId} not found");
                }
                
                return snapshot.ConvertTo<User>();
            }
            catch (FirebaseException ex)
            {
                throw new UserRetrievalException($"Failed to retrieve user {userId}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error retrieving user {userId}", ex);
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
                throw new RepositoryNotInitializedException("UserRepository not initialized");
            }
        }
        
        /// <summary>
        /// Valida un ID de usuario.
        /// </summary>
        /// <param name="userId">ID de usuario a validar</param>
        /// <exception cref="ArgumentNullException">Si userId es null o vacío</exception>
        private void ValidateUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="user">Usuario a crear</param>
        /// <exception cref="UserCreationException">Si falla la creación</exception>
        public async Task CreateUserAsync(User user)
        {
            ValidateRepositoryInitialized();
            ValidateUser(user);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(user.UserId);
                await docRef.SetAsync(user);
            }
            catch (FirebaseException ex)
            {
                throw new UserCreationException($"Failed to create user {user.UserId}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error creating user {user.UserId}", ex);
            }
        }

        /// <summary>
        /// Valida un usuario para creación.
        /// </summary>
        /// <param name="user">Usuario a validar</param>
        /// <exception cref="ArgumentNullException">Si user es null</exception>
        /// <exception cref="InvalidUserException">Si el ID de usuario es inválido</exception>
        private void ValidateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.UserId))
            {
                throw new InvalidUserException("User ID cannot be null or empty");
            }
        }

        /// <summary>
        /// Añade un partido al historial de juegos del usuario.
        /// Crea el usuario si no existe.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="partyId">ID del partido a añadir</param>
        /// <exception cref="UserUpdateException">Si falla la actualización</exception>
        public async Task AddGameToUserHistoryAsync(string userId, string partyId)
        {
            ValidateRepositoryInitialized();
            ValidateUserId(userId);
            ValidatePartyId(partyId);

            try
            {
                var user = await GetUserAsync(userId);
                if (user == null)
                {
                    // Create user if doesn't exist
                    user = new User 
                    { 
                        UserId = userId, 
                        GameHistory = new List<string>(),
                        Username = $"Player_{userId.Substring(0, Math.Min(8, userId.Length))}"
                    };
                    await CreateUserAsync(user);
                }

                if (!user.GameHistory.Contains(partyId))
                {
                    user.GameHistory.Add(partyId);
                    await UpdateUserAsync(userId, new Dictionary<string, object>
                    {
                        { "GameHistory", user.GameHistory },
                        { "LastPlayedAt", Timestamp.GetCurrentTimestamp() }
                    });
                }
            }
            catch (FirebaseException ex)
            {
                throw new UserUpdateException($"Failed to add game {partyId} to user {userId} history", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error adding game {partyId} to user {userId} history", ex);
            }
        }
        
        /// <summary>
        /// Valida un ID de partido.
        /// </summary>
        /// <param name="partyId">ID de partido a validar</param>
        /// <exception cref="ArgumentNullException">Si partyId es null o vacío</exception>
        private void ValidatePartyId(string partyId)
        {
            if (string.IsNullOrEmpty(partyId))
            {
                throw new ArgumentNullException(nameof(partyId));
            }
        }

        /// <summary>
        /// Actualiza las estadísticas de un usuario.
        /// Incrementa experiencia y puntuación total.
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar</param>
        /// <param name="experienceGained">Experiencia ganada</param>
        /// <param name="scoreGained">Puntuación ganada</param>
        /// <exception cref="UserUpdateException">Si falla la actualización</exception>
        public async Task UpdateUserStatsAsync(string userId, int experienceGained, int scoreGained)
        {
            ValidateRepositoryInitialized();
            ValidateUserId(userId);
            ValidateStatsValues(experienceGained, scoreGained);

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "Experience", FieldValue.Increment(experienceGained) },
                    { "TotalScore", FieldValue.Increment(scoreGained) },
                    { "LastPlayedAt", Timestamp.GetCurrentTimestamp() }
                };
                await UpdateUserAsync(userId, updates);
            }
            catch (UserUpdateException)
            {
                throw; // Re-throw specific exceptions
            }
            catch (FirebaseException ex)
            {
                throw new UserUpdateException($"Failed to update stats for user {userId}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error updating stats for user {userId}", ex);
            }
        }
        
        /// <summary>
        /// Valida los valores de estadísticas.
        /// </summary>
        /// <param name="experienceGained">Experiencia ganada</param>
        /// <param name="scoreGained">Puntuación ganada</param>
        /// <exception cref="ArgumentException">Si los valores son negativos</exception>
        private void ValidateStatsValues(int experienceGained, int scoreGained)
        {
            if (experienceGained < 0)
            {
                throw new ArgumentException("Experience gained cannot be negative", nameof(experienceGained));
            }
            
            if (scoreGained < 0)
            {
                throw new ArgumentException("Score gained cannot be negative", nameof(scoreGained));
            }
        }

        /// <summary>
        /// Obtiene los mejores jugadores por puntuación total.
        /// </summary>
        /// <param name="limit">Límite de resultados a retornar</param>
        /// <returns>Lista de usuarios ordenados por puntuación</returns>
        /// <exception cref="UserRetrievalException">Si falla la recuperación</exception>
        public async Task<List<User>> GetTopPlayersAsync(int limit = 10)
        {
            ValidateRepositoryInitialized();
            ValidateLimit(limit);

            List<User> result = new List<User>();
            
            try
            {
                var query = _database.Collection(_collectionName)
                    .OrderByDescending("TotalScore")
                    .Limit(limit);

                var snapshot = await query.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    result.Add(document.ConvertTo<User>());
                }
            }
            catch (FirebaseException ex)
            {
                throw new UserRetrievalException("Failed to get top players", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Unexpected error getting top players", ex);
            }
            
            return result;
        }
        
        /// <summary>
        /// Valida el límite de resultados.
        /// </summary>
        /// <param name="limit">Límite a validar</param>
        /// <exception cref="ArgumentException">Si el límite no es válido</exception>
        private void ValidateLimit(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentException("Limit must be greater than 0", nameof(limit));
            }
        }

        /// <summary>
        /// Verifica si un usuario existe en la base de datos.
        /// </summary>
        /// <param name="userId">ID del usuario a verificar</param>
        /// <returns>True si el usuario existe</returns>
        /// <exception cref="RepositoryException">Si falla la verificación</exception>
        public async Task<bool> UserExistsAsync(string userId)
        {
            ValidateRepositoryInitialized();
            ValidateUserId(userId);

            bool result = false;
            
            try
            {
                var user = await GetUserAsync(userId);
                result = user != null;
            }
            catch (UserRetrievalException)
            {
                throw; // Re-throw specific exceptions
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error checking if user {userId} exists", ex);
            }
            
            return result;
        }
    }
}
