using UnityEngine;

/// <summary>
/// Optional component to visualize drone state with colors and debug info
/// </summary>
public class DroneVisualizer : MonoBehaviour
{
    [SerializeField] private Drone drone;
    [SerializeField] private Renderer droneRenderer;
    [SerializeField] private bool showDebugInfo = true;

    private Color idleColor = Color.gray;
    private Color searchColor = Color.yellow;
    private Color moveColor = Color.cyan;
    private Color collectColor = Color.green;
    private Color returnColor = Color.blue;

    private void Awake()
    {
        if (drone == null)
            drone = GetComponent<Drone>();
        
        if (droneRenderer == null)
            droneRenderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (droneRenderer == null || drone == null)
            return;

        Color targetColor = idleColor;
        
        switch (drone.CurrentState)
        {
            case Drone.DroneState.Idle:
                targetColor = idleColor;
                break;
            case Drone.DroneState.SearchResource:
                targetColor = searchColor;
                break;
            case Drone.DroneState.MoveToResource:
                targetColor = moveColor;
                break;
            case Drone.DroneState.CollectResource:
                targetColor = collectColor;
                break;
            case Drone.DroneState.ReturnToBase:
                targetColor = returnColor;
                break;
        }

        droneRenderer.material.color = targetColor;
    }

    private void OnGUI()
    {
        if (!showDebugInfo || drone == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        
        if (screenPos.z > 0)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            
            string info = $"{drone.CurrentState}\nCarrying: {drone.CarriedResources}";
            GUI.Label(new Rect(screenPos.x - 30, Screen.height - screenPos.y - 30, 100, 40), info, style);
        }
    }
}
