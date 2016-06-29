using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MaterialColorMatchSlider : MonoBehaviour
{
    public ColorSetting Setting;
    public Material Material;
    public Slider Slider
    {
        get { return _slider ?? (_slider = GetComponent<Slider>()); }
    }

    private Slider _slider;

    public void OnEnable()
    {
        if (Material == null) return;
        var c = Material.color;
        switch (Setting)
        {
            case ColorSetting.R:
                Slider.value = c.r;
                break;
            case ColorSetting.G:
                Slider.value = c.g;
                break;
            case ColorSetting.B:
                Slider.value = c.b;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnValueChanges()
    {
        if (Material == null) return;
        var c = Material.color;
        switch (Setting)
        {
            case ColorSetting.R:
                c.r = Slider.value;
                break;
            case ColorSetting.G:
                c.g = Slider.value;
                break;
            case ColorSetting.B:
                c.b = Slider.value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Material.color = c;
    }

    public enum ColorSetting
    {
        R,
        G,
        B,
    }
}
