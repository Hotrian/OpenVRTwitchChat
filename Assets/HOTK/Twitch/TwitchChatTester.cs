using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.UI;
using Valve.VR;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(TwitchIRC), typeof(TextMesh))]
public class TwitchChatTester : MonoBehaviour
{
    public static TwitchChatTester Instance
    {
        get { return _instance ?? (_instance = FindObjectOfType<TwitchChatTester>()); }
    }
    private static TwitchChatTester _instance;

    public int ChatLineCount = 27; // Max line count for our display

    public InputField UsernameBox;
    public InputField OAuthBox;
    public InputField ChannelBox;
    public Button ConnectButton;
    public Text ConnectButtonText;

    public TextMesh TextMesh // Used to check each message as it's being built to WordWrap each message into lines
    {
        get { return _textMesh ?? (_textMesh = GetComponent<TextMesh>()); }
    }
    private TextMesh _textMesh;

    public GameObject TextMeshBase; // Cloned into ChatTextMeshes

    private bool _hasGeneratedTextMeshes = false;
    public TextMesh[] ChatTextMeshes; // Each one of these is a line on our Chat Display
    public Renderer[] ChatTextRenderers; // Each one of these is a line on our Chat Display
    
    // These are used to display ViewerCount and ChannelName when connected
    public TextMesh ViewerCountTextMesh;
    public TextMesh ChannelNameTextMesh;

    // These are used to play message sounds
    public AudioSource IncomingMessageSoundSource1;
    public AudioSource IncomingMessageSoundSource2;
    public AudioSource IncomingMessageSoundSource3;
    public AudioSource IncomingMessageSoundSource4;
    public AudioSource IncomingMessageSoundSource5;
    public AudioSource IncomingMessageSoundSource6;

    public AudioSource NewFollowerSoundSource;

    public TwitchIRC IRC
    {
        get { return _irc ?? (_irc = GetComponent<TwitchIRC>()); }
    }
    private TwitchIRC _irc;

    private readonly List<TwitchChat> _userChat = new List<TwitchChat>(); // Used to store the currect messages on the Chat Display

    private readonly Dictionary<int, Material> _emoteMap = new Dictionary<int, Material>(); // Used temporarily to store the currect emotes on a given line

    private readonly Stopwatch _messageSoundStopwatch = new Stopwatch(); // Used to prevent message sound spamming
    private readonly Stopwatch _newFollowerSoundStopwatch = new Stopwatch(); // Used to prevent message sound spamming

    public bool Connected
    {
        get; private set;
    }

    public void Awake()
    {
        _instance = this;
        GenChatTexts();
    }

    public void Start()
    {
        ClearViewerCountAndChannelName("Disconnected");
        StartCoroutine("SyncWithSteamVR");
    }

    private string _username;

    private void GenRandomJustinFan()
    {
        if (!string.IsNullOrEmpty(_username)) return;
        var r = new System.Random();
        var n = r.Next().ToString();
        if (n.Length > 5) n = n.Substring(0, 5);
        _username = "JustinFan" + n;
    }

