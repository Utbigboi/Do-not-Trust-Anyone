using UnityEngine;

namespace CaseDesk
{
    // Tracks whether a phone (tip) or the fax (leak) is currently "open" and drives the notepad UI.
    public class CallController : MonoBehaviour
    {
        public static CallController I { get; private set; }
        public enum Mode { None, Tip, Leak, Result }

        public Mode mode = Mode.None;
        public Party tipFaction = Party.Red;
        public string resultText = "";

        public bool IsOpen => mode != Mode.None;   // blocks table interaction while true

        void Awake() { I = this; }

        public void OpenTip(Party f) { if (!IsOpen) { mode = Mode.Tip; tipFaction = f; } }
        public void OpenLeak() { if (!IsOpen) mode = Mode.Leak; }
        public void OpenResult(string text) { mode = Mode.Result; resultText = text; }
        public void Close() { mode = Mode.None; }
    }
}
