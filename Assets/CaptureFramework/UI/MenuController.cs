using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private CaptureManager captureManager;
    [SerializeField] private NetworkManager networkManager;

    [Header("Menu Pages")]
    [SerializeField] public GameObject welcomeMenu;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject recordMenu;
    [SerializeField] public GameObject streamMenu;
    [SerializeField] public GameObject processingMenu;
    [SerializeField] public GameObject settingsMenu;

    [Header("UI Elements")]
    [SerializeField] public RawImage statusImage;
    [SerializeField] public TextMeshProUGUI statusText;

    private bool isWaitingForProcessingFinished = false;

    // All available pages
    [Serializable]
    public enum Menu
    {
        WelcomeMenu = 0,
        MainMenu = 1,
        RecordMenu = 2,
        StreamMenu = 3,
        ProcessingMenu = 4,
        SettingsMenu = 5

    }
    void Awake()
    {
        CaptureManager.OnRecordingStopped += OnRecordingStopped;
        NetworkManager.OnNetworkStatusChanged += OnNetworkStatusChanged;
    }

    void Start()
    {
        ShowMenu((int)Menu.WelcomeMenu);
    }

    void Update()
    {
        // When the recording is finished and capturemanager sent all data and we are waiting for processing to finish, show Main Menu again
        // We need isWaitingForProcessingFinished bool, because otherwise we would show the Main Menu every frame 
        if (!captureManager.isRecording && !networkManager.isProcessing && isWaitingForProcessingFinished)
        {
            ShowMenu((int)Menu.MainMenu);
            isWaitingForProcessingFinished = false;
        }
    }

    public void StartRecording()
    {
        gameObject.SetActive(false);
        captureManager.StartRecording();
    }

    public void OnRecordingStopped()
    {
        isWaitingForProcessingFinished = true;
        ShowMenu((int)Menu.ProcessingMenu);
    }

    public void OnNetworkStatusChanged()
    {
        if (networkManager.isConnected)
        {
            statusImage.color = Color.green;
            statusText.text = "Connected";
        }
        else
        {
            statusImage.color = Color.red;
            statusText.text = "Disconnected";
        }
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

    public void ShowMenu(int menuId)
    {
        gameObject.SetActive(true);

        // Hide all menus
        welcomeMenu.SetActive(false);
        mainMenu.SetActive(false);
        recordMenu.SetActive(false);
        streamMenu.SetActive(false);
        processingMenu.SetActive(false);
        settingsMenu.SetActive(false);

        // Show the selected menu
        switch ((Menu)menuId)
        {
            case Menu.WelcomeMenu:
                welcomeMenu.SetActive(true);
                break;
            case Menu.MainMenu:
                mainMenu.SetActive(true);
                break;
            case Menu.RecordMenu:
                recordMenu.SetActive(true);
                break;
            case Menu.StreamMenu:
                streamMenu.SetActive(true);
                break;
            case Menu.ProcessingMenu:
                processingMenu.SetActive(true);
                break;
            case Menu.SettingsMenu:
                settingsMenu.SetActive(true);
                break;
        }
    }
}
