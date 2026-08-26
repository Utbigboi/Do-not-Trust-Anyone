using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Put this on ANY evidence object. Needs a Collider (hover/drag/click) and MeshRenderers.
    // Outline is drawn by QuickOutline and toggled on hover + while dragging.
    [DisallowMultipleComponent]
    public class EvidenceObject : MonoBehaviour
    {
        public static readonly List<EvidenceObject> All = new List<EvidenceObject>();

        [Header("Identity")]
        public string displayName = "Evidence";

        [Header("Hover / drag outline (QuickOutline)")]
        public Color outlineColor = Color.white;
        [Range(0f, 12f)] public float outlineWidth = 6f;
        public global::Outline.Mode outlineMode = global::Outline.Mode.OutlineVisible;

        public bool OnWorkspace { get; set; }

        EvidenceHighlighter hl;

        void Awake() { RefreshHighlighter(); }
        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void RefreshHighlighter()
        {
            hl = GetComponent<EvidenceHighlighter>();
            if (!hl) hl = gameObject.AddComponent<EvidenceHighlighter>();
            hl.color = outlineColor;
            hl.width = outlineWidth;
            hl.mode = outlineMode;
            hl.Init();
        }

        public void SetHighlight(bool on) { if (hl) hl.SetOn(on); }

        // planar footprint radius (x/z), used for separation
        public float FootprintRadius()
        {
            var col = GetComponentInChildren<Collider>();
            if (col) { var e = col.bounds.extents; return Mathf.Max(e.x, e.z); }
            return 0.1f;
        }
    }
}
