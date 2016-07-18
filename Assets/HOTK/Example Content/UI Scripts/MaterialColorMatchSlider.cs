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
        var c = GetMaterialTexture().GetPixel(0,0);
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
            case ColorSetting.A:
                Slider.value = c.a;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Texture2D GetMaterialTexture()
    {
        return (Texture2D) (Material.mainTexture ?? (Material.mainTexture = TwitchChatTester.GenerateBaseTexture()));
    }

    public void OnValueChanges()
    {
        if (Material == null) return;
        var tex = GetMaterialTexture();
        var c = tex.GetPixel(0, 0);
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
            case ColorSetting.A:
                c.a = Slider.value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        tex.SetPixel(0, 0, c);
        tex.Apply();
    }

    public enum ColorSetting
    {
        R,
        G,
        B,
        A,
    }
}