    public void ToggleConnect()
    {
        if (!Connected)
        {
            var anonymousLogin = false;
            if (!string.IsNullOrEmpty(UsernameBox.text))
            {
                if (string.IsNullOrEmpty(OAuthBox.text))
                {
                    AddSystemNotice("OAuth not found. Connecting Anonymously.", TwitchIRC.NoticeColor.Yellow);
                    GenRandomJustinFan();
                    UsernameBox.text = _username;
                    OAuthBox.text = "";
                    anonymousLogin = true;
                }
            }
            else
            {
                AddSystemNotice("Username not found. Connecting Anonymously.", TwitchIRC.NoticeColor.Yellow);
                GenRandomJustinFan();
                UsernameBox.text = _username;
                OAuthBox.text = "";
                anonymousLogin = true;
            }
            if (ChannelBox != null && ChannelBox.text != "")
            {
                if (ChannelBox.text.Contains(" "))
                {
                    AddSystemNotice("Channel name invalid!", TwitchIRC.NoticeColor.Red);
                    return;
                }
                UsernameBox.interactable = false;
                OAuthBox.interactable = false;
                ChannelBox.interactable = false;
                ConnectButtonText.text = "Press to Disconnect";

                Connected = true;
                OnChatMsg(new TwitchIRC.TwitchMessage(TwitchIRC.ToTwitchNotice(string.Format("Logging into #{0} as {1}!", ChannelFirstLetterToUpper(ChannelBox.text), FirstLetterToUpper(UsernameBox.text)))));
                IRC.NickName = anonymousLogin ? _username : UsernameBox.text;
                IRC.Oauth = anonymousLogin ? "a" : OAuthBox.text;
                IRC.ChannelName = ChannelBox.text.Trim().ToLower();

                IRC.enabled = true;
                IRC.MessageRecievedEvent.AddListener(OnChatMsg);
                IRC.StartIRC();
                _knownFollowers.Clear();
                StopCoroutine("UpdateViews");
                StopCoroutine("UpdateFollowers");
                StopCoroutine("SyncWithSteamVR");
                _gettingInitialFollowers = true;
                StartCoroutine("UpdateViews");
                StartCoroutine("UpdateFollowers");
                StartCoroutine("SyncWithSteamVR");
            }
            else AddSystemNotice("Unable to Connect: Enter a Valid Channel Name!", TwitchIRC.NoticeColor.Red);
        }
        else
        {
            UsernameBox.interactable = true;
            OAuthBox.interactable = true;
            ChannelBox.interactable = true;
            ConnectButtonText.text = "Press to Connect";

            Connected = false;
            IRC.MessageRecievedEvent.RemoveListener(OnChatMsg);
            IRC.enabled = false;
            OnChatMsg(new TwitchIRC.TwitchMessage(TwitchIRC.ToTwitchNotice("Disconnected!", TwitchIRC.NoticeColor.Red)));
            _knownFollowers.Clear();
            StopCoroutine("UpdateViews");
            StopCoroutine("UpdateFollowers");
            ClearViewerCountAndChannelName("Disconnected");
        }
    }

