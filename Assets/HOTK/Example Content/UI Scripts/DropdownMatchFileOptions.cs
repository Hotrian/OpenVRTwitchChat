using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

[RequireComponent(typeof (Dropdown))]
public class DropdownMatchFileOptions : MonoBehaviour
{
    public string RelativeFolderToReadFrom;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

    private bool _firstLoad = true;

    public void OnEnable()
    {
        Dropdown.ClearOptions();
        var info = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, RelativeFolderToReadFrom));
        var fileInfo = info.GetFiles();
        var strings = (from file in fileInfo where file.Name.EndsWith(".wav") || file.Name.EndsWith(".ogg") select file.Name).ToList();

        Dropdown.AddOptions(strings);
        for (var i = 0; i < Dropdown.options.Count; i ++)
        {
            if (Dropdown.options[i].text != "gui-sound-effects-004.wav") continue;
            Dropdown.value = i;
            break;
        }
    }

    public void SetDropdownState(string val)
    {
        StartCoroutine("LoadSound", "file:///" + Application.streamingAssetsPath + "/" + RelativeFolderToReadFrom + "/" + Dropdown.options[Dropdown.value].text);
    }

    private IEnumerator LoadSound(string filePath)
    {
        var www = new WWW(filePath);
        yield return www;

        if (www.error != null)
            Debug.Log(www.error);

        var clip = www.GetAudioClip(false, true);

        if (clip != null)
        {
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                TwitchChatTester.Instance.SetMessageSound(clip);
                if (_firstLoad) _firstLoad = false;
                else TwitchChatTester.Instance.PlayMessageSound();
            } else Debug.LogWarning("Couldn't load " + filePath);
        } else Debug.LogWarning("Couldn't find " + filePath);
    }

    public bool SetToOption(string val, bool forceSound = false)
    {
        if (Dropdown.options[Dropdown.value].text == val)
        {
            if (forceSound) TwitchChatTester.Instance.PlayMessageSound();
            return true;
        }

        for (var i = 0; i < Dropdown.options.Count; i ++)
        {
            if (Dropdown.options[i].text != val) continue;
            Dropdown.value = i;
            return true;
        }
        return false;
    }
}
