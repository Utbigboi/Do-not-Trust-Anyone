using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Put this on ANY evidence object. Needs a Collider. A Rigidbody is added automatically.
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

        [Header("Physics")]
        [Tooltip("Stop the piece tipping onto its side (it can still turn/yaw).")]
        public bool keepUpright = true;
        [Tooltip("How quickly a dropped piece levels out.")]
        public float settleTime = 0.25f;

        public bool OnWorkspace { get; set; }

        EvidenceHighlighter hl;
        Rigidbody rb;
        Coroutine settleCo;

        void Awake()
        {
            RefreshHighlighter();
            rb = GetComponent<Rigidbody>();
            if (!rb) rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (keepUpright)
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

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

        // Gently level the piece (keep its yaw, remove pitch/roll).
        public void SettleFlat()
        {
            if (!isActiveAndEnabled) return;
            if (settleCo != null) StopCoroutine(settleCo);
            settleCo = StartCoroutine(SettleRoutine());
        }

        IEnumerator SettleRoutine()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            Quaternion start = transform.rotation;
            Vector3 e = transform.eulerAngles;
            Quaternion target = Quaternion.Euler(0f, e.y, 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, settleTime);
                transform.rotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                if (rb && !rb.isKinematic) rb.angularVelocity = Vector3.zero;
                yield return null;
            }
            transform.rotation = target;
        }
    }
}
