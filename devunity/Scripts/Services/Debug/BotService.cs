using System.Collections.Generic;
using UnityEngine;
using ChibiCocina.Core;
using ChibiCocina.Core.Exceptions;
using ChibiCocina.Services;
using ChibitsLink.GameSide;

namespace ChibiCocina.Services
{
    /// <summary>
    /// Servicio para gestión de bots jugadores.
    /// Crea, maneja y elimina bots para testing y desarrollo.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Utilizado para simular jugadores en modo desarrollo.
    /// Permite testing de funcionalidades sin jugadores reales.
    /// Se integra con PlayerManager para gestión de jugadores.
    /// </remarks>
    public class BotService : MonoBehaviour
    {
        /// <summary>Instancia global del servicio (patrón Singleton)</summary>
        public static BotService Instance { get; private set; }
        
        [Header("Configuración de Bots")]
        /// <summary>Número máximo de bots permitidos</summary>
        public int maxBots = 10;
        /// <summary>IDs de personajes por defecto para bots</summary>
        public string[] defaultCharacterIds = { "char_1", "char_2", "char_3", "char_4" };
        /// <summary>Nivel mínimo para bots</summary>
        public int minLevel = 1;
        /// <summary>Nivel máximo para bots</summary>
        public int maxLevel = 15;
        
        /// <summary>Lista de bots activos</summary>
        private List<BotModel> activeBots;
        /// <summary>Contador para IDs únicos de bots</summary>
        private int botCounter;
        
        /// <summary>
        /// Inicialización del servicio de bots.
        /// Establece el patrón Singleton y configura estado inicial.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeBotService();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Inicializa el servicio de bots.
        /// Prepara listas y contadores para gestión de bots.
        /// </summary>
        private void InitializeBotService()
        {
            activeBots = new List<BotModel>();
            botCounter = 0;
            
            DebugLogService.Instance?.Log(DebugModule.Player, "[BotService] Servicio inicializado");
        }
        
        /// <summary>
        /// Spawnea una cantidad específica de bots.
        /// Crea bots con datos aleatorios y los registra en el sistema.
        /// </summary>
        /// <param name="count">Número de bots a crear</param>
        /// <returns>Número de bots creados exitosamente</returns>
        /// <exception cref="BotServiceException">Si hay errores en la creación</exception>
        public int SpawnBots(int count)
        {
            if (count <= 0)
            {
                throw new BotServiceException("La cantidad de bots debe ser mayor a 0");
            }
            
            if (activeBots.Count + count > maxBots)
            {
                throw new BotServiceException($"No se pueden crear {count} bots. Máximo permitido: {maxBots}");
            }
            
            if (PlayerManager.Instance == null)
            {
                throw new BotServiceException("PlayerManager no encontrado. El sistema de red debe estar iniciado.");
            }
            
            int spawnedCount = 0;
            BotModel bot;
            
            for (int i = 0; i < count; i++)
            {
                try
                {
                    bot = CreateBot();
                    activeBots.Add(bot);
                    spawnedCount++;
                    
                    DebugLogService.Instance?.Log(DebugModule.Player, $"Bot creado: {bot.Id} ({bot.Name})");
                }
                catch (System.NullReferenceException ex)
                {
                    DebugLogService.Instance?.LogError(DebugModule.Player, $"Null reference error creating bot {i + 1}: {ex.Message}");
                }
                catch (System.ArgumentException ex)
                {
                    DebugLogService.Instance?.LogError(DebugModule.Player, $"Argument error creating bot {i + 1}: {ex.Message}");
                }
            }
            
            DebugLogService.Instance?.Log(DebugModule.Player, $"Total bots spawnados: {spawnedCount}/{count}");
            return spawnedCount;
        }
        
        /// <summary>
        /// Crea un nuevo bot con datos aleatorios.
        /// Genera ID, nombre, personaje y nivel aleatorios.
        /// </summary>
        /// <returns>Nuevo modelo de bot creado</returns>
        private BotModel CreateBot()
        {
            botCounter++;
            string botId = $"BOT_{botCounter:D4}";
            string botName = $"Bot {botCounter}";
            string characterId = GetRandomCharacterId();
            int level = Random.Range(minLevel, maxLevel + 1);
            
            var bot = new BotModel
            {
                Id = botId,
                Name = botName,
                CharacterId = characterId,
                Level = level,
                CreatedAt = System.DateTime.Now,
                IsActive = true
            };
            
            // Registrar bot en PlayerManager
            PlayerManager.Instance.HandlePlayerJoin(botId, characterId, botName, level);
            
            return bot;
        }
        
