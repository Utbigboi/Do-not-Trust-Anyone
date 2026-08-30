using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CaseDesk
{
    // Tiny helpers to build screen-space overlays from code (menu / game over / tutorial).
    public static class UIKit
    {
        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<StandaloneInputModule>();
            }
        }

        public static Canvas Canvas(string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = order;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            return c;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        public static Image Dim(Transform parent, Color col)
        {
            var rt = Rect("Dim", parent); Stretch(rt);
            var img = rt.gameObject.AddComponent<Image>(); img.color = col; img.raycastTarget = true;
            return img;
        }

        // centered vertical box with a translucent backing
        public static RectTransform Box(Transform parent, Vector2 size, Color backing)
        {
            var rt = Rect("Box", parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>(); img.color = backing;
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(40, 40, 36, 36); v.spacing = 16;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            v.childAlignment = TextAnchor.MiddleCenter;
            return rt;
        }

        public static TMP_Text Text(Transform parent, string s, int size, Color col, TMP_FontAsset font, float minH = 0)
        {
            var rt = Rect("Text", parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = s; t.fontSize = size; t.color = col; t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.Normal;
            if (font) t.font = font;
            if (minH > 0) { var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = minH; }
            return t;
        }

        public static Button Button(Transform parent, string label, int size, Color btnCol, Color txtCol, TMP_FontAsset font, System.Action onClick, float height = 60)
        {
            var rt = Rect("Btn_" + label, parent);
            var img = rt.gameObject.AddComponent<Image>(); img.color = btnCol;
            var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = height;
            b.onClick.AddListener(() => { AudioManager.I?.Click(); onClick?.Invoke(); });
            var trt = Rect("Label", rt); Stretch(trt);
            var t = trt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = label; t.fontSize = size; t.color = txtCol; t.alignment = TextAlignmentOptions.Center;
            if (font) t.font = font;
            return b;
        }
    }
}
