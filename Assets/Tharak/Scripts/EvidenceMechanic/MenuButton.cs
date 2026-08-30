using UnityEngine;
using UnityEngine.EventSystems;

namespace CaseDesk
{
    // Put on each menu Button. Pops the button up a little on hover and plays a click SFX.
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Hover pop")]
        public float hoverScale = 1.08f;
        public float speed = 12f;

        [Header("Hover sound (soft tick)")]
        public AudioClip hoverSfx;
        public float hoverMaxSeconds = 0f;

        [Header("Click sound (optional; else uses AudioManager click)")]
        public AudioClip clickSfx;
        public float clickMaxSeconds = 0f;

        Vector3 baseScale;
        Vector3 target;

        void Awake() { baseScale = transform.localScale; target = baseScale; }
        void OnEnable() { transform.localScale = baseScale; target = baseScale; }

        void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * speed);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            target = baseScale * hoverScale;
            if (hoverSfx != null) AudioManager.I?.SfxClip(hoverSfx, hoverMaxSeconds);
            else AudioManager.I?.Hover();
        }
        public void OnPointerExit(PointerEventData e) { target = baseScale; }

        public void OnPointerClick(PointerEventData e)
        {
            if (clickSfx != null) AudioManager.I?.SfxClip(clickSfx, clickMaxSeconds);
            else AudioManager.I?.Click();
        }
    }
}
