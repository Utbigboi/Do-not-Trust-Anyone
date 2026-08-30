using TMPro;
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
        [Tooltip("Extra pixels of gap under THIS clue in the notepad (0 = use the list default).")]
        public float extraGapBelow = 0f;

        [Header("Hover look")]
        public Color hoverColor = new Color(1f, 0.9f, 0.3f);
        public float hoverScale = 1.15f;
        [Tooltip("Hidden until hovered, then the ring/text fades in (recommended).")]
        public bool hiddenUntilHover = true;
        public float fadeSpeed = 10f;
        [Tooltip("Gentle pulse while hovered.")]
        public float pulseAmount = 0.08f;
        public float pulseSpeed = 4f;
        [Tooltip("Keep a TMP text marker visible after it's been noted (don't hide it on click).")]
        public bool keepTextAfterFound = true;
        [Tooltip("For TMP markers: leave the material/shader colours alone (script won't tint or fade them).")]
        public bool tmpUseOwnColors = true;

        public bool Found { get; private set; }

        Renderer rend;
        TMP_Text tmp;                 // set if this marker is a TextMeshPro (3D) object
        Color baseColor;
        Vector3 baseScale;
        bool hovering;
        float alpha;                  // current shown alpha (0..1)

        void Awake()
        {
            baseScale = transform.localScale;
            tmp = GetComponent<TMP_Text>();
            if (tmp != null)
                baseColor = tmp.color;
            else
            {
                rend = GetComponent<Renderer>();
                if (rend)
                    baseColor = rend.material.HasProperty("_BaseColor") ? rend.material.GetColor("_BaseColor") : rend.material.color;
            }
            bool skipColor = tmp != null && tmpUseOwnColors;
            alpha = (hiddenUntilHover && !skipColor) ? 0f : 1f;
            if (!skipColor) ApplyAlpha(alpha);
        }

        void Update()
        {
            if (Found) return;

            bool skipColor = tmp != null && tmpUseOwnColors;
            if (!skipColor)
            {
                float target = hovering ? 1f : (hiddenUntilHover ? 0f : 1f);
                alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);
                ApplyAlpha(alpha);
            }

            // scale/pulse still applies (nice hover cue without touching colours)
            float pulse = hovering ? 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount : 1f;
            transform.localScale = baseScale * (hovering ? hoverScale : 1f) * pulse;
        }

        void ApplyAlpha(float a)
        {
            if (tmp != null)
            {
                if (tmpUseOwnColors) return;            // respect your material/shader colours
                var tc = hovering ? hoverColor : baseColor; tc.a = a; tmp.color = tc;
                return;
            }
            if (rend)
            {
                var c = hovering ? hoverColor : baseColor; c.a = a;
                if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", c);
                else rend.material.color = c;
            }
        }

        public void SetHover(bool on)
        {
            if (Found) return;
            hovering = on;
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
                extraGapBelow = extraGapBelow,
                source = ev ? ev.displayName : transform.root.name
            };
            if (Notepad.I) Notepad.I.Add(o);

            var c = GetComponent<Collider>(); if (c) c.enabled = false;   // can't be noted twice

            if (tmp != null)
            {
                if (!keepTextAfterFound) tmp.enabled = false;
                else
                {
                    hovering = false;
                    if (!tmpUseOwnColors) { var col = baseColor; col.a = 1f; tmp.color = col; }
                    transform.localScale = baseScale;
                }
            }
            else
            {
                var r = GetComponent<Renderer>(); if (r) r.enabled = false;
            }
        }
    }
}