    private IEnumerator SyncWithSteamVR()
    {
        while (Application.isPlaying)
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
            {
                var trackingSpace = compositor.GetTrackingSpace();
                SteamVR_Render.instance.trackingSpace = trackingSpace;
            }
            yield return new WaitForSeconds(10f);
        }
    }

    private readonly Dictionary<uint, FollowsData> _knownFollowers = new Dictionary<uint, FollowsData>();
    private bool _gettingInitialFollowers;

    private IEnumerator UpdateFollowers()
    {
        while (Connected && IRC.ChannelName.Length > 0)
        {
            var form = new WWWForm();
            form.AddField("name", "value");
            var headers = form.headers;
            var url = URLAntiCacheRandomizer("https://api.twitch.tv/kraken/channels/" + IRC.ChannelName + "/follows?limit=100");

            headers["Client-ID"] = "REMOVED FOR GITHUB"; //#TODO Replace with your Client-ID
            var www = new WWW(url, null, headers);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                var obj = JsonUtility.FromJson<FollowsDataFull>(www.text);
                if (obj != null)
                {
                    if (obj.follows != null)
                    {
                        if (obj.follows.Length > 0)
                        {
                            Debug.Log("Found " + obj._total + " followers, retrieved top " + obj.follows.Length);
                            foreach (var follower in obj.follows.Where(follower => !_knownFollowers.ContainsKey(follower.user._id)))
                            {
                                _knownFollowers.Add(follower.user._id, follower);
                                if (_gettingInitialFollowers) continue;
                                OnChatMsg(
                                    new TwitchIRC.TwitchMessage(
                                        TwitchIRC.ToTwitchNotice(
                                            follower.user.display_name + " is now following!",
                                            TwitchIRC.NoticeColor.Purple)));
                                PlayNewFollowerSound();
                            }
                            _gettingInitialFollowers = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Error on page (" + url + "): " + www.error);
            }
            yield return new WaitForSeconds(30f);
        }
    }

    public static string URLAntiCacheRandomizer(string url)
    {
        var r = "";
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        var result = url + "?p=" + r;
        return result;
    }

    // Update the view count as often as possible
    private IEnumerator UpdateViews()
    {
        while (Connected && IRC.ChannelName.Length > 0)
        {
            var form = new WWWForm();
            form.AddField("name", "value");
            var headers = form.headers;
            var url = "https://api.twitch.tv/kraken/streams/" + IRC.ChannelName;

            headers["Client-ID"] = "REMOVED FOR GITHUB"; //#TODO Replace with your Client-ID
            var www = new WWW(url, null, headers);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                var obj = JsonUtility.FromJson<ChannelDataFull>(www.text);
                if (obj != null)
                {
                    if (obj.stream != null)
                    {
                        if (obj.stream.channel != null)
                        {
                            if (ChannelNameTextMesh != null)
                            {
                                var text = "";
                                if (!string.IsNullOrEmpty(obj.stream.channel.display_name))
                                    text = string.Format("#{0}", obj.stream.channel.display_name);
                                else if (!string.IsNullOrEmpty(obj.stream.channel.name))
                                    text = string.Format("#{0}", obj.stream.channel.name);
                                else text = "Not Streaming";
                                ChannelNameTextMesh.text = text;
                            }
                            if (ViewerCountTextMesh != null)
                                ViewerCountTextMesh.text = string.Format("Viewers: {0}", obj.stream.viewers);
                        }
                        else
                        {
                            ClearViewerCountAndChannelName();
                        }
                    }
                    else
                    {
                        ClearViewerCountAndChannelName();
                    }
                }
            }
            else
            {
                Debug.LogError("Error on page (" + url + "): " + www.error);
            }
            yield return new WaitForSeconds(10f);
        }
    }

    // Reset the channel and viewer text
    private void ClearViewerCountAndChannelName(string channelText = null)
    {
        if (ChannelNameTextMesh != null) ChannelNameTextMesh.text = (channelText ?? "");
        if (ViewerCountTextMesh != null) ViewerCountTextMesh.text = "";
    }

    // Process a given message and pass it down the correct channels
    private void OnChatMsg(TwitchIRC.TwitchMessage message)
    {
        var cmd = message.Message.Split(' ');
        var nickname = cmd[0].Split('!')[0].Substring(1);
        var mode = cmd[1];
        var channel = cmd[2].Substring(1);
        var len = cmd[0].Length + cmd[1].Length + cmd[2].Length + 4;
        var chat = message.Message.Substring(len);

        switch (mode)
        {
            case "NOTICE":
                // Compatability with real Twitch System messages
                if (nickname == "tmi.twitch.tv")
                {
                    nickname = "Twitch";
                    if (chat.StartsWith("Error"))
                        channel = "System-Red";
                    else if (chat == "Login unsuccessful")
                        channel = "System-Red";
                }
                // Convert Notice to Name Color
                switch (channel)
                {
                    case "System-Green":
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(0f, 1f, 0f)), chat);
                        break;
                    case "System-Red":
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(1f, 0f, 0f)), chat);
                        break;
                    case "System-Blue":
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(0f, 0.4f, 1f)), chat);
                        break;
                    case "System-Yellow":
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(1f, 1f, 0f)), chat);
                        break;
                    case "System-Purple":
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(1f, 0f, 1f)), chat);
                        break;
                    default:
                        AddMsg(nickname, TwitchIRC.ColorToHex(new Color(1f, 1f, 1f)), chat);
                        break;
                }
                break;
            case "PRIVMSG":
                AddMsg(FirstLetterToUpper(nickname), TwitchIRC.GetUserColor(nickname), chat, message.Emotes);
                PlayMessageSound();
                break;
        }
    }

    /// <summary>
    /// Set the Pitch of the Chat Message sound
    /// </summary>
    /// <param name="pitch"></param>
    public void SetMessagePitch(float pitch)
    {
        IncomingMessageSoundSource1.pitch = pitch;
        IncomingMessageSoundSource2.pitch = pitch;
        IncomingMessageSoundSource3.pitch = pitch;
        IncomingMessageSoundSource4.pitch = pitch;
        IncomingMessageSoundSource5.pitch = pitch;
        IncomingMessageSoundSource6.pitch = pitch;
    }

    /// <summary>
    /// Set the AudioClip to be played when Chat Messages are received
    /// </summary>
    /// <param name="sound"></param>
    public void SetMessageSound(AudioClip sound)
    {
        IncomingMessageSoundSource1.clip = sound;
        IncomingMessageSoundSource2.clip = sound;
        IncomingMessageSoundSource3.clip = sound;
        IncomingMessageSoundSource4.clip = sound;
        IncomingMessageSoundSource5.clip = sound;
        IncomingMessageSoundSource6.clip = sound;
    }

    /// <summary>
    /// Set the Volume of the Chat Message sound
    /// </summary>
    /// <param name="volume"></param>
    public void SetMessageVolume(float volume)
    {
        IncomingMessageSoundSource1.volume = volume;
        IncomingMessageSoundSource2.volume = volume;
        IncomingMessageSoundSource3.volume = volume;
        IncomingMessageSoundSource4.volume = volume;
        IncomingMessageSoundSource5.volume = volume;
        IncomingMessageSoundSource6.volume = volume;
    }

    // Play a sound on the next free sound player
    public void PlayMessageSound()
    {
        // Prevent the message sound from spamming too rapidly
        if (_messageSoundStopwatch.IsRunning)
        {
            if (_messageSoundStopwatch.ElapsedMilliseconds < 50) return;
            _messageSoundStopwatch.Reset();
            _messageSoundStopwatch.Start();
        }
        else
            _messageSoundStopwatch.Start();

        // Play the message sound on an available channel (allows simultaneous message sounds)
        if (IncomingMessageSoundSource1 != null && IncomingMessageSoundSource1.clip != null && !IncomingMessageSoundSource1.isPlaying) IncomingMessageSoundSource1.Play();
        else if (IncomingMessageSoundSource2 != null && IncomingMessageSoundSource2.clip != null && !IncomingMessageSoundSource2.isPlaying) IncomingMessageSoundSource2.Play();
        else if (IncomingMessageSoundSource3 != null && IncomingMessageSoundSource3.clip != null && !IncomingMessageSoundSource3.isPlaying) IncomingMessageSoundSource3.Play();
        else if (IncomingMessageSoundSource4 != null && IncomingMessageSoundSource4.clip != null && !IncomingMessageSoundSource4.isPlaying) IncomingMessageSoundSource4.Play();
        else if (IncomingMessageSoundSource5 != null && IncomingMessageSoundSource5.clip != null && !IncomingMessageSoundSource5.isPlaying) IncomingMessageSoundSource5.Play();
        else if (IncomingMessageSoundSource6 != null && IncomingMessageSoundSource6.clip != null) IncomingMessageSoundSource6.Play();
    }

    /// <summary>
    /// Set the Pitch of the New Follower sound
    /// </summary>
    /// <param name="pitch"></param>
    public void SetNewFollowerPitch(float pitch)
    {
        NewFollowerSoundSource.pitch = pitch;
    }

    /// <summary>
    /// Set the AudioClip to be played when New Followers are received
    /// </summary>
    /// <param name="sound"></param>
    public void SetNewFollowerSound(AudioClip sound)
    {
        NewFollowerSoundSource.clip = sound;
    }

    /// <summary>
    /// Set the Volume of the New Follower sound
    /// </summary>
    /// <param name="volume"></param>
    public void SetNewFollowerVolume(float volume)
    {
        NewFollowerSoundSource.volume = volume;
    }

    public void PlayNewFollowerSound()
    {
        // Prevent the message sound from spamming too rapidly
        if (_newFollowerSoundStopwatch.IsRunning)
        {
            if (_newFollowerSoundStopwatch.ElapsedMilliseconds < 50) return;
            _newFollowerSoundStopwatch.Reset();
            _newFollowerSoundStopwatch.Start();
        }
        else
            _newFollowerSoundStopwatch.Start();

        if (NewFollowerSoundSource != null && NewFollowerSoundSource.clip != null) NewFollowerSoundSource.Play();
    }

    // Add a system message to the chat display
    public void AddSystemNotice(string msgIn, TwitchIRC.NoticeColor colorEnum = TwitchIRC.NoticeColor.Blue)
    {
        OnChatMsg(new TwitchIRC.TwitchMessage(TwitchIRC.ToNotice("System", msgIn, colorEnum)));
    }

    // Add a given message to our list of messages, pushing them to the chat display
    private void AddMsg(string nickname, string color, string chat, List<TwitchIRC.EmoteKey> emotes = null )
    {
        _userChat.Add(new TwitchChat(nickname, color, chat, emotes ?? new List<TwitchIRC.EmoteKey>()));

        // Remove excess messages
        while (_userChat.Count > ChatLineCount)
            _userChat.RemoveAt(0);

        StartCoroutine(TwitchEmoteMaterialRecycler.Instance.UpdateEmoteMaterials(this, new TwitchChatUpdate(_userChat.ToArray(), _userChat.SelectMany(d => d.Emotes).ToList().Select(d => d.EmoteId).ToArray())));
    }

    // Generate the TextMeshes if required and wordwrap the given
    // messages such that they _should_ fit into lines for our TextMeshes
    public void SetChatMessages(TwitchChatUpdate chatUpdate, TwitchEmoteMaterialRecycler.EmoteMaterial[] mats)
    {
        GenChatTexts();
        WordWrapText(chatUpdate.Messages.ToList(), mats);
    }

    private static readonly System.Object BuilderLocker = new System.Object();
    // Build the given messages and materials into a list of lines for our TextMeshes
    private void WordWrapText(List<TwitchChat> messages, TwitchEmoteMaterialRecycler.EmoteMaterial[] mats) // TODO: Don't rebuild the #$%^ing list every time we get a message, just push the data along and rebuild the newest line
    {
        try
        {
            lock (BuilderLocker)
            {
                mats = mats.DistinctBy(p => p.Id).ToArray();
                _emoteMap.Clear();
                for (var i = 0; i < mats.Length; i++)
                {
                    _emoteMap.Add(mats[i].Id, mats[i].Material);
                }
                const int maxEmotesPerLine = 7;
                var lines = new List<LineEmotePair>();
                TextMesh.text = "";
                var ren = TextMesh.GetComponent<Renderer>();
                const float rowLimit = 0.975f; //find the sweet spot
                var messageEmotes = new List<MaterialIndexPair>[messages.Count];
                for (var mi = 0; mi < messages.Count; mi++)
                {
                    messageEmotes[mi] = new List<MaterialIndexPair>();
                }
                for (var mi = 0; mi < messages.Count; mi++)
                {
                    var m = messages[mi];
                    TextMesh.text = string.Format("<color=#{0}FF>{1}</color>: ", m.Color, m.Name);
                    var builder = "";
                    var message = m.Message;

                    // Insert the Emotes for this message
                    if (m.Emotes != null && m.Emotes.Count > 0)
                    {
                        var indexIncrease = 0;
                        var nextIndex = 0;
                        foreach (var key in m.Emotes)
                        {
                            // Cache the emote list so we can have seven unique emotes per message
                            Material mat;
                            if (!_emoteMap.TryGetValue(key.EmoteId, out mat)) continue;
                            var ind = 0;
                            var foundKey = false;
                            foreach (var matPair in messageEmotes[mi].Where(matPair => key.EmoteId == matPair.EmoteId))
                            {
                                foundKey = true;
                                ind = matPair.Index;
                                break;
                            }
                            if (!foundKey)
                            {
                                ind = nextIndex;
                                messageEmotes[mi].Add(new MaterialIndexPair(ind, key.EmoteId, mat));
                                nextIndex++;
                            }
                            
                            var text = string.Format("<quad material={0} size=64 x=0 y=0 width=1 height=1 />", ind);
                            message = message.Insert(key.EmoteStart + indexIncrease, text);
                            indexIncrease += text.Length;
                        }
                    }

                    // Convert this message into Lines, such that they do not exceed the bounds of the chat box
                    var buildingQuad = false;
                    var buildingQuadNow = false;
                    var quadBuilder = "";
                    var parts = message.Split(' ');
                    var lineEmotes = new List<MaterialIndexPair>();
                    var lineEmoteCount = 0;
                    var emotes = messageEmotes[mi].ToArray();
                    var emoteMap = new Dictionary<int, int>();
                    var currentIndex = -1;
                    foreach (var t in parts)
                    {
                        builder = TextMesh.text;
                        if (t == "<quad") // This is the beginning of an emote
                            buildingQuad = true;
                        if (!buildingQuad)
                        {
                            // Add these text pieces 
                            TextMesh.text += t + " ";
                            if (ren.bounds.extents.x > rowLimit)
                            {
                                lines.Add(new LineEmotePair(builder.TrimEnd(), lineEmotes.ToArray()));
                                lineEmotes.Clear();
                                emoteMap.Clear();
                                lineEmoteCount = 0;
                                TextMesh.text = t + " ";
                            }
                            builder = TextMesh.text;
                        }
                        else
                        {
                            if (buildingQuadNow || lineEmoteCount < maxEmotesPerLine)
                            {
                                buildingQuadNow = true; // Allow an emoji to finish building even if it exceeds the limits
                                // Here we are constructing the quad used in the TextMesh so that it will display an Emoji
                                string te;
                                if (t.StartsWith("material="))
                                {
                                    // Remap the materials for this line
                                    var index = int.Parse(t.Substring(9, 1));
                                    currentIndex = index;
                                    int ind;
                                    if (emoteMap.TryGetValue(index, out ind))
                                    {
                                        // This emote was already used on this line
                                        te = "material=" + ind;
                                    }
                                    else
                                    {
                                        // This emote is new to this line, we must map it for future use on this line
                                        lineEmoteCount++;
                                        lineEmotes.Add(new MaterialIndexPair(lineEmoteCount, emotes[index].EmoteId, emotes[index].Material));
                                        te = "material=" + lineEmoteCount;
                                        emoteMap.Add(index, lineEmoteCount);
                                    }
                                }
                                else te = t;

                                quadBuilder += te + " ";
                                if (t != "/>") continue;
                                buildingQuad = false;
                                buildingQuadNow = false;
                                TextMesh.text += quadBuilder.TrimEnd() + " ";

                                if (currentIndex == -1)
                                {
                                    Debug.LogWarning("This shouldn't happen..");
                                    continue;
                                }
                                if (ren.bounds.extents.x > rowLimit)
                                {
                                    // This Emoji violates the line's bounds
                                    // Check if this material belongs on the next line only
                                    var curId = lineEmotes[lineEmoteCount - 1].EmoteId;
                                    var count = 0;
                                    foreach (var emote in lineEmotes.Where(emote => emote.EmoteId == curId))
                                    {
                                        count++;
                                        if (count >= 2)
                                            break;
                                    }
                                    if (count == 1) lineEmotes.RemoveAt(lineEmoteCount - 1); // Remove the last material if it only belongs to this emote
                                    lines.Add(new LineEmotePair(builder.TrimEnd(), lineEmotes.ToArray())); // Assign the previous working buffer to the last line
                                    // Push this Emoji to the next line and reconstruct it there
                                    lineEmotes.Clear();
                                    emoteMap.Clear();

                                    var nextQuad = "";
                                    var nexQuadParts = quadBuilder.Split(' ');

                                    lineEmoteCount = 1;
                                    // Reconstruct this Emoji for the next line
                                    foreach (var tt in nexQuadParts)
                                    {
                                        string tte;
                                        if (tt.StartsWith("material="))
                                        {
                                            tte = "material=" + lineEmoteCount;
                                        }
                                        else tte = tt;

                                        nextQuad += tte + " ";
                                    }
                                    // Update the Emoji map with the material for the Emoji being pushed to the next line
                                    lineEmotes.Add(new MaterialIndexPair(lineEmoteCount, emotes[currentIndex].EmoteId, emotes[currentIndex].Material));
                                    emoteMap.Add(currentIndex, lineEmoteCount);
                                    currentIndex = -1;
                                    TextMesh.text = " " + nextQuad.Trim() + " ";
                                }
                                // Clear the working buffers
                                builder = TextMesh.text;
                                quadBuilder = "";
                            }
                            else // Too many emotes on this line, ignore this quad
                            {
                                if (t != "/>") continue;
                                buildingQuad = false;
                            }
                        }
                    }
                    lines.Add(new LineEmotePair(builder.TrimEnd(), lineEmotes.ToArray())); // Add the final line from the builder to the list of lines
                }
                // Clear the builder's text
                TextMesh.text = "";

                // Remove excess lines
                while (lines.Count > ChatLineCount)
                    lines.RemoveAt(0);

                // Set the Emote materials and TextMesh texts
                    var offset = ChatLineCount - lines.Count;
                for (var i = 0; i < ChatLineCount; i++)
                {
                    if (i >= lines.Count) continue;
                    SetMaterialSize(ChatTextRenderers[i + offset], lines[i].EmoteList.Length + 1);
                    for (var j = 0; j < lines[i].EmoteList.Length; j++)
                    {
                        SetMaterial(ChatTextRenderers[i + offset], lines[i].EmoteList[j].Index, lines[i].EmoteList[j].Material);
                    }
                    ChatTextMeshes[i + offset].text = lines[i].LineText;
                }

                // Refresh the texts to force them to display correctly
                foreach (TextMesh t in ChatTextMeshes)
                {
                    t.anchor = TextAnchor.UpperLeft;
                    t.anchor = TextAnchor.LowerLeft;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Assign a material to a given renderer (used to set Emoji materials)
    public static void SetMaterial(Renderer ren, int index, Material material)
    {
        var mats = ren.sharedMaterials;
        if (index < 0 || index >= mats.Length) return;
        mats[index] = material;
        ren.sharedMaterials = mats;
    }

    // Set the size of the Materials Array on a given Renderer (used to set Emoji materials)
    public static void SetMaterialSize(Renderer ren, int count)
    {
        if (count < 1) return;
        // Make new Materials Array
        var mats = ren.sharedMaterials;
        var newMats = new Material[count];

        // Copy old Materials Array
        for (var i = 0; i < Mathf.Min(mats.Length, count); i++)
        {
            newMats[i] = mats[i];
        }

        // Apply new Materials Array
        ren.sharedMaterials = newMats;
    }

    private static readonly System.Object GenTextMeshLocker = new System.Object();
    // Generate our Text Meshes and ensure they are only generated once
    private void GenChatTexts()
    {
        lock (GenTextMeshLocker)
        {
            if (_hasGeneratedTextMeshes) return;
            ChatTextMeshes = new TextMesh[ChatLineCount];
            ChatTextRenderers = new Renderer[ChatLineCount];
            for (var i = 0; i < ChatLineCount; i++)
            {
                var obj = GameObject.Instantiate(TextMeshBase);
                ChatTextRenderers[i] = obj.GetComponent<Renderer>();
                ChatTextMeshes[i] = obj.GetComponent<TextMesh>();
                ChatTextMeshes[i].text = "";
                obj.transform.parent = gameObject.transform.parent;
                obj.transform.localScale = new Vector3(0.005f, 0.005f, 1f);
                obj.transform.localPosition = new Vector3(-0.5f, 0.465f - (0.037f * i), -1f);
                obj.SetActive(true);
            }
            _hasGeneratedTextMeshes = true;
        }
    }

    // Convert the first letter of the given string to a Capital Letter
    public static string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    // Convert the first letter, and every first letter after an underscore to a Capital Letter
    // This looks a bit nicer before we have the proper format for this channel name
    public static string ChannelFirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        var endsWith_ = str.EndsWith("_");
        if (endsWith_) str = str.Substring(0, str.Length - 1);

        if (str.Length <= 1) return str.ToUpper();
        var pieces = str.Split('_');
        var st = "";
        for (var i = 0; i < pieces.Length; i++)
        {
            st += char.ToUpper(pieces[i][0]) + pieces[i].Substring(1);
            if (i < pieces.Length - 1)
                st += "_";
        }
        if (endsWith_) st += "_";
        return st;
    }

    internal static Texture2D GenerateBaseTexture()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.45f, 0.2f, 0.75f));
        tex.Apply();
        return tex;
    }

    // Contains a single chat message
    public struct TwitchChat
    {
        public readonly string Name;
        public readonly string Color;
        public readonly string Message;
        public readonly List<TwitchIRC.EmoteKey> Emotes;

        public TwitchChat(string name, string color, string message, List<TwitchIRC.EmoteKey> emotes)
        {
            Name = name;
            Color = color;
            Message = message;
            Emotes = emotes;
        }
    }

    // Contains a group of chat messages
    public struct TwitchChatUpdate
    {
        public readonly TwitchChat[] Messages;
        public readonly int[] EmoteIds;

        public TwitchChatUpdate(TwitchChat[] messages, int[] emoteIds)
        {
            Messages = messages;
            EmoteIds = emoteIds;
        }
    }

    // Passes material/emoteid information for a given message/line
    public struct MaterialIndexPair
    {
        public readonly int Index;
        public readonly int EmoteId;
        public readonly Material Material;

        public MaterialIndexPair(int index, int emoteId, Material material)
        {
            Index = index;
            EmoteId = emoteId;
            Material = material;
        }
    }

    // Passes material/text information for a given line
    public struct LineEmotePair
    {
        public readonly string LineText;
        public readonly MaterialIndexPair[] EmoteList;

        public LineEmotePair(string lineText, MaterialIndexPair[] emoteList)
        {
            LineText = lineText;
            EmoteList = emoteList;
        }
    }

    // These are filled by JsonUtility so the compiler is confused
