using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CaseDesk
{
    // Non-gating tutorial: a stack of hint cards the player clicks through at the start.
    public class TutorialUI : MonoBehaviour
    {
        public static TutorialUI I { get; private set; }

        [Header("Look")]
        public TMP_FontAsset font;
        public Color panelColor = new Color(0.10f, 0.09f, 0.08f, 0.92f);
        public Color textColor = new Color(0.95f, 0.92f, 0.85f);
        public Color buttonColor = new Color(0.22f, 0.18f, 0.15f, 1f);

        [TextArea] public List<string> hints = new List<string>
        {
            "You're Abel — the Ledger. Everyone's dirt passes across this desk. Let's work a case.",
            "Click a document on the desk to bring it up. Hold-drag to turn it over.",
            "Hover the marks on the evidence and click them to jot the clue in your notepad. Right-click to set it down.",
            "Pick up a phone to tip a faction — it stirs suspicion. But a clue can only be phoned in once, and tipping a lie will cost you.",
            "When you're ready, use the fax to leak the story to the Gull — decide who takes the fall.",
            "Keep your credibility up, and don't let a gang's heat hit 100. Good luck."
        };

        GameObject root;
        TMP_Text body;
        int index;

        void Awake() { I = this; Build(); root.SetActive(false); }

        void Build()
        {
            UIKit.EnsureEventSystem();
            var canvas = UIKit.Canvas("TutorialCanvas", 180);
            root = canvas.gameObject;

            var rt = UIKit.Rect("HintPanel", root.transform);
            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(900, 200); rt.anchoredPosition = new Vector2(0, 40);
            var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>(); img.color = panelColor;
            var v = rt.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            v.padding = new RectOffset(30, 30, 22, 22); v.spacing = 12;
            v.childControlWidth = v.childControlHeight = true; v.childForceExpandWidth = true;
            v.childAlignment = TextAnchor.MiddleCenter;

            body = UIKit.Text(rt, "", 26, textColor, font, 90);
            var row = UIKit.Rect("Row", rt);
            var h = row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            h.spacing = 14; h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = true;
            h.childAlignment = TextAnchor.MiddleCenter;
            UIKit.Button(row, "Skip", 24, buttonColor, textColor, font, Hide, 48);
            UIKit.Button(row, "Next", 24, buttonColor, textColor, font, Next, 48);
        }

        public void Begin()
        {
            if (hints == null || hints.Count == 0) return;
            index = 0; body.text = hints[0]; root.SetActive(true);
        }

        void Next()
        {
            index++;
            if (index >= hints.Count) { Hide(); return; }
            body.text = hints[index];
        }

        void Hide() { root.SetActive(false); }
    }
}
