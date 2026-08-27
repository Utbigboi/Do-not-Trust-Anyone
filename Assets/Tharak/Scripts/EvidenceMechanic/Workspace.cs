using UnityEngine;

namespace CaseDesk
{
    // Centre slot holding ONE piece. Dropping another replaces it. Physics-aware.
    public class Workspace : MonoBehaviour
    {
        public Transform slotAnchor;
        public float radius = 0.28f;
        public float ejectOffset = 0.4f;
        [Tooltip("Surface height pieces rest on. Assign your TableTop; falls back to slotAnchor's Y.")]
        public Transform surface;
        public float surfaceLift = 0.002f;   // tiny gap so it doesn't z-fight the table

        public EvidenceObject Current { get; private set; }

        void Awake() { if (!slotAnchor) slotAnchor = transform; }

        public bool IsOver(Vector3 world)
        {
            Vector3 a = slotAnchor.position; a.y = 0;
            Vector3 b = world; b.y = 0;
            return Vector3.Distance(a, b) <= radius;
        }

        public void Place(EvidenceObject e)
        {
            if (Current == e) { Snap(e); return; }
            if (Current != null) Eject(Current);
            Current = e;
            e.OnWorkspace = true;
            Snap(e);
        }

        public void Remove(EvidenceObject e)
        {
            if (Current == e) Current = null;
            e.OnWorkspace = false;
            var rb = e.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
        }

        void Snap(EvidenceObject e)
        {
            var rb = e.GetComponent<Rigidbody>();
            if (rb)
            {
                if (!rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                rb.isKinematic = true;
            }

            float surfaceY = surface ? surface.position.y : slotAnchor.position.y;

            // X/Z from the slot; Y rests the piece's OWN collider bottom on the surface.
            Vector3 p = new Vector3(slotAnchor.position.x, surfaceY + surfaceLift, slotAnchor.position.z);

            var col = e.GetComponent<Collider>();           // the piece's own collider (not children/walls)
            if (col)
            {
                float half = col.bounds.extents.y;          // half its height in world space
                if (half > 0f && half < 2f)                 // sanity clamp so a bad collider can't fling it
                    p.y = surfaceY + surfaceLift + half;
            }
            e.transform.position = p;
        }

        void Eject(EvidenceObject e)
        {
            e.OnWorkspace = false;
            if (Current == e) Current = null;
            var rb = e.GetComponent<Rigidbody>();
            if (rb) { rb.isKinematic = false; rb.useGravity = true; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            Vector3 p = slotAnchor.position + Vector3.right * ejectOffset;
            e.transform.position = p;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            var c = (slotAnchor ? slotAnchor : transform).position;
            Gizmos.DrawWireSphere(c, radius);
        }
    }
}
