using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // Events for settings changes
    public event Action OnNetworkSettingsChanged;
    public event Action OnGraphicsSettingsChanged;
    public event Action OnCameraSettingsChanged;

    // Default settings
    [Header("Network Settings")]
    [SerializeField] private string _serverIP = "172.20.10.3";
    [SerializeField] private int _serverPort = 8080;

    [Header("Graphics Settings")]
    [SerializeField] private ImageQuality _imageQuality = ImageQuality._75;
    [SerializeField] private ImageResolution _imageResolution = ImageResolution._800x600;
    [SerializeField] private CameraEye _cameraEye = CameraEye.Left;

    // Properties with event triggers
    public string serverIP
    {
        get => _serverIP;
        set
        {
            if (_serverIP != value)
            {
                _serverIP = value;
                OnNetworkSettingsChanged?.Invoke();
            }
        }
    }

    public int serverPort
    {
        get => _serverPort;
        set
        {
            if (_serverPort != value)
            {
                _serverPort = value;
                OnNetworkSettingsChanged?.Invoke();
            }
        }
    }

    public ImageQuality imageQuality
    {
        get => _imageQuality;
        set
        {
            if (_imageQuality != value)
            {
                _imageQuality = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
    }

    public ImageResolution imageResolution
    {
        get => _imageResolution;
        set
        {
            if (_imageResolution != value)
            {
                _imageResolution = value;
                OnGraphicsSettingsChanged?.Invoke();
                OnCameraSettingsChanged?.Invoke();
            }
        }
    }

    public CameraEye cameraEye
    {
        get => _cameraEye;
        set
        {
            if (_cameraEye != value)
            {
                _cameraEye = value;
                OnCameraSettingsChanged?.Invoke();
            }
        }
    }

    public enum ImageQuality { _100 = 100, _75 = 75, _50 = 50, _25 = 25 }
    public enum CameraEye { Left, Right }
    public enum ImageResolution { _1280x960, _800x600, _640x480, _320x240 }

    public Vector2Int GetImageResolution()
    {
        switch (imageResolution)
        {
            case ImageResolution._1280x960:
                return new Vector2Int(1280, 960);
            case ImageResolution._800x600:
                return new Vector2Int(800, 600);
            case ImageResolution._640x480:
                return new Vector2Int(640, 480);
            case ImageResolution._320x240:
                return new Vector2Int(320, 240);
            default:
                return new Vector2Int(800, 600);
        }
    }

    void Start()
    {
        LoadSettings();
        Debug.Log("Settings Loaded: " + serverIP + ":" + serverPort);
    }
    public void LoadSettings()
    {
        Instance.serverIP = PlayerPrefs.GetString("ServerIP", "192.168.1.1");
        Instance.serverPort = PlayerPrefs.GetInt("ServerPort", 8080);
        Instance.imageQuality = (ImageQuality)PlayerPrefs.GetInt("ImageQuality", (int)ImageQuality._75);
        Instance.imageResolution = (ImageResolution)PlayerPrefs.GetInt("ImageResolution", (int)ImageResolution._800x600);
        Instance.cameraEye = (CameraEye)PlayerPrefs.GetInt("CameraEye", (int)CameraEye.Left);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetString("ServerIP", Instance.serverIP);
        PlayerPrefs.SetInt("ServerPort", Instance.serverPort);
        PlayerPrefs.SetInt("ImageQuality", (int)Instance.imageQuality);
        PlayerPrefs.SetInt("ImageResolution", (int)Instance.imageResolution);
        PlayerPrefs.SetInt("CameraEye", (int)Instance.cameraEye);
        PlayerPrefs.Save();
    }

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
