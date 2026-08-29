using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CaseDesk
{
    // Builds the ENTIRE screen-space notepad UI from code. No Canvas/prefab setup needed:
    // add this to one empty object, assign a Texture + a TMP font, press Play.
    // Shows while inspecting a piece, or when a phone/fax is open. Replaces CaseHUD / NotepadUI.
    public class NotepadUICode : MonoBehaviour
    {
        [Header("State ref")]
        public EvidenceInspector inspector;         // Main Camera's EvidenceInspector

        [Header("Look")]
        public Texture notepadTexture;              // your notepad background (optional)
        public TMP_FontAsset font;                  // handwriting TMP font (optional)
        public Color paperTint = Color.white;
        public Color textColor = new Color(0.24f, 0.18f, 0.12f);   // ink
        public Vector2 panelSize = new Vector2(620, 720);
        public int fontSize = 28;

        [Header("Content margins (px)")]
        [Tooltip("Symmetric side padding for the whole page (header/meters center within this).")]
        public int padSide = 90;
        public int padTop = 90;
        public int padBottom = 80;
        public float itemSpacing = 10f;
        [Tooltip("Extra left inset for the NOTE LIST and button rows, to clear the spiral binding.")]
        public int listLeftInset = 70;
        [Tooltip("Right inset for the button rows so they sit symmetrically.")]
        public int buttonRightInset = 20;

        [Header("Buttons")]
        public Color buttonColor = new Color(0.16f, 0.12f, 0.10f, 0.9f);
        public Color buttonTextColor = new Color(0.96f, 0.93f, 0.86f);
        public float buttonHeight = 48f;

        [Header("Rows — outline checkbox")]
        public Color toggleBorderColor = new Color(0f, 0f, 0f, 1f);        // black outline
        public Color toggleCheckColor = new Color(0.12f, 0.10f, 0.08f, 1f);// fill when ticked
        public float toggleBorderWidth = 2.5f;
        public float toggleSize = 26f;

        GameObject root;
        RectTransform content;
        TMP_Text headerText, metersText, resultText;
        GameObject tipGroup, leakGroup, resultGroup;
        readonly List<Toggle> rowToggles = new List<Toggle>();
        int lastCount = -1;

        void Awake() { Build(); }

        // ---------------- build ----------------
        void Build()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<StandaloneInputModule>();
            }

            var canvasGO = new GameObject("NotepadCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // root panel (right side)
            root = NewRect("Notepad", canvasGO.transform).gameObject;
            var rrt = root.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(1f, 0.5f);
            rrt.pivot = new Vector2(1f, 0.5f);
            rrt.sizeDelta = panelSize;
            rrt.anchoredPosition = new Vector2(-20f, 0f);
            var bg = root.AddComponent<RawImage>();
            bg.texture = notepadTexture;
            bg.color = paperTint;
            bg.raycastTarget = true;
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(padSide, padSide, padTop, padBottom);
            vlg.spacing = itemSpacing;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            headerText = MakeText(root.transform, "NOTES", fontSize + 6, TextAlignmentOptions.Center, 46);
            metersText = MakeText(root.transform, "", fontSize - 8, TextAlignmentOptions.Center, 56);

            content = MakeScroll(root.transform);   // flexible height list

            tipGroup = MakeGroup(root.transform);
            MakeButton(tipGroup.transform, "Tip selected", TipSelected);
            MakeButton(tipGroup.transform, "Hang up", CloseCall);

            leakGroup = MakeGroup(root.transform, false);              // vertical stack of two rows
            var leakTop = MakeRowContainer(leakGroup.transform, true, true); // 3 buttons, expand to fill
            MakeButton(leakTop.transform, "Blame Red", () => Leak(Party.Red));
            MakeButton(leakTop.transform, "Blame Blue", () => Leak(Party.Blue));
            MakeButton(leakTop.transform, "Leak It", () => Leak(Party.None));
            var leakBottom = MakeRowContainer(leakGroup.transform, true); // centered Cancel
            MakeButton(leakBottom.transform, "Cancel", CloseCall, 220f);

            resultGroup = MakeGroup(root.transform, false);
            resultText = MakeText(resultGroup.transform, "", fontSize - 6, TextAlignmentOptions.TopLeft, 120);
            MakeButton(resultGroup.transform, "Next case", NextOrRestart);

            root.SetActive(false);
        }

        // ---------------- update ----------------
        void Update()
        {
            var cm = CaseManager.I; var np = Notepad.I;
            if (cm == null || np == null) { if (root) root.SetActive(false); return; }
            var cc = CallController.I; var rm = RoundManager.I;

            bool end = rm != null && rm.state != RoundManager.State.Playing;
            bool call = cc != null && cc.IsOpen;
            bool focused = inspector != null && inspector.IsFocused;
            bool show = end || call || focused;
            root.SetActive(show);
            if (!show) return;

            if (np.entries.Count != lastCount) Rebuild();

            var mode = cc != null ? cc.mode : CallController.Mode.None;
            bool tipMode = call && mode == CallController.Mode.Tip;
            bool leakMode = call && mode == CallController.Mode.Leak;
            bool resultMode = end || (call && mode == CallController.Mode.Result);

            headerText.text = resultMode ? "THE EVENING GULL"
                            : tipMode ? "PHONE — tip the " + cc.tipFaction
                            : leakMode ? "FAX — feed the Gull"
                            : "NOTES";
            metersText.text = $"Red {cm.susRed}   Blue {cm.susBlue}   Board {cm.susOrg}\nCredibility {cm.credibility}";

            tipGroup.SetActive(tipMode);
            leakGroup.SetActive(leakMode);
            resultGroup.SetActive(resultMode);

            if (end) resultText.text = (rm.state == RoundManager.State.Won ? "You made it out (for now).\n" : "You're finished.\n") + cm.lastResult;
            else if (resultMode) resultText.text = cc.resultText;

            bool selectable = tipMode || leakMode;
            foreach (var t in rowToggles) if (t) t.interactable = selectable;
        }

        void Rebuild()
        {
            foreach (Transform c in content) Destroy(c.gameObject);
            rowToggles.Clear();
            var np = Notepad.I;
            lastCount = np.entries.Count;
            for (int i = 0; i < np.entries.Count; i++)
            {
                var o = np.entries[i];
                rowToggles.Add(MakeRow(content, "• " + o.text + "   (" + o.source + ")"));
            }
        }

        List<Observation> Checked()
        {
            var l = new List<Observation>();
            var np = Notepad.I;
            for (int i = 0; i < rowToggles.Count && i < np.entries.Count; i++)
                if (rowToggles[i] && rowToggles[i].isOn) l.Add(np.entries[i]);
            return l;
        }
        void ClearToggles() { foreach (var t in rowToggles) if (t) t.isOn = false; }

        void TipSelected() { var cc = CallController.I; if (cc == null) return; foreach (var o in Checked()) CaseManager.I.Tip(o, cc.tipFaction); ClearToggles(); }
        void Leak(Party v) { CaseManager.I.Accuse(v, Checked()); CallController.I.OpenResult(CaseManager.I.lastResult); ClearToggles(); }
        void CloseCall() { CallController.I?.Close(); ClearToggles(); }
        void NextOrRestart()
        {
            var rm = RoundManager.I;
            if (rm != null && rm.state != RoundManager.State.Playing) { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); return; }
            CallController.I?.Close(); rm?.Advance();
        }

        // ---------------- ui helpers ----------------
        RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        TMP_Text MakeText(Transform parent, string s, int size, TextAlignmentOptions align, float minHeight)
        {
            var rt = NewRect("Text", parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = s; t.fontSize = size; t.color = textColor; t.alignment = align; t.textWrappingMode = TextWrappingModes.Normal;
            if (font) t.font = font;
            var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = minHeight;
            return t;
        }

        GameObject MakeRowContainer(Transform parent, bool horizontal, bool expand = false)
        {
            var rt = NewRect("Row", parent);
            rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);
            var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = buttonHeight + 4f;
            if (horizontal)
            {
                var hg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
                hg.spacing = 8; hg.childControlWidth = hg.childControlHeight = true;
                hg.childForceExpandWidth = expand; hg.childForceExpandHeight = false;
                hg.childAlignment = TextAnchor.MiddleCenter;   // centers the Cancel row
            }
            else
            {
                var vg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
                vg.spacing = 6; vg.childControlWidth = vg.childControlHeight = true;
                vg.childForceExpandWidth = false; vg.childForceExpandHeight = false;
                vg.childAlignment = TextAnchor.MiddleCenter;
            }
            return rt.gameObject;
        }

        GameObject MakeGroup(Transform parent, bool horizontal = true)
        {
            var rt = NewRect("Group", parent);
            var pad = new RectOffset(listLeftInset, buttonRightInset, 0, 0);   // clear the spiral on the left
            if (horizontal)
            {
                var hg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
                hg.padding = pad;
                hg.spacing = 10;
                hg.childControlWidth = hg.childControlHeight = true;
                hg.childForceExpandWidth = true; hg.childForceExpandHeight = false;
                hg.childAlignment = TextAnchor.MiddleCenter;
            }
            else
            {
                var vg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
                vg.padding = pad;
                vg.spacing = 6;
                vg.childControlWidth = vg.childControlHeight = true;
                vg.childForceExpandWidth = true; vg.childForceExpandHeight = false;
                vg.childAlignment = TextAnchor.MiddleCenter;
            }
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        void MakeButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, float fixedWidth = 0f)
        {
            var rt = NewRect("Btn_" + label, parent);
            var img = rt.gameObject.AddComponent<Image>(); img.color = buttonColor;
            var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(onClick);
            var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = buttonHeight; le.flexibleWidth = fixedWidth > 0f ? 0f : 1f;
            if (fixedWidth > 0f) { le.preferredWidth = fixedWidth; le.minWidth = fixedWidth; }
            var trt = NewRect("Label", rt); Stretch(trt);
            var t = trt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = label; t.fontSize = fontSize - 4; t.color = buttonTextColor; t.alignment = TextAlignmentOptions.Center;
            if (font) t.font = font;
        }

        Toggle MakeRow(Transform parent, string text)
        {
            // Row is a plain container; the label fills it with right padding reserved for the box.
            var rt = NewRect("Row", parent);
            rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);
            var le = rt.gameObject.AddComponent<LayoutElement>(); le.minHeight = 44;
            var fitter = rt.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rowVLG = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            rowVLG.padding = new RectOffset(0, (int)toggleSize + 14, 4, 4);   // reserve space on the right for the box
            rowVLG.childControlWidth = rowVLG.childControlHeight = true;
            rowVLG.childForceExpandWidth = true; rowVLG.childForceExpandHeight = false;

            // label
            var lbRt = NewRect("Label", rt);
            var t = lbRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = fontSize - 6; t.color = textColor;
            t.alignment = TextAlignmentOptions.Left;
            t.textWrappingMode = TextWrappingModes.Normal;
            if (font) t.font = font;

            // checkbox pinned to the RIGHT edge of the row, vertically centred (not in the layout)
            var tgRt = NewRect("Toggle", rt);
            tgRt.anchorMin = new Vector2(1f, 0.5f); tgRt.anchorMax = new Vector2(1f, 0.5f);
            tgRt.pivot = new Vector2(1f, 0.5f);
            tgRt.sizeDelta = new Vector2(toggleSize, toggleSize);
            tgRt.anchoredPosition = new Vector2(-90f, 15.5f);
            var leTg = tgRt.gameObject.AddComponent<LayoutElement>(); leTg.ignoreLayout = true;   // don't let the row layout move it

            var hit = tgRt.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0);
            var toggle = tgRt.gameObject.AddComponent<Toggle>();
            toggle.isOn = false; toggle.transition = Selectable.Transition.None; toggle.targetGraphic = hit;

            float bw = toggleBorderWidth;
            AddBar(tgRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, bw));   // top
            AddBar(tgRt, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, bw));   // bottom
            AddBar(tgRt, new Vector2(0, 0), new Vector2(0, 1), new Vector2(bw, 0));   // left
            AddBar(tgRt, new Vector2(1, 0), new Vector2(1, 1), new Vector2(bw, 0));   // right

            var ckRt = NewRect("Check", tgRt); Stretch(ckRt); ckRt.sizeDelta = new Vector2(-(bw * 2f + 6f), -(bw * 2f + 6f));
            var ckImg = ckRt.gameObject.AddComponent<Image>(); ckImg.color = toggleCheckColor;
            toggle.graphic = ckImg;

            return toggle;
        }

        RectTransform MakeScroll(Transform parent)
        {
            var scrollRt = NewRect("Scroll", parent);
            var sle = scrollRt.gameObject.AddComponent<LayoutElement>(); sle.flexibleHeight = 1;
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;

            var viewport = NewRect("Viewport", scrollRt); Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var vImg = viewport.gameObject.AddComponent<Image>(); vImg.color = new Color(1, 1, 1, 0.001f);

            var contentRt = NewRect("Content", viewport);
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1); contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, contentRt.sizeDelta.y);       // width == viewport width
            contentRt.anchoredPosition = new Vector2(0, contentRt.anchoredPosition.y);
            var v = contentRt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(listLeftInset, 0, 0, 0);   // push notes right, off the spiral
            v.spacing = 6; v.childControlWidth = v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            var fitter = contentRt.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport; scroll.content = contentRt;
            return contentRt;
        }

        void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        void AddBar(RectTransform parent, Vector2 aMin, Vector2 aMax, Vector2 size)
        {
            var rt = NewRect("Bar", parent);
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            var img = rt.gameObject.AddComponent<Image>(); img.color = toggleBorderColor;
        }
    }
}
