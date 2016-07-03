using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PitchMatchSlider : MonoBehaviour
{
    public Slider Slider
    {
        get { return _slider ?? (_slider = GetComponent<Slider>()); }
    }

    private Slider _slider;

    public void OnSliderChanged()
    {
        TwitchChatTester.Instance.SetMessagePitch(Slider.value);
    }

    public void OnSliderEndDrag(bool playSound = true)
    {
        OnSliderChanged();
        if (playSound) TwitchChatTester.Instance.PlayMessageSound();
    }
}