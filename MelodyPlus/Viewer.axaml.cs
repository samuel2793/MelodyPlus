using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Threading.Tasks;
using System;
using DominantColor;
using ReactiveUI;
using Avalonia.Input;
using Avalonia.Media;
using System.Linq;
using QRCoder;
using System.IO;

namespace MelodyPlus
{
    public class ViewerViewModel : ReactiveObject
    {
        private string track = "<< Track >>";
        private string artist = "<< Artist >>";
        private string album = "<< Album >>";
        private string playlist = "<< Playlist >>";
        private string progressText = "∞/∞";
        private Avalonia.Media.Imaging.IBitmap albumCover;
        private Avalonia.Media.Imaging.IBitmap playlistCode;
        private IBrush titleColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#ff0000"));
        private IBrush accentColourFaded = new SolidColorBrush(Avalonia.Media.Color.Parse("#ff0000"));
        private IBrush backColour = UserSettings.Settings.BackColour;
        private IBrush foreColour = UserSettings.Settings.ForeColour;
        private int trackLength;
        private int currentPosition;
        private UserSettings settings = UserSettings.Settings;
        private Avalonia.Input.Cursor cursor = Avalonia.Input.Cursor.Parse("Hand");

        public string Track { get => track; set => this.RaiseAndSetIfChanged(ref track, value); }
        public string Artist { get => artist; set => this.RaiseAndSetIfChanged(ref artist, value); }
        public string Album { get => album; set => this.RaiseAndSetIfChanged(ref album, value); }
        public string Playlist { get => playlist; set => this.RaiseAndSetIfChanged(ref playlist, value); }
        public string ProgressText { get => progressText; set => this.RaiseAndSetIfChanged(ref progressText, value); }
        public int TrackLength
        {
            get => trackLength; set
            {
                this.RaiseAndSetIfChanged(ref trackLength, value);
                ProgressText = $"{TimeSpan.FromMilliseconds(CurrentPosition):m\\:ss}/{TimeSpan.FromMilliseconds(TrackLength):m\\:ss}";
            }
        }
        public int CurrentPosition
        {
            get => currentPosition; set
            {
                this.RaiseAndSetIfChanged(ref currentPosition, value);
                ProgressText = $"{TimeSpan.FromMilliseconds(CurrentPosition):m\\:ss}/{TimeSpan.FromMilliseconds(TrackLength):m\\:ss}";
            }
        }
        public Avalonia.Media.Imaging.IBitmap AlbumCover { get => albumCover; set => this.RaiseAndSetIfChanged(ref albumCover, value); }
        public Avalonia.Media.Imaging.IBitmap PlaylistCode { get => playlistCode; set => this.RaiseAndSetIfChanged(ref playlistCode, value); }
        public IBrush BackColour { get => backColour; set => this.RaiseAndSetIfChanged(ref backColour, value); }
        public IBrush ForeColour { get => foreColour; set => this.RaiseAndSetIfChanged(ref foreColour, value); }
        public IBrush AccentColour { get => titleColour; set => this.RaiseAndSetIfChanged(ref titleColour, value); }
        public IBrush AccentColourFaded { get => accentColourFaded; set => this.RaiseAndSetIfChanged(ref accentColourFaded, value); }
        public UserSettings Settings { get => settings; set => this.RaiseAndSetIfChanged(ref settings, value); }
        public Avalonia.Input.Cursor Cursor { get => cursor; set => this.RaiseAndSetIfChanged(ref cursor, value); }
    }
    public class Viewer : Window
    {
        //public Color TitleColour { get; set; }
        public ViewerViewModel Model;
        public Viewer()
        {
            this.InitializeComponent();
            DataContext = Model = new ViewerViewModel();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        internal void UpdateImageHeight()
        {

            var info = this.FindControl<StackPanel>("info");
            var albumCover = this.FindControl<Avalonia.Controls.Image>("albumCover");
            var playlistCode = this.FindControl<Avalonia.Controls.Image>("playlistCode");
            albumCover.Height = info.DesiredSize.Height;
            playlistCode.Height = info.DesiredSize.Height;
        }

        private static int borderWidth = 5;
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var location = e.GetPosition(this);
                if (location.Y <= borderWidth)
                {
                    BeginMoveDrag(e);
                }
            }
        }

