using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 1.5f, -4f);

    [Header("Mouse Sensitivity")]
    [SerializeField] private float sensitivityX = 0.1f;
    [SerializeField] private float sensitivityY = 0.1f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minTilt = -20f;
    [SerializeField] private float maxTilt = 45f;

    private float _yaw;
    private float _pitch;
    private Vector2 _lookInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _yaw = target.eulerAngles.y;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {
        HandleRotation();
        FollowTarget();
    }

    private void HandleRotation()
    {
        _yaw += _lookInput.x * sensitivityX;
        _pitch -= _lookInput.y * sensitivityY;
        _pitch = Mathf.Clamp(_pitch, minTilt, maxTilt);
    }

    private void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position = target.position + rotation * offset;
        transform.rotation = rotation;
    }
}