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

    // All available pages
    [Serializable]
    public enum Menu {
        WelcomeMenu = 0,
        MainMenu = 1,
        RecordMenu = 2,
        StreamMenu = 3,
        ProcessingMenu = 4

    }
    void Start()
    {
        gameObject.SetActive(true);
        // Hide all menus at start
        welcomeMenu.SetActive(true);
        mainMenu.SetActive(false);
        recordMenu.SetActive(false);
        streamMenu.SetActive(false);
        processingMenu.SetActive(false);

    }
    void Awake()
    {
        distanceFromCamera = transform.position.z;
    }

    void Update()
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

    public void HideMenu()
    {
        // Hide the menu
        gameObject.SetActive(false);
    }

    public void ShowMenu(int menuId) {
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

        // Show the selected menu
        switch ((Menu)menuId) {
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
        }
    }

    public void StartRecording()
    {
        // Hide the menu
        gameObject.SetActive(false);
        // Show the recording menu
        captureManager.StartRecording();
    }

    public void StopRecording() {
        ShowMenu((int)Menu.MainMenu);
    }


    public void SelectCollectionIDChanged()
    {
        Debug.Log("Selected Dropdown: " + collectionIdDropdown.value);
        selectedCollectionID = collectionIdDropdown.value;
    }
}
