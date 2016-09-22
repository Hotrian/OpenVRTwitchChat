using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

[RequireComponent(typeof(Dropdown))]
public class DropdownMatchFileOptions : MonoBehaviour
{
    public ControllerMode ControllingVolumeOf;
    public string RelativeFolderToReadFrom;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

    public void OnEnable()
    {
        _ignoringSounds = true;
        Dropdown.ClearOptions();
        var info = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, RelativeFolderToReadFrom));
        var fileInfo = info.GetFiles();
        var strings = (from file in fileInfo where file.Name.EndsWith(".wav") || file.Name.EndsWith(".ogg") select file.Name).ToList();

        Dropdown.AddOptions(strings);
        for (var i = 0; i < Dropdown.options.Count; i++)
        {
            if (Dropdown.options[i].text != "gui-sound-effects-004.wav") continue;
            Dropdown.value = i;
            if (i == 0) SetDropdownStateInternal("", false);
            break;
        }
        _ignoringSounds = false;
    }

    public void SetDropdownState(string val)
    {
        SetDropdownStateInternal(val, !_ignoringSounds);
    }

    private bool _ignoringSounds = true;

    public void SetDropdownStateInternal(string val, bool forceSound)
    {
        StartCoroutine(LoadSound("file:///" + Application.streamingAssetsPath + "/" + RelativeFolderToReadFrom + "/" + Dropdown.options[Dropdown.value].text, forceSound));
    }

    private IEnumerator LoadSound(string filePath, bool forceSound)
    {
        var www = new WWW(filePath);
        yield return www;

        if (www.error != null)
            Debug.Log(www.error);

        var clip = www.GetAudioClip(false, false);

        if (clip != null)
        {
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                switch (ControllingVolumeOf)
                {
                    case ControllerMode.MessageSound:
                        TwitchChatTester.Instance.SetMessageSound(clip);
                        break;
                    case ControllerMode.FollowerSound:
                        TwitchChatTester.Instance.SetNewFollowerSound(clip);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (forceSound && !_ignoringSounds) playSound();
            }
            else Debug.LogWarning("Couldn't load " + filePath);
        }
        else Debug.LogWarning("Couldn't find " + filePath);
    }

    public bool SetToOption(string val, bool forceSound = false)
    {
        if (Dropdown.options[Dropdown.value].text == val)
        {
            if (forceSound) playSound();
            return true;
        }

        for (var i = 0; i < Dropdown.options.Count; i++)
        {
            if (Dropdown.options[i].text != val) continue;
            if (!forceSound)
                _ignoringSounds = true;
            Dropdown.value = i;
            _ignoringSounds = false;
            return true;
        }
        return false;
    }

    private void playSound()
    {
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
