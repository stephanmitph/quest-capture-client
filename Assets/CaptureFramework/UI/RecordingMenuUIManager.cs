using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RecordingMenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text promptText;
    public TMP_Text durationText;

    void Start()
    {
        SettingsManager.Instance.OnCollectionChanged += () => {
            if (SettingsManager.Instance.Collection != null)
            {
                promptText.text = SettingsManager.Instance.Collection.promptText;
                durationText.text = $"{SettingsManager.Instance.Collection.duration}s"; 
            }
        };
    }

}
