using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] public float reconnectDelay = 1.0f;
    [SerializeField] public float checkServerStatusDelay = 5.0f;
    [SerializeField] public int maxSendQueueSize = 5000;

    private string serverAddress;
    private int port;
    private Thread networkThread;
    private Thread serverStatusThread;
    private bool isNetworkLoopRunning = false;
    private Queue<FrameData> frameQueue = new Queue<FrameData>();
    private object queueLock = new object();
    private float timeSinceLastStatusCheck = 0;

    [HideInInspector] public bool isProcessing = false;
    [HideInInspector] public bool isConnected = false;

    public static event Action OnNetworkStatusChanged;

    public void StartNetworkLoop()
    {
        serverAddress = SettingsManager.Instance.serverIP;
        port = SettingsManager.Instance.serverPort;
        isNetworkLoopRunning = true;

        frameQueue.Clear();
        EnqueueFrameData(new FrameData(0));
        networkThread = new Thread(NetworkLoop);
        networkThread.Start();
    }

    public void StopNetworkLoop()
    {
        EnqueueFrameData(new FrameData(2));
        isNetworkLoopRunning = false;
        networkThread?.Join(1000);
    }

    public void EnqueueFrameData(FrameData frameData)
    {
        lock (queueLock)
        {
            frameQueue.Enqueue(frameData);
        }
    }

    void Update()
    {
        timeSinceLastStatusCheck += Time.deltaTime;
        if (timeSinceLastStatusCheck >= checkServerStatusDelay)
        {
            timeSinceLastStatusCheck = 0;
            CheckServerStatus();
        }
    }

    private void CheckServerStatus()
    {
        try
        {
            using (TcpClient client = new TcpClient(SettingsManager.Instance.serverIP, SettingsManager.Instance.serverPort))
            {
                isConnected = true;
                OnNetworkStatusChanged.Invoke();
            }
        }
        catch
        {
            isConnected = false;
            OnNetworkStatusChanged.Invoke();
        }
    }
    private void NetworkLoop()
    {
        while (isNetworkLoopRunning)
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(serverAddress, port);

                using (NetworkStream stream = client.GetStream())
                {
                    while (client.Connected && (isNetworkLoopRunning || isProcessing))
                    {
                        FrameData frameData = null;

                        lock (queueLock)
                        {
                            if (frameQueue.Count > 0)
                            {
                                frameData = frameQueue.Dequeue();
                            }
                        }

                        if (frameData != null)
                        {
                            isProcessing = true;
                            try
                            {
                                SendData(stream, frameData);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Network error: {ex.Message}");
                                break;
                            }
                        }
                        else
                        {
                            isProcessing = false;
                            Thread.Sleep(5);
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection error: {ex.Message}");
            }
            finally
            {
                if (client.Connected)
                    client.Close();

                Debug.Log($"Disconnected from server. Reconnecting in {reconnectDelay} seconds...");
                Thread.Sleep((int)(reconnectDelay * 1000));
            }
        }
    }

    private void SendData(NetworkStream stream, FrameData frameData)
    {
        Debug.Log("SENDING MESSAGE WITH TYPE: " + frameData.MessageType);
        // Send message type (0 = Begin, 1 = Frame data, 2 = End)
        byte[] messageType = new byte[] { frameData.MessageType };
        stream.Write(messageType, 0, messageType.Length);

        if (frameData.MessageType == 0)
        {
            // Send collection ID
            byte[] collectionIdBytes = BitConverter.GetBytes(frameData.CollectionId);
            stream.Write(collectionIdBytes, 0, collectionIdBytes.Length);
        }

        // Send tracking data length
        if (frameData.MessageType == 1)
        {
            byte[] trackingBytes = Encoding.UTF8.GetBytes(frameData.TrackingJson);
            byte[] trackingLengthBytes = BitConverter.GetBytes(trackingBytes.Length);
            stream.Write(trackingLengthBytes, 0, trackingLengthBytes.Length);

            // Send tracking data
            stream.Write(trackingBytes, 0, trackingBytes.Length);

            // Send image data length
            byte[] imageLengthBytes = BitConverter.GetBytes(frameData.ImageData.Length);
            stream.Write(imageLengthBytes, 0, imageLengthBytes.Length);

            // Send image data
            stream.Write(frameData.ImageData, 0, frameData.ImageData.Length);
        }

        stream.Flush();
    }
}