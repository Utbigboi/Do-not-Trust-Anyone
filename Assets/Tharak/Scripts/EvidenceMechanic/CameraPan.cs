using UnityEngine;

namespace CaseDesk
{
    // Latching pan: a command sets the target to Left / Centre / Right and it STAYS there
    // until you command otherwise. It does not follow the live mouse position.
    //
    // Commands:
    //   A / Left Arrow      -> pan left and hold
    //   D / Right Arrow     -> pan right and hold
    //   S / Down / Space    -> return to centre
    //   (optional) click the left/right screen edge to latch that side; click centre to recentre.
    public class CameraPan : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public EvidenceInspector inspector;

        [Header("Yaw targets (degrees, relative to start rotation)")]
        public float leftYaw = -35f;
        public float centreYaw = 0f;
        public float rightYaw = 35f;

        [Header("Feel")]
        public float smoothTime = 0.18f;

        [Header("Inputs")]
        public bool keyControl = true;
        public bool edgeClickControl = true;   // CLICK an edge to latch that side (not hover)
        public float edgePixels = 80f;

        enum Side { Left, Centre, Right }
        Side side = Side.Centre;

        float basePitch, baseYaw, yaw, vel;

        void Awake() { if (!cam) cam = GetComponent<Camera>() ?? Camera.main; }

        void Start()
        {
            var e = cam.transform.eulerAngles;
            basePitch = e.x; baseYaw = e.y; yaw = 0f;
        }

        void Update()
        {
            if (inspector && inspector.Busy) return;

            // --- change the latched side only on an explicit command ---
            if (keyControl)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) side = Side.Left;
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) side = Side.Right;
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Space)) side = Side.Centre;
            }
            if (edgeClickControl && Input.GetMouseButtonDown(1)) // right-click an edge / middle to latch
            {
                float mx = Input.mousePosition.x;
                if (mx <= edgePixels) side = Side.Left;
                else if (mx >= Screen.width - edgePixels) side = Side.Right;
                else side = Side.Centre;
            }

            float target = side == Side.Left ? leftYaw : side == Side.Right ? rightYaw : centreYaw;
            yaw = Mathf.SmoothDampAngle(yaw, target, ref vel, smoothTime);
            cam.transform.rotation = Quaternion.Euler(basePitch, baseYaw + yaw, 0f);
        }

        // hook these to UI buttons if you like
        public void PanLeft() => side = Side.Left;
        public void PanRight() => side = Side.Right;
        public void PanCentre() => side = Side.Centre;
    }
}
