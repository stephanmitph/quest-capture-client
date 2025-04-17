
using System.Collections;
using Meta.XR.EnvironmentDepth;
using PassthroughCameraSamples;
using UnityEngine;
using UnityEngine.Rendering;

public class FrameDataProvider : MonoBehaviour
{

    [SerializeField] public WebCamTextureManager webCamTextureManager;
    [SerializeField] public EnvironmentDepthManager environmentDepthManager;  // Add this field

    private Texture2D reuseTexture; // Reusable texture to avoid allocation/deallocation overhead
    private RenderTexture depthReuseTexture; // For storing depth texture
    private Texture2D depthTexture2D; // For converting RenderTexture to Texture2D

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
        // Initialize depth textures
        if (environmentDepthManager != null)
        {
            // Wait until depth is available (optional)
            while (!environmentDepthManager.IsDepthAvailable)
            {
                yield return null;
            }

            // Create RenderTexture to receive depth data
            depthReuseTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
            depthReuseTexture.Create();

            // Create Texture2D for encoding
            depthTexture2D = new Texture2D(512, 512, TextureFormat.RFloat, false);
        }
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

        // Capture depth data if available
        if (environmentDepthManager != null && environmentDepthManager.IsDepthAvailable)
        {
            // Get depth texture from shader property
            Shader.SetGlobalTexture("_EnvironmentDepthTexture", depthReuseTexture, RenderTextureSubElement.Color);

            // Copy the RenderTexture to Texture2D
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = depthReuseTexture;
            depthTexture2D.ReadPixels(new Rect(0, 0, depthReuseTexture.width, depthReuseTexture.height), 0, 0);
            depthTexture2D.Apply();
            RenderTexture.active = prevRT;

            // Encode the depth data as PNG (maintains precision better than JPG)
            // For depth data, PNG is better as it's lossless
            frameData.DepthData = ImageConversion.EncodeToPNG(depthTexture2D);
        }

        // Capture tracking data
        frameData.TrackingJson = JsonUtility.ToJson(trackingDataProvider.CaptureTrackingData(frameId));

        // Data message type
        frameData.MessageType = 1;

        return frameData;
    }

}
