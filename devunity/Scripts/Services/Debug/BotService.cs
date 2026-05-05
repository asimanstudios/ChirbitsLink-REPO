using System.Collections.Generic;
using UnityEngine;
using ChibiCocina.Core;
using ChibiCocina.Core.Exceptions;
using ChibiCocina.Services;
using ChibitsLink.GameSide;

namespace ChibiCocina.Services
{
    public class BotService : MonoBehaviour
    {
        public static BotService Instance { get; private set; }
        
        [Header("Configuración de Bots")]
        public int maxBots = 10;
        public string[] defaultCharacterIds = { "char_1", "char_2", "char_3", "char_4" };
        public int minLevel = 1;
        public int maxLevel = 15;
        
        private List<BotModel> activeBots;
        private int botCounter;
        
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
        
        private void InitializeBotService()
        {
            activeBots = new List<BotModel>();
            botCounter = 0;
            
            DebugLogService.Instance?.Log(DebugModule.Player, "[BotService] Servicio inicializado");
        }
        
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
            
            for (int i = 0; i < count; i++)
            {
                try
                {
                    BotModel bot = CreateBot();
                    activeBots.Add(bot);
                    spawnedCount++;
                    
                    DebugLogService.Instance?.Log(DebugModule.Player, $"Bot creado: {bot.Id} ({bot.Name})");
                }
                catch (System.Exception ex)
                {
                    DebugLogService.Instance?.LogError(DebugModule.Player, $"Error creando bot {i + 1}: {ex.Message}");
                }
            }
            
            DebugLogService.Instance?.Log(DebugModule.Player, $"Total bots spawnados: {spawnedCount}/{count}");
            return spawnedCount;
        }
        
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
        
        public void RemoveBot(string botId)
        {
            BotModel bot = FindBot(botId);
            if (bot == null)
            {
                DebugLogService.Instance?.LogWarning(DebugModule.Player, $"Bot no encontrado: {botId}");
                return;
            }
            
            bot.IsActive = false;
            activeBots.Remove(bot);
            
            // Notificar a PlayerManager
            PlayerManager.Instance?.HandlePlayerDisconnect(botId);
            
            DebugLogService.Instance?.Log(DebugModule.Player, $"Bot removido: {botId}");
        }
        
        public void RemoveAllBots()
        {
            var botsToRemove = new List<BotModel>(activeBots);
            
            foreach (var bot in botsToRemove)
            {
                RemoveBot(bot.Id);
            }
            
            DebugLogService.Instance?.Log(DebugModule.Player, "Todos los bots han sido removidos");
        }
        
        public BotModel FindBot(string botId)
        {
            return activeBots.Find(bot => bot.Id == botId && bot.IsActive);
        }
        
        public List<BotModel> GetActiveBots()
        {
            return new List<BotModel>(activeBots.FindAll(bot => bot.IsActive));
        }
        
        public int GetActiveBotCount()
        {
            return activeBots.Count;
        }
        
        public bool IsBot(string playerId)
        {
            return playerId.StartsWith("BOT_");
        }
        
        public void UpdateBotLevel(string botId, int newLevel)
        {
            BotModel bot = FindBot(botId);
            if (bot != null)
            {
                bot.Level = newLevel;
                DebugLogService.Instance?.Log(DebugModule.Player, $"Bot {botId} nivel actualizado a {newLevel}");
            }
        }
        
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
    
    [System.Serializable]
    public class BotModel
    {
        public string Id;
        public string Name;
        public string CharacterId;
        public int Level;
        public System.DateTime CreatedAt;
        public bool IsActive;
    }
}
