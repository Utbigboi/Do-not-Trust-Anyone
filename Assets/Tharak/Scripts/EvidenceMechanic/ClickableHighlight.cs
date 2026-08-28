using UnityEngine;

namespace CaseDesk
{
    // Drop on a Phone/Fax (or any IClickable) to give it a QuickOutline that TableController
    // toggles on hover. Same look as evidence, without needing EvidenceObject.
    [DisallowMultipleComponent]
    public class ClickableHighlight : MonoBehaviour
    {
        public Color color = Color.white;
        [Range(0f, 12f)] public float width = 6f;
        public global::Outline.Mode mode = global::Outline.Mode.OutlineVisible;

        global::Outline outline;

        void Awake()
        {
            outline = GetComponent<global::Outline>();
            if (!outline) outline = gameObject.AddComponent<global::Outline>();
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
            outline.OutlineMode = mode;
            outline.enabled = false;
        }

        public void SetHighlight(bool on) { if (outline) outline.enabled = on; }
    }
}
