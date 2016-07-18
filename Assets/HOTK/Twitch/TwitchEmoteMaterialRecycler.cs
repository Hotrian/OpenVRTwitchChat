using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TwitchEmoteMaterialRecycler : MonoBehaviour
{
    public static TwitchEmoteMaterialRecycler Instance
    {
        get { return _instance ?? (_instance = new GameObject("TwitchEmoteMaterialRecycler", typeof(TwitchEmoteMaterialRecycler)) { hideFlags = HideFlags.HideInHierarchy }.GetComponent<TwitchEmoteMaterialRecycler>()); }
    }

    private static TwitchEmoteMaterialRecycler _instance;

    private readonly List<EmoteMaterial> _activeEmoteMaterials = new List<EmoteMaterial>(); 

    public Material GenerateEmoteMaterial()
    {
        var material = new Material(Shader.Find("Unlit/Transparent"))
        {
            name = "Emote Material"
        };
        return material;
    }

    private static readonly System.Object ActiveEmoteLocker = new System.Object();
    public IEnumerator UpdateEmoteMaterials(TwitchChatTester chat, TwitchChatTester.TwitchChatUpdate chatUpdate)
    {
        List<EmoteMaterial> list;
        var newList = new List<EmoteMaterial>();
        lock (ActiveEmoteLocker)
        {
            // Generate a list of Materials no longer being used
            var rem = (from mat in _activeEmoteMaterials where !chatUpdate.EmoteIds.Contains(mat.Id) select mat.Id).ToList();
            // Remove materials that are no longer being used 
            foreach (var r in rem)
            {
                for (var i = 0; i < _activeEmoteMaterials.Count; i ++)
                {
                    if (_activeEmoteMaterials[i].Id != r) continue;
                    //Debug.Log("Not using " + _activeEmoteMaterials[i].Id);
                    _activeEmoteMaterials.RemoveAt(i);
                    break;
                }
            }
            list = _activeEmoteMaterials.ToArray().ToList();
            // Add materials that are now being used
            foreach (var mat in (from newId in chatUpdate.EmoteIds.ToList().Distinct() let found = list.Any(existing => existing.Id == newId) where !found select newId).Select(newId => new EmoteMaterial(newId, GenerateEmoteMaterial())))
            {
                _activeEmoteMaterials.Add(mat);
                newList.Add(mat);
            }
        }

        foreach (var mat in newList)
        {
            yield return StartCoroutine(LoadEmote(mat));
            list.Add(mat);
        }

        chat.SetChatMessages(chatUpdate, list.ToArray());
    }

    IEnumerator LoadEmote(EmoteMaterial emote)
    {
        var tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        emote.Material.mainTexture = tex;
        var www = new WWW(string.Format("http://static-cdn.jtvnw.net/emoticons/v1/{0}/1.0", emote.Id));
        //Debug.Log("Loading " + emote.Id);
        yield return www;
        try
        {
            www.LoadImageIntoTexture(tex);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public struct EmoteMaterial
    {
        public readonly int Id;
        public readonly Material Material;

        public EmoteMaterial(int id, Material material)
        {
            Id = id;
            Material = material;
        }
    }
}
