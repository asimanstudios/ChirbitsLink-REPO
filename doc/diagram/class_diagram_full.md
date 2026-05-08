# Chirbits UNITY — Diagramas de Clases

> Empieza por la **Vista 0** (visión global) y luego consulta las vistas detalladas 1–5 por subsistema.

---

## Vista 0 — Visión Global

```mermaid
---
config:
  layout: elk
  theme: redux
  look: handDrawn
---
classDiagram
    %% =========================================
    %%              UI
    %% =========================================
    namespace UI {
        class LobbyUIManager {
            <<View>>
            +lobbyManager : LobbyManager
            +tcpServer : TcpNetworkServer
            +votingDuration : float
            +UpdatePlayerList(names)
            -OnCreateLobby()
            -MonitorLobbyState() Task
            -StartVotingCountdown()
        }
    }

    %% =========================================
    %%              CORE
    %% =========================================
    namespace Core {
        class LobbyManager {
            <<Singleton>>
            +RoomCode : string
            +GameState : string
            +SessionScores : Dict~string,int~
            +CreateNewLobbyAsync() Task~Party~
            +DecideWinnerAndStartGameAsync()
            +UpdatePlayerScoreAsync(code, uid, pts)
            +FinalizePartyScoresAsync(code)
            +ReturnToLobby()
            +SeedDataAsync(chars, games)
        }

        class PlayerManager {
            <<Singleton>>
            +spawnPoints : List~Transform~
            +HandlePlayerJoin(uid, charId, name, lvl)
            +HandleControllerInput(uid, json)
            +HandlePlayerDisconnect(uid)
            +GetPlayerName(uid) string
            +CleanupAllBots()
            -SpawnPlayer(uid, charId, index)
            -ConfigureCameras()
        }

        class GameManager {
            <<Singleton>>
            +maxPlayers : int
            +gameTime : float
            +StartGame()
            +EndGame()
            +CanStartGame() bool
            +GetRemainingTime() float
        }
    }

    %% =========================================
    %%              NETWORK
    %% =========================================
    namespace Network {
        class TcpNetworkServer {
            <<Singleton · TCP>>
            +port : int
            +IsRunning : bool
            +StartServerAsync() Task
            +StopServer()
            +GetRoomCode() string
            +SetRoomCode(code)
            +BroadcastMessage(msg)
            +SendMessageToUser(uid, msg)
            -HandleConnectCommand()
            -HandleInputCommand()
        }

        class FirebaseManager {
            <<Singleton · Firestore>>
            +PartyRepository : PartyRepository
            +UserRepository : UserRepository
            +FinalizePartyScoresAsync()
            -InitializeFirebase()
        }
    }

    %% =========================================
    %%             PLAYER
    %% =========================================
    namespace Player {
        class PlayerMovement {
            <<IChibitsController · IPushable>>
            +walkSpeed : float
            +jumpForce : float
            +IsGrounded : bool
            +ProcessJoystick(x, y)
            +ProcessButton(id, state)
            +ApplyPush(force, duration)
            +SetStats(walk, run, acc, air, jump)
        }

        class PlayerIdentity {
            +userId : string
            +username : string
            +level : int
        }
    }

    %% =========================================
    %%            MINIGAMES
    %% =========================================
    namespace Minigames {
        class BaseMinigameManager {
            <<Abstract · FSM>>
            #countdownTime : float
            #currentState : MinigameState
            #players : List~GameObject~
            +IsGameRunning : bool
            +GetIdentity(obj) PlayerIdentity
            #ReportScore(uid, pts)
            #OnGameStarted()*
            #WaitUntilGameEnds()* IEnumerator
        }

        class BombTagGameManager {
            <<Singleton>>
            +carrier : GameObject
            +remainingTime : float
            +GetCarrierName() string
            +GetAliveCount() int
        }

        class CoinCollectorGameManager {
            <<Singleton>>
            +remainingTime : float
            +SumarMoneda(uid, pts)
            +ObtenerRanking() List
        }

        class HookPartyManager {
            <<Singleton>>
            +TimeRemaining : float
            +IsPlaying : bool
            -EscanearYConfigurarJugadores()
        }
    }

    %% =========================================
    %%               DATA
    %% =========================================
    namespace Data {
        class Party {
            <<Firestore>>
            +RoomCode : string
            +GameState : string
            +PlayerIds : List~string~
            +Votes : Dict~string,int~
            +PlayerScores : Dict~string,int~
        }

        class User {
            <<Firestore>>
            +Id : string
            +Username : string
            +Level : int
            +Experience : int
            +GameHistory : List~string~
        }
    }

    %% =========================================
    %%          RELATIONSHIPS (Clean)
    %% =========================================
    LobbyUIManager --> LobbyManager
    LobbyUIManager --> TcpNetworkServer

    LobbyManager --> PlayerManager
    LobbyManager --> Party
    LobbyManager ..> FirebaseManager
    LobbyManager ..> GameManager

    TcpNetworkServer --> PlayerManager
    FirebaseManager ..> Party
    FirebaseManager ..> User

    PlayerManager --> PlayerMovement
    PlayerManager --> PlayerIdentity

    BombTagGameManager --|> BaseMinigameManager
    HookPartyManager --|> BaseMinigameManager
    BaseMinigameManager ..> LobbyManager
```

