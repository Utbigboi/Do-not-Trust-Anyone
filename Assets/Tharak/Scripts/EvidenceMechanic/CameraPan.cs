using UnityEngine;

namespace CaseDesk
{
    // Stepped pan between three held positions: Left / Centre / Right.
    // Right edge = step one right; left edge = step one left; each HOLDS.
    // Anti-sensitivity: after a step you must (a) leave the edge, or (b) wait `repeatDelay`,
    // before it will step again. With requireLeaveEdge = true, sitting on the edge does nothing.
    public class CameraPan : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public EvidenceInspector inspector;

        [Header("Yaw positions (degrees, relative to start rotation)")]
        public float leftYaw = -35f;
        public float centreYaw = 0f;
        public float rightYaw = 35f;

        [Header("Feel")]
        public float smoothTime = 0.18f;

        [Header("Edge trigger")]
        public float edgePixels = 80f;
        [Tooltip("If true, you MUST move the mouse off the edge before it will step again (recommended).")]
        public bool requireLeaveEdge = true;
        [Tooltip("If requireLeaveEdge is false, this is the cooldown before holding the edge steps again.")]
        public float repeatDelay = 0.5f;
        public bool allowKeys = true;

        enum Step { Left = -1, Centre = 0, Right = 1 }
        Step step = Step.Centre;

        float basePitch, baseYaw, yaw, vel;
        bool armed = true;       // ready to accept a step
        float nextRepeat;        // for the cooldown mode

        void Awake() { if (!cam) cam = GetComponent<Camera>() ?? Camera.main; }

        void Start() { CaptureBase(); }

        // Re-read the current camera rotation as the pan 'centre'. Call after the intro fly-in
        // so the pan is anchored to the PLAY pose, not the menu angle.
        public void CaptureBase()
        {
            var e = cam.transform.eulerAngles;
            basePitch = e.x; baseYaw = e.y; yaw = 0f; vel = 0f; step = Step.Centre;
        }

        void Update()
        {
            if (inspector && inspector.Busy) return;

            float mx = Input.mousePosition.x;
            bool atLeft = mx >= 0 && mx <= edgePixels;
            bool atRight = mx <= Screen.width && mx >= Screen.width - edgePixels;
            bool onEdge = atLeft || atRight;

            if (!onEdge)
            {
                armed = true;                 // left the edge -> ready again
            }
            else if (armed)
            {
                if (atRight) StepRight(); else StepLeft();
                armed = false;
                nextRepeat = Time.time + repeatDelay;
            }
            else if (!requireLeaveEdge && Time.time >= nextRepeat)
            {
                if (atRight) StepRight(); else StepLeft();
                nextRepeat = Time.time + repeatDelay;
            }

            if (allowKeys)
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) StepRight();
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) StepLeft();
            }

            float target = step == Step.Left ? leftYaw : step == Step.Right ? rightYaw : centreYaw;
            yaw = Mathf.SmoothDampAngle(yaw, target, ref vel, smoothTime);
            cam.transform.rotation = Quaternion.Euler(basePitch, baseYaw + yaw, 0f);
        }

        void StepRight() { step = (Step)Mathf.Min((int)step + 1, 1); }
        void StepLeft() { step = (Step)Mathf.Max((int)step - 1, -1); }

        public void NudgeRight() => StepRight();
        public void NudgeLeft() => StepLeft();
    }
}
