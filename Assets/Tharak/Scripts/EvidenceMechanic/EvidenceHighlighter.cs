using UnityEngine;

namespace CaseDesk
{
    // Toggles the QuickOutline `Outline` component on/off. Auto-added & configured by EvidenceObject.
    // (QuickOutline's class is `Outline` in the global namespace, hence global::Outline.)
    [DisallowMultipleComponent]
    public class EvidenceHighlighter : MonoBehaviour
    {
        public Color color = Color.white;
        [Range(0f, 12f)] public float width = 6f;
        public global::Outline.Mode mode = global::Outline.Mode.OutlineVisible;

        global::Outline outline;

        void Awake() { if (!outline) Init(); }

        public void Init()
        {
            outline = GetComponent<global::Outline>();
            if (!outline) outline = gameObject.AddComponent<global::Outline>();
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
            outline.OutlineMode = mode;
            outline.enabled = false;   // start hidden; SetOn drives it
        }

        public void SetOn(bool on)
        {
            if (outline) outline.enabled = on;
        }
    }
}
