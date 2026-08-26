using UnityEngine;

namespace CaseDesk
{
    // Hover + drag outline, drag across the table (pieces don't overlap), click to inspect.
    public class TableController : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Workspace workspace;
        public EvidenceInspector inspector;

        [Header("Tuning")]
        public float rayLength = 100f;
        public float clickThresholdPixels = 8f;
        public float separationPadding = 0.02f;   // extra gap between pieces

        EvidenceObject highlighted, dragging;
        Plane dragPlane;
        Vector3 grabOffset;
        Vector2 downPos;
        bool isDrag, pressedWorkspace;

        void Awake() { if (!cam) cam = Camera.main; }

        void Update()
        {
            if (inspector && inspector.IsFocused) { SetHighlight(null); return; }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                downPos = Input.mousePosition;
                isDrag = false; pressedWorkspace = false; dragging = null;
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
                {
                    var ev = hit.collider.GetComponentInParent<EvidenceObject>();
                    if (ev != null)
                    {
                        dragging = ev;
                        dragPlane = new Plane(Vector3.up, new Vector3(0f, ev.transform.position.y, 0f));
                        grabOffset = dragPlane.Raycast(ray, out float d) ? ev.transform.position - ray.GetPoint(d) : Vector3.zero;
                    }
                    else if (hit.collider.GetComponentInParent<Workspace>() != null) pressedWorkspace = true;
                }
            }

            if (dragging != null && Input.GetMouseButton(0))
            {
                if (!isDrag && Vector2.Distance(Input.mousePosition, downPos) > clickThresholdPixels)
                {
                    isDrag = true;
                    if (dragging.OnWorkspace && workspace) workspace.Remove(dragging);
                }
                if (isDrag && dragPlane.Raycast(ray, out float d))
                {
                    Vector3 pt = ray.GetPoint(d) + grabOffset;
                    pt.y = dragging.transform.position.y;
                    dragging.transform.position = pt;
                    PushOthersAside();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!isDrag)
                {
                    if (dragging != null && dragging.OnWorkspace) inspector?.Focus(dragging);
                    else if (pressedWorkspace && workspace && workspace.Current != null) inspector?.Focus(workspace.Current);
                }
                else if (dragging != null && workspace && workspace.IsOver(dragging.transform.position))
                {
                    workspace.Place(dragging);
                }
                dragging = null; isDrag = false; pressedWorkspace = false;
            }

            EvidenceObject desired;
            if (dragging != null) desired = dragging;
            else if (!Input.GetMouseButton(0))
            {
                desired = null;
                if (Physics.Raycast(ray, out RaycastHit h, rayLength))
                    desired = h.collider.GetComponentInParent<EvidenceObject>();
            }
            else desired = highlighted;
            SetHighlight(desired);
        }

        // shove any piece the dragged one overlaps out to a non-overlapping distance
        void PushOthersAside()
        {
            if (dragging == null) return;
            float rd = dragging.FootprintRadius();
            var a = dragging.transform.position;
            foreach (var other in EvidenceObject.All)
            {
                if (other == null || other == dragging || other.OnWorkspace) continue;
                var b = other.transform.position;
                float dx = b.x - a.x, dz = b.z - a.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                float min = rd + other.FootprintRadius() + separationPadding;
                if (dist < min)
                {
                    float ux, uz;
                    if (dist > 0.0001f) { ux = dx / dist; uz = dz / dist; }
                    else { ux = 1f; uz = 0f; }
                    float push = min - dist;
                    var p = other.transform.position;
                    p.x += ux * push; p.z += uz * push;
                    other.transform.position = p;
                }
            }
        }

        void SetHighlight(EvidenceObject e)
        {
            if (highlighted == e) return;
            if (highlighted) highlighted.SetHighlight(false);
            highlighted = e;
            if (highlighted) highlighted.SetHighlight(true);
        }
    }
}
