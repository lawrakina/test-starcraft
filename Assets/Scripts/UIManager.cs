using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text resourceText;
    [SerializeField] private Text droneText;
    [SerializeField] private Text instructionsText;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Camera minimapCamera;

    private void Start()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "Press SPACE to build a new drone\nDrones automatically collect resources";
        }

        SetupMinimap();
    }

    private void SetupMinimap()
    {
        if (minimapCamera != null && minimapImage != null)
        {
            RenderTexture minimapTexture = new RenderTexture(256, 256, 16);
            minimapCamera.targetTexture = minimapTexture;
            minimapImage.texture = minimapTexture;
        }
    }

    public void UpdateResourceCount(int count)
    {
        if (resourceText != null)
        {
            resourceText.text = $"Resources: {count}";
        }
    }

    public void UpdateDroneCount(int count)
    {
        if (droneText != null)
        {
            droneText.text = $"Drones: {count}";
        }
    }
}