---

## Vista 1 — Core Managers

```mermaid
classDiagram
    direction LR

    class LobbyUIManager {
        <<View>>
        +votingDuration : float
        +UpdatePlayerList(names)
        +RefreshNetworkInterfaces()
        -OnCreateLobby()
        -MonitorLobbyState() Task
        -StartVotingCountdown()
    }

    class LobbyManager {
        <<Singleton>>
        +RoomCode : string
        +GameState : string
        +SessionScores : Dict~string,int~
        +CreateNewLobbyAsync() Task
        +DecideWinnerAndStartGameAsync()
        +ToggleReadyAsync(code, uid, ready)
        +RegisterVoteAsync(code, gameId)
        +UpdatePlayerScoreAsync(code, uid, pts)
        +FinalizePartyScoresAsync(code)
        +ReturnToLobby()
        +SeedDataAsync(chars, games)
        +GetAvailableNetworkInterfaces() List
    }

    class PlayerManager {
        <<Singleton>>
        +spawnPoints : List~Transform~
        +lobbyCharacterPrefabs : List
        +gameCharacterPrefabs : List
        +HandlePlayerJoin(uid, charId, name, lvl)
        +HandleCharacterSync(uid, newCharId)
        +HandleControllerInput(uid, json)
        +HandlePlayerDisconnect(uid)
        +GetPlayerName(uid) string
        +CleanupAllBots()
        -SpawnPlayer(uid, charId, index)
        -ConfigureCameras()
        -DelayedSpawnRoutine() IEnumerator
    }

    class GameManager {
        <<Singleton>>
        +maxPlayers : int
        +gameTime : float
        +OnGameStateChanged : Action
        +StartGame()
        +EndGame()
        +PlayerConnected()
        +PlayerDisconnected()
        +CanStartGame() bool
        +GetRemainingTime() float
    }

    class SplitScreenManager {
        <<Singleton>>
        +cameras : Camera[]
        +splitScreenActive : bool
        +UpdatePlayers(count)
        +GetPlayerCamera(index) Camera
    }

    class UnityMainThreadDispatcher {
        <<Singleton · Utility>>
        +Enqueue(action)
        +EnqueueAsync(action) Task
    }

    class IChibitsController {
        <<Interface>>
        +ProcessJoystick(x, y)
        +ProcessButton(id, state)
    }

    LobbyUIManager --> LobbyManager             : llama a
    LobbyUIManager --> GameManager              : lee estado
    LobbyManager   --> PlayerManager            : controla spawn
    LobbyManager   ..> UnityMainThreadDispatcher : dispatches
    LobbyManager   ..> GameManager              : notifica estado
    PlayerManager  ..> IChibitsController       : enruta input a
    GameManager    --> SplitScreenManager       : configura cámaras
```

---

## Vista 2 — Red y Persistencia

```mermaid
classDiagram
    direction LR

    class TcpNetworkServer {
        <<Singleton · TCP>>
        +port : int
        +IsRunning : bool
        +ConnectedClientCount : int
        +StartServerAsync() Task~bool~
        +StopServer()
        +GetRoomCode() string
        +SetRoomCode(code)
        +BroadcastMessage(msg)
        +SendMessageToUser(uid, msg)
        +RefreshUIPlayerList()
        -HandleClientAsync(client) Task
        -HandleConnectCommand(client, data)
        -HandleDisconnectCommand(client, data)
        -HandleInputCommand(client, data)
        -DisconnectClient(client)
    }

    class FirebaseManager {
        <<Singleton · Firestore>>
        +PartyRepository : PartyRepository
        +UserRepository : UserRepository
        +SessionRepository : SessionRepository
        +UpdateGameSession(host, players, pts)
        +FinalizePartyScoresAsync(code, scores) Task
        -InitializeFirebase()
    }

    class UserRepository {
        +GetUserAsync(uid) Task~User~
        +CreateUserAsync(user)
        +UpdateUserStatsAsync(uid, xp, lvl)
        +AddGameToUserHistoryAsync(uid, code)
        +UserExistsAsync(uid) Task~bool~
    }

    class PartyRepository {
        +GetPartyAsync(code) Task~Party~
        +CreatePartyAsync(party)
        +UpdatePartyAsync(code, updates)
        +DeletePartyAsync(code)
    }

    class SessionRepository {
        +UpdateGameSessionAsync(host, players, pts)
    }

    class NetworkingEvents {
        <<static>>
        +event OnClientConnected
        +event OnClientDisconnected
        +event OnMessageReceived
        +RaiseConnected(uid, endpoint)
        +RaiseDisconnected(uid)
        +RaiseMessageReceived(args)
    }

    class BotService {
        <<Singleton>>
        +maxBots : int
        +SpawnBots(count) int
        +RemoveBot(botId)
        +RemoveAllBots()
        +IsBot(playerId) bool
        +GetActiveBotCount() int
    }

    FirebaseManager  o-- UserRepository     : owns
    FirebaseManager  o-- PartyRepository    : owns
    FirebaseManager  o-- SessionRepository  : owns
    TcpNetworkServer ..> NetworkingEvents   : raises
    TcpNetworkServer ..> FirebaseManager    : informa scores
    BotService       ..> TcpNetworkServer   : simula conexiones
```

