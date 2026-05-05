using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core;
using ChibitsLink.GameSide;

namespace ChibiCocina.BombTag
{
    public enum BombTagState { Preparing, Countdown, InGame, Result, Ending }

    public class GestorBombTag : BaseMinigameManager
    {
        public static GestorBombTag Instance { get; private set; }

        [Header("Runtime")]
        public float remainingTime;
        public GameObject carrier;
        
        private GameObject spawnedBomb;
        private List<GameObject> ranking = new List<GameObject>();
        private Dictionary<GameObject, PlayerIdentity> playerIdentities = new Dictionary<GameObject, PlayerIdentity>();
        private BombaTag config;
        private float transferCooldown = 0f;
        private int countdownVal;
        private bool isExploding = false;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void OnGamePreparing()
        {
            Debug.Log("[BombTag] OnGamePreparing - Caching identities");
            ranking.Clear();
            playerIdentities.Clear();
            remainingTime = 0;
            carrier = null;
            isExploding = false;
            if (spawnedBomb != null) { Destroy(spawnedBomb); spawnedBomb = null; }

            // Filter and cache immediately to avoid GetComponent during gameplay
            var validPlayers = players.Where(p => p != null).ToList();
            players.Clear();
            foreach (var p in validPlayers)
            {
                var id = p.GetComponent<PlayerIdentity>() ?? p.GetComponentInParent<PlayerIdentity>();
                if (id != null)
                {
                    players.Add(p);
                    playerIdentities[p] = id;
                }
            }
            
            Debug.Log($"[BombTag] {players.Count} players ready.");
            config = FindObjectsByType<BombaTag>(FindObjectsSortMode.None).FirstOrDefault(c => c.bombPrefab != null);
        }

        protected override void OnCountdownTick(int tick) => countdownVal = tick;

        protected override void OnGameStarted()
        {
            StartNewRound();
        }

        private void StartNewRound()
        {
            bool canStartRound = IsGameRunning;
            if (canStartRound)
            {
                var alive = GetAlivePlayers();
                canStartRound = alive.Count > 1;
                if (canStartRound)
                {
                    GameObject victim = alive[Random.Range(0, alive.Count)];
                    Debug.Log($"[BombTag] Starting round. Target: {victim.name}");
                    
                    if (spawnedBomb != null) { Destroy(spawnedBomb); spawnedBomb = null; }
                    if (config != null && config.bombPrefab != null)
                    {
                        spawnedBomb = Instantiate(config.bombPrefab);
                        spawnedBomb.SetActive(true);
                        SetCarrier(victim);
                        remainingTime = config.bombDuration;
                        transferCooldown = 1.5f;
                    }
                }
            }
        }

        private void SetCarrier(GameObject target)
        {
            bool canSetCarrier = target != null && spawnedBomb != null;
            if (canSetCarrier)
            {
                carrier = target;
                spawnedBomb.transform.SetParent(carrier.transform, false);
                float height = (config != null ? config.verticalOffset : 2f) + 0.5f;
                spawnedBomb.transform.localPosition = Vector3.up * height;
                spawnedBomb.transform.localRotation = Quaternion.identity;
                
                foreach(var c in spawnedBomb.GetComponentsInChildren<Collider>()) c.enabled = false;
                foreach(var rb in spawnedBomb.GetComponentsInChildren<Rigidbody>()) rb.isKinematic = true;
            }
        }

        protected override IEnumerator WaitUntilGameEnds()
        {
            bool keepRunning = IsGameRunning;
            while (keepRunning)
            {
                yield return null;

                // Stop logic while exploding
                if (isExploding)
                {
                    keepRunning = IsGameRunning;
                }
                else
                {
                    var alive = GetAlivePlayers();
                    bool hasSingleSurvivor = alive.Count <= 1;
                    if (hasSingleSurvivor)
                    {
                        keepRunning = false;
                    }

                    if (keepRunning && transferCooldown > 0) transferCooldown -= Time.deltaTime;

                    if (keepRunning && remainingTime > 0)
                    {
                        remainingTime -= Time.deltaTime;
                        UpdateBombVisuals();

                        if (transferCooldown <= 0)
                        {
                            bool transferDone = false;
                            foreach (var p in alive)
                            {
                                bool canTransfer = p != carrier
                                                   && !transferDone
                                                   && Vector3.Distance(carrier.transform.position, p.transform.position) < 1.7f;
                                if (canTransfer)
                                {
                                    SetCarrier(p);
                                    transferCooldown = 1.2f;
                                    transferDone = true;
                                }
                            }
                        }

                        if (remainingTime <= 0)
                        {
                            remainingTime = 0;
                            StartCoroutine(ProcessExplosion());
                        }
                    }
                }
            }

            Debug.Log("[BombTag] Bucle finalizado. Esperando un momento antes de resultados...");
            yield return new WaitForSecondsRealtime(0.5f);
            FinalizeGame();
        }

