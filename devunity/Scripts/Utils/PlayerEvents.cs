using UnityEngine;
using UnityEngine.Events;
using ChibiCocina.Models;

[System.Serializable]
public class PlayerMoveEvent : UnityEvent<Vector2> { }

[System.Serializable]
public class PlayerJumpEvent : UnityEvent { }

[System.Serializable]
public class PlayerInteractEvent : UnityEvent { }

