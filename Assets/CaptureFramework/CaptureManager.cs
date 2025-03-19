using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using PassthroughCameraSamples;
using UnityEngine;
using UnityEngine.UI;

public class CaptureManager : MonoBehaviour
{
    [SerializeField] private WebCamTextureManager webCamTextureManager;
    [SerializeField] private RawImage image;
    [SerializeField] public int jpegQuality = 75;
    [SerializeField] public string serverAddress = "192.168.1.2";
    [SerializeField] public float reconnectDelay = 1.0f;
    [SerializeField] public float checkStatusDelay = 5.0f;
    [SerializeField] public int port = 8080;
    [SerializeField] public int maxQueueSize = 1000;

    private string currentIpAddress = "Not Available";
    private bool isEnabled = false;
    private Thread? networkThread;
    private bool isRunning = false;
    private float timeSinceLastFrame = 0;
    private float timeSinceLastStatusCheck = 0;

    // Add this field to track when recording started
    private float recordingStartTime = 0f;
    private int framesCaptured = 0;

    // Frame queue for storing encoded images
    private Queue<byte[]> frameQueue = new Queue<byte[]>();
    private object queueLock = new object();

    // Reusable texture to avoid allocation/deallocation overhead
    private Texture2D reuseTexture;

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

        webCamTextureManager.WebCamTexture.requestedFPS = 60;
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
                reuseTexture.SetPixels32(webCamTextureManager.WebCamTexture.GetPixels32());
                reuseTexture.Apply();

                // Encode to JPEG
                byte[] encodedFrame = ImageConversion.EncodeToJPG(reuseTexture, jpegQuality);

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
                    frameQueue.Enqueue(encodedFrame);
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
                        byte[] frameData = null;

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
                                Debug.Log($"VIDEOSTREAM: Sending frame of size {frameData.Length}");

                                // Send frame size header
                                var sizeBytes = BitConverter.GetBytes(frameData.Length);
                                stream.Write(sizeBytes, 0, sizeBytes.Length);

                                // Send frame data
                                stream.Write(frameData, 0, frameData.Length);
                                stream.Flush();
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
