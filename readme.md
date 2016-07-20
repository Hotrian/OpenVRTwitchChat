### This is the new home of the TwitchChatOverlay repository!
### It was previously located [over here](https://github.com/Hotrian/HeadlessOverlayToolkit/tree/twitchtest)!
#### See also my [OpenVRDesktopDisplayPortal](https://github.com/Hotrian/OpenVRDesktopDisplayPortal) for putting any desktop window into VR :D
====
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8PWSSHWNCWWQU)

This is a stripped down version of the SteamVR Unity Plugin with a custom Overlay script that displays twitch chat!

To use this, Download and launch [the latest release](https://github.com/Hotrian/OpenVRTwitchChat/releases), and enter your Twitch Username, OAuth Key (get your [OAuth Key here](http://www.twitchapps.com/tmi/)) and the desired Channel, then press "Press to Connect" and it should momentarilly connect to your Twitch chat. Check behind one of your controllers (should be left!) for your chat display! It should zoom up when you look at it.

#### Oculus Rift users:
We're receiving reports that some Rift users find some games are incompatible with the SteamVR Overlay system. You can read more about it in [the issue posted here](https://github.com/Hotrian/OpenVRTwitchChat/issues/4). The jist of things is that some games seem to skip the SteamVR Compositor and draw directly to the Rift instead. Check the SteamVR Display Mirror and see if you can see the Overlays there. If you can see the Overlays in the Mirror but not the Rift, then that game is probably incompatible :(. Please post your findings in [the issue](https://github.com/Hotrian/OpenVRTwitchChat/issues/4).

## Features
- See your favorite Twitch.tv chat in VR! From _almost_ any SteamVR game!
- Easily attach Chat to the Screen, a Tracked Controller, or drop it in the World.
- Easily snap Controller attached Chat to a set "Base Position".
- Offset Chat positionally and rotationally.
- Basic Gaze Detection and Animation support (Fade In/Out and/or Scale Up/Down on Gaze).
- Basic Save/Load Support! Saves all settings _except_ OAuth Key!
- Notification Sound on Chat Message Received (Set Volume to 0 to Disable)
- Active Viewer Count (Updated as often as Twitch allows)
- "Experimental" Emote Support ;) ![kappa](http://static-cdn.jtvnw.net/emoticons/v1/25/1.0) ![kappa](http://static-cdn.jtvnw.net/emoticons/v1/25/1.0)

## Basic Controls
(click to enlarge) (actual controls might vary slightly as this is still under development)
![basic controls](http://image.prntscr.com/image/224cbd14d02f4e209879ef9d5f647be2.png)

## Demos
Note that these demos are a bit old, and do not represent the current state of the branch.
- ![Here is a GIF](https://thumbs.gfycat.com/SinfulHonestGenet-size_restricted.gif)
- Please see this [higher quality view of the GIF above](https://gfycat.com/SinfulHonestGenet) of the Twitch Chat in action!
- [Check out this Youtube Video](https://www.youtube.com/watch?v=JMk7Vy1Zq_s) which shows the desktop UI in action!
- [**Here it is inside TiltBrush!**](https://www.youtube.com/watch?v=tpqIQ5UkGrY) The text is perfectly readable in headset, but seems a little far away on the Display Mirror :P

## Known Issues
- SteamVR_ControllerManager.cs doesn't correctly auto-identify controllers for me, so I wrote my own manager, HOTK_TrackedDeviceManager.cs. My Device Manager is super pre-alpha but should correctly identify both Controllers as long as at least one of them is assigned to either the left or right hand, and they are both connected. If neither Controller is assigned to a hand, they are assigned on a first come first serve basis. If only one Controller is connected, and it isn't already assigned, it will be assigned to the right hand.
- Oculus Rift Users are reporting some games seem to be incompatible, please see [the issue posted here](https://github.com/Hotrian/OpenVRTwitchChat/issues/4).
- If you launch it and nothing happens in VR, check the Data folder for output_log.txt and look for "Connect to VR Server Failed (301)" if you're getting that error, try restarting your PC, and if that doesn't work [try this solution](https://www.reddit.com/r/Vive/comments/4p9hxg/wip_i_just_released_the_first_build_of_my_cross/d4kmvrj) by /u/GrindheadJim:

>For clarification, what I did was right-click on the .exe file, clicked "Properties", went to the "Compatibility" tab, and checked the box under "Run as Administrator". For the record, I have also done this with Steam and SteamVR. If you're having any issues with any of these programs, I would start there. 

## Additional Notes / Tips & Tricks
- When attaching Overlays to controllers, the offset is reoriented to match the Base Position's orientation. X+ should always move the Overlay to the Right, Y+ should always move Up, and Z+ should always move Forward, relative to the Overlay.
- You can copy and paste your OAuth key, you don't have to manually type it out, but you do have to get it off that site, or off another site that generates Twitch OAuth keys.
- OAuth is not saved for security reasons.
- You can put the Overlay up in the sky and tilt it if you don't like it on the controllers and find it obtrusive in the world. Just set the Base Position to "World", then mess with the middle "Positional Control" slider and the top "Rotational Control" slider until you find a position that works for you :)
- You can stream the Display Mirror if you want your viewers to be able to see the Overlay, or you can stream the game's output if you do not.
- To save a new settings profile, select "New.." from the dropdown and press "Save", then enter a name in the box just below and press the new "Save" button.
- To delete a profile, select the profile you want to delete from the dropdown box, and click the Delete button twice. On the first click the text will change red and the tooltip will change. On second click the profile will be deleted. There is no undo so use this wisely. Select a different profile or press Load or Save to reset the Delete button.
- You can disable Chat Notification Sounds by setting the Volume slider to 0.

## How can I help?

If you know how to program, we could always use help! Feel free to fork the repo and improve it in any way you see fit; but if you don't know how but still want to contribute, we always need more beta testers! Download the release and share it around! If you want to do more, donations are always cool too! You'll be funding my programming endeavors, including cool projects like these VR Overlays: [![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8PWSSHWNCWWQU)

## Special Thanks

(No endorsements are intended to be implied.)

Thanks to Grahnz for the base [TwitchIRC.cs](https://github.com/Grahnz/TwitchIRC-Unity/blob/master/TwitchIRC.cs) script! The license file is available [here](../master/TwitchIRC-LICENSE.txt).

Thanks to [Eric Daily](http://tutsplus.com/authors/eric-daily) for the base [SaveLoad](http://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934) script! The license file is available [here](../master/SaveLoad-LICENSE.txt).

Thanks to [Jalastram](https://www.freesound.org/people/jalastram/) for some of their sounds! The license file for these can be found [here](../master/Assets/StreamingAssets/Twitch/Sounds/GUI Sound Effects/LICENSE.txt).

Thanks to everyone who has tested it so far! The feedback has really helped speed things along!
