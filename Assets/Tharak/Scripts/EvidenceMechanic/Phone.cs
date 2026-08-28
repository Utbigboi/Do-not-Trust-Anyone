using UnityEngine;

namespace CaseDesk
{
    // A tip line. Set Tip Faction to Red or Blue (or Organisation). Click to pick up the receiver
    // and open the notepad in tip mode; hang up from the UI.
    public class Phone : MonoBehaviour, IClickable
    {
        public Party tipFaction = Party.Red;

        [Header("Optional receiver animation")]
        public Transform receiver;      // a child that lifts when on call
        public float receiverLift = 0.04f;
        Vector3 receiverHome;

        void Awake() { if (receiver) receiverHome = receiver.localPosition; }

        public void OnClick()
        {
            if (CallController.I == null) return;
            if (CallController.I.IsOpen) return;      // one call at a time
            CallController.I.OpenTip(tipFaction);
            if (receiver) receiver.localPosition = receiverHome + Vector3.up * receiverLift;
        }

        void Update()
        {
            // drop the receiver back when the call closes
            if (receiver && CallController.I != null && CallController.I.mode != CallController.Mode.Tip)
                receiver.localPosition = receiverHome;
        }
    }
}
