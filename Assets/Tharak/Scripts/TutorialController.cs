using System.Collections;
using UnityEngine;

namespace SafehouseDesk
{
    // Act 0: trains you to trust one line, then the dead phone rings.
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController Instance { get; private set; }

        [Header("Scene refs")]
        public Telephone trustedPhone;   // the line you're taught to trust
        public Telephone deadPhone;      // rings for the first time at the betrayal
        public ShiftController shifts;
        public Transform tutorialSpot;   // where the tutorial document spawns

        bool reported;

        void Awake() { Instance = this; }

        IEnumerator Start()
        {
            yield return null; // let GameManager.Awake run
            GameManager.Instance.SetPhase(GamePhase.Tutorial);
            if (trustedPhone) trustedPhone.active = true;
            if (deadPhone) deadPhone.active = false;

            GameManager.Instance.Say("SAFEHOUSE. Your handler, VESPER, is on the line.", 4f);
            yield return new WaitForSeconds(4f);
            GameManager.Instance.Say("VESPER: There's a document in the tray. Look at it, press [E] to pick it up.", 6f);

            var data = GameContent.Tutorial();
            Vector3 pos = tutorialSpot ? tutorialSpot.position : new Vector3(0f, 0.9f, 0.5f);
            Document.Create(data, pos, shifts ? shifts.docMaterial : null);

            yield return new WaitUntil(() => PlayerInteractor.Instance.Held != null);
            GameManager.Instance.Say("VESPER: Good. Press [F] to inspect it under the lamp - always check the seal.", 6f);

            yield return new WaitUntil(() => PlayerInteractor.Instance.Held != null && PlayerInteractor.Instance.Held.inspected);
            GameManager.Instance.Say("VESPER: Authentic. Now pick up OUR line and report it. Straight, like I taught you.", 8f);
            // The player calls trustedPhone -> ResolveCall -> HandleTutorialReport.
        }

        // Returns true if the document should be consumed (i.e. the report went through).
        public bool HandleTutorialReport(FactionId faction, Document doc, ReportAction action)
        {
            if (reported) return true;

            if (action != ReportAction.ReportStraight)
            {
                GameManager.Instance.Say("VESPER: Not now. Report it honestly - pick up the line again.", 5f);
                return false; // keep the doc so the player can retry
            }

            reported = true;
            StartCoroutine(Betrayal());
            return true;
        }

        IEnumerator Betrayal()
        {
            GameManager.Instance.Say("You hang up. The report is filed.", 3f);
            yield return new WaitForSeconds(3f);
            GameManager.Instance.Say("...", 1.5f);
            yield return new WaitForSeconds(1.5f);

            if (deadPhone)
            {
                deadPhone.ringing = true;
                deadPhone.onAnswered = OnAnsweredSecondPhone;
            }
            GameManager.Instance.Say("The phone that never rings is ringing. [E] Answer it.", 10f);
        }

        void OnAnsweredSecondPhone() => StartCoroutine(Reveal());

        IEnumerator Reveal()
        {
            GameManager.Instance.Say("???: That line you trust? It's ours. You just burned a Bureau agent.", 6f);
            yield return new WaitForSeconds(6f);
            GameManager.Instance.Say("???: You've been the double agent all along. From now on you work us both.", 6f);
            yield return new WaitForSeconds(6f);

            if (trustedPhone) trustedPhone.active = true;
            if (deadPhone) deadPhone.active = true;

            GameManager.Instance.Say("Trust no one. Not even the tutorial.", 4f);
            yield return new WaitForSeconds(4f);

            if (shifts) shifts.BeginRealGame();
        }
    }
}
