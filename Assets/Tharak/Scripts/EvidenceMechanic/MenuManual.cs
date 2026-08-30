using UnityEngine;

namespace CaseDesk
{
    // For a hand-built menu Canvas. Hook these methods to your Button OnClick events.
    // Start -> hide menu, camera fly-in, begin round + tutorial. Credits -> show credits panel.
    public class MenuManual : MonoBehaviour
    {
        [Header("Your UI")]
        public GameObject menuPanel;      // the panel holding Title + buttons
        public GameObject creditsPanel;   // hidden until Credits pressed

        RoundManager rm;

        void Awake()
        {
            rm = FindFirstObjectByType<RoundManager>();
            if (rm) rm.autoStart = false;                 // wait for Start
        }

        void Start()
        {
            if (menuPanel) menuPanel.SetActive(true);
            if (creditsPanel) creditsPanel.SetActive(false);
        }

        // ---- hook these to Button OnClick ----
        public void OnStart()
        {
            if (menuPanel) menuPanel.SetActive(false);
            AudioManager.I?.PlayMusic();
            if (CameraIntro.I != null) CameraIntro.I.FlyIn(BeginPlay);
            else BeginPlay();
        }

        public void OnCredits()
        {
            if (creditsPanel) creditsPanel.SetActive(true);
            if (menuPanel) menuPanel.SetActive(false);
        }

        public void OnCreditsBack()
        {
            if (creditsPanel) creditsPanel.SetActive(false);
            if (menuPanel) menuPanel.SetActive(true);
        }

        public void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void BeginPlay()
        {
            rm?.StartGame();
            TutorialUI.I?.Begin();
        }
    }
}
