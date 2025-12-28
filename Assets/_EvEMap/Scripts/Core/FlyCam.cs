using _ProjectEvE.Scripts.UX;
using UnityEngine;

public class FlyCam : MonoBehaviour {
    public float ZoomToPlanetDistance = 2;
    public float ZoomToPlanetSpeed = 2;
    public float ZoomToPlanetRotationSpeed = 1;
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public float ZoomSensitivity = 0.1f;
    public float MinZoom = 0.1f;
    public float MaxZoom = 50f;
    [SerializeField] private UISystem targetSystem = null;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float desiredZoomLevel = 1;
    
    
    void Update() {
        // Movement
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.E)) moveDirection += transform.up;
        if (Input.GetKey(KeyCode.Q)) moveDirection -= transform.up;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        
        // Zooming in and out
        desiredZoomLevel = Mathf.Clamp(desiredZoomLevel + Input.mouseScrollDelta.y, MinZoom, MaxZoom);
        Map.Instance.transform.localScale = Vector3.one* desiredZoomLevel;
        
        // Zooming toward planet
        if (targetSystem != null) {
            float distanceToTarget = Vector3.Distance(transform.position, targetSystem.transform.position);
            
            // Move toward target
            transform.position = Vector3.Lerp(transform.position, targetSystem.transform.position, Time.deltaTime*ZoomToPlanetSpeed);

            // Face target
            var targetRotation = Quaternion.LookRotation(targetSystem.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * ZoomToPlanetRotationSpeed);
            if (distanceToTarget <= ZoomToPlanetDistance) {
                targetSystem = null;
            }
        }

        // Looking around
        if (Input.GetMouseButton(1)) {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limit vertical rotation

            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }

    public void ZoomToSystem(UISystem uiSystem) {
        targetSystem = uiSystem;
    }
}