using UnityEngine;

namespace brickpack {
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 10f;
        public float mouseSensitivity = 100f;

        private float pitch = 0f;
        private float yaw = 0f;

        void Update()
        {

            float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            transform.Translate(horizontal, 0, vertical);


            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, -90f, 90f);

                transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }
        }
    }
}