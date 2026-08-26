using UnityEngine;

namespace CaseDesk
{
    // Centre slot that holds ONE piece. Dropping another replaces it (the old one is ejected
    // back onto the table). Needs a Collider so clicks on the empty zone register.
    public class Workspace : MonoBehaviour
    {
        public Transform slotAnchor;      // where the piece sits; defaults to this object
        public float radius = 0.28f;      // drop / click zone on the table plane
        public float ejectOffset = 0.4f;  // how far a replaced piece is pushed aside (+X)

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
            if (Current != null) Eject(Current);   // only one allowed
            Current = e;
            e.OnWorkspace = true;
            Snap(e);
        }

        public void Remove(EvidenceObject e)
        {
            if (Current == e) Current = null;
            e.OnWorkspace = false;
        }

        void Snap(EvidenceObject e)
        {
            Vector3 p = slotAnchor.position;
            p.y = e.transform.position.y;   // keep its resting height
            e.transform.position = p;
        }

        void Eject(EvidenceObject e)
        {
            e.OnWorkspace = false;
            if (Current == e) Current = null;
            Vector3 p = slotAnchor.position + Vector3.right * ejectOffset;
            p.y = e.transform.position.y;
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
