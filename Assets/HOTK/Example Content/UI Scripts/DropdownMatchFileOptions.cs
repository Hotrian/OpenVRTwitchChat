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

    public void OnEnable()
    {
        Dropdown.ClearOptions();
        var info = new DirectoryInfo(Application.dataPath + "/Resources/" + RelativeFolderToReadFrom);
        var fileInfo = info.GetFiles();
        var strings = (from file in fileInfo where file.Name.EndsWith(".wav") select file.Name.Substring(0, file.Name.Length - 4)).ToList();

        Dropdown.AddOptions(strings);
        Dropdown.value = 0;
    }

    public void SetDropdownState(string val)
    {
        var sound = Resources.Load(RelativeFolderToReadFrom + "/" + Dropdown.options[Dropdown.value].text) as AudioClip;
        if (sound != null)
        {
            TwitchChatTester.Instance.SetMessageSound(sound);
            TwitchChatTester.Instance.PlayMessageSound();
        }
        else
        {
            Debug.LogWarning("Couldn't load " + Application.dataPath + "/Resources/" + RelativeFolderToReadFrom + "/" + Dropdown.options[Dropdown.value].text);
        }
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
