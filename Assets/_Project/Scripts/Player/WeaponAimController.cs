using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class WeaponAimController : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private float aimSpeed = 15f;

    [Header("Clamp")]
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;

    private PlayerController _playerController;
    private Transform _cameraTransform;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (weaponPivot == null) return;
        if (_playerController == null) return;

        AimWeapon();
    }

    private void AimWeapon()
    {
        Vector3 aimDirection = _playerController.GetAimDirection();

        // Convert aim direction to local rotation
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);

        float verticalAngle = targetRotation.eulerAngles.x;

        if (verticalAngle > 180f) verticalAngle -= 360f;

        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        // Apply only vertical rotation to weapon pivot
        Quaternion weaponRotation = Quaternion.Euler(verticalAngle, weaponPivot.localEulerAngles.y, 0f);

        // Smooth rotation
        weaponPivot.localRotation = Quaternion.Slerp(
            weaponPivot.localRotation,
            weaponRotation,
            aimSpeed * Time.deltaTime
        );
    }
}
