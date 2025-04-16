using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button recordButton;
    public TMP_Dropdown collectionIdDropdown;

    private bool fetched = false;
    private float timeSinceLastFetch = 0;
    private Collection[] collections;

    private void Start()
    {
        SettingsManager.Instance.OnNetworkSettingsChanged += () => { fetched = false; };
        // Set dropdown values for Image Quality
        collectionIdDropdown.ClearOptions();
        collectionIdDropdown.AddOptions(new List<string> { "Default" });
        collectionIdDropdown.value = 0;
        collectionIdDropdown.onValueChanged.AddListener(OnCollectionIdChanged);
    }
    private void Update()
    {
        timeSinceLastFetch += Time.deltaTime;
        if (!fetched && timeSinceLastFetch >= 2.0f)
        {
            timeSinceLastFetch = 0;
            Task.Run(async () =>
            {
                Collection[] collections = await APIClient.GetCollectionsAsync();
                if (collections != null && collections.Length > 0)
                {
                    this.collections = collections;
                    List<string> collectionNames = new List<string>();
                    foreach (var collection in collections)
                    {
                        collectionNames.Add(collection.name);
                    }
                    collectionIdDropdown.ClearOptions();
                    collectionIdDropdown.AddOptions(collectionNames);
                    fetched = true;
                }
            });
        }
    }

    // Called when the user changes the image quality
    public void OnCollectionIdChanged(int index)
    {
        if (collections != null && index < collections.Length)
        {
            SettingsManager.Instance.Collection = collections[index];
        }
    }

    // Called when the user changes the camera eye (Left/Right)
    private void OnCameraEyeChanged(int value)
    {
        SettingsManager.Instance.cameraEye = value == 0 ? SettingsManager.CameraEye.Left : SettingsManager.CameraEye.Right;
    }
}
