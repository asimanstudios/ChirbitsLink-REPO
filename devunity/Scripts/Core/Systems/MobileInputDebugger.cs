using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ChibitsLink.GameSide;

namespace ChibitsLink.Core.Systems
{
    /// <summary>
    /// Herramienta de diagnóstico para input móvil.
    /// Muestra estado en tiempo real de:
    ///  - Servidor TCP (activo / puerto)
    ///  - Cada jugador: userId, GameObject asociado, IChibitsController encontrado
    /// </summary>
    /// <remarks>
    /// SOLO activo en Editor o Builds de Desarrollo.
    /// Se desactiva automáticamente en Release.
    /// Añadir este componente al mismo GameObject que TcpServer o a un Manager vacío.
    /// </remarks>
    public class MobileInputDebugger : MonoBehaviour
    {
        [Header("Configuración de Visualización")]
        /// <summary>Mostrar en pantalla</summary>
        [Tooltip("Esquina de pantalla donde aparece el panel HUD de depuración")]
        public bool showOnScreen = true;

        /// <summary>Tamaño de fuente para el panel</summary>
        [Tooltip("Tamaño de fuente para el panel de depuración")]
        public int fontSize = 14;

        /// <summary>Estilo del panel</summary>
        private GUIStyle _panelStyle;
        /// <summary>Estilo del texto</summary>
        private GUIStyle _textStyle;
        /// <summary>Indica si los estilos están listos</summary>
        private bool _stylesReady;

        /// <summary>
        /// Inicialización del depurador.
        /// Se destruye automáticamente en builds de release.
        /// </summary>
        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            // In release builds we don't want this overhead
            Destroy(this);
#else
            Debug.Log("[MobileInputDebugger] Activated. Only visible in Editor/Development Build.");
#endif
        }

        /// <summary>
        /// Inicializa los estilos GUI para el panel de depuración.
        /// </summary>
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
