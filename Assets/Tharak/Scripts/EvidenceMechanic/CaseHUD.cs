using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaseDesk
{
    // Notepad + tip/leak UI, driven by CallController. A phone opens Tip mode; the fax opens Leak mode.
    // Meters always visible. Replace the visuals later; the wiring (CaseManager.Tip/Accuse) stays.
    public class CaseHUD : MonoBehaviour
    {
        bool[] check = new bool[0];
        Vector2 scroll;

        void OnGUI()
        {
            var cm = CaseManager.I; var np = Notepad.I;
            if (cm == null || np == null) return;
            var cc = CallController.I; var rm = RoundManager.I;

            // meters (always)
            GUILayout.BeginArea(new Rect(10, 10, 360, 70), GUI.skin.box);
            if (rm != null && rm.Current != null) GUILayout.Label("CASE: " + rm.Current.caseName);
            GUILayout.Label($"SUSPICION   Red {cm.susRed}   Blue {cm.susBlue}   Board {cm.susOrg}");
            GUILayout.Label($"CREDIBILITY  {cm.credibility}");
            GUILayout.EndArea();

            // end screen
            if (rm != null && rm.state != RoundManager.State.Playing)
            {
                float w = 360, h = 130, x = Screen.width / 2f - w / 2f, y = Screen.height / 2f - h / 2f;
                GUILayout.BeginArea(new Rect(x, y, w, h), GUI.skin.box);
                GUILayout.Label(rm.state == RoundManager.State.Won ? "YOU MADE IT OUT (for now)." : "YOU'RE FINISHED.");
                GUILayout.Label(cm.lastResult);
                if (GUILayout.Button("Restart")) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                GUILayout.EndArea();
                return;
            }

            if (cc == null || cc.mode == CallController.Mode.None) return;

            // panel
            float pw = 380, ph = Screen.height - 120;
            GUILayout.BeginArea(new Rect(Screen.width - pw - 10, 90, pw, ph), GUI.skin.box);

            if (cc.mode == CallController.Mode.Result)
            {
                GUILayout.Label("THE EVENING GULL");
                GUILayout.Label(cc.resultText);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next case")) { cc.Close(); rm?.Advance(); }
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label(cc.mode == CallController.Mode.Tip
                ? "PHONE — tip the " + cc.tipFaction
                : "FAX — feed the Gull (tick your backing):");

            if (check.Length != np.entries.Count) System.Array.Resize(ref check, np.entries.Count);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < np.entries.Count; i++)
            {
                var o = np.entries[i];
                check[i] = GUILayout.Toggle(check[i], $"[{o.source}] {o.text}");
            }
            GUILayout.EndScrollView();

            if (cc.mode == CallController.Mode.Tip)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Tip selected")) { foreach (var o in Checked()) cm.Tip(o, cc.tipFaction); ClearChecks(); }
                if (GUILayout.Button("Hang up")) cc.Close();
                GUILayout.EndHorizontal();
            }
            else // Leak
            {
                if (GUILayout.Button("Blame the Marigolds (Red)")) Leak(Party.Red);
                if (GUILayout.Button("Blame the Rinse (Blue)")) Leak(Party.Blue);
                if (GUILayout.Button("Blow it open (the Board)")) Leak(Party.None);
                if (GUILayout.Button("Cancel")) cc.Close();
            }

            GUILayout.EndArea();
        }

        void Leak(Party verdict)
        {
            CaseManager.I.Accuse(verdict, Checked());
            CallController.I.OpenResult(CaseManager.I.lastResult);
            ClearChecks();
        }

        List<Observation> Checked()
        {
            var l = new List<Observation>();
            var np = Notepad.I;
            for (int i = 0; i < np.entries.Count && i < check.Length; i++)
                if (check[i]) l.Add(np.entries[i]);
            return l;
        }

        void ClearChecks() { for (int i = 0; i < check.Length; i++) check[i] = false; }
    }
}
