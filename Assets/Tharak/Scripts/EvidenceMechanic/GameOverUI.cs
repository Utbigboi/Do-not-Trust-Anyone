using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaseDesk
{
    // Win / lose overlay. Watches RoundManager.state and shows when the game ends.
    public class GameOverUI : MonoBehaviour
    {
        [Header("Look")]
        public TMP_FontAsset font;
        public Color dim = new Color(0f, 0f, 0f, 0.85f);
        public Color boxColor = new Color(0.10f, 0.09f, 0.08f, 0.96f);
        public Color textColor = new Color(0.95f, 0.92f, 0.85f);
        public Color buttonColor = new Color(0.20f, 0.17f, 0.14f, 1f);

        GameObject root;
        TMP_Text titleText, bodyText;
        bool shown;

        void Awake() { Build(); root.SetActive(false); }

        void Build()
        {
            UIKit.EnsureEventSystem();
            var canvas = UIKit.Canvas("GameOverCanvas", 190);
            root = canvas.gameObject;
            UIKit.Dim(root.transform, dim);
            var box = UIKit.Box(root.transform, new Vector2(760, 460), boxColor);
            titleText = UIKit.Text(box, "", 60, textColor, font, 80);
            bodyText = UIKit.Text(box, "", 26, textColor, font, 120);
            UIKit.Button(box, "Restart", 30, buttonColor, textColor, font, Restart);
            UIKit.Button(box, "Quit", 26, buttonColor, textColor, font, Quit);
        }

        void Update()
        {
            var rm = RoundManager.I; var cm = CaseManager.I;
            if (rm == null || shown) return;
            if (rm.state == RoundManager.State.Playing) return;

            shown = true;
            bool won = rm.state == RoundManager.State.Won;
            titleText.text = won ? "YOU MADE IT OUT" : "YOU'RE FINISHED";
            bodyText.text = (won ? "The story ran. For now, you're still breathing.\n\n"
                                 : "The city closed the file on you.\n\n") + (cm ? cm.lastResult : "");
            root.SetActive(true);
            Time.timeScale = 0f;
        }

        void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
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
