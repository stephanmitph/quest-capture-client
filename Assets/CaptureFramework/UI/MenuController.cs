using System;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private CaptureManager captureManager;
    [Header("Menu Pages")]
    [SerializeField] public GameObject welcomeMenu;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject recordMenu;
    [SerializeField] public GameObject streamMenu;
    [SerializeField] public GameObject processingMenu;
    [SerializeField] public GameObject settingsMenu;

    private bool isRecording = false;
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
    void Start()
    {
        ShowMenu((int)Menu.WelcomeMenu);
    }

    void UpdateUI()
    {
        // When the recording is finished and capturemanager sent all data and we are waiting for processing to finish, show Main Menu again
        // We need isWaitingForProcessingFinished bool, because otherwise we would show the Main Menu every frame 
        if (!isRecording && !captureManager.isProcessing && isWaitingForProcessingFinished)
        {
            ShowMenu((int)Menu.MainMenu);
            isWaitingForProcessingFinished = false;
        } 
    }

    void Update()
    {
        UpdateUI();
    }

    public void HideMenu()
    {
        // Hide the menu
        gameObject.SetActive(false);
    }

    public void ShowMenu(int menuId)
    {
        Debug.Log("Show Menu: " + menuId);
        Debug.Log("Show Menu: " + (Menu)menuId);
        // Set Menu active
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

    public void StartRecording()
    {
        isRecording = true;
        gameObject.SetActive(false);
        captureManager.StartRecording();
    }

    public void StopRecording()
    {
        isWaitingForProcessingFinished = true;
        isRecording = false;
        ShowMenu((int)Menu.ProcessingMenu);
    }
}
