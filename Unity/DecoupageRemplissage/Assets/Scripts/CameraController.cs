using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cameraScene;
    public float zoomSpeed = 1f;
    public float minZoom = 1f;
    public float maxZoom = 20f;
    public float panSpeed = 0.5f;

    private UIManager uiManager;
    private Vector3 dragOrigin;
    private bool isPanning = false;

    private void Awake()
    {
        if (cameraScene == null)
        {
            cameraScene = Camera.main;
        }
        uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Update()
    {
        GererZoom();
        GererPan();
    }

    private void GererZoom()
    {
        if (uiManager != null && uiManager.IsMouseOverUI()) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (cameraScene.orthographic)
            {
                cameraScene.orthographicSize -= scroll * zoomSpeed;
                cameraScene.orthographicSize = Mathf.Clamp(cameraScene.orthographicSize, minZoom, maxZoom);
            }
            else
            {
                Vector3 pos = cameraScene.transform.position;
                pos.z += scroll * zoomSpeed;
                pos.z = Mathf.Clamp(pos.z, -maxZoom, -minZoom);
                cameraScene.transform.position = pos;
            }
        }
    }

    private void GererPan()
    {
        if (uiManager != null && uiManager.IsMouseOverUI()) return;
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cameraScene.ScreenToWorldPoint(Input.mousePosition);
            isPanning = true;
        }

        if (Input.GetMouseButton(2) && isPanning)
        {
            Vector3 currentPos = cameraScene.ScreenToWorldPoint(Input.mousePosition);
            Vector3 diff = dragOrigin - currentPos;
            cameraScene.transform.position += diff * panSpeed;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }
    }

    public void ResetCamera()
    {
        cameraScene.transform.position = new Vector3(0, 0, -10);
        if (cameraScene.orthographic)
        {
            cameraScene.orthographicSize = 5f;
        }
    }
}