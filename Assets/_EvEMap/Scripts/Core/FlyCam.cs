using UnityEngine;
using UnityEngine;

public class FlyCam : MonoBehaviour {
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

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
}