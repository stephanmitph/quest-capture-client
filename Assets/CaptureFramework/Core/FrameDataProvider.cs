
using System.Collections;
using PassthroughCameraSamples;
using UnityEngine;

public class FrameDataProvider : MonoBehaviour
{

    [SerializeField] public WebCamTextureManager webCamTextureManager;

    private Texture2D reuseTexture; // Reusable texture to avoid allocation/deallocation overhead

    private TrackingDataProvider trackingDataProvider;

    void Awake()
    {
        trackingDataProvider = GetComponent<TrackingDataProvider>();
    }

    void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnGraphicsSettingsChanged += OnCaptureSettingsChanged;
            SettingsManager.Instance.OnCameraSettingsChanged += OnCaptureSettingsChanged;
        }
    }

    private void OnCaptureSettingsChanged()
    {
        webCamTextureManager.ResetWebCamTexture(SettingsManager.Instance.GetImageResolution(), (PassthroughCameraEye)SettingsManager.Instance.cameraEye);
        reuseTexture = new Texture2D(
            SettingsManager.Instance.GetImageResolution().x,
            SettingsManager.Instance.GetImageResolution().y,
            TextureFormat.RGBA32,
            false);
    }

    private IEnumerator Start()
    {
        while (webCamTextureManager.WebCamTexture == null)
        {
            yield return null;
        }
        webCamTextureManager.WebCamTexture.Pause();

        // Create a reusable texture with the webcam dimensions
        reuseTexture = new Texture2D(
            webCamTextureManager.WebCamTexture.width,
            webCamTextureManager.WebCamTexture.height,
            TextureFormat.RGBA32,
            false);
    }

    public void StartCapture()
    {
        webCamTextureManager.WebCamTexture.Play();
    }

    public void PauseCapture()
    {
        webCamTextureManager.WebCamTexture.Pause();
    }

    public FrameData GetFrameData(int frameId)
    {
        FrameData frameData = new FrameData();

        // Capture the frame from the webcam
        reuseTexture.SetPixels(webCamTextureManager.WebCamTexture.GetPixels());
        reuseTexture.Apply();

        // Convert the texture to a byte array
        frameData.ImageData = ImageConversion.EncodeToJPG(reuseTexture, (int)SettingsManager.Instance.imageQuality);

        // Capture tracking data
        frameData.TrackingJson = JsonUtility.ToJson(trackingDataProvider.CaptureTrackingData(frameId));

        // Data message type
        frameData.MessageType = 1;

        return frameData;
    }

}
