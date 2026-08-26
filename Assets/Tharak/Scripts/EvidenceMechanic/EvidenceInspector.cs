using System.Collections;
using UnityEngine;

namespace CaseDesk
{
    // Focus: camera tweens to the focus pose while the piece lifts to hover in front of it.
    // Overview pose is captured at focus time, so it returns to wherever you've panned to.
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

        public bool IsFocused { get; private set; }
        public bool Busy { get; private set; }   // true through the whole focus/return cycle

        EvidenceObject current;
        Transform originalParent;
        Vector3 originalPos;
        Quaternion originalRot;
        Vector3 overviewPos;
        Quaternion overviewRot;
        float savedNear;
        bool ready;
        Coroutine routine;

        void Awake() { if (!cam) cam = GetComponent<Camera>() ?? Camera.main; }

        public void Focus(EvidenceObject e)
        {
            if (Busy || e == null) return;
            current = e; IsFocused = true; Busy = true; ready = false;

            overviewPos = cam.transform.position;   // capture current (possibly panned) pose
            overviewRot = cam.transform.rotation;

            originalParent = e.transform.parent;
            originalPos = e.transform.position;
            originalRot = e.transform.rotation;

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
            var e = current; current = null;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Return(e));
        }

        void Update()
        {
            if (!IsFocused || current == null || !ready) return;

            current.transform.position = cam.transform.position + cam.transform.forward * holdDistance;

            if (Input.GetMouseButton(0))
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                current.transform.Rotate(cam.transform.up, -mx * rotateSpeed * Time.deltaTime, Space.World);
                current.transform.Rotate(cam.transform.right, my * rotateSpeed * Time.deltaTime, Space.World);
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) Exit();
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
            if (e) { e.transform.SetPositionAndRotation(originalPos, originalRot); e.transform.SetParent(originalParent, true); }
            cam.nearClipPlane = savedNear;
            Busy = false;
        }

        void OnGUI()
        {
            if (!IsFocused) return;
            GUI.Label(new Rect(20, Screen.height - 34, 640, 24),
                "Focused: " + (current ? current.displayName : "") + "   |  hold-drag = rotate,  right-click / Esc = back");
            if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 30), "Back")) Exit();
        }
    }
}
