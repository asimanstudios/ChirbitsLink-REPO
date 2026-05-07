using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ChibitsLink.GameSide;

namespace ChibitsLink.Core.Systems
{
    /// <summary>
    /// Mobile input diagnostic tool.
    /// Displays real-time status in OnGUI of:
    ///  - TCP server (active / port)
    ///  - Each player: userId, associated GameObject, found IChibitsController
    ///
    /// ONLY active in Editor or Development Builds. Auto-deactivates in Release.
    /// Add this component to the same GameObject as TcpServer or to an empty Manager.
    /// </summary>
    public class MobileInputDebugger : MonoBehaviour
    {
        [Header("Display Configuration")]
        [Tooltip("Screen corner where debug HUD panel appears")]
        public bool showOnScreen = true;

        [Tooltip("Font size for debug panel")]
        public int fontSize = 14;

        private GUIStyle _panelStyle;
        private GUIStyle _textStyle;
        private bool _stylesReady;

        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            // In release builds we don't want this overhead
            Destroy(this);
#else
            Debug.Log("[MobileInputDebugger] Activated. Only visible in Editor/Development Build.");
#endif
        }

        private void InitStyles()
        {
            if (!_stylesReady)
            {
                _panelStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(8, 8, 8, 8)
                };

                _textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    wordWrap = false,
                    richText = true
                };

                _stylesReady = true;
            }
        }

        private void OnGUI()
        {
            if (mostrarEnPantalla)
            {
                InitStyles();

                var sb = new StringBuilder();

            // ── TCP Server ──────────────────────────────────────────────
            bool tcpOk = TcpServer.Instance != null;
            sb.AppendLine(tcpOk
                ? "<color=lime>● TCP Server activo</color>"
                : "<color=red>✖ TcpServer.Instance == null</color>");

            // ── PlayerManager ───────────────────────────────────────────
            bool pmOk = PlayerManager.Instance != null;
            sb.AppendLine(pmOk
                ? "<color=lime>● PlayerManager activo</color>"
                : "<color=red>✖ PlayerManager.Instance == null</color>");

            if (pmOk)
            {
                // Usamos reflection para leer el diccionario privado de jugadores
                // sin modificar PlayerManager.
                var field = typeof(PlayerManager).GetField("_playerObjects",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    var dict = field.GetValue(PlayerManager.Instance)
                        as Dictionary<string, GameObject>;

                    if (dict != null && dict.Count > 0)
                    {
                        sb.AppendLine($"\n<b>Jugadores ({dict.Count}):</b>");
                        int idx = 1;
                        foreach (var kv in dict)
                        {
                            string uid = kv.Key;
                            GameObject obj = kv.Value;

                            if (obj == null)
                            {
                                sb.AppendLine($"  P{idx} <color=red>[GameObject destruido]</color>  uid={uid}");
                            }
                            else
                            {
                                var ctrl = obj.GetComponentInChildren<PlayerManager.IChibitsController>(true);
                                string ctrlStatus = ctrl != null
                                    ? $"<color=lime>IChibitsController ✔ ({ctrl.GetType().Name})</color>"
                                    : "<color=red>IChibitsController ✖ (no encontrado)</color>";

                                sb.AppendLine($"  P{idx} <b>{obj.name}</b>");
                                sb.AppendLine($"      uid: {uid}");
                                sb.AppendLine($"      {ctrlStatus}");
                            }
                            idx++;
                        }
                    }
                    else
                    {
                        sb.AppendLine("  <color=yellow>(sin jugadores registrados)</color>");
                    }
                }
            }

            // ── Render ──────────────────────────────────────────────────
            float w = 420f;
            Rect panelRect = new Rect(10, 10, w, 0); // altura auto

            // Calcular altura real del contenido
            float contentH = _textStyle.CalcHeight(new GUIContent(sb.ToString()), w - 16f) + 20f;
            panelRect.height = contentH;

                GUI.Box(panelRect, GUIContent.none, _panelStyle);
                GUI.Label(new Rect(panelRect.x + 8, panelRect.y + 8,
                                   panelRect.width - 16, contentH), sb.ToString(), _textStyle);
            }
        }
    }
}
