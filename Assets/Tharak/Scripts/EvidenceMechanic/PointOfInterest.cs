using UnityEngine;

namespace CaseDesk
{
    // A clickable clue hotspot on an evidence piece (1-3 per piece). Highlights on hover
    // while the piece is focused; clicked -> logs to the Notepad.
    [RequireComponent(typeof(Collider))]
    public class PointOfInterest : MonoBehaviour
    {
        [TextArea] public string text = "A detail worth noting.";
        public Party implicates = Party.None;
        public Reliability reliability = Reliability.Solid;
        [Range(1, 3)] public int strength = 1;

        [Header("Hover look")]
        public Color hoverColor = new Color(1f, 0.9f, 0.3f);
        public float hoverScale = 1.35f;

        public bool Found { get; private set; }

        Renderer rend;
        Color baseColor;
        Vector3 baseScale;
        bool hovering;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            baseScale = transform.localScale;
            if (rend)
                baseColor = rend.material.HasProperty("_BaseColor") ? rend.material.GetColor("_BaseColor") : rend.material.color;
        }

        public void SetHover(bool on)
        {
            if (Found || hovering == on) return;
            hovering = on;
            transform.localScale = on ? baseScale * hoverScale : baseScale;
            if (rend)
            {
                var col = on ? hoverColor : baseColor;
                if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", col);
                else rend.material.color = col;
            }
        }

        public void Discover()
        {
            if (Found) return;
            Found = true;

            var ev = GetComponentInParent<EvidenceObject>();
            var o = new Observation
            {
                id = name + "_" + GetInstanceID(),
                text = text,
                implicates = implicates,
                reliability = reliability,
                strength = strength,
                source = ev ? ev.displayName : transform.root.name
            };
            if (Notepad.I) Notepad.I.Add(o);

            var r = GetComponent<Renderer>(); if (r) r.enabled = false;
            var c = GetComponent<Collider>(); if (c) c.enabled = false;
        }
    }
}
