using UnityEngine;

namespace CaseDesk
{
    // Keeps evidence on the table: builds invisible wall colliders around the edge and
    // clamps the drag target inside. Put on your TableSystem and assign TableTop.
    public class TableBounds : MonoBehaviour
    {
        public Transform tableTop;                       // surface centre (x,z) + height (y)
        public Vector2 size = new Vector2(1.59f, 0.6f);  // FULL width (x) and depth (z) of the worktop
        public float margin = 0.06f;                     // keep pieces this far from the edge

        [Header("Invisible walls")]
        public bool buildWalls = true;
        public float wallHeight = 0.4f;
        public float wallThickness = 0.05f;

        public Vector3 Center => tableTop ? tableTop.position : transform.position;

        void Awake() { if (buildWalls) BuildWalls(); }

        public Vector3 Clamp(Vector3 p)
        {
            var c = Center;
            float hx = size.x * 0.5f - margin;
            float hz = size.y * 0.5f - margin;
            p.x = Mathf.Clamp(p.x, c.x - hx, c.x + hx);
            p.z = Mathf.Clamp(p.z, c.z - hz, c.z + hz);
            return p;
        }

        void BuildWalls()
        {
            var c = Center;
            float hx = size.x * 0.5f, hz = size.y * 0.5f;
            Make("Wall_L", new Vector3(c.x - hx - wallThickness * 0.5f, c.y + wallHeight * 0.5f, c.z), new Vector3(wallThickness, wallHeight, size.y + wallThickness * 2f));
            Make("Wall_R", new Vector3(c.x + hx + wallThickness * 0.5f, c.y + wallHeight * 0.5f, c.z), new Vector3(wallThickness, wallHeight, size.y + wallThickness * 2f));
            Make("Wall_B", new Vector3(c.x, c.y + wallHeight * 0.5f, c.z - hz - wallThickness * 0.5f), new Vector3(size.x + wallThickness * 2f, wallHeight, wallThickness));
            Make("Wall_F", new Vector3(c.x, c.y + wallHeight * 0.5f, c.z + hz + wallThickness * 0.5f), new Vector3(size.x + wallThickness * 2f, wallHeight, wallThickness));
        }

        void Make(string n, Vector3 pos, Vector3 sz)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.AddComponent<BoxCollider>().size = sz;   // no renderer => invisible
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.7f);
            var c = Center;
            Gizmos.DrawWireCube(new Vector3(c.x, c.y, c.z), new Vector3(size.x, 0.02f, size.y));
        }
    }
}
