using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float zoomFactor = 1.0f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float zoomSmoothSpeed = 0.125f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float currentZoomVelocity;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;

        // Adjust the camera's orthographic size based on the black hole's scale
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, target.localScale.x * zoomFactor);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref currentZoomVelocity, zoomSmoothSpeed);
    }
}
