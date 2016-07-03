using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Slider))]
public class VolumeMatchSlider : MonoBehaviour
{
    public Slider Slider
    {
        get { return _slider ?? (_slider = GetComponent<Slider>()); }
    }

    private Slider _slider;

    public void OnSliderChanged()
    {
        TwitchChatTester.Instance.SetMessageVolume(Slider.value);
    }

    public void OnSliderEndDrag(bool playSound = true)
    {
        OnSliderChanged();
        if (playSound) TwitchChatTester.Instance.PlayMessageSound();
    }
}