using UnityEngine;

namespace SafehouseDesk
{
    // Clamped seated look - no locomotion.
    public class MouseLook : MonoBehaviour
    {
        public float sensitivity = 2f;
        public float yawLimit = 75f;
        public float pitchLimit = 65f;
        float yaw, pitch;

        void Start()
        {
            var e = transform.localEulerAngles;
            yaw = e.y > 180 ? e.y - 360 : e.y;
            pitch = e.x > 180 ? e.x - 360 : e.x;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.UIOpen) return;

            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            yaw = Mathf.Clamp(yaw, -yawLimit, yawLimit);
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
            transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        }
    }
}
