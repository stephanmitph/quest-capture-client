using System;
using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MenuController menuController; // Reference to MenuController 
    [SerializeField] private Canvas recordingOverlay; // Reference to RecordingOverlay 

    public static event Action OnRecordingStopped;

    [HideInInspector] public bool isRecording = false;
    private float recordingStartTime = 0f;
    private float recordingEndTime = 0f;
    private float maxRecordingTime = 30f;
    private int frameCount = 0;
    private float timeSinceLastFrame = 0f;

    private FrameDataProvider frameDataProvider;
    private NetworkManager networkManager;

    public void Awake()
    {
        frameDataProvider = GetComponent<FrameDataProvider>();
        networkManager = GetComponent<NetworkManager>();
        recordingOverlay.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRecording)
        {
            timeSinceLastFrame += Time.deltaTime;
            if (frameDataProvider.webCamTextureManager.WebCamTexture.didUpdateThisFrame)
            {
                Console.WriteLine("Frame Updated, current FPS: " + 1 / timeSinceLastFrame);
                timeSinceLastFrame = 0;

                FrameData frameData = frameDataProvider.GetFrameData(frameCount);
                frameCount++;
                networkManager.EnqueueFrameData(frameData);
            }

            // Stop recording if B button is pressed or after maxRecordingTime seconds
            if (OVRInput.Get(OVRInput.Button.Two) || Time.time - recordingStartTime > maxRecordingTime)
            {
                StopRecording();
            }
        }
    }

    public void StartRecording(float maxRecordingTime = 30)
    {
        frameDataProvider.StartCapture();
        // Enable environment depth if available
        if (frameDataProvider.environmentDepthManager != null)
        {
            frameDataProvider.environmentDepthManager.enabled = true;
        }

        isRecording = true;
        frameCount = 0;
        recordingStartTime = Time.time;
        this.maxRecordingTime = maxRecordingTime;

        networkManager.StartNetworkLoop();
        recordingOverlay.gameObject.SetActive(true);
    }

    public void StopRecording()
    {
        isRecording = false;
        recordingEndTime = Time.time;
        recordingOverlay.gameObject.SetActive(false);
        frameDataProvider.PauseCapture();

        if (frameDataProvider.environmentDepthManager != null)
        {
            frameDataProvider.environmentDepthManager.enabled = false;
        }

        networkManager.StopNetworkLoop();
        OnRecordingStopped.Invoke();
    }
}
