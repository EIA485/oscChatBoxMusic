using System.Net;
using System.ComponentModel;
using Windows.Media.Control;
using WindowsMediaController;
Console.WriteLine("program started");

var mediaManager = new MediaManager();
MediaManager.MediaSession? SelectedSession = null;
bool isPlaying = true;
string songName = "";
var sesionToProperties = new Dictionary<MediaManager.MediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties>();

mediaManager.OnAnyMediaPropertyChanged += updateStr;
mediaManager.OnAnyPlaybackStateChanged += updateBool;
string FormatedName(GlobalSystemMediaTransportControlsSessionMediaProperties playbackInfo) => $"{(string.IsNullOrEmpty(playbackInfo.Artist) ? "" : $"{playbackInfo.Artist} - ")}{playbackInfo.Title}";

void updateStr(MediaManager.MediaSession session, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
{
    sesionToProperties[session] = mediaProperties;
    CheckSessionNull(session);
    if (session == SelectedSession)
    {
        songName = FormatedName(mediaProperties);
        ConsoleColorSelected();
    }
    Console.WriteLine($"{session.Id}: {FormatedName(mediaProperties)}");
    ConsoleColorReset();
}


void updateBool(MediaManager.MediaSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
{
    CheckSessionNull(session);
    if (SelectedSession == session)
    {
        isPlaying = Paused(playbackInfo.PlaybackStatus) ?? isPlaying;
        ConsoleColorSelected();
    }
    Console.WriteLine($"{session.Id} is now {playbackInfo.PlaybackStatus}");
    ConsoleColorReset();
}

bool? Paused(GlobalSystemMediaTransportControlsSessionPlaybackStatus playbackStatus)
{
    switch (playbackStatus)
    {
        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed:
            return false;
        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened:
        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
            return true;
    }
    return null;
}

void CheckSessionNull(MediaManager.MediaSession session)
{
    if (SelectedSession == null && ProbablySpotify(session)) SelectedSession = session;
}

bool ProbablySpotify(MediaManager.MediaSession session) => session.Id.StartsWith("SpotifyAB.SpotifyMusic") && session.Id.EndsWith("!Spotify");
void ConsoleColorSelected() => Console.ForegroundColor = ConsoleColor.Green;
void ConsoleColorReset() => Console.ForegroundColor = ConsoleColor.Gray;

mediaManager.Start();
Console.WriteLine("Media Manager Initialized");

var OSC = new SimpleOSC();
byte[] oscBuf = new byte[65535];

Console.WriteLine("connecting to osc");

OSC.OpenClient(9001); // this needed otherwise simple osc breaks
OSC.SetUnconnectedEndpoint(new IPEndPoint(IPAddress.Loopback, 9000));

BackgroundWorker bw = new BackgroundWorker();
bw.DoWork += (a, b) =>
{
    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter && mediaManager.CurrentMediaSessions.Count > 1)
    {
        var lastSession = SelectedSession;
        string[] sessionNames = mediaManager.CurrentMediaSessions.Select((e) => e.Key).ToArray();
        SelectedSession = mediaManager.CurrentMediaSessions[sessionNames[ConsoleHelper.MultiChoice(sessionNames)]];
        ConsoleColorSelected();
        Console.WriteLine($"{SelectedSession.Id} has been selected");
        songName = sesionToProperties[SelectedSession] == null ? "" : FormatedName(sesionToProperties[SelectedSession]);
        isPlaying = Paused(SelectedSession.ControlSession.GetPlaybackInfo().PlaybackStatus) ?? false;
        if (isPlaying) Console.WriteLine("updated osc to: " + songName);
        ConsoleColorReset();
    }
    else Thread.Sleep(150);
};
bw.RunWorkerCompleted += (a, b) => { if (!bw.IsBusy) bw.RunWorkerAsync(); };
bw.RunWorkerAsync();

bool lastIsPlaying = false;
while (true)
{
    bool offFrame = ((lastIsPlaying != isPlaying) && (lastIsPlaying == true));

    if ((isPlaying && SelectedSession != null && !string.IsNullOrEmpty(songName)) || offFrame)
        OSC.SendOSCPacket(new SimpleOSC.OSCMessage { path = "/chatbox/input", arguments = new object[2] { offFrame ? "" : songName, true/*send msg right away*/ }, typeTag = "" }, oscBuf);

    if (lastIsPlaying != isPlaying)
    {
        lastIsPlaying = isPlaying;
    }
    Thread.Sleep(1500);
}
