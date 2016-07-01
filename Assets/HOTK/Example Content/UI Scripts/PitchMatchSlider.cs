using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PitchMatchSlider : MonoBehaviour
{
    public AudioSource AudioSource;

    public Slider Slider
    {
        get { return _slider ?? (_slider = GetComponent<Slider>()); }
    }

    private Slider _slider;

    public void OnSliderChanged()
    {
        if (AudioSource == null) return;
        AudioSource.pitch = Slider.value;
    }

    public void OnSliderEndDrag(bool playSound = true)
    {
        OnSliderChanged();
        if (playSound && AudioSource != null && AudioSource.clip != null) AudioSource.Play();
    }
}