using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class DropdownSaveLoadController : MonoBehaviour
{
    public HOTK_Overlay OverlayToSave;

    public InputField UsernameField;
    public InputField ChannelField;
    public Material BackgroundMaterial;

    public OffsetMatchSlider XSlider;
    public OffsetMatchSlider YSlider;
    public OffsetMatchSlider ZSlider;

    public RotationMatchSlider RXSlider;
    public RotationMatchSlider RYSlider;
    public RotationMatchSlider RZSlider;

    public DropdownMatchEnumOptions DeviceDropdown;
    public DropdownMatchEnumOptions PointDropdown;
    public DropdownMatchEnumOptions AnimationDropdown;

    public MaterialColorMatchSlider RSlider;
    public MaterialColorMatchSlider GSlider;
    public MaterialColorMatchSlider BSlider;

    public InputField AlphaStartField;
    public InputField AlphaEndField;
    public InputField AlphaSpeedField;
    public InputField ScaleStartField;
    public InputField ScaleEndField;
    public InputField ScaleSpeedField;

    public Button SaveButton;
    public Button LoadButton;

    public InputField SaveName;
    public Button SaveNewButton;
    public Button CancelNewButton;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

    private static string NewString = "New..";

    public void OnEnable()
    {
        if (TwitchSettingsSaver.SavedSettings.Count == 0)
        {
            TwitchSettingsSaver.Load();
        }
        ReloadOptions();
    }

    private void ReloadOptions()
    {
        Dropdown.ClearOptions();
        var strings = new List<string> { NewString };
        strings.AddRange(TwitchSettingsSaver.SavedSettings.Select(config => config.Key));

        Dropdown.AddOptions(strings);

        // If no settings loaded yet, select "New"
        if (string.IsNullOrEmpty(TwitchSettingsSaver.Current))
        {
            Dropdown.value = 0;
            OnValueChanges();
        }
        else // If settings are loaded, try and select the current settings
        {
            for (var i = 0; i < Dropdown.options.Count; i++)
            {
                if (Dropdown.options[i].text != TwitchSettingsSaver.Current) continue;
                Dropdown.value = i;
                OnValueChanges();
                break;
            }
        }
    }

    private bool SavingNew = false;

    public void OnValueChanges()
    {
        if (SavingNew)
        {
            Dropdown.interactable = false;
            SaveName.interactable = true;
            CancelNewButton.interactable = true;
            LoadButton.interactable = false;
            SaveButton.interactable = false;
        }
        else
        {
            Dropdown.interactable = true;
            SaveName.interactable = false;
            SaveNewButton.interactable = false;
            CancelNewButton.interactable = false;
            if (Dropdown.options[Dropdown.value].text == NewString)
            {
                LoadButton.interactable = false;
                SaveButton.interactable = true;
            }
            else
            {
                LoadButton.interactable = true;
                SaveButton.interactable = true;
            }
        }
    }

    public void OnLoadPressed() // Load an existing save
    {
        TwitchSettings settings;
        if (!TwitchSettingsSaver.SavedSettings.TryGetValue(Dropdown.options[Dropdown.value].text, out settings)) return;
        TwitchChatTester.Instance.AddSystemNotice("Loading saved settings " + Dropdown.options[Dropdown.value].text);
        TwitchSettingsSaver.Current = Dropdown.options[Dropdown.value].text;
        UsernameField.text = settings.Username;
        ChannelField.text = settings.Channel;

        XSlider.Slider.value = settings.X;
        YSlider.Slider.value = settings.Y;
        ZSlider.Slider.value = settings.Z;

        RXSlider.Slider.value = settings.RX;
        RYSlider.Slider.value = settings.RY;
        RZSlider.Slider.value = settings.RZ;

        DeviceDropdown.SetToOption(settings.Device.ToString());
        PointDropdown.SetToOption(settings.Point.ToString());
        AnimationDropdown.SetToOption(settings.Animation.ToString());

        RSlider.Slider.value = settings.BackgroundR;
        GSlider.Slider.value = settings.BackgroundG;
        BSlider.Slider.value = settings.BackgroundB;

        AlphaStartField.text = settings.AlphaStart.ToString();
        AlphaEndField.text = settings.AlphaEnd.ToString();
        AlphaSpeedField.text = settings.AlphaSpeed.ToString();
        ScaleStartField.text = settings.ScaleStart.ToString();
        ScaleEndField.text = settings.ScaleEnd.ToString();
        ScaleSpeedField.text = settings.ScaleSpeed.ToString();

        AlphaStartField.onEndEdit.Invoke("");
        AlphaEndField.onEndEdit.Invoke("");
        AlphaSpeedField.onEndEdit.Invoke("");
        ScaleStartField.onEndEdit.Invoke("");
        ScaleEndField.onEndEdit.Invoke("");
        ScaleSpeedField.onEndEdit.Invoke("");
    }

    public void OnSavePressed()
    {
        if (Dropdown.options[Dropdown.value].text == NewString) // Start creating a new save
        {
            SavingNew = true;
            OnValueChanges();
        }
        else // Overwrite an existing save
        {
            TwitchSettings settings;
            if (!TwitchSettingsSaver.SavedSettings.TryGetValue(Dropdown.options[Dropdown.value].text, out settings)) return;
            TwitchChatTester.Instance.AddSystemNotice("Overwriting saved settings " + Dropdown.options[Dropdown.value].text);
            settings.Username = UsernameField.text;
            settings.Channel = ChannelField.text;
            settings.X = OverlayToSave.AnchorOffset.x; settings.Y = OverlayToSave.AnchorOffset.y; settings.Z = OverlayToSave.AnchorOffset.z;
            settings.RX = OverlayToSave.transform.eulerAngles.x; settings.RY = OverlayToSave.transform.eulerAngles.y; settings.RZ = OverlayToSave.transform.eulerAngles.z;

            settings.Device = OverlayToSave.AnchorDevice;
            settings.Point = OverlayToSave.AnchorPoint;
            settings.Animation = OverlayToSave.AnimateOnGaze;

            settings.BackgroundR = BackgroundMaterial.color.r;
            settings.BackgroundG = BackgroundMaterial.color.g;
            settings.BackgroundB = BackgroundMaterial.color.b;

            settings.AlphaStart = OverlayToSave.Alpha; settings.AlphaEnd = OverlayToSave.Alpha2; settings.AlphaSpeed = OverlayToSave.AlphaSpeed;
            settings.ScaleStart = OverlayToSave.Scale; settings.ScaleEnd = OverlayToSave.Scale2; settings.ScaleSpeed = OverlayToSave.ScaleSpeed;
            TwitchSettingsSaver.Save();
        }
    }

    public void OnSaveNewPressed()
    {
        if (string.IsNullOrEmpty(SaveName.text) || TwitchSettingsSaver.SavedSettings.ContainsKey(SaveName.text)) return;
        SavingNew = false;
        TwitchChatTester.Instance.AddSystemNotice("Adding saved settings " + SaveName.text);
        TwitchSettingsSaver.SavedSettings.Add(SaveName.text, ConvertToTwitchSettings(OverlayToSave));
        TwitchSettingsSaver.Save();
        SaveName.text = "";
        ReloadOptions();
    }

    private TwitchSettings ConvertToTwitchSettings(HOTK_Overlay o) // Create a new save state
    {
        return new TwitchSettings()
        {
            Username = UsernameField.text,
            Channel = ChannelField.text,
            X = o.AnchorOffset.x, Y = o.AnchorOffset.y, Z = o.AnchorOffset.z,
            RX = o.transform.eulerAngles.x, RY = o.transform.eulerAngles.y, RZ = o.transform.eulerAngles.z,

            Device = o.AnchorDevice,
            Point = o.AnchorPoint,
            Animation = o.AnimateOnGaze,

            BackgroundR = BackgroundMaterial.color.r,
            BackgroundG = BackgroundMaterial.color.g,
            BackgroundB = BackgroundMaterial.color.b,

            AlphaStart = o.Alpha, AlphaEnd = o.Alpha2, AlphaSpeed = o.AlphaSpeed,
            ScaleStart = o.Scale, ScaleEnd = o.Scale2, ScaleSpeed = o.ScaleSpeed,
        };
    }

    public void OnCancelNewPressed()
    {
        SavingNew = false;
        SaveName.text = "";
        OnValueChanges();
    }

    public void OnSaveNameChanged()
    {
        if (string.IsNullOrEmpty(SaveName.text) || SaveName.text == NewString)
        {
            SaveNewButton.interactable = false;
        }
        else
        {
            SaveNewButton.interactable = true;
        }
    }
}
