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

    [Header("Hand Tracking")]
    [SerializeField] private OVRHand leftOVRHand;  // Reference to left OVRHand
    [SerializeField] private OVRHand rightOVRHand; // Reference to right OVRHand
    [SerializeField] private OVRSkeleton leftOVRSkeleton;  // Reference to left OVRSkeleton
    [SerializeField] private OVRSkeleton rightOVRSkeleton; // Reference to right OVRSkeleton


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
    private float recordingEndTime = 0f;
    private int framesCaptured = 0;
    private Queue<FrameData> frameQueue = new Queue<FrameData>(); // Frame queue for storing encoded images
    private object queueLock = new object();
    private Texture2D reuseTexture; // Reusable texture to avoid allocation/deallocation overhead

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

                // Increment frames captured counter
                framesCaptured++;

                // Add frame to queue
                lock (queueLock)
                {
                    // If queue is too large, drop oldest frame
                    while (frameQueue.Count >= maxQueueSize)
                    {
                        frameQueue.Dequeue();
                        Debug.LogWarning("VIDEOSTREAM: Frame queue full, dropping oldest frame");
                    }

                    frameQueue.Enqueue(frameData);
                    Debug.Log($"VIDEOSTREAM: Queued new frame. Queue size: {frameQueue.Count}");
                }

            }
        }

        // Toggle streaming with button press
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            recordingStartTime = Time.time;
            framesCaptured = 0;
            isEnabled = true;
            Debug.Log($"VIDEOSTREAM: Streaming is now {isEnabled}");
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            if (isEnabled)
            {
                recordingEndTime = Time.time;
            }

            isEnabled = false;
            Debug.Log($"VIDEOSTREAM: Streaming is now {isEnabled}");
        }
    }

    private TrackingData CaptureTrackingData()
    {
        TrackingData data = new TrackingData();

        // Set frame number
        data.frame = framesCaptured;

        // Set timestamp
        data.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Get head pose
        Pose headPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
        data.headPosition = new TrackingData.Vector3Serializable(headPose.position);
        data.headRotation = new TrackingData.QuaternionSerializable(headPose.rotation);

        // Capture left hand data with skeletal tracking
        data.leftHand = CaptureHandData(leftOVRHand, leftOVRSkeleton, OVRInput.Controller.LHand);

        // Capture right hand data with skeletal tracking
        data.rightHand = CaptureHandData(rightOVRHand, rightOVRSkeleton, OVRInput.Controller.RHand);

        return data;
    }

    private TrackingData.HandData CaptureHandData(OVRHand ovrHand, OVRSkeleton ovrSkeleton, OVRInput.Controller controller)
    {
        TrackingData.HandData handData = new TrackingData.HandData();

        // Basic tracking data
        handData.isTracked = OVRInput.GetControllerPositionTracked(controller);
        handData.wristPosition = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerPosition(controller));
        handData.wristRotation = new TrackingData.QuaternionSerializable(OVRInput.GetLocalControllerRotation(controller));
        Debug.Log($"Wrist Position: {handData.wristPosition.x}, {handData.wristPosition.y}, {handData.wristPosition.z}");

        // Check if we have access to skeletal data
        if (ovrHand != null && ovrSkeleton != null && ovrHand.IsTracked && ovrSkeleton.IsInitialized && ovrSkeleton.Bones.Count > 0)
        {
            handData.hasSkeletalData = true;

            // Get bone data
            int boneCount = ovrSkeleton.Bones.Count;
            Debug.Log($"VIDEOSTREAM: Capturing {boneCount} bones for {controller}");

            handData.bones = new TrackingData.HandData.BoneData[boneCount];
            handData.fingerPinchStates = new bool[5];
            handData.fingerPinchStrengths = new float[5];
            handData.fingerPinchConfidence = new float[5];

            for (int i = 0; i < boneCount; i++)
            {
                OVRBone bone = ovrSkeleton.Bones[i];
                Vector3 position = bone.Transform.localPosition;
                Quaternion rotation = bone.Transform.localRotation;

                // Create bone data
                handData.bones[i] = new TrackingData.HandData.BoneData
                {
                    id = i,
                    position = new TrackingData.Vector3Serializable(position),
                    rotation = new TrackingData.QuaternionSerializable(rotation)
                };
            }
            for (int i = 0; i < 5; i++)
            {
                handData.fingerPinchStates[i] = ovrHand.GetFingerIsPinching((OVRHand.HandFinger)i);
                handData.fingerPinchStrengths[i] = ovrHand.GetFingerPinchStrength((OVRHand.HandFinger)i);
                handData.fingerPinchConfidence[i] = (float)ovrHand.GetFingerConfidence((OVRHand.HandFinger)i);
            }
        }
        else
        {
            handData.hasSkeletalData = false;
            handData.bones = new TrackingData.HandData.BoneData[0];
            handData.fingerPinchStrengths = new float[0];
            handData.fingerPinchConfidence = new float[0];
        }

        return handData;
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
                                // Send message type (1 = frame data)
                                byte[] messageType = new byte[] { 1 };
                                stream.Write(messageType, 0, messageType.Length);

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
