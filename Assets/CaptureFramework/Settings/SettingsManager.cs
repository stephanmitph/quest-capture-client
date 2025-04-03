using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // Default settings here

    [Header("Network Settings")]
    public string serverIP = "172.20.10.3";  
    public int serverPort = 8080;

    [Header("Graphics Settings")]
    public ImageQuality imageQuality = ImageQuality._75;
    public ImageResolution imageResolution = ImageResolution._800x600;
    public CameraEye cameraEye = CameraEye.Left;

    public enum ImageQuality { _100 = 100, _75 = 75, _50 = 50, _25 = 25 }
    public enum CameraEye { Left, Right }
    public enum ImageResolution { _1280x960, _800x600, _640x480, _320x240 }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
