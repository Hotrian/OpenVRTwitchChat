using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public DropdownMatchFileOptions ChatSoundDropdown;
    public VolumeMatchSlider VolumeSlider;
    public PitchMatchSlider PitchSlider;
    public DropdownMatchFileOptions NewFollowerSoundDropdown;
    public VolumeMatchSlider NewFollowerVolumeSlider;
    public PitchMatchSlider NewFollowerPitchSlider;

    public DropdownMatchEnumOptions DeviceDropdown;
    public DropdownMatchEnumOptions PointDropdown;
    public DropdownMatchEnumOptions AnimationDropdown;

    public MaterialColorMatchSlider RSlider;
    public MaterialColorMatchSlider GSlider;
    public MaterialColorMatchSlider BSlider;
    public MaterialColorMatchSlider ASlider;

    public InputField AlphaStartField;
    public InputField AlphaEndField;
    public InputField AlphaSpeedField;
    public InputField ScaleStartField;
    public InputField ScaleEndField;
    public InputField ScaleSpeedField;

    public Button SaveButton;
    public Button LoadButton;
    public Button DeleteButton;
    public Text DeleteButtonText;

    public EventTrigger DeleteButtonTriggers;

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
        if (TwitchSettingsSaver.CurrentProgramSettings == null) TwitchSettingsSaver.LoadProgramSettings();
        ReloadOptions();
        if (TwitchSettingsSaver.CurrentProgramSettings != null && !string.IsNullOrEmpty(TwitchSettingsSaver.CurrentProgramSettings.LastProfile)) OnLoadPressed(true);
    }

    private void ReloadOptions()
    {
        Dropdown.ClearOptions();
        var strings = new List<string> { NewString };
        strings.AddRange(TwitchSettingsSaver.SavedProfiles.Select(config => config.Key));

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

    private bool _savingNew;

    public void OnValueChanges()
    {
        CancelConfirmingDelete();
        if (_savingNew)
        {
            Dropdown.interactable = false;
            SaveName.interactable = true;
            CancelNewButton.interactable = true;
            DeleteButton.interactable = false;
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
                DeleteButton.interactable = false;
                LoadButton.interactable = false;
                SaveButton.interactable = true;
            }
            else
            {
                DeleteButton.interactable = true;
                LoadButton.interactable = true;
                SaveButton.interactable = true;
            }
        }
    }

    public void OnLoadPressed(bool startup = false) // Loads an existing save
    {
        CancelConfirmingDelete();
        TwitchSettings settings;
        if (!TwitchSettingsSaver.SavedProfiles.TryGetValue(Dropdown.options[Dropdown.value].text, out settings)) return;
        TwitchChatTester.Instance.AddSystemNotice(startup ? "Loading last used settings " + Dropdown.options[Dropdown.value].text : "Loading saved settings " + Dropdown.options[Dropdown.value].text);
        TwitchSettingsSaver.Current = Dropdown.options[Dropdown.value].text;
        if (!startup) TwitchSettingsSaver.SaveProgramSettings();
        if (!TwitchChatTester.Instance.Connected) UsernameField.text = settings.Username;
        if (!TwitchChatTester.Instance.Connected) ChannelField.text = settings.Channel;

        XSlider.Slider.value = settings.X;
        YSlider.Slider.value = settings.Y;
        ZSlider.Slider.value = settings.Z;

        RXSlider.Slider.value = settings.RX;
        RYSlider.Slider.value = settings.RY;
        RZSlider.Slider.value = settings.RZ;

        if (settings.SaveFileVersion >= 1) // Save File compatability
        {
            VolumeSlider.Slider.value = settings.Volume;
            VolumeSlider.OnSliderEndDrag(false);
            PitchSlider.Slider.value = settings.Pitch;
            PitchSlider.OnSliderEndDrag(false);
        }
        ChatSoundDropdown.SetToOption(settings.SaveFileVersion >= 1 ? (settings.SaveFileVersion >= 2 ? settings.ChatSound : settings.ChatSound + ".wav") : "gui-sound-effects-031.wav", !startup); // Save File compatability

        if (settings.SaveFileVersion >= 4) // Save File compatability
        {
            NewFollowerVolumeSlider.Slider.value = settings.FollowerVolume;
            NewFollowerVolumeSlider.OnSliderEndDrag(false);
            NewFollowerPitchSlider.Slider.value = settings.FollowerPitch;
            NewFollowerPitchSlider.OnSliderEndDrag(false);
        }
        NewFollowerSoundDropdown.SetToOption(settings.SaveFileVersion >= 4 ? settings.FollowerSound : "gui-sound-effects-038.wav"); // Save File compatability

        DeviceDropdown.SetToOption(settings.Device.ToString());
        PointDropdown.SetToOption(settings.Point.ToString());
        AnimationDropdown.SetToOption(settings.Animation.ToString());

        RSlider.Slider.value = settings.BackgroundR;
        GSlider.Slider.value = settings.BackgroundG;
        BSlider.Slider.value = settings.BackgroundB;
        ASlider.Slider.value = (settings.SaveFileVersion >= 3 ? settings.BackgroundA : 1.0f); // Save File compatability

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

    private bool _confirmingDelete;
    private string _deleteTextDefault = "Delete the selected profile.";
    private string _deleteTextConfirm = "Really Delete?";

    public void OnDeleteButtonTooltip(bool forced = false)
    {
        if (_confirmingDelete)
        {
            if (forced || TooltipController.Instance.GetTooltipText() == _deleteTextDefault)
                TooltipController.Instance.SetTooltipText(_deleteTextConfirm);
        }
        else
        {
            if (forced || TooltipController.Instance.GetTooltipText() == _deleteTextConfirm)
                TooltipController.Instance.SetTooltipText(_deleteTextDefault);
        }
    }

    public void CancelConfirmingDelete()
    {
        _confirmingDelete = false;
        DeleteButtonText.color = new Color(0.196f, 0.196f, 0.196f, 1f);
        OnDeleteButtonTooltip();
    }

    public void OnDeletePressed()
    {
        if (!_confirmingDelete)
        {
            _confirmingDelete = true;
            DeleteButtonText.color = Color.red;
            OnDeleteButtonTooltip();
        }
        else
        {
            TwitchSettingsSaver.DeleteProfile(Dropdown.options[Dropdown.value].text);
            CancelConfirmingDelete();
            ReloadOptions();
        }
    }

    /// <summary>
    /// Overwrite an existing save, or save a new one
    /// </summary>
    public void OnSavePressed()
    {
        CancelConfirmingDelete();
        if (Dropdown.options[Dropdown.value].text == NewString) // Start creating a new save
        {
            _savingNew = true;
            OnValueChanges();
        }
        else // Overwrite an existing save
        {
            TwitchSettings settings;
            if (!TwitchSettingsSaver.SavedProfiles.TryGetValue(Dropdown.options[Dropdown.value].text, out settings)) return;
            TwitchChatTester.Instance.AddSystemNotice("Overwriting saved settings " + Dropdown.options[Dropdown.value].text);
            settings.SaveFileVersion = TwitchSettings.CurrentSaveVersion;

            settings.Username = UsernameField.text;
            settings.Channel = ChannelField.text;
            settings.X = OverlayToSave.AnchorOffset.x; settings.Y = OverlayToSave.AnchorOffset.y; settings.Z = OverlayToSave.AnchorOffset.z;
            settings.RX = OverlayToSave.transform.eulerAngles.x; settings.RY = OverlayToSave.transform.eulerAngles.y; settings.RZ = OverlayToSave.transform.eulerAngles.z;

            settings.ChatSound = ChatSoundDropdown.Dropdown.options[ChatSoundDropdown.Dropdown.value].text;
            settings.Volume = VolumeSlider.Slider.value;
            settings.Pitch = PitchSlider.Slider.value;

            settings.FollowerSound = NewFollowerSoundDropdown.Dropdown.options[NewFollowerSoundDropdown.Dropdown.value].text;
            settings.FollowerVolume = NewFollowerVolumeSlider.Slider.value;
            settings.FollowerPitch = NewFollowerPitchSlider.Slider.value;

            settings.Device = OverlayToSave.AnchorDevice;
            settings.Point = OverlayToSave.AnchorPoint;
            settings.Animation = OverlayToSave.AnimateOnGaze;

            var backgroundColor = GetMaterialTexture().GetPixel(0, 0);
            settings.BackgroundR = backgroundColor.r;
            settings.BackgroundG = backgroundColor.g;
            settings.BackgroundB = backgroundColor.b;
            settings.BackgroundA = backgroundColor.a;

            settings.AlphaStart = OverlayToSave.Alpha; settings.AlphaEnd = OverlayToSave.Alpha2; settings.AlphaSpeed = OverlayToSave.AlphaSpeed;
            settings.ScaleStart = OverlayToSave.Scale; settings.ScaleEnd = OverlayToSave.Scale2; settings.ScaleSpeed = OverlayToSave.ScaleSpeed;
            TwitchSettingsSaver.SaveProfiles();
        }
    }

    private Texture2D GetMaterialTexture()
    {
        return (Texture2D)(BackgroundMaterial.mainTexture ?? (BackgroundMaterial.mainTexture = TwitchChatTester.GenerateBaseTexture()));
    }

    public void OnSaveNewPressed()
    {
        if (string.IsNullOrEmpty(SaveName.text) || TwitchSettingsSaver.SavedProfiles.ContainsKey(SaveName.text)) return;
        _savingNew = false;
        TwitchChatTester.Instance.AddSystemNotice("Adding saved settings " + SaveName.text);
        TwitchSettingsSaver.SavedProfiles.Add(SaveName.text, ConvertToTwitchSettings(OverlayToSave));
        TwitchSettingsSaver.SaveProfiles();
        SaveName.text = "";
        ReloadOptions();
    }

    /// <summary>
    /// Create a new Save
    /// </summary>
    private TwitchSettings ConvertToTwitchSettings(HOTK_Overlay o) // Create a new save state
    {
        var backgroundColor = GetMaterialTexture().GetPixel(0, 0);
        return new TwitchSettings()
        {
            SaveFileVersion = TwitchSettings.CurrentSaveVersion,

            Username = UsernameField.text,
            Channel = ChannelField.text,
            X = o.AnchorOffset.x,
            Y = o.AnchorOffset.y,
            Z = o.AnchorOffset.z,
            RX = o.transform.eulerAngles.x,
            RY = o.transform.eulerAngles.y,
            RZ = o.transform.eulerAngles.z,

            ChatSound = ChatSoundDropdown.Dropdown.options[ChatSoundDropdown.Dropdown.value].text,
            Volume = VolumeSlider.Slider.value,
            Pitch = PitchSlider.Slider.value,

            FollowerSound = NewFollowerSoundDropdown.Dropdown.options[NewFollowerSoundDropdown.Dropdown.value].text,
            FollowerVolume = NewFollowerVolumeSlider.Slider.value,
            FollowerPitch = NewFollowerPitchSlider.Slider.value,

            Device = o.AnchorDevice,
            Point = o.AnchorPoint,
            Animation = o.AnimateOnGaze,

            BackgroundR = backgroundColor.r,
            BackgroundG = backgroundColor.g,
            BackgroundB = backgroundColor.b,
            BackgroundA = backgroundColor.a,

            AlphaStart = o.Alpha,
            AlphaEnd = o.Alpha2,
            AlphaSpeed = o.AlphaSpeed,
            ScaleStart = o.Scale,
            ScaleEnd = o.Scale2,
            ScaleSpeed = o.ScaleSpeed,
        };
    }

    public void OnCancelNewPressed()
    {
        _savingNew = false;
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