        /// <summary>
        /// Obtiene un ID de personaje aleatorio.
        /// Prioriza personajes disponibles en PlayerManager.
        /// </summary>
        /// <returns>ID de personaje aleatorio</returns>
        private string GetRandomCharacterId()
        {
            var availableChars = PlayerManager.Instance?.GetAllCharacterIds();
            
            bool hasAvailableChars = availableChars != null && availableChars.Count > 0;
            if (hasAvailableChars)
            {
                int randomIndex = Random.Range(0, availableChars.Count);
                return availableChars[randomIndex];
            }
            
            // Fallback a personajes por defecto
            int fallbackIndex = Random.Range(0, defaultCharacterIds.Length);
            return defaultCharacterIds[fallbackIndex];
        }
        
        /// <summary>
        /// Remueve un bot específico del sistema.
        /// Desactiva el bot y notifica a PlayerManager.
        /// </summary>
        /// <param name="botId">ID del bot a remover</param>
        public void RemoveBot(string botId)
        {
            BotModel bot = FindBot(botId);
            if (bot != null)
            {
                bot.IsActive = false;
                activeBots.Remove(bot);
                
                // Notificar a PlayerManager
                PlayerManager.Instance?.HandlePlayerDisconnect(botId);
                
                DebugLogService.Instance?.Log(DebugModule.Player, $"Bot removido: {botId}");
            }
            else
            {
                DebugLogService.Instance?.LogWarning(DebugModule.Player, $"Bot no encontrado: {botId}");
            }
        }
        
        /// <summary>
        /// Remueve todos los bots activos del sistema.
        /// Utiliza RemoveBot para cada bot individualmente.
        /// </summary>
        public void RemoveAllBots()
        {
            var botsToRemove = new List<BotModel>(activeBots);
            
            foreach (var bot in botsToRemove)
            {
                RemoveBot(bot.Id);
            }
            
            DebugLogService.Instance?.Log(DebugModule.Player, "Todos los bots han sido removidos");
        }
        
        /// <summary>
        /// Busca un bot activo por su ID.
        /// Solo busca bots que estén activos.
        /// </summary>
        /// <param name="botId">ID del bot a buscar</param>
        /// <returns>Bot encontrado o null</returns>
        public BotModel FindBot(string botId)
        {
            return activeBots.Find(bot => bot.Id == botId && bot.IsActive);
        }
        
        /// <summary>
        /// Obtiene la lista de bots activos.
        /// Retorna una copia de la lista de bots activos.
        /// </summary>
        /// <returns>Lista de bots activos</returns>
        public List<BotModel> GetActiveBots()
        {
            return new List<BotModel>(activeBots.FindAll(bot => bot.IsActive));
        }
        
        /// <summary>
        /// Obtiene el número de bots activos.
        /// </summary>
        /// <returns>Número de bots activos</returns>
        public int GetActiveBotCount()
        {
            return activeBots.Count;
        }
        
        /// <summary>
        /// Verifica si un ID de jugador corresponde a un bot.
        /// Comprueba si el ID comienza con "BOT_".
        /// </summary>
        /// <param name="playerId">ID del jugador a verificar</param>
        /// <returns>True si es un bot</returns>
        public bool IsBot(string playerId)
        {
            return playerId.StartsWith("BOT_");
        }
        
        /// <summary>
        /// Actualiza el nivel de un bot específico.
        /// Modifica el nivel del bot en el modelo local.
        /// </summary>
        /// <param name="botId">ID del bot a actualizar</param>
        /// <param name="newLevel">Nuevo nivel del bot</param>
        public void UpdateBotLevel(string botId, int newLevel)
        {
            BotModel bot = FindBot(botId);
            if (bot != null)
            {
                bot.Level = newLevel;
                DebugLogService.Instance?.Log(DebugModule.Player, $"Bot {botId} nivel actualizado a {newLevel}");
            }
        }
        
        /// <summary>
        /// Actualiza el personaje de un bot específico.
        /// Modifica el ID del personaje en el modelo local.
        /// </summary>
        /// <param name="botId">ID del bot a actualizar</param>
        /// <param name="newCharacterId">Nuevo ID de personaje</param>
        public void UpdateBotCharacter(string botId, string newCharacterId)
        {
            BotModel bot = FindBot(botId);
            if (bot != null)
            {
                bot.CharacterId = newCharacterId;
                DebugLogService.Instance?.Log(DebugModule.Player, $"Bot {botId} personaje actualizado a {newCharacterId}");
            }
        }
}
