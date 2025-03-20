using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using PassthroughCameraSamples;
using UnityEngine;
using UnityEngine.UI;

public class CaptureManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private WebCamTextureManager webCamTextureManager;
    [SerializeField] public int jpegQuality = 75;

    [Header("Network Settings")]
    [SerializeField] public string serverAddress = "192.168.1.2";
    [SerializeField] public int port = 8080;
    [SerializeField] public float reconnectDelay = 1.0f;
    [SerializeField] public float checkStatusDelay = 5.0f;
    [SerializeField] public int maxQueueSize = 1000;

    [Header("References")]
    [SerializeField] private OVRCameraRig cameraRig; // Reference to OVRCameraRig

    // Private fields
    private string currentIpAddress = "Not Available";
    private bool isEnabled = false;
    private Thread? networkThread;
    private bool isRunning = false;
    private float timeSinceLastFrame = 0;
    private float timeSinceLastStatusCheck = 0;

    // Recoding fields
    // Track when recording started
    private float recordingStartTime = 0f;
    private int framesCaptured = 0;
    private Queue<FrameData> frameQueue = new Queue<FrameData>(); // Frame queue for storing encoded images
    private object queueLock = new object();
    private Texture2D reuseTexture; // Reusable texture to avoid allocation/deallocation overhead

    public class FrameData
    {
        public byte[] imageData;
        public string trackingJson;
    }

    private IEnumerator Start()
    {
        while (webCamTextureManager.WebCamTexture == null)
        {
            yield return null;
        }

        // Create a reusable texture with the webcam dimensions
        reuseTexture = new Texture2D(
            webCamTextureManager.WebCamTexture.width,
            webCamTextureManager.WebCamTexture.height,
            TextureFormat.RGBA32,
            false);

        // Start the network thread
        isRunning = true;
        networkThread = new Thread(NetworkLoop);
        networkThread.Start();
    }

    void Update()
    {
        timeSinceLastFrame += Time.deltaTime;

        // Process new frame if available
        if (webCamTextureManager.WebCamTexture.didUpdateThisFrame)
        {
            Console.WriteLine("Frame Updated, current FPS: " + 1 / timeSinceLastFrame);
            timeSinceLastFrame = 0;
            // Log queue size
            Debug.Log($"VIDEOSTREAM: Queue size: {frameQueue.Count}");

            // Use GetPixels32 for better performance
            if (isEnabled)
            {
                // Capture image
                reuseTexture.SetPixels32(webCamTextureManager.WebCamTexture.GetPixels32());
                reuseTexture.Apply();
                byte[] encodedFrame = ImageConversion.EncodeToJPG(reuseTexture, jpegQuality);

                // Capture tracking data
                TrackingData trackingData = CaptureTrackingData();
                string trackingJson = JsonUtility.ToJson(trackingData);

                // Create frame data object with both image and tracking data
                FrameData frameData = new FrameData
                {
                    imageData = encodedFrame,
                    trackingJson = trackingJson
                };

                // Add frame to queue
                lock (queueLock)
                {
                    // If queue is too large, drop oldest frame
                    while (frameQueue.Count >= maxQueueSize)
                    {
                        frameQueue.Dequeue();
                        Debug.LogWarning("VIDEOSTREAM: Frame queue full, dropping oldest frame");
                    }

                    // Increment frames captured counter
                    framesCaptured++;
                    frameQueue.Enqueue(frameData);
                    Debug.Log($"VIDEOSTREAM: Queued new frame. Queue size: {frameQueue.Count}");
                }

            }
        }

        // Toggle streaming with button press
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            isEnabled = true;
            Debug.Log($"VIDEOSTREAM: Streaming is now {isEnabled}");

            recordingStartTime = Time.time;
            framesCaptured = 0;
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {

            if (isEnabled)
            {
                float recordingDuration = Time.time - recordingStartTime;
                Debug.Log($"VIDEOSTREAM: Recording stopped. Duration: {recordingDuration:F2} seconds, Frames captured: {framesCaptured}, Average FPS: {(framesCaptured / recordingDuration):F2}");
            }

            isEnabled = false;
            Debug.Log($"VIDEOSTREAM: Streaming is now {isEnabled}");
        }
    }

    private TrackingData CaptureTrackingData()
    {
        TrackingData data = new TrackingData();

        // Set timestamp
        data.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Get head pose
        Pose headPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
        data.headPosition = new TrackingData.Vector3Serializable(headPose.position);
        data.headRotation = new TrackingData.QuaternionSerializable(headPose.rotation);

        // Other appraoch
        // Transform centerEye = cameraRig.centerEyeAnchor;
        // data.headPosition = new TrackingData.Vector3Serializable(centerEye.position);
        // data.headRotation = new TrackingData.QuaternionSerializable(centerEye.rotation);

        // Get hand data from OVRInput (simplified - doesn't require OVRHand components)
        
        data.leftHand = new TrackingData.HandData
        {
            isTracked = OVRInput.GetControllerPositionTracked(OVRInput.Controller.LHand),
            position = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand)),
            rotation = new TrackingData.QuaternionSerializable(OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand)),
            pinchStrength = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LHand)
        };

        data.rightHand = new TrackingData.HandData
        {
            isTracked = OVRInput.GetControllerPositionTracked(OVRInput.Controller.RHand),
            position = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand)),
            rotation = new TrackingData.QuaternionSerializable(OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand)),
            pinchStrength = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RHand)
        };

        return data;
    }

    private void NetworkLoop()
    {
        while (isRunning)
        {
            TcpClient client = new TcpClient();

            try
            {
                Debug.Log($"VIDEOSTREAM: Attempting to connect to server at {serverAddress}:{port}");
                client.Connect(serverAddress, port);
                Debug.Log("VIDEOSTREAM: Connected to server successfully");

                using (NetworkStream stream = client.GetStream())
                {
                    while (isRunning && client.Connected)
                    {
                        FrameData frameData = null;

                        // Try to get the oldest frame from queue
                        lock (queueLock)
                        {
                            if (frameQueue.Count > 0)
                            {
                                frameData = frameQueue.Dequeue();
                                Debug.Log($"VIDEOSTREAM: Dequeued frame. Queue size: {frameQueue.Count}");
                            }
                        }

                        if (isEnabled && frameData != null || (!isEnabled && frameData != null && frameQueue.Count > 0))
                        {
                            try
                            {
                                Debug.Log($"VIDEOSTREAM: Sending frame of size {frameData.imageData.Length} bytes");

                                // Send tracking data length
                                byte[] trackingBytes = Encoding.UTF8.GetBytes(frameData.trackingJson);
                                byte[] trackingLengthBytes = BitConverter.GetBytes(trackingBytes.Length);
                                stream.Write(trackingLengthBytes, 0, trackingLengthBytes.Length);

                                // Send tracking data
                                stream.Write(trackingBytes, 0, trackingBytes.Length);

                                // Send image data length
                                byte[] imageLengthBytes = BitConverter.GetBytes(frameData.imageData.Length);
                                stream.Write(imageLengthBytes, 0, imageLengthBytes.Length);

                                // Send image data
                                stream.Write(frameData.imageData, 0, frameData.imageData.Length);
                                stream.Flush();

                                Debug.Log($"VIDEOSTREAM: Sent frame with {frameData.imageData.Length} bytes of image and {trackingBytes.Length} bytes of tracking data");

                                // // Send frame size header
                                // var sizeBytes = BitConverter.GetBytes(frameData.Length);
                                // stream.Write(sizeBytes, 0, sizeBytes.Length);

                                // // Send frame data
                                // stream.Write(frameData, 0, frameData.Length);
                                // stream.Flush();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"VIDEOSTREAM: Error sending data: {ex.Message}");
                                break;
                            }
                        }
                        else
                        {
                            // Small delay if no frames to send
                            Thread.Sleep(5);
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"VIDEOSTREAM: Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"VIDEOSTREAM: Connection error: {ex.Message}");
            }
            finally
            {
                if (client.Connected)
                    client.Close();

                Debug.Log($"VIDEOSTREAM: Disconnected from server. Reconnecting in {reconnectDelay} seconds...");
                Thread.Sleep((int)(reconnectDelay * 1000));
            }
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        networkThread?.Join(1000);

        // Clean up reusable texture
        if (reuseTexture != null)
            Destroy(reuseTexture);
    }
}
