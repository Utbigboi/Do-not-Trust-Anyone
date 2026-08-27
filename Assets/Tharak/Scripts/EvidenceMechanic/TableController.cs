using UnityEngine;

namespace CaseDesk
{
    // Physics drag: grabbing a piece lifts it and it follows the cursor (Rigidbody-driven),
    // scroll to raise/lower, release to drop & settle. Clamped to the table via TableBounds.
    public class TableController : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Workspace workspace;
        public EvidenceInspector inspector;
        public TableBounds bounds;

        [Header("Raycast")]
        public float rayLength = 100f;
        public float clickThresholdPixels = 8f;

        [Header("Physics drag")]
        public float dragHeight = 0.15f;      // lift above the surface when grabbed
        public float minHeight = 0.02f;
        public float maxHeight = 0.5f;
        public float heightScrollSpeed = 0.15f;
        public float followStrength = 14f;    // how hard it chases the cursor
        public float maxDragSpeed = 8f;

        EvidenceObject highlighted, dragging;
        Rigidbody dragRb;
        Plane plane;
        Vector3 grabOffset, dragTarget;
        Vector2 downPos;
        bool isDrag, pressedWorkspace;
        float targetHeight;

        void Awake() { if (!cam) cam = Camera.main; }

        float SurfaceY => bounds ? bounds.Center.y : 0f;

        void Update()
        {
            if (inspector && inspector.IsFocused) { SetHighlight(null); return; }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                downPos = Input.mousePosition;
                isDrag = false; pressedWorkspace = false; dragging = null; dragRb = null;

                if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
                {
                    var ev = hit.collider.GetComponentInParent<EvidenceObject>();
                    if (ev != null)
                    {
                        dragging = ev;
                        dragRb = ev.GetComponent<Rigidbody>();
                        targetHeight = SurfaceY + dragHeight;
                        plane = new Plane(Vector3.up, new Vector3(0f, targetHeight, 0f));
                        grabOffset = plane.Raycast(ray, out float d) ? ev.transform.position - ray.GetPoint(d) : Vector3.zero;
                        grabOffset.y = 0f;
                        // NOTE: don't remove from workspace / unfreeze here — wait until a real drag starts,
                        // so a plain click on a workspace piece still focuses it.
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
                    if (dragRb) { dragRb.isKinematic = false; dragRb.useGravity = false; }
                }
                if (isDrag)
                {
                    plane = new Plane(Vector3.up, new Vector3(0f, targetHeight, 0f));
                    if (plane.Raycast(ray, out float d))
                    {
                        Vector3 pt = ray.GetPoint(d) + grabOffset;
                        pt.y = targetHeight;
                        if (bounds) pt = bounds.Clamp(pt);
                        dragTarget = pt;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                bool focused = false;
                if (!isDrag)
                {
                    if (dragging != null && dragging.OnWorkspace) { inspector?.Focus(dragging); focused = true; }
                    else if (pressedWorkspace && workspace && workspace.Current != null) { inspector?.Focus(workspace.Current); focused = true; }
                }
                else if (dragging != null)
                {
                    if (workspace && workspace.IsOver(dragging.transform.position))
                    {
                        workspace.Place(dragging);   // locks it in the slot
                    }
                    else
                    {
                        if (dragRb != null) dragRb.useGravity = true;   // drop
                        dragging.SettleFlat();                          // lay it flat gently
                    }
                }
                // safety: any still-free body gets gravity back
                if (!focused && dragRb != null && !dragRb.isKinematic) dragRb.useGravity = true;

                dragging = null; dragRb = null; isDrag = false; pressedWorkspace = false;
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

        void FixedUpdate()
        {
            if (isDrag && dragRb != null)
            {
                Vector3 to = dragTarget - dragRb.position;
                dragRb.linearVelocity = Vector3.ClampMagnitude(to * followStrength, maxDragSpeed);
                dragRb.angularVelocity *= 0.85f;
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
