using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the positioning and orientation of a UI element in relation to a VR camera.
/// This component ensures the UI follows the VR camera's movement while maintaining a fixed distance and always facing the user.
/// </summary>
/// <remarks>
/// The UI element will:
/// - Maintain a constant distance from the VR camera
/// - Smoothly follow the camera's position
/// - Always face towards the user
/// - Ignore vertical camera movement
/// </remarks>
public class HMDFollowingUI : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform vrCamera;  // Reference to the VR Camera
    [SerializeField] private float smoothSpeed = 5.0f;
    private Vector3 targetPosition;
    private float distanceFromCamera;

    private void Awake()
    {
        distanceFromCamera = transform.position.z;
    }

    private void Update()
    {
        UpdateCanvasPosition();
    }

    private void UpdateCanvasPosition()
    {
        // Calculate target position in front of the camera
        Vector3 forwardDirection = vrCamera.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        // Distance from the camera calculated by distanceFromCamera
        targetPosition = vrCamera.position + forwardDirection * distanceFromCamera;

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Make the camera face the VR camera
        transform.LookAt(vrCamera.position);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);
    }
}