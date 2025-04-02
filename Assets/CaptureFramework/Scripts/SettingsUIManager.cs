using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;  // Required for TMP components

public class SettingsUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField serverIPInputField;
    public TMP_InputField serverPortInputField;
    public TMP_Dropdown imageQualityDropdown;
    public TMP_Dropdown imageResolutionDropdown;
    public TMP_Dropdown cameraEyeDropdown;

    private void Start()
    {
        // Log all settings to the console
        Debug.Log($"Server IP: {SettingsManager.Instance.serverIP}");
        Debug.Log($"Server Port: {SettingsManager.Instance.serverPort}");
        Debug.Log($"Image Quality: {SettingsManager.Instance.imageQuality}");
        Debug.Log($"Image Resolution: {SettingsManager.Instance.imageResolution}"); 
        Debug.Log($"Camera Eye: {SettingsManager.Instance.cameraEye}");
        // Initialize InputFields and Dropdowns with current values from SettingsManager
        serverIPInputField.text = SettingsManager.Instance.serverIP;
        serverPortInputField.text = SettingsManager.Instance.serverPort.ToString();

        // Set dropdown values for Image Quality
        imageQualityDropdown.ClearOptions();
        imageQualityDropdown.AddOptions(new List<string> { "100", "75", "50", "25" });
        imageQualityDropdown.value = Mathf.Max(0, System.Array.IndexOf(new string[] { "_100", "_75", "_50", "_25" }, SettingsManager.Instance.imageQuality.ToString()));

        // Set dropdown values for Image Resolution
        imageResolutionDropdown.ClearOptions();
        imageResolutionDropdown.AddOptions(new List<string> { "1280x960", "800x600", "640x480", "320x240" });
        imageResolutionDropdown.value = Mathf.Max(0, System.Array.IndexOf(new string[] { "_1280x960", "_800x600", "_640x480", "_320x240" }, SettingsManager.Instance.imageResolution.ToString()));

        // Set dropdown for Camera Eye (Left, Right)
        cameraEyeDropdown.ClearOptions();
        cameraEyeDropdown.AddOptions(new List<string> { "Left", "Right" });
        cameraEyeDropdown.value = SettingsManager.Instance.cameraEye == SettingsManager.CameraEye.Left ? 0 : 1;

        // Add listeners for changes
        serverIPInputField.onEndEdit.AddListener(OnServerIPChanged);
        serverPortInputField.onEndEdit.AddListener(OnServerPortChanged);
        imageQualityDropdown.onValueChanged.AddListener(OnImageQualityChanged);
        imageResolutionDropdown.onValueChanged.AddListener(OnImageResolutionChanged);
        cameraEyeDropdown.onValueChanged.AddListener(OnCameraEyeChanged);
    }

    // Called when the user changes the server IP
    private void OnServerIPChanged(string value)
    {
        SettingsManager.Instance.serverIP = value;
    }

    // Called when the user changes the server port
    private void OnServerPortChanged(string value)
    {
        if (int.TryParse(value, out int port))
        {
            SettingsManager.Instance.serverPort = port;
        }
        else
        {
            Debug.LogWarning("Invalid server port input.");
        }
    }

    // Called when the user changes the image quality
    private void OnImageQualityChanged(int value)
    {
        // Map dropdown selection to the corresponding image quality value
        SettingsManager.Instance.imageQuality = (SettingsManager.ImageQuality)(100 - (value * 25));  // Dropdown value: 0 -> 100, 1 -> 75, etc.
    }

    // Called when the user changes the image resolution
    private void OnImageResolutionChanged(int value)
    {
        // Map dropdown selection to the corresponding enum value
        SettingsManager.ImageResolution[] resolutions = { 
            SettingsManager.ImageResolution._1280x960,
            SettingsManager.ImageResolution._800x600,
            SettingsManager.ImageResolution._640x480,
            SettingsManager.ImageResolution._320x240
        };
        SettingsManager.Instance.imageResolution = resolutions[value];
    }

    // Called when the user changes the camera eye (Left/Right)
    private void OnCameraEyeChanged(int value)
    {
        SettingsManager.Instance.cameraEye = value == 0 ? SettingsManager.CameraEye.Left : SettingsManager.CameraEye.Right;
    }
}
