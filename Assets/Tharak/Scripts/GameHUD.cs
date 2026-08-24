using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafehouseDesk
{
    // All UI is drawn in code (IMGUI) so the MVP needs no Canvas, fonts, or art.
    public class GameHUD : MonoBehaviour
    {
        GUIStyle label, big, box, btn;

        void EnsureStyles()
        {
            if (label != null) return;
            label = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
            big   = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            box   = new GUIStyle(GUI.skin.box);
            btn   = new GUIStyle(GUI.skin.button) { fontSize = 15 };
        }

        void OnGUI()
        {
            EnsureStyles();
            var gm = GameManager.Instance;
            if (gm == null) return;

            Meter("BUREAU", gm.Bureau, 12, 12);
            Meter("SYNDICATE", gm.Syndicate, 290, 12);
            Crosshair(gm);
            DocPanel(gm);
            PromptText(gm);
            Subtitle(gm);
            CallMenu(gm);
            EndScreen(gm);
            InspectOverlay();
        }

        void Meter(string name, FactionState f, float x, float y)
        {
            string tag = f.active ? "" : "  (DEAD)";
            GUI.Label(new Rect(x, y, 260, 20), $"<b>{name}{tag}</b>", label);
            GUI.Label(new Rect(x, y + 20, 90, 20), $"Value {f.value}", label);
            GUI.Box(new Rect(x + 92, y + 22, 150, 13), GUIContent.none, box);
            GUI.Box(new Rect(x + 92, y + 22, 150f * f.value / 100f, 13), GUIContent.none, box);
            GUI.Label(new Rect(x, y + 38, 90, 20), $"Suspicion {f.suspicion}", label);
            GUI.Box(new Rect(x + 92, y + 40, 150, 13), GUIContent.none, box);
            var old = GUI.color; GUI.color = new Color(0.9f, 0.35f, 0.35f);
            GUI.Box(new Rect(x + 92, y + 40, 150f * f.suspicion / 100f, 13), GUIContent.none, box);
            GUI.color = old;
        }

        void Crosshair(GameManager gm)
        {
            if (gm.UIOpen) return;
            GUI.Label(new Rect(Screen.width / 2f - 4, Screen.height / 2f - 12, 20, 20), "+", label);
        }

        void DocPanel(GameManager gm)
        {
            var pi = PlayerInteractor.Instance;
            if (pi == null) return;
            var d = pi.Held != null ? pi.Held : pi.Focused;
            if (d == null) return;
            var lines = d.HudLines();
            float w = 470, h = 22 * lines.Length + 30;
            float x = Screen.width - w - 16, y = Screen.height - h - 92;
            GUI.Box(new Rect(x, y, w, h), pi.Held != null ? "IN HAND" : "ON DESK", box);
            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(x + 12, y + 24 + i * 22, w - 24, 22), lines[i], label);
        }

        void PromptText(GameManager gm)
        {
            if (gm.UIOpen) return;
            var pi = PlayerInteractor.Instance;
            if (pi == null || string.IsNullOrEmpty(pi.Prompt)) return;
            var s = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(Screen.width / 2f - 220, Screen.height / 2f + 24, 440, 24), "<b>" + pi.Prompt + "</b>", s);
        }

        void Subtitle(GameManager gm)
        {
            if (string.IsNullOrEmpty(gm.Subtitle)) return;
            var s = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            GUI.Box(new Rect(Screen.width / 2f - 380, Screen.height - 72, 760, 42), GUIContent.none, box);
            GUI.Label(new Rect(Screen.width / 2f - 380, Screen.height - 72, 760, 42), gm.Subtitle, s);
        }

        void CallMenu(GameManager gm)
        {
            if (!gm.IsCallOpen) return;
            float w = 440, h = 260;
            float x = Screen.width / 2f - w / 2f, y = Screen.height / 2f - h / 2f;
            GUI.Box(new Rect(x, y, w, h), $"CALL - {gm.CallFaction} LINE", box);
            if (gm.CallDocument != null)
                GUI.Label(new Rect(x + 16, y + 30, w - 32, 60), "\"" + gm.CallDocument.data.text + "\"", label);

            float by = y + 104;
            if (GUI.Button(new Rect(x + 20, by, w - 40, 42), "Report it straight (truthful)", btn)) gm.ResolveCall(ReportAction.ReportStraight);
            if (GUI.Button(new Rect(x + 20, by + 50, w - 40, 42), "Spin it / lie", btn)) gm.ResolveCall(ReportAction.Lie);
            if (GUI.Button(new Rect(x + 20, by + 100, w - 40, 42), "Downplay / stall", btn)) gm.ResolveCall(ReportAction.Downplay);
        }

        void EndScreen(GameManager gm)
        {
            if (gm.Phase != GamePhase.Won && gm.Phase != GamePhase.Lost) return;
            float w = 460, h = 200;
            float x = Screen.width / 2f - w / 2f, y = Screen.height / 2f - h / 2f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, box);
            GUI.Label(new Rect(x, y + 20, w, 40), gm.Phase == GamePhase.Won ? "YOU SURVIVED" : "GAME OVER", big);
            var s = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x + 20, y + 74, w - 40, 44), gm.Subtitle, s);
            if (GUI.Button(new Rect(x + w / 2f - 80, y + h - 54, 160, 40), "Restart", btn))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    
        void InspectOverlay()
        {
            var insp = Object.FindFirstObjectByType<DocumentInspector>();
            if (insp == null || !insp.Inspecting) return;
            var lines = insp.Overlay();
            if (lines == null) return;
            float w = 520, h = 26 * lines.Length + 24;
            float x = Screen.width / 2f - w / 2f, y = 70;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, box);
            var title = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(x + 12, y + 12 + i * 26, w - 24, 26), lines[i], i == 0 ? big2() : title);
        }

        GUIStyle _b2;
        GUIStyle big2()
        {
            if (_b2 == null) _b2 = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 18 };
            return _b2;
        }

}
}
