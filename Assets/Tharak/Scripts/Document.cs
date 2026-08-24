using Unity.VisualScripting;
using UnityEngine;

namespace SafehouseDesk
{
    //A piece of intel you can pick up and inspect.Built from primitives at runtime.
    // Now a thin cuboid "plate" (not a flat quad) so it never goes edge-on / paper-thin.
    public class Document : MonoBehaviour, IInteractable
    {
        public IntelData data;
        public bool inspected;         // opened close inspection at least once
        public bool watermarkChecked;  // held under the lamp on the back

        public static Document Create(IntelData data, Vector3 pos, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);       // solid plate
            go.name = "Document_" + data.id;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);          // large face up
            go.transform.localScale = new Vector3(0.21f, 0.297f, 0.02f);    // A4-ish, ~2cm thick

            var r = go.GetComponent<Renderer>();
            if (mat != null) r.material = mat;
            r.material.color = data.hasSeal ? new Color(0.86f, 0.78f, 0.66f)
                                            : new Color(0.93f, 0.93f, 0.90f);

            // Cube already carries a BoxCollider sized to the mesh - keep it.

            var d = go.AddComponent<Document>();
            d.data = data;
            return d;
        }

        public string Prompt(PlayerInteractor p) => p.Held == null ? "[E] Pick up document" : "";

        public void Interact(PlayerInteractor p)
        {
            if (p.Held == null) p.PickUp(this);
        }

        public string[] HudLines()
        {
            string seal = data.hasSeal ? "Red wax seal (front)" : "No seal";
            string wm;
            if (!inspected) wm = "[F] Inspect it closely";
            else if (!watermarkChecked) wm = "Flip it + hold under the lamp to read the watermark";
            else if (!data.hasSeal) wm = "Watermark: none (plain paper)";
            else if (data.sealIsForged) wm = "Watermark: MISSING - the seal is forged!";
            else wm = "Watermark: genuine";

            return new[] { "\"" + data.text + "\"", "Source: " + data.source, seal, wm };
        }
    }
}
