using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class DropdownMatchEnumOptions : MonoBehaviour
{
    public HOTK_Overlay Overlay;

    public EnumSelection EnumOptions;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

    public void OnEnable()
    {
        Dropdown.ClearOptions();
        var strings = new List<string>();
        switch (EnumOptions)
        {
            case EnumSelection.AttachmentDevice:
                strings.AddRange(from object e in Enum.GetValues(typeof (HOTK_Overlay.AttachmentDevice)) select e.ToString());
                Dropdown.AddOptions(strings);
                Dropdown.value = strings.IndexOf(Overlay.AnchorDevice.ToString());
                break;
            case EnumSelection.AttachmentPoint:
                strings.AddRange(from object e in Enum.GetValues(typeof(HOTK_Overlay.AttachmentPoint)) select e.ToString());
                Dropdown.AddOptions(strings);
                Dropdown.value = strings.IndexOf(Overlay.AnchorPoint.ToString());
                break;
            case EnumSelection.AnimationType:
                strings.AddRange(from object e in Enum.GetValues(typeof(HOTK_Overlay.AnimationType)) select e.ToString());
                Dropdown.AddOptions(strings);
                Dropdown.value = strings.IndexOf(Overlay.AnimateOnGaze.ToString());
                break;
        }
    }

    public void SetDropdownState(string val)
    {
        switch (EnumOptions)
        {
            case EnumSelection.AttachmentDevice:
                Overlay.AnchorDevice = (HOTK_Overlay.AttachmentDevice) Enum.Parse(typeof(HOTK_Overlay.AttachmentDevice), Dropdown.options[Dropdown.value].text);
                break;
            case EnumSelection.AttachmentPoint:
                Overlay.AnchorPoint = (HOTK_Overlay.AttachmentPoint) Enum.Parse(typeof(HOTK_Overlay.AttachmentPoint), Dropdown.options[Dropdown.value].text);
                break;
            case EnumSelection.AnimationType:
                Overlay.AnimateOnGaze = (HOTK_Overlay.AnimationType) Enum.Parse(typeof(HOTK_Overlay.AnimationType), Dropdown.options[Dropdown.value].text);
                break;
        }
    }

    public void SetToOption(string text)
    {
        for (var i = 0; i < Dropdown.options.Count; i ++)
        {
            if (Dropdown.options[i].text != text) continue;
            Dropdown.value = i;
            break;
        }
    }

    public enum EnumSelection 
    {
        AttachmentDevice,
        AttachmentPoint,
        AnimationType
    }
}