---

## Vista 3 — Sistema de Jugador

```mermaid
classDiagram
    direction LR

    class IChibitsController {
        <<Interface>>
        +ProcessJoystick(x, y)
        +ProcessButton(id, state)
    }

    class IPushable {
        <<Interface>>
        +ApplyPush(force, duration)
    }

    class PlayerMovement {
        <<MonoBehaviour>>
        +walkSpeed : float
        +runSpeed : float
        +jumpForce : float
        +IsGrounded : bool
        +ProcessJoystick(x, y)
        +ProcessButton(id, state)
        +ApplyPush(force, duration)
        +EnableController(active)
        +SetStats(walk, run, acc, air, jump)
    }

    class PlayerAnimationController {
        -animator : Animator
        +UpdateAnimator()
        +TriggerJump()
        +SetMovementState(isRunning, isGrounded)
    }

    class PlayerAudioController {
        -audioSource : AudioSource
        +PlayJumpSound()
        +UpdateStepAudio(grounded, speed)
    }

    class PlayerCombatController {
        +combatWalkSpeed : float
        +combatJumpForce : float
        +lobbyWalkSpeed : float
        +lobbyJumpForce : float
        +ApplySceneSpecificStats()
        -ApplyCombatStats()
        -ApplyLobbyStats()
    }

    class PlayerIdentity {
        +userId : string
        +username : string
        +level : int
    }

    class AudioService {
        +Initialize(source)
        +PlayJumpSound(clip)
        +PlayFootstepSound(clip)
        +SetVolume(volume)
    }

    IChibitsController  <|.. PlayerMovement
    IPushable           <|.. PlayerMovement
    PlayerMovement      o-- PlayerAnimationController
    PlayerMovement      o-- PlayerAudioController
    PlayerMovement      o-- PlayerCombatController
    PlayerMovement      --> PlayerIdentity
    PlayerAudioController --> AudioService
```

---

## Vista 4 — Minijuegos

