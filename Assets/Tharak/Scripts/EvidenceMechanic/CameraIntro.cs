using System;
using System.Collections;
using UnityEngine;

namespace CaseDesk
{
    // Holds a cinematic "menu" camera pose and lerps the camera to the play pose on Start.
    // The menu shows the game itself from a distance; pressing Play flies into position.
    public class CameraIntro : MonoBehaviour
    {
        public static CameraIntro I { get; private set; }

        [Header("Refs")]
        public Camera cam;                 // Main Camera (auto = Camera.main)
        public Transform menuPose;         // where the camera sits behind the menu
        public Transform playPose;         // the normal gameplay camera pose

        [Header("Move")]
        public float moveTime = 1.6f;
        public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Pause other camera scripts during the fly-in")]
        public CameraPan cameraPan;        // disabled until we arrive
        public EvidenceInspector inspector;

        public bool Moving { get; private set; }
        public bool Arrived { get; private set; }

        void Awake()
        {
            I = this;
            if (!cam) cam = Camera.main;
        }

        void Start()
        {
            // snap to the menu pose at boot so the menu shows the cinematic angle
            if (menuPose && cam) cam.transform.SetPositionAndRotation(menuPose.position, menuPose.rotation);
            if (cameraPan) cameraPan.enabled = false;   // no edge-pan while in menu
        }

        // Call from MenuUI Start. onArrive fires when the camera reaches the play pose.
        public void FlyIn(Action onArrive)
        {
            if (Moving || Arrived) { onArrive?.Invoke(); return; }
            StartCoroutine(FlyRoutine(onArrive));
        }

        IEnumerator FlyRoutine(Action onArrive)
        {
            Moving = true;
            Vector3 p0 = cam.transform.position; Quaternion r0 = cam.transform.rotation;
            Vector3 p1 = playPose ? playPose.position : p0;
            Quaternion r1 = playPose ? playPose.rotation : r0;

            float k = 0f;
            // use unscaled time so it plays even while the game is paused (timeScale 0)
            while (k < 1f)
            {
                k += Time.unscaledDeltaTime / Mathf.Max(0.01f, moveTime);
                float s = ease.Evaluate(Mathf.Clamp01(k));
                cam.transform.SetPositionAndRotation(Vector3.LerpUnclamped(p0, p1, s), Quaternion.SlerpUnclamped(r0, r1, s));
                yield return null;
            }
            cam.transform.SetPositionAndRotation(p1, r1);

            Moving = false; Arrived = true;
            if (cameraPan) { cameraPan.enabled = true; cameraPan.CaptureBase(); }  // anchor pan to the play pose
            onArrive?.Invoke();
        }
    }
}
