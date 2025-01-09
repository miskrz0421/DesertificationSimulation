using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Vector2 boundaryMin = new Vector2(-20, -20); 
    public Vector2 boundaryMax = new Vector2(20, 20);
    public float zoomSpeed = 4f;
    public float minZoom = 2.5f; 
    public float maxZoom = 25f;   

    public TextMeshProUGUI zoomText;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No main camera found!");
        }

        cam.orthographicSize = 17.5f;
        UpdateZoomText(); 
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 newPosition = transform.position + new Vector3(moveX, moveY, 0) * moveSpeed * 0.01f;

        float cameraHalfHeight = cam.orthographicSize; 
        float cameraHalfWidth = cameraHalfHeight * cam.aspect; 

        float minX = boundaryMin.x;
        float maxX = boundaryMax.x;
        float minY = boundaryMin.y; 
        float maxY = boundaryMax.y; 

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            float newZoom = cam.orthographicSize - scrollInput * zoomSpeed;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

            cam.orthographicSize = newZoom;

            UpdateZoomText(); 
        }
    }

    void UpdateZoomText()
    {
        if (zoomText != null)
        {
            float zoomLevel = 5.0f/cam.orthographicSize; 
            zoomText.text = $"Zoom: {zoomLevel:F2}x"; 
        }
    }
}
