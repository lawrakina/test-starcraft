using UnityEngine;

/// <summary>
/// Simple camera controller for panning and zooming
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private Vector2 panLimitX = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 panLimitZ = new Vector2(-50f, 50f);

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical);
        Vector3 newPosition = transform.position + direction * panSpeed * Time.deltaTime;

        // Clamp position
        newPosition.x = Mathf.Clamp(newPosition.x, panLimitX.x, panLimitX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, panLimitZ.x, panLimitZ.y);

        transform.position = newPosition;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= scroll * zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
            transform.position = newPosition;
        }
    }

    public void ResetCamera()
    {
        transform.position = startPosition;
    }
}
