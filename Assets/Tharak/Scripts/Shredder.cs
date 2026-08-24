using UnityEngine;

namespace SafehouseDesk
{
    // Destroy the held document (safe way to kill bait or evidence).
    public class Shredder : MonoBehaviour, IInteractable
    {
        public string Prompt(PlayerInteractor p) => p.Held == null ? "Shredder (nothing to shred)" : "[E] Shred the document";
        public void Interact(PlayerInteractor p) { if (p.Held != null) p.ShredHeld(); }
    }
}
