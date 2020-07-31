using Avalonia.Media;
using MelodyPlus.Localization;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;

namespace MelodyPlus
{
    
    public class UserSettings : ReactiveObject
    {
        public static UserSettings Settings = new UserSettings();
        private static UserSettings Default => new UserSettings()
        {
            DarkMode = true,
            Color = "Green",
            ProgressBarStyle = "Blocks"
        };
        private static string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MelodyPlus");
        private static string path = Path.Combine(directory, "Settings.json");
        private bool darkMode;
        private IBrush backColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#111111"));
        private IBrush foreColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#ababab"));
        private IBrush accentColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#ababab"));
        private CultureInfo culture = CultureInfo.CurrentUICulture;
        private bool progressBar;
        private bool album;
        private bool playlist;
        private bool playlistCode;

        public static void Load()
        {
            if (Directory.Exists(directory) && File.Exists(path))
            {
                Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(path));
            }
            else
            {
                Settings = Default;
            }
        }
        public static void Save()
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(Settings));
        }

        [DataMember]
        public string RefreshToken { get; set; }
        [DataMember]
        public bool DarkMode { get => darkMode; set => this.RaiseAndSetIfChanged(ref darkMode, value); }
        [DataMember]
        public bool ProgressBar { get => progressBar; set => this.RaiseAndSetIfChanged(ref progressBar, value); }
        [DataMember]
        public bool Album { get => album; set => this.RaiseAndSetIfChanged(ref album, value); }
        [DataMember]
        public bool Playlist { get => playlist; set => this.RaiseAndSetIfChanged(ref playlist, value); }
        [DataMember]
        public bool PlaylistCode { get => playlistCode; set => this.RaiseAndSetIfChanged(ref playlistCode, value); }
        [DataMember]
        public string ProgressBarStyle { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public IBrush BackColour { get => backColour; set => this.RaiseAndSetIfChanged(ref backColour, value); }
        [DataMember]
        public IBrush ForeColour { get => foreColour; set => this.RaiseAndSetIfChanged(ref foreColour, value); }
        public IBrush AccentColour { get => accentColour; set => this.RaiseAndSetIfChanged(ref accentColour, value); }

        public CultureInfo Culture
        {
            get => culture; set
            {
                this.RaiseAndSetIfChanged(ref culture, value);
                if (value != null)
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                    Translator.Instance.Invalidate();
                }
            }
        }

        public List<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo> { CultureInfo.GetCultureInfo("en-GB"), CultureInfo.GetCultureInfo("fr-FR"), CultureInfo.GetCultureInfo("ko-KR") };
    }
}