        private Token lastToken;
        private static SpotifyWebAPI _spotify;
        private PlaybackContext _playback;
        private string _currentTrackId;
        public PngByteQRCode QRCode;
        private string LastPlaylist;
        private System.Threading.CancellationTokenSource authready = new System.Threading.CancellationTokenSource();
        public static string ExchangeServerURI;
        public async Task ConnectToSpotify()
        {
            TokenSwapAuth auth = new TokenSwapAuth(
            exchangeServerUri: ExchangeServerURI,
            serverUri: "http://localhost:4002",
            scope: Scope.UserReadPlaybackState
            )
            {
                TimeAccessExpiry = true
            };

            if (!string.IsNullOrWhiteSpace(UserSettings.Settings.RefreshToken))
            {
                lastToken = await auth.RefreshAuthAsync(UserSettings.Settings.RefreshToken);
                Console.WriteLine("Token Gotten");
                if (lastToken == null)
                {
                    Console.WriteLine("Token is null? O_o");
                }
                lastToken.RefreshToken = UserSettings.Settings.RefreshToken;
                Console.WriteLine("Set Refresh Token Gotten");
                _spotify = new SpotifyWebAPI()
                {
                    TokenType = lastToken.TokenType,
                    AccessToken = lastToken.AccessToken
                };
                auth.Start();

            }
            else
            {
                auth.AuthReceived += async (sender, response) =>
                {
                    lastToken = await auth.ExchangeCodeAsync(response.Code);

                    _spotify = new SpotifyWebAPI()
                    {
                        TokenType = lastToken.TokenType,
                        AccessToken = lastToken.AccessToken
                    };
                    UserSettings.Settings.RefreshToken = lastToken.RefreshToken;
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => UserSettings.Save());

                    authready.Cancel();
                };
                auth.Start();
                //Open Browser until get embeded browser on .net core cross platform
                auth.OpenBrowser();
                //Forms.Browser.Navigate(auth.GetUri());
                //Forms.Browser.ShowDialog();
            }
            auth.OnAccessTokenExpired += async (sender, e) =>
            {
                _spotify.AccessToken = (await auth.RefreshAuthAsync(lastToken.RefreshToken)).AccessToken;

            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public static string RRGGBBAAToAARRGGBB(string s1)
        {
            return s1.Substring(s1.Length - 2) + s1.Remove(s1.Length - 2);
        }
        public async Task CheckTrack()
        {
            while (true)
            {

                if (_spotify is null)
                {
                    await Task.Delay(-1, authready.Token).ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled);
                    continue;
                }
                try
                {
                    _playback = _spotify.GetPlayback();
                }
                catch (Exception)
                {
                    await Task.Delay(5000);
                    continue;
                }
                // With HasError() you can check the error that got back of the spotify API.
                if (_playback.HasError())
                {
                    // Logger.Log(2, "Error Status: " + _playback.Error.Status + " Msg: " + _playback.Error.Message);
                    await Task.Delay(5000);
                    continue;
                }

                if (_playback.Item is null)
                {
                    await Task.Delay(5000);
                    continue;
                }
                if ((_currentTrackId ?? "") == (_playback.Item.Uri ?? ""))
                {
                    Model.CurrentPosition = _playback.ProgressMs;
                    await Task.Delay(1000);
                    continue;
                }
                _currentTrackId = _playback.Item.Uri;
                Model.Track = _playback.Item.Name;
                Model.TrackLength = _playback.Item.DurationMs;
                Model.Artist = "";
                foreach (SimpleArtist artist in _playback.Item.Artists)
                {
                    if (string.IsNullOrEmpty(Model.Artist))
                    {
                        Model.Artist = artist.Name;
                    }
                    else
                    {
                        Model.Artist = Model.Artist + " - " + artist.Name;
                    }
                }
                Model.Album = _playback.Item.Album.Name;
                if (_playback.Item.Album.Images.Count > 0)
                {

                    var albumCover = Helpers.GetImageFromUri(_playback.Item.Album.Images[1].Url);
                    Model.AlbumCover = _playback.Item.Album.Images[0].Url is object ? Helpers.GetBitmapFromImage(albumCover) : null;
                    if (Model.AlbumCover != null)
                    {
                        var hueColorCalculator = new DominantHueColorCalculator();
                        SixLabors.ImageSharp.Color hueColor = hueColorCalculator.CalculateDominantColor(albumCover);
                        UserSettings.Settings.AccentColour = Model.AccentColour = new SolidColorBrush(Avalonia.Media.Color.Parse($"#{RRGGBBAAToAARRGGBB(hueColor.ToHex())}"));                        
                        Model.AccentColourFaded = new SolidColorBrush(Avalonia.Media.Color.Parse($"#{RRGGBBAAToAARRGGBB(hueColor.WithAlpha(0.4f).ToHex())}"));
                    }

                }
                else
                {
                    Model.AlbumCover = null;
                }
                if ((_playback.Context?.Type ?? "") == "playlist")
                {
                    string playlistID = _playback.Context.Uri.Split(':').Last();
                    var playlist = _spotify.GetPlaylist(playlistID);
                    if ((playlistID ?? "") != (LastPlaylist ?? ""))
                    {
                        Model.Playlist = playlist.Name;
                        if (playlist.Public)
                        {
                            var payload = new PayloadGenerator.Url(playlist.ExternalUrls["spotify"]);
                            var qrgenerator = new QRCodeGenerator();
                            var qrcodeData = qrgenerator.CreateQrCode(payload.ToString(), QRCodeGenerator.ECCLevel.Q);
                            QRCode = new PngByteQRCode(qrcodeData);
                            if (UserSettings.Settings.DarkMode)
                            {
                                var png = QRCode.GetGraphic(6, new byte[4] { 171, 171, 171, 255 }, new byte[4] { 17, 17, 17, 255 });
                                using var stream = new MemoryStream(png);
                                stream.Seek(0, SeekOrigin.Begin);

                                Model.PlaylistCode = new Avalonia.Media.Imaging.Bitmap(stream);
                            }
                            else
                            {
                                var png = QRCode.GetGraphic(6, new byte[4] { 64,64,64, 255 },new byte[4] { 255,255,255, 255 });
                                using var stream = new MemoryStream(png);
                                stream.Seek(0, SeekOrigin.Begin);

                                Model.PlaylistCode = new Avalonia.Media.Imaging.Bitmap(stream);
                            }
                            LastPlaylist = playlistID;
                        }
                        else
                        {
                            Model.PlaylistCode = null;
                            LastPlaylist = null;
                            QRCode = null;
                        }
                    }
                }
                else
                {
                    QRCode = null;
                    LastPlaylist = null;
                    Model.Playlist = string.Empty;
                    Model.PlaylistCode = null;
                }
            }
        }

    }
}
