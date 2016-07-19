using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Slider))]
public class VolumeMatchSlider : MonoBehaviour
{
    public ControllerMode ControllingVolumeOf;

    public Slider Slider
    {
        get { return _slider ?? (_slider = GetComponent<Slider>()); }
    }

    private Slider _slider;

    public void OnSliderChanged()
    {
        switch (ControllingVolumeOf)
        {
            case ControllerMode.MessageSound:
                TwitchChatTester.Instance.SetMessageVolume(Slider.value);
                break;
            case ControllerMode.FollowerSound:
                TwitchChatTester.Instance.SetNewFollowerVolume(Slider.value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnSliderEndDrag(bool playSound = true)
    {
        OnSliderChanged();
        if (playSound)
            switch (ControllingVolumeOf)
            {
                case ControllerMode.MessageSound:
                    TwitchChatTester.Instance.PlayMessageSound();
                    break;
                case ControllerMode.FollowerSound:
                    TwitchChatTester.Instance.PlayNewFollowerSound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        
    }

    public enum ControllerMode
    {
        MessageSound,
        FollowerSound
    }
}