        private void UpdateBombVisuals()
        {
            bool canUpdateBombVisuals = spawnedBomb != null && carrier != null;
            if (canUpdateBombVisuals)
            {
                float height = (config != null ? config.verticalOffset : 2f) + 0.5f;
                spawnedBomb.transform.position = carrier.transform.position + Vector3.up * height;
                spawnedBomb.transform.rotation = Quaternion.identity;

                float freq = remainingTime <= 5f ? 15f : 5f;
                float amp = remainingTime <= 5f ? 0.15f : 0.05f;
                float scale = 0.5f + Mathf.Sin(Time.time * freq) * amp;
                
                Transform model = spawnedBomb.transform.Find("Model") ?? spawnedBomb.transform;
                model.localScale = Vector3.one * scale;

                if (remainingTime > 0 && config.tickSFX != null)
                {
                    var src = spawnedBomb.GetComponent<AudioSource>() ?? spawnedBomb.AddComponent<AudioSource>();
                    if (!src.isPlaying) { src.clip = config.tickSFX; src.loop = true; src.Play(); }
                    src.pitch = remainingTime <= 5f ? 1.5f : 1.0f;
                }
            }
        }

        private IEnumerator ProcessExplosion()
        {
            bool canExplode = !isExploding && carrier != null;
            if (canExplode)
            {
                isExploding = true;

                GameObject victim = carrier;
                Debug.Log($"[BombTag] BOOM! Explosion for {victim.name}");

                if (config != null)
                {
                    if (config.explosionVFX != null) Destroy(Instantiate(config.explosionVFX, victim.transform.position, Quaternion.identity), 2f);
                    if (config.explosionSFX != null) PlaySound(config.explosionSFX);
                }

                if (spawnedBomb != null) { Destroy(spawnedBomb); spawnedBomb = null; }
                if (!ranking.Contains(victim)) ranking.Add(victim);
                carrier = null;

                yield return new WaitForSecondsRealtime(0.3f);
                if (victim != null) victim.SetActive(false);
                
                isExploding = false;

                var alive = GetAlivePlayers();
                if (alive.Count > 1 && IsGameRunning)
                {
                    yield return new WaitForSecondsRealtime(1.5f);
                    StartNewRound();
                }
            }
        }

        private void FinalizeGame()
        {
            var survivors = GetAlivePlayers();
            foreach(var s in survivors) if(!ranking.Contains(s)) ranking.Add(s);

            Debug.Log($"[BombTag] FinalizeGame - Ranking count: {ranking.Count}");
            for (int i = 0; i < ranking.Count; i++)
            {
                GameObject p = ranking[i];
                if (p != null && playerIdentities.TryGetValue(p, out PlayerIdentity id))
                {
                    Debug.Log($"[BombTag] Reporting {id.username}: {(i + 1) * 10}");
                    ReportScore(id.userId, (i + 1) * 10);
                }
            }
        }

        private List<GameObject> GetAlivePlayers()
        {
            List<GameObject> alive = new List<GameObject>();
            if (players == null) return alive;
            for(int i=0; i<players.Count; i++)
            {
                if (players[i] != null && players[i].activeInHierarchy) alive.Add(players[i]);
            }
            return alive;
        }

        // --- UI API ---
        public string GetCarrierName()
        {
            if (carrier == null) return "None";
            return GetPlayerName(carrier);
        }
        public int GetAliveCount() => GetAlivePlayers().Count;
        public List<GameObject> GetWinners() => GetAlivePlayers();
        public List<GameObject> GetEliminationOrder() => ranking;
        public string GetPlayerName(GameObject p)
        {
            if (p == null) return "Unknown";
            
            // Priority 1: Use cached identity username
            if (playerIdentities.TryGetValue(p, out PlayerIdentity id))
            {
                if (!string.IsNullOrEmpty(id.username)) return id.username;
                
                // Priority 2: Modular approach - Fetch from central repository
                if (PlayerManager.Instance != null && !string.IsNullOrEmpty(id.userId))
                {
                    string repoName = PlayerManager.Instance.GetPlayerName(id.userId);
                    if (repoName != "Jugador") return repoName;
                }
            }

            // Priority 3: Fallback - manual search if not cached
            var manualId = p.GetComponent<PlayerIdentity>() ?? p.GetComponentInParent<PlayerIdentity>();
            if (manualId != null && PlayerManager.Instance != null)
            {
                return PlayerManager.Instance.GetPlayerName(manualId.userId);
            }

            return p.name;
        }

        public BombTagState CurrentState => currentState switch {
            MinigameState.Preparing => BombTagState.Preparing,
            MinigameState.Countdown => BombTagState.Countdown,
            MinigameState.InGame    => BombTagState.InGame,
            MinigameState.Result    => BombTagState.Result,
            MinigameState.Ending    => BombTagState.Ending,
            _                       => BombTagState.Preparing
        };
        public float remainingBombTime => remainingTime;
        public int currentCountdown => countdownVal;
        public float resultTimeRemaining => resultTime;
    }
}
