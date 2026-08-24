using UnityEngine;

namespace SafehouseDesk
{
    // Seated first-person: raycast from screen center, pick up documents,
    // and route E-presses. Close inspection is handled by DocumentInspector.
    public class PlayerInteractor : MonoBehaviour
    {
        public static PlayerInteractor Instance { get; private set; }

        [Header("Refs")]
        public Camera cam;
        public Transform holdPoint;
        [Header("Tuning")]
        public float reach = 3f;

        public Document Held { get; private set; }
        public Document Focused { get; private set; }
        public string Prompt { get; private set; } = "";

        void Awake()
        {
            Instance = this;
            if (cam == null) cam = GetComponent<Camera>();
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            Prompt = ""; Focused = null;
            if (gm != null && gm.UIOpen) return; // paused during call / inspection / end screen

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, reach))
            {
                var inter = hit.collider.GetComponentInParent<IInteractable>();
                if (inter != null)
                {
                    Prompt = inter.Prompt(this);
                    if (inter is Document doc) Focused = doc;
                    if (Input.GetKeyDown(KeyCode.E)) inter.Interact(this);
                }
            }

            if (Held != null && holdPoint != null)
                Held.transform.SetPositionAndRotation(holdPoint.position, holdPoint.rotation);
        }

        public void PickUp(Document d)
        {
            if (Held != null) return;
            Held = d;
            var col = d.GetComponent<Collider>();
            if (col) col.enabled = false;
            d.transform.SetParent(holdPoint, false);
            d.transform.localPosition = Vector3.zero;
            d.transform.localRotation = Quaternion.identity;
        }

        public void ConsumeHeld()
        {
            if (Held == null) return;
            var go = Held.gameObject;
            Held = null;
            Destroy(go);
            if (ShiftController.Instance != null) ShiftController.Instance.DecrementRemaining();
        }

        public void ShredHeld()
        {
            if (Held == null) return;
            GameManager.Instance.Say("Shredded. Gone for good - and reported to no one.", 2.5f);
            ConsumeHeld();
        }
    }
}
