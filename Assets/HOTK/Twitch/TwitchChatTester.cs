using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(TwitchIRC), typeof(TextMesh))]
public class TwitchChatTester : MonoBehaviour
{
    public static TwitchChatTester Instance
    {
        get { return _instance ?? (_instance = FindObjectOfType<TwitchChatTester>()); }
    }

    private static TwitchChatTester _instance;

    public struct TwitchChat
    {
        public readonly string Name;
        public readonly string Color;
        public readonly string Message;

        public TwitchChat(string name, string color, string message)
        {
            Name = name;
            Color = color;
            Message = message;
        }
    }

    public InputField UsernameBox;
    public InputField OAuthBox;
    public InputField ChannelBox;
    public Button ConnectButton;
    public Text ConnectButtonText;

    public TextMesh TextMesh
    {
        get { return _textMesh ?? (_textMesh = GetComponent<TextMesh>()); }
    }
    private TextMesh _textMesh;

    public TwitchIRC IRC
    {
        get { return _irc ?? (_irc = GetComponent<TwitchIRC>()); }
    }
    private TwitchIRC _irc;

    private readonly List<TwitchChat> _userChat = new List<TwitchChat>();

    private bool _connected;

    public void Awake()
    {
        _instance = this;
    }

    public void ToggleConnect()
    {
        if (!_connected)
        {
            if (UsernameBox != null && UsernameBox.text != "")
            {
                if (OAuthBox != null && OAuthBox.text != "")
                {
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

                        _connected = true;
                        OnChatMsg(TwitchIRC.ToTwitchNotice(string.Format("Logging into #{0} as {1}!", ChannelBox.text, UsernameBox.text)));
                        IRC.NickName = UsernameBox.text;
                        IRC.Oauth = OAuthBox.text;
                        IRC.ChannelName = ChannelBox.text.ToLower();

                        IRC.enabled = true;
                        IRC.MessageRecievedEvent.AddListener(OnChatMsg);
                        IRC.StartIRC();
                    }
                    else AddSystemNotice("Unable to Connect: Enter a Valid Channel Name!", TwitchIRC.NoticeColor.Red);
                }
                else AddSystemNotice("Unable to Connect: Enter a Valid OAuth Key! http://www.twitchapps.com/tmi/", TwitchIRC.NoticeColor.Red);
            }
            else AddSystemNotice("Unable to Connect: Enter a Valid Username!", TwitchIRC.NoticeColor.Red);
        }
        else
        {
            UsernameBox.interactable = true;
            OAuthBox.interactable = true;
            ChannelBox.interactable = true;
            ConnectButtonText.text = "Press to Connect";

            _connected = false;
            IRC.MessageRecievedEvent.RemoveListener(OnChatMsg);
            IRC.enabled = false;
            OnChatMsg(TwitchIRC.ToTwitchNotice("Disconnected!", TwitchIRC.NoticeColor.Red));
        }
    }

    private void OnChatMsg(string msg)
    {
        var cmd = msg.Split(' ');
        var nickname = cmd[0].Split('!')[0].Substring(1);
        var mode = cmd[1];
        var channel = cmd[2].Substring(1);
        var len = cmd[0].Length + cmd[1].Length + cmd[2].Length + 4;
        var chat = msg.Substring(len);

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
                AddMsg(FirstLetterToUpper(nickname), TwitchIRC.GetUserColor(nickname), chat);
                break;
        }
    }

    public void AddSystemNotice(string msgIn, TwitchIRC.NoticeColor colorEnum = TwitchIRC.NoticeColor.Blue)
    {
        OnChatMsg(TwitchIRC.ToNotice("System", msgIn, colorEnum));
    }

    private void AddMsg(string nickname, string color, string chat)
    {
        _userChat.Add(new TwitchChat(nickname, color, chat));

        while (_userChat.Count > 27)
            _userChat.RemoveAt(0);
        
        WordWrapText(_userChat);
    }

    private void WordWrapText(List<TwitchChat> messages)
    {
        var lines = new List<string>();
        TextMesh.text = "";
        var ren = TextMesh.GetComponent<Renderer>();
        var rowLimit = 0.975f; //find the sweet spot
        foreach (var m in messages)
        {
            TextMesh.text = string.Format("<color=#{0}FF>{1}</color>: ", m.Color, m.Name);
            string builder = "";
            var parts = m.Message.Split(' ');
            foreach (var t in parts)
            {
                builder = TextMesh.text;
                TextMesh.text += t + " ";
                if (ren.bounds.extents.x > rowLimit)
                {
                    lines.Add(builder.TrimEnd() + System.Environment.NewLine);
                    TextMesh.text = t + " ";
                }
                builder = TextMesh.text;
            }
            lines.Add(builder.TrimEnd() + System.Environment.NewLine);
        }
        
        TextMesh.text = lines.Aggregate("", (current, t) => current + t);
    }

    public static string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
}