#pragma warning disable 649
    // ReSharper disable InconsistentNaming
    [Serializable]
    private class ChannelDataFull
    {
        public ChannelLinksData _links;
        public StreamData stream;
    }

    [Serializable]
    private class ChannelLinksData
    {
        public string channel;
        public string self;
    }

    [Serializable]
    private class StreamData
    {
        public string game;
        public uint viewers;
        public float average_fps;
        public uint delay;
        public uint video_height;
        public bool is_playlist;
        public string created_at;
        public uint _id;
        public StreamChannelData channel;
        public StreamPreviewData preview;
        public StreamLinksData _links;
    }
    
    [Serializable]
    private class StreamChannelData
    {
        public bool mature;
        public string status;
        public string broadcaster_language;
        public string display_name;
        public string game;
        public string delay;
        public string language;
        public uint _id;
        public string name;
        public string created_at;
        public string updated_at;
        public string logo;
        public string banner;
        public string video_banner;
        public string background;
        public string profile_banner;
        public string profile_banner_background_color;
        public bool partner;
        public string url;
        public uint views;
        public uint followers;
        public StreamChanneLinksData _links;
    }

    [Serializable]
    private class StreamChanneLinksData
    {
        public string self;
        public string follows;
        public string commercial;
        public string stream_key;
        public string chat;
        public string features;
        public string subscriptions;
        public string editors;
        public string teams;
        public string videos;
    }

    [Serializable]
    private class StreamPreviewData
    {
        public string small;
        public string medium;
        public string large;
        public string template;
    }

    [Serializable]
    private class StreamLinksData
    {
        public string self;
    }

    [Serializable]
    private class FollowsDataFull
    {
        public uint _total;
        public FollowsLinksData _links;
        public FollowsData[] follows;
    }

    [Serializable]
    private class FollowsLinksData
    {
        public string next;
        public string self;
    }

    [Serializable]
    private class FollowsData
    {
        public string created_at;
        public FollowerLinks _links;
        public bool notifications;
        public FollowerData user;
    }

    [Serializable]
    private class FollowerData
    {
        public FollowerLinks _links;
        public bool staff;
        public string logo;
        public string display_name;
        public string created_at;
        public string updated_at;
        public uint _id;
        public string name;
    }

    [Serializable]
    private class FollowerLinks
    {
        public string self;
    }
#pragma warning restore 649
    // ReSharper restore InconsistentNaming
}

public static class LinqExtensions 
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}