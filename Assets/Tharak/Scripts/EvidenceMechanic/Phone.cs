using UnityEngine;

namespace CaseDesk
{
    // A tip line. Set Tip Faction (Red/Blue). Each phone can have its OWN pickup sound
    // (dial phone vs walkie-talkie). Click to pick up and open the notepad in tip mode.
    public class Phone : MonoBehaviour, IClickable
    {
        public Party tipFaction = Party.Red;

        [Header("This phone's sound (leave empty to use AudioManager's default)")]
        public AudioClip pickupSfx;
        public float sfxMaxSeconds = 0f;   // 0 = play whole clip

        [Header("Optional receiver animation")]
        public Transform receiver;
        public float receiverLift = 0.04f;
        Vector3 receiverHome;

        void Awake() { if (receiver) receiverHome = receiver.localPosition; }

        public void OnClick()
        {
            if (CallController.I == null || CallController.I.IsOpen) return;
            CallController.I.OpenTip(tipFaction);

            if (pickupSfx != null) AudioManager.I?.SfxClip(pickupSfx, sfxMaxSeconds);
            else AudioManager.I?.Phone();

            if (receiver) receiver.localPosition = receiverHome + Vector3.up * receiverLift;
        }

        void Update()
        {
            if (receiver && CallController.I != null && CallController.I.mode != CallController.Mode.Tip)
                receiver.localPosition = receiverHome;
        }
    }
}