```mermaid
classDiagram
    direction LR

    class IMinigameManager {
        <<Interface>>
        +StartGame()
        +EndGame()
        +IsGameRunning : bool
    }

    class BaseMinigameManager {
        <<Abstract · MonoBehaviour>>
        #countdownTime : float
        #resultTime : float
        #currentState : MinigameState
        #players : List~GameObject~
        #identityCache : Dict
        +IsGameRunning : bool
        +GetIdentity(obj) PlayerIdentity
        +PlaySound(clip)
        +StartGame()
        +EndGame()
        #ScanPlayers()
        #ReportScore(uid, pts)
        #OnGamePreparing()
        #OnCountdownTick(tick)
        #OnGameStarted()*
        #WaitUntilGameEnds()* IEnumerator
        #OnGameResults()
    }

    class BombTagGameManager {
        <<Singleton>>
        +carrier : GameObject
        +remainingTime : float
        +CurrentState : BombTagState
        +GetCarrierName() string
        +GetAliveCount() int
        +GetWinners() List
        +GetEliminationOrder() List
    }

    class BombTagPhysics {
        +transferDistance : float
        +initialCooldownTime : float
        +SpawnBomb(target)
        +SetCarrier(newCarrier)
        +UpdateTransfer(alivePlayers) bool
        +UpdateBombVisuals(remainingTime)
        +ProcessExplosion() IEnumerator
        +DestroyBomb()
        +IsExploding() bool
    }

    class BombTagScoring {
        +Initialize(playerIdentities)
        +AddElimination(player)
        +ProcessFinalScoring(callback)
        +GetEliminationOrder() List
        +GetPlayerName(player) string
    }

    class BombaTag {
        <<Config>>
        +bombPrefab : GameObject
        +bombDuration : float
        +tickSFX : AudioClip
        +explosionVFX : GameObject
        +explosionSFX : AudioClip
    }

    class CoinCollectorGameManager {
        <<Singleton>>
        +remainingTime : float
        +CurrentState : CoinCollectorState
        +SumarMoneda(uid, pts)
        +ObtenerRanking() List
        +GetVivos() List
    }

    class CoinSpawner {
        <<Singleton>>
        +maxCoins : int
        +spawnInterval : float
        +coinTiers : List~CoinTier~
    }

    class HookPartyManager {
        <<Singleton>>
        +gameDurationSeconds : float
        +TimeRemaining : float
        +CurrentState : HookPartyState
        +IsPlaying : bool
        +playerAttachmentPrefab : GameObject
        +hookTipPrefab : GameObject
        -EscanearYConfigurarJugadores()
    }

    class PlayerHookSystem {
        +hookMaxDistance : float
        +retractForce : float
        +swingForce : float
        +SetupUX(feet, tip, sfx...)
        -TryShootHook()
        -ReleaseHook()
        -ManageRopeLength()
        -SwingWithJoystick()
    }

    class HookPartyController {
        <<IChibitsController>>
        +AimDirection : Vector2
        +ConsumeHookTrigger() bool
        +ProcessJoystick(x, y)
        +ProcessButton(id, state)
    }

    class ScoreManager {
        <<Singleton>>
        +AddScore(uid, pts)
        +GetAllScores() Dictionary
        +GetScore(uid) int
    }

    class BaseCollectible {
        <<Abstract>>
        +valor : int
        +efectoColeccion : GameObject
        +sonidoColeccion : AudioClip
        #CanBeCollected()* bool
        #OnCollect(userId)*
        #OnTriggerEnter(other)
        #TriggerVisualEffects()
    }

    IMinigameManager        <|.. BaseMinigameManager
    BaseMinigameManager     <|-- BombTagGameManager
    BaseMinigameManager     <|-- CoinCollectorGameManager
    BombTagGameManager      o-- BombTagPhysics
    BombTagGameManager      o-- BombTagScoring
    BombTagGameManager      --> BombaTag
    HookPartyManager        o-- ScoreManager
    HookPartyManager        ..> PlayerHookSystem    : inyecta
    HookPartyManager        ..> HookPartyController : inyecta
    PlayerHookSystem        --> HookPartyController
    CoinSpawner             ..> CoinCollectorGameManager
    BaseCollectible         ..> BaseMinigameManager
```

---

## Vista 5 — Modelos y Excepciones

```mermaid
classDiagram
    direction LR

    class User {
        <<FirestoreData>>
        +Id : string
        +Email : string
        +Username : string
        +SelectedCharacterId : string
        +Level : int
        +Experience : int
        +GameHistory : List~string~
        +XpClaimedParties : List~string~
    }

    class Party {
        <<FirestoreData>>
        +RoomCode : string
        +Name : string
        +IpAddress : string
        +Port : int
        +GameState : string
        +PlayerIds : List~string~
        +ReadyPlayerIds : List~string~
        +Votes : Dict~string,int~
        +PlayerScores : Dict~string,int~
        +ParticipantNames : Dict~string,string~
        +ParticipantCharacters : Dict~string,string~
    }

    class Character {
        <<FirestoreData>>
        +Id : string
        +Name : string
        +ImageUrl : string
        +Description : string
    }

    class Game {
        <<FirestoreData>>
        +Id : string
        +Name : string
        +Description : string
        +ImageUrl : string
    }

    class RoomState {
        <<static constants>>
        LOBBY
        VOTING
        IN_GAME
        CLOSED
    }

    class MinigameState {
        <<enum>>
        Preparing
        Countdown
        InGame
        Result
        Ending
    }

    class ChirbitsGameException {
        <<Exception>>
    }
    class FirestoreSyncException {
        +Collection : string
    }
    class SocketProtocolException {
        +RawMessage : string
    }
    class SessionLogicException
    class RepositoryException { <<Exception>> }
    class PartyException
    class UserException
    class SessionException
    class NetworkServiceException

    Party --> Character : ref por Id
    Party --> Game      : ref por Id
    User  --> Character : SelectedCharacterId

    FirestoreSyncException  --|> ChirbitsGameException
    SocketProtocolException --|> ChirbitsGameException
    SessionLogicException   --|> ChirbitsGameException
    PartyException          --|> RepositoryException
    UserException           --|> RepositoryException
    SessionException        --|> RepositoryException
    NetworkServiceException --|> RepositoryException
```
