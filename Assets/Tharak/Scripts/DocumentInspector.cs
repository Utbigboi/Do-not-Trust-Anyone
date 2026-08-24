using UnityEngine;

namespace SafehouseDesk
{
    // Close-up inspection: hold a document up, CLICK-AND-DRAG to rotate it, flip it over,
    // and hold it under the lamp to read the watermark. Attach to the Main Camera.
    public class DocumentInspector : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;

        [Header("Keys")]
        public KeyCode toggleKey = KeyCode.F;
        public KeyCode lampKey = KeyCode.Space;
        public int dragButton = 0;        // 0 = left mouse

        [Header("Tuning")]
        public float distance = 0.55f;
        public float inspectNearClip = 0.05f;
        public float rotateSpeed = 260f;

        public bool Inspecting { get; private set; }
        Document doc;
        float savedNearClip;
        bool underLamp;
        bool backToCamera;
        bool dragging;

        void Awake() { if (cam == null) cam = GetComponent<Camera>() ?? Camera.main; }

        void Update()
        {
            var gm = GameManager.Instance;
            var pi = PlayerInteractor.Instance;
            if (gm == null || pi == null) return;

            if (!Inspecting)
            {
                if (!gm.UIOpen && pi.Held != null && Input.GetKeyDown(toggleKey))
                    Enter(pi.Held);
                return;
            }

            if (doc == null || pi.Held == null) { Exit(); return; }
            if (Input.GetKeyDown(toggleKey)) { Exit(); return; }

            // rotate ONLY while the mouse button is held and dragging
            dragging = Input.GetMouseButton(dragButton);
            if (dragging)
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                doc.transform.Rotate(cam.transform.up, -mx * rotateSpeed * Time.deltaTime, Space.World);
                doc.transform.Rotate(cam.transform.right, my * rotateSpeed * Time.deltaTime, Space.World);
            }

            doc.transform.position = cam.transform.position + cam.transform.forward * distance;

            backToCamera = Vector3.Dot(doc.transform.forward, cam.transform.forward) > 0.35f;

            underLamp = Input.GetKey(lampKey);
            if (underLamp && backToCamera && !doc.watermarkChecked)
            {
                doc.watermarkChecked = true;
                string result = !doc.data.hasSeal ? "plain paper, no watermark"
                              : doc.data.sealIsForged ? "the watermark is MISSING - forged seal!"
                              : "the watermark is genuine";
                GameManager.Instance.Say("Under the lamp you can see: " + result, 5f);
            }
        }

        void Enter(Document d)
        {
            doc = d;
            doc.inspected = true;
            Inspecting = true;
            GameManager.Instance.Inspecting = true;
            if (cam != null) { savedNearClip = cam.nearClipPlane; cam.nearClipPlane = inspectNearClip; }
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;  // show pointer to grab with
            doc.transform.SetParent(null, true);
            GameManager.Instance.Say("Inspecting. Click + drag to turn it over. Hold [Space] under the lamp. [F] to set it down.", 6f);
        }

        void Exit()
        {
            Inspecting = false;
            dragging = false;
            if (GameManager.Instance != null) GameManager.Instance.Inspecting = false;
            if (cam != null) cam.nearClipPlane = savedNearClip;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; // back to gameplay look
            var pi = PlayerInteractor.Instance;
            if (doc != null && pi != null && pi.holdPoint != null)
            {
                doc.transform.SetParent(pi.holdPoint, false);
                doc.transform.localPosition = Vector3.zero;
                doc.transform.localRotation = Quaternion.identity;
            }
            doc = null;
        }

        public string[] Overlay()
        {
            if (!Inspecting || doc == null) return null;
            string seal = doc.data.hasSeal ? "Seal: red wax present" : "Seal: none";
            string wm = doc.watermarkChecked
                ? (!doc.data.hasSeal ? "Watermark: none"
                    : doc.data.sealIsForged ? "Watermark: MISSING - forgery!" : "Watermark: genuine")
                : (backToCamera ? "Watermark: hold [Space] to read it" : "Watermark: turn to the back");
            string hint = dragging ? "(turning...)" : "Click + drag to rotate";
            return new[] { "INSPECTING", "\"" + doc.data.text + "\"", seal, wm, hint + "   |   [F] set it down" };
        }
    }
}
