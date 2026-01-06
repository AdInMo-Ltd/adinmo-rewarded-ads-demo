using UnityEngine;

namespace Adinmo.Examples
{
    public class CapsuleController : MonoBehaviour
    {
        [SerializeField]
        GameObject CameraRoot;
        [SerializeField]
        float speed = 10.0f;
        [SerializeField]
        float mouseSpeed = 10.0f;
        CharacterController characterController;
        float localXAngle = 0;
        // Start is called before the first frame update
        void Start()
        {
            if (Application.isMobilePlatform)
            {
                enabled = false;
            }
            else
            {
                characterController = gameObject.GetComponent<CharacterController>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 inputVector = Vector2.zero;
            if (UnityEngine.Input.GetKey("w"))
            {
                inputVector = Vector2.up;
            }
            else if (UnityEngine.Input.GetKey("s"))
            {
                inputVector = Vector2.down;
            }
            if (UnityEngine.Input.GetKey("a"))
            {
                inputVector = Vector2.left;
            }
            else if (UnityEngine.Input.GetKey("d"))
            {
                inputVector = Vector2.right;
            }
            if (inputVector != Vector2.zero)
            {
                Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;
                characterController.Move(speed * Time.deltaTime * desiredMove);
            }
            if (UnityEngine.Input.GetMouseButton(1)
#if !UNITY_EDITOR
                || UnityEngine.Input.GetMouseButton(0)
#endif
            )
            {

                transform.Rotate(Vector3.up, UnityEngine.Input.GetAxis("Mouse X") * mouseSpeed);
                localXAngle -= UnityEngine.Input.GetAxis("Mouse Y") * mouseSpeed;
                if (localXAngle > 85)
                    localXAngle = 85;
                else if (localXAngle < -85)
                    localXAngle = -85;
                CameraRoot.transform.localEulerAngles = new Vector3(localXAngle, 0, 0);
            }
        }
    }
}