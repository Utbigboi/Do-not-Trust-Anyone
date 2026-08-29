using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CaseDesk
{
    // Screen-space notepad. Shows while inspecting a piece, or when a phone/fax is open.
    // Replaces the old IMGUI CaseHUD. Assign the UI refs in the Inspector.
    public class NotepadUI : MonoBehaviour
    {
        [Header("State refs")]
        public EvidenceInspector inspector;      // Main Camera's EvidenceInspector

        [Header("Root")]
        public GameObject root;                  // the whole notepad panel (toggled on/off)

        [Header("Texts")]
        public TMP_Text headerText;
        public TMP_Text metersText;
        public TMP_Text resultText;

        [Header("List")]
        public RectTransform listContent;        // ScrollView -> Content
        public NoteRow rowPrefab;

        [Header("Groups (button containers)")]
        public GameObject tipGroup;              // Tip + Hang up
        public GameObject leakGroup;             // 3 leak buttons + Cancel
        public GameObject resultGroup;           // result text + Next

        [Header("Buttons")]
        public Button tipButton;
        public Button hangupButton;
        public Button leakRedButton;
        public Button leakBlueButton;
        public Button leakBoardButton;
        public Button cancelButton;
        public Button nextButton;

        readonly List<NoteRow> rows = new List<NoteRow>();
        int lastCount = -1;

        void Start()
        {
            if (tipButton) tipButton.onClick.AddListener(TipSelected);
            if (hangupButton) hangupButton.onClick.AddListener(CloseCall);
            if (cancelButton) cancelButton.onClick.AddListener(CloseCall);
            if (leakRedButton) leakRedButton.onClick.AddListener(() => Leak(Party.Red));
            if (leakBlueButton) leakBlueButton.onClick.AddListener(() => Leak(Party.Blue));
            if (leakBoardButton) leakBoardButton.onClick.AddListener(() => Leak(Party.None));
            if (nextButton) nextButton.onClick.AddListener(NextOrRestart);
        }

        void Update()
        {
            var cm = CaseManager.I; var np = Notepad.I;
            if (cm == null || np == null) { if (root) root.SetActive(false); return; }
            var cc = CallController.I; var rm = RoundManager.I;

            bool end = rm != null && rm.state != RoundManager.State.Playing;
            bool call = cc != null && cc.IsOpen;
            bool focused = inspector != null && inspector.IsFocused;
            bool show = end || call || focused;

            if (root) root.SetActive(show);
            if (!show) return;

            if (np.entries.Count != lastCount) Rebuild();

            var mode = cc != null ? cc.mode : CallController.Mode.None;
            bool tipMode = call && mode == CallController.Mode.Tip;
            bool leakMode = call && mode == CallController.Mode.Leak;
            bool resultMode = end || (call && mode == CallController.Mode.Result);

            if (headerText)
                headerText.text = resultMode ? "THE EVENING GULL"
                                : tipMode ? "PHONE — tip the " + cc.tipFaction
                                : leakMode ? "FAX — feed the Gull"
                                : "NOTES";

            if (metersText)
                metersText.text = $"Red {cm.susRed}   Blue {cm.susBlue}   Board {cm.susOrg}\nCredibility {cm.credibility}";

            if (tipGroup) tipGroup.SetActive(tipMode);
            if (leakGroup) leakGroup.SetActive(leakMode);
            if (resultGroup) resultGroup.SetActive(resultMode);

            if (resultText)
            {
                if (end) resultText.text = (rm.state == RoundManager.State.Won ? "You made it out (for now).\n" : "You're finished.\n") + cm.lastResult;
                else if (resultMode) resultText.text = cc.resultText;
            }

            bool selectable = tipMode || leakMode;
            foreach (var r in rows) if (r && r.toggle) r.toggle.interactable = selectable;
        }

        void Rebuild()
        {
            foreach (var r in rows) if (r) Destroy(r.gameObject);
            rows.Clear();
            var np = Notepad.I;
            lastCount = np.entries.Count;
            for (int i = 0; i < np.entries.Count; i++)
            {
                var o = np.entries[i];
                var row = Instantiate(rowPrefab, listContent);
                row.Set(i, "• " + o.text + "   (" + o.source + ")");
                rows.Add(row);
            }
        }

        List<Observation> Checked()
        {
            var l = new List<Observation>();
            var np = Notepad.I;
            foreach (var r in rows)
                if (r && r.toggle && r.toggle.isOn && r.index < np.entries.Count)
                    l.Add(np.entries[r.index]);
            return l;
        }

        void ClearToggles() { foreach (var r in rows) if (r && r.toggle) r.toggle.isOn = false; }

        void TipSelected()
        {
            var cc = CallController.I; if (cc == null) return;
            foreach (var o in Checked()) CaseManager.I.Tip(o, cc.tipFaction);
            ClearToggles();
        }

        void Leak(Party verdict)
        {
            CaseManager.I.Accuse(verdict, Checked());
            CallController.I.OpenResult(CaseManager.I.lastResult);
            ClearToggles();
        }

        void NextOrRestart()
        {
            var rm = RoundManager.I;
            if (rm != null && rm.state != RoundManager.State.Playing)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            CallController.I?.Close();
            rm?.Advance();
        }

        void CloseCall() { CallController.I?.Close(); ClearToggles(); }
    }
}
