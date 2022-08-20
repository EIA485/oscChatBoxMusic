using System.Net;
using WindowsMediaController;
using Windows.Media.Control;
Console.WriteLine("program started");

var mediaManager = new MediaManager();
mediaManager.OnAnyMediaPropertyChanged += updateStr;
mediaManager.OnAnyPlaybackStateChanged += updateBool;

string songName = "";

void updateStr(MediaManager.MediaSession session, GlobalSystemMediaTransportControlsSessionMediaProperties playbackInfo)
{
    if (IsValidSession(session))
    {
        songName = $"{(string.IsNullOrEmpty(playbackInfo.Artist) ? "" : $"{playbackInfo.Artist} - ")}{playbackInfo.Title}";
        Console.WriteLine($"{session.Id} {(string.IsNullOrEmpty(playbackInfo.Artist) ? "" : $"{playbackInfo.Artist} - ")}{playbackInfo.Title}");
    }
}

bool isPlaying = false;

void updateBool(MediaManager.MediaSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
{
    if (IsValidSession(session))
    {
        switch (playbackInfo.PlaybackStatus)
        {
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed:
                isPlaying = false;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened:
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                isPlaying = true;
                break;
        }
        Console.WriteLine($"{session.Id} is now {playbackInfo.PlaybackStatus}");
    }
}

bool IsValidSession(MediaManager.MediaSession session)
{
    return session.Id.StartsWith("SpotifyAB.SpotifyMusic") && session.Id.EndsWith("!Spotify");
}

mediaManager.Start();
Console.WriteLine("Media Manager Initialized");

var OSC = new SimpleOSC();
byte[] oscBuf = new byte[65535];

Console.WriteLine("connecting to osc");

OSC.OpenClient(9001); // this needed otherwise simple osc breaks
OSC.SetUnconnectedEndpoint(new IPEndPoint(IPAddress.Loopback, 9000));

bool lastIsPlaying = false;
while (true)
{
    bool offFrame = ((lastIsPlaying != isPlaying) && (lastIsPlaying == true));

    if (isPlaying || offFrame)
        OSC.SendOSCPacket(new SimpleOSC.OSCMessage { path = "/chatbox/input", arguments = new object[2] { offFrame ? "" : songName, true/*send msg right away*/ }, typeTag = "" }, oscBuf);

    if (lastIsPlaying != isPlaying)
    {
        lastIsPlaying = isPlaying;
    }
    Thread.Sleep(1500);
}
