using System;
using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Transform vrCamera;  // Assign the VR Camera or XR Rig
    public float smoothSpeed = 2.0f;
    private Vector3 targetPosition;
    private float distanceFromCamera;

    private int selectedCollectionID = 0;
    public TMP_Dropdown collectionIdDropdown;

    [SerializeField] private CaptureManager captureManager;
    // References to the pages
    [SerializeField] public GameObject welcomeMenu;
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject recordMenu;
    [SerializeField] public GameObject streamMenu;
    [SerializeField] public GameObject processingMenu;
    [SerializeField] public GameObject optionsMenu;

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
        OptionsMenu = 5

    }
    void Start()
    {
        ShowMenu((int)Menu.WelcomeMenu);
        distanceFromCamera = transform.position.z;
    }

    void UpdateCanvas()
    {
        // Calculate target position in front of the camera
        Vector3 forwardDirection = vrCamera.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        // Distance from the camera calulated by distanceFromCamera
        targetPosition = vrCamera.position + forwardDirection * distanceFromCamera;

        // Smoothly move the menu to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Make the menu face the camera and flip it 180 degrees
        transform.LookAt(vrCamera.position);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);

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
        UpdateCanvas();
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
        optionsMenu.SetActive(false);

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
            case Menu.OptionsMenu:
                optionsMenu.SetActive(true);
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


    public void SelectCollectionIDChanged()
    {
        Debug.Log("Selected Dropdown: " + collectionIdDropdown.value);
        selectedCollectionID = collectionIdDropdown.value;
    }
}
