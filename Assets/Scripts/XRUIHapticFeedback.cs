using UnityEngine;
using UnityEngine.EventSystems;
// using UnityEngine.XR.Interaction.Toolkit.UI;
// using UnityEngine.XR.Interaction.Toolkit;
using System;
using Oculus.Interaction;
using Oculus.Haptics;

[System.Serializable]
public class HapticSettings
{
    public bool active;
    [Range(0f, 1f)]
    public float intensity;
    public float duration;
}

public class XRUIHapticFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public HapticSettings OnHoverEnter; 
    public HapticSettings OnHoverExit;
    public HapticSettings OnSelectEnter;
    public HapticSettings OnSelectExit;
    private BaseInputModule InputModule => EventSystem.current.currentInputModule;


    [SerializeField] private HapticClip clip1;

    private HapticClipPlayer rightClipPlayer1;

    protected virtual void Start()
    {
        rightClipPlayer1 = new HapticClipPlayer(clip1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnHoverEnter.active)
            TriggerHaptic(eventData, OnHoverEnter);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnHoverExit.active)
            TriggerHaptic(eventData, OnHoverExit);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnSelectEnter.active)
            TriggerHaptic(eventData, OnSelectEnter);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnSelectExit.active)
            TriggerHaptic(eventData, OnSelectExit);
    }

    private void TriggerHaptic(PointerEventData eventData, HapticSettings hapticSettings)
    {
        rightClipPlayer1.Play(Controller.Right);
    }


    protected virtual void OnDestroy()
    {
        rightClipPlayer1?.Dispose();
    }

    // Upon exiting the application (or when playmode is stopped) we release the haptic clip players and uninitialize (dispose) the SDK.
    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }
}