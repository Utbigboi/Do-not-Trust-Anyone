using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaseDesk
{
    // Start menu as an overlay in the same scene. Pauses the game until Start is pressed.
    public class MenuUI : MonoBehaviour
    {
        [Header("Look")]
        public TMP_FontAsset font;
        public string title = "THE LEDGER";
        public string subtitle = "a fixer's last case";
        public Color dim = new Color(0f, 0f, 0f, 0.85f);
        public Color boxColor = new Color(0.10f, 0.09f, 0.08f, 0.95f);
        public Color textColor = new Color(0.95f, 0.92f, 0.85f);
        public Color buttonColor = new Color(0.20f, 0.17f, 0.14f, 1f);

        GameObject root;
        RoundManager rm;

        void Awake()
        {
            rm = FindFirstObjectByType<RoundManager>();
            if (rm) rm.autoStart = false;     // wait for Start
            Build();
            Show(true);
        }

        void Build()
        {
            UIKit.EnsureEventSystem();
            var canvas = UIKit.Canvas("MenuCanvas", 200);
            root = canvas.gameObject;
            UIKit.Dim(root.transform, dim);
            var box = UIKit.Box(root.transform, new Vector2(680, 460), boxColor);
            UIKit.Text(box, title, 64, textColor, font, 80);
            UIKit.Text(box, subtitle, 28, new Color(textColor.r, textColor.g, textColor.b, 0.7f), font, 44);
            UIKit.Text(box, "", 10, textColor, font, 20);
            UIKit.Button(box, "Start", 34, buttonColor, textColor, font, StartGame);
            UIKit.Button(box, "Quit", 28, buttonColor, textColor, font, Quit);
        }

        void Show(bool on)
        {
            if (root) root.SetActive(on);
            Time.timeScale = on ? 0f : 1f;
        }

        void StartGame()
        {
            Show(false);
            AudioManager.I?.PlayMusic();
            rm?.StartGame();
            TutorialUI.I?.Begin();
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
