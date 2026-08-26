using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Optional: scatters primitive evidence (spaced out) so it runs on Play.
    public class EvidenceSpawner : MonoBehaviour
    {
        public Transform tableTop;
        public Vector2 area = new Vector2(1.2f, 0.5f);
        public int count = 4;
        public float minSpacing = 0.28f;    // min distance between spawned pieces
        public string[] names = { "Matchbook", "Photograph", "Letter", "Bottle" };

        [Header("Outline (QuickOutline)")]
        public Color outlineColor = Color.white;
        [Range(0f, 12f)] public float outlineWidth = 6f;
        public global::Outline.Mode outlineMode = global::Outline.Mode.OutlineVisible;

        void Start()
        {
            Vector3 c = tableTop ? tableTop.position : Vector3.zero;
            var placed = new List<Vector2>();

            for (int i = 0; i < count; i++)
            {
                Vector2 xz = PickSpot(c, placed);
                placed.Add(xz);

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Evidence_" + (i < names.Length ? names[i] : i.ToString());
                go.transform.localScale = Shape(i);
                float y = c.y + go.transform.localScale.y / 2f;
                go.transform.position = new Vector3(xz.x, y, xz.y);
                go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                var mat = go.GetComponent<Renderer>().material;
                Color col = Color.HSVToRGB((i * 0.16f) % 1f, 0.45f, 0.9f);
                mat.color = col;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);

                var e = go.AddComponent<EvidenceObject>();
                e.displayName = go.name.Replace("Evidence_", "");
                e.outlineColor = outlineColor;
                e.outlineWidth = outlineWidth;
                e.outlineMode = outlineMode;
                e.RefreshHighlighter();
            }
        }

        Vector2 PickSpot(Vector3 c, List<Vector2> placed)
        {
            for (int tries = 0; tries < 40; tries++)
            {
                float x = c.x + Random.Range(-area.x / 2f, area.x / 2f);
                float z = c.z + Random.Range(-area.y / 2f, area.y / 2f);
                var p = new Vector2(x, z);
                bool ok = true;
                foreach (var q in placed)
                    if (Vector2.Distance(p, q) < minSpacing) { ok = false; break; }
                if (ok) return p;
            }
            // fallback: just offset along a row
            return new Vector2(c.x - area.x / 2f + placed.Count * minSpacing, c.z);
        }

        Vector3 Shape(int i)
        {
            switch (i % 3)
            {
                case 0:  return new Vector3(0.18f, 0.02f, 0.25f);
                case 1:  return new Vector3(0.12f, 0.06f, 0.08f);
                default: return new Vector3(0.06f, 0.14f, 0.06f);
            }
        }
    }
}
