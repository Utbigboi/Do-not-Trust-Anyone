using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Minimal test UI (IMGUI) for the notepad, tipping and accusing.
    // Replace later with the real phones (tip) and fax (accuse). Put on any object.
    public class CaseHUD : MonoBehaviour
    {
        bool[] check = new bool[0];
        Vector2 scroll;

        void OnGUI()
        {
            var cm = CaseManager.I; var np = Notepad.I;
            if (cm == null || np == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 340, Screen.height - 20), GUI.skin.box);
            GUILayout.Label($"SUSPICION  Red {cm.susRed}   Blue {cm.susBlue}   Org {cm.susOrg}");
            GUILayout.Label($"CREDIBILITY  {cm.credibility}");
            GUILayout.Space(4);
            GUILayout.Label("NOTEPAD (tick = select):");

            if (check.Length != np.entries.Count) System.Array.Resize(ref check, np.entries.Count);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(Screen.height - 230));
            for (int i = 0; i < np.entries.Count; i++)
            {
                var o = np.entries[i];
                check[i] = GUILayout.Toggle(check[i], $"[{o.source}] {o.text}");
            }
            GUILayout.EndScrollView();

            GUILayout.Space(4);
            GUILayout.Label("Tip selected to:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Red")) TipChecked(Party.Red);
            if (GUILayout.Button("Blue")) TipChecked(Party.Blue);
            if (GUILayout.Button("Org")) TipChecked(Party.Organisation);
            GUILayout.EndHorizontal();

            GUILayout.Label("Accuse (selected = backing):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Red")) CaseManager.I.Accuse(Party.Red, Checked());
            if (GUILayout.Button("Blue")) CaseManager.I.Accuse(Party.Blue, Checked());
            if (GUILayout.Button("Both Innocent")) CaseManager.I.Accuse(Party.None, Checked());
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(cm.lastResult)) GUILayout.Label(cm.lastResult);
            GUILayout.EndArea();
        }

        List<Observation> Checked()
        {
            var l = new List<Observation>();
            var np = Notepad.I;
            for (int i = 0; i < np.entries.Count && i < check.Length; i++)
                if (check[i]) l.Add(np.entries[i]);
            return l;
        }

        void TipChecked(Party f) { foreach (var o in Checked()) CaseManager.I.Tip(o, f); }
    }
}
