using UnityEngine;

namespace SafehouseDesk
{
    // Builds a placeholder room (floor, 4 walls, optional ceiling) from primitives at runtime.
    // Attach to any GameObject (e.g. Managers). Works in Built-in and URP.
    public class RoomBuilder : MonoBehaviour
    {
        [Header("Room size (metres)")]
        public Vector3 center = new Vector3(0f, 0f, 0.3f);
        public float width = 5f;    // X
        public float depth = 5f;    // Z
        public float height = 3f;   // Y
        public float thickness = 0.15f;

        [Header("Colours")]
        public Color floorColor   = new Color(0.16f, 0.15f, 0.14f);
        public Color wallColor    = new Color(0.21f, 0.20f, 0.19f);
        public Color ceilingColor = new Color(0.10f, 0.10f, 0.11f);
        public bool buildCeiling  = true;

        void Awake() => Build();

        void Build()
        {
            var root = new GameObject("Room").transform;
            root.SetParent(transform, false);
            float hw = width / 2f, hd = depth / 2f;

            Slab(root, "Floor",   new Vector3(center.x, -thickness / 2f, center.z),           new Vector3(width, thickness, depth), floorColor);
            if (buildCeiling)
                Slab(root, "Ceiling", new Vector3(center.x, height + thickness / 2f, center.z), new Vector3(width, thickness, depth), ceilingColor);

            Slab(root, "Wall_Back",  new Vector3(center.x, height / 2f, center.z - hd - thickness / 2f), new Vector3(width + thickness * 2f, height, thickness), wallColor);
            Slab(root, "Wall_Front", new Vector3(center.x, height / 2f, center.z + hd + thickness / 2f), new Vector3(width + thickness * 2f, height, thickness), wallColor);
            Slab(root, "Wall_Left",  new Vector3(center.x - hw - thickness / 2f, height / 2f, center.z), new Vector3(thickness, height, depth), wallColor);
            Slab(root, "Wall_Right", new Vector3(center.x + hw + thickness / 2f, height / 2f, center.z), new Vector3(thickness, height, depth), wallColor);
        }

        void Slab(Transform parent, string name, Vector3 pos, Vector3 size, Color c)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = size;
            var mat = go.GetComponent<Renderer>().material;
            mat.color = c;                                   // Built-in (_Color)
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c); // URP/HDRP
        }
    }
}
