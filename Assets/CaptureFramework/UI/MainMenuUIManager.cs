using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkManager networkManager;

    [Header("UI References")]
    public Button recordButton;
    public TMP_Dropdown collectionIdDropdown;
    public TMP_Text mustConnectedText;

    private bool fetched = false;
    private float timeSinceLastFetch = 0;
    private Collection[] collections;

    private void Start()
    {
        SettingsManager.Instance.OnNetworkSettingsChanged += () => { fetched = false; };
        NetworkManager.OnNetworkStatusChanged += OnNetworkStatusChanged;
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

    public void OnNetworkStatusChanged()
    {
        // Update the record button interactability based on network connection status
        recordButton.interactable = networkManager.isConnected;

        // Show or hide the mustConnectedText based on network connection status
        if (networkManager.isConnected)
        {
            mustConnectedText.gameObject.SetActive(false);
        }
        else
        {
            mustConnectedText.gameObject.SetActive(true);
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
}
