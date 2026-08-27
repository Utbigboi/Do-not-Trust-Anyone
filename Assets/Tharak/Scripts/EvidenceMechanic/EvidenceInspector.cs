using System.Collections;
using UnityEngine;

namespace CaseDesk
{
    // Focus: camera tweens to the focus pose while the piece lifts to hover in front of it.
    // Rigidbody is frozen during focus. POIs highlight on hover; a click (not drag) discovers them.
    public class EvidenceInspector : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Transform focusCameraPose;

        [Header("Tuning")]
        public float holdDistance = 0.35f;
        public float tweenTime = 0.4f;
        public float rotateSpeed = 260f;
        public float focusNearClip = 0.03f;
        public float clickPixels = 8f;

        public bool IsFocused { get; private set; }
        public bool Busy { get; private set; }

        EvidenceObject current;
        Transform originalParent;
        Vector3 originalPos;
        Quaternion originalRot;
        Vector3 overviewPos;
        Quaternion overviewRot;
        float savedNear;
        bool ready;
        Coroutine routine;
        Vector2 downPos;
        Rigidbody focusRb;
        PointOfInterest hoverPoi;

        void Awake() { if (!cam) cam = GetComponent<Camera>() ?? Camera.main; }

        public void Focus(EvidenceObject e)
        {
            if (Busy || e == null) return;
            current = e; IsFocused = true; Busy = true; ready = false;

            overviewPos = cam.transform.position;
            overviewRot = cam.transform.rotation;
            originalParent = e.transform.parent;
            originalPos = e.transform.position;
            originalRot = e.transform.rotation;

            focusRb = e.GetComponent<Rigidbody>();
            if (focusRb)
            {
                if (!focusRb.isKinematic) { focusRb.linearVelocity = Vector3.zero; focusRb.angularVelocity = Vector3.zero; }
                focusRb.isKinematic = true;
            }

            savedNear = cam.nearClipPlane;
            cam.nearClipPlane = focusNearClip;
            e.transform.SetParent(null, true);

            Vector3 camP1 = focusCameraPose ? focusCameraPose.position : cam.transform.position;
            Quaternion camR1 = focusCameraPose ? focusCameraPose.rotation : cam.transform.rotation;
            Vector3 fwd = camR1 * Vector3.forward;
            Vector3 objP1 = camP1 + fwd * holdDistance;
            Quaternion objR1 = Quaternion.LookRotation(-fwd, Vector3.up);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Move(camP1, camR1, e.transform, objP1, objR1));
        }

        public void Exit()
        {
            if (!IsFocused) return;
            IsFocused = false; ready = false;
            if (hoverPoi) { hoverPoi.SetHover(false); hoverPoi = null; }
            var e = current; current = null;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Return(e));
        }

        void Update()
        {
            if (!IsFocused || current == null || !ready) return;

            current.transform.position = cam.transform.position + cam.transform.forward * holdDistance;

            if (Input.GetMouseButtonDown(0)) downPos = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                // raw, unsmoothed delta -> rotateSpeed actually controls the feel
                float mx = Input.GetAxisRaw("Mouse X");
                float my = Input.GetAxisRaw("Mouse Y");
                current.transform.Rotate(cam.transform.up, -mx * rotateSpeed, Space.World);
                current.transform.Rotate(cam.transform.right, my * rotateSpeed, Space.World);
            }
            if (Input.GetMouseButtonUp(0) && Vector2.Distance(Input.mousePosition, downPos) <= clickPixels)
                TryDiscover();

            HoverPOI();

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) Exit();
        }

        void HoverPOI()
        {
            PointOfInterest over = null;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            foreach (var h in Physics.RaycastAll(ray, 5f))
            {
                var poi = h.collider.GetComponentInParent<PointOfInterest>();
                if (poi != null && !poi.Found && poi.transform.IsChildOf(current.transform)) { over = poi; break; }
            }
            if (over != hoverPoi)
            {
                if (hoverPoi) hoverPoi.SetHover(false);
                hoverPoi = over;
                if (hoverPoi) hoverPoi.SetHover(true);
            }
        }

        void TryDiscover()
        {
            if (current == null) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            foreach (var h in Physics.RaycastAll(ray, 5f))
            {
                var poi = h.collider.GetComponentInParent<PointOfInterest>();
                if (poi != null && poi.transform.IsChildOf(current.transform)) { poi.Discover(); if (hoverPoi == poi) hoverPoi = null; return; }
            }
        }

        IEnumerator Move(Vector3 cP, Quaternion cR, Transform obj, Vector3 oP, Quaternion oR)
        {
            Vector3 cP0 = cam.transform.position; Quaternion cR0 = cam.transform.rotation;
            Vector3 oP0 = obj.position; Quaternion oR0 = obj.rotation;
            float k = 0f;
            while (k < 1f)
            {
                k += Time.deltaTime / Mathf.Max(0.01f, tweenTime);
                float s = Mathf.SmoothStep(0f, 1f, k);
                cam.transform.SetPositionAndRotation(Vector3.Lerp(cP0, cP, s), Quaternion.Slerp(cR0, cR, s));
                obj.SetPositionAndRotation(Vector3.Lerp(oP0, oP, s), Quaternion.Slerp(oR0, oR, s));
                yield return null;
            }
            cam.transform.SetPositionAndRotation(cP, cR);
            obj.SetPositionAndRotation(oP, oR);
            ready = true;
        }

        IEnumerator Return(EvidenceObject e)
        {
            Vector3 cP0 = cam.transform.position; Quaternion cR0 = cam.transform.rotation;
            Vector3 oP0 = e ? e.transform.position : Vector3.zero;
            Quaternion oR0 = e ? e.transform.rotation : Quaternion.identity;
            float k = 0f;
            while (k < 1f)
            {
                k += Time.deltaTime / Mathf.Max(0.01f, tweenTime);
                float s = Mathf.SmoothStep(0f, 1f, k);
                cam.transform.SetPositionAndRotation(Vector3.Lerp(cP0, overviewPos, s), Quaternion.Slerp(cR0, overviewRot, s));
                if (e) e.transform.SetPositionAndRotation(Vector3.Lerp(oP0, originalPos, s), Quaternion.Slerp(oR0, originalRot, s));
                yield return null;
            }
            cam.transform.SetPositionAndRotation(overviewPos, overviewRot);
            if (e)
            {
                e.transform.SetPositionAndRotation(originalPos, originalRot);
                e.transform.SetParent(originalParent, true);
                var rb = e.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = e.OnWorkspace;      // stay locked if it lives on the workspace
                    rb.useGravity = !e.OnWorkspace;      // otherwise fall/settle on the table
                    if (!rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                }
                if (!e.OnWorkspace) e.SettleFlat();

            }
            cam.nearClipPlane = savedNear;
            Busy = false;
        }

        void OnGUI()
        {
            if (!IsFocused) return;
            GUI.Label(new Rect(20, Screen.height - 34, 660, 24),
                "Focused: " + (current ? current.displayName : "") + "   |  hold-drag = rotate,  click a marker = note it,  right-click / Esc = back");
            if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 30), "Back")) Exit();
        }
    }
}
