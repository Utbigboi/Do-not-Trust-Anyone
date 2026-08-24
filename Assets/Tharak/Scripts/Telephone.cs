using System;
using UnityEngine;

namespace SafehouseDesk
{
    // One faction line. Lift it with a document in hand to open the call menu.
    public class Telephone : MonoBehaviour, IInteractable
    {
        public FactionId faction = FactionId.Bureau;
        public bool active = false;   // can you place calls on this line?
        public bool ringing = false;  // incoming (scripted) call
        public Action onAnswered;     // callback for a scripted ring

        Vector3 baseScale; float t;
        void Awake() { baseScale = transform.localScale; }
        void Update()
        {
            if (ringing) { t += Time.deltaTime * 10f; transform.localScale = baseScale * (1f + Mathf.Abs(Mathf.Sin(t)) * 0.08f); }
            else transform.localScale = baseScale;
        }

        public string Prompt(PlayerInteractor p)
        {
            if (ringing) return "[E] Answer the phone";
            if (!active) return "This line is dead";
            if (p.Held == null) return $"[E] {faction} line (need a document in hand)";
            return $"[E] Call the {faction} line";
        }

        public void Interact(PlayerInteractor p)
        {
            if (ringing) { ringing = false; var cb = onAnswered; onAnswered = null; cb?.Invoke(); return; }
            if (!active) { GameManager.Instance.Say("That phone is silent. Dead line.", 2.5f); return; }
            if (p.Held == null) { GameManager.Instance.Say("You need a document in hand to report.", 2.5f); return; }
            GameManager.Instance.OpenCall(faction, p.Held);
        }
    }
}
