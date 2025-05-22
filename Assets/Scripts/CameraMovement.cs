using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float viewSpeed;

    private float pitch = 0f;

    void Update()
    {
        UpdateCamera();
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float independentMoveSpeed = (Input.GetKey(KeyCode.LeftShift) ? 2 * movementSpeed : movementSpeed) * Time.deltaTime;

        Vector3 moveVector = transform.rotation * new Vector3(Input.GetAxis("Horizontal") * independentMoveSpeed, Input.GetAxis("Elevation") * independentMoveSpeed, Input.GetAxis("Vertical") * independentMoveSpeed);

        transform.position += moveVector;
    }

    private void UpdateCamera()
    {
        var mouseDelta = Input.mousePositionDelta;
        pitch = Mathf.Clamp(pitch - mouseDelta.y * viewSpeed, -90f, 90f);
        float yaw = transform.eulerAngles.y + mouseDelta.x * viewSpeed;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
}
