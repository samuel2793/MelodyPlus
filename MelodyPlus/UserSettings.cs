using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MelodyPlus
{
    [DataContract]
    public class SizeClass
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Text { get; set; }

        

        public static SizeClass Tiny = new SizeClass(0, "Tiny");
        public static SizeClass Small = new SizeClass(1, "Small");
        public static SizeClass Medium = new SizeClass(2, "Medium");
        public static SizeClass Large = new SizeClass(3, "Large");
        public SizeClass() { }
        public SizeClass(int v1, string v2)
        {
            ID = v1;
            Text = v2;
        }
    }
    public class UserSettings : ReactiveObject
    {
        public static UserSettings Settings = new UserSettings();
        private static UserSettings Default => new UserSettings()
        {
            DarkMode = true,
            //Size = SizeClass.Medium,
            Color = "Green",
            ProgressBarStyle = "Blocks"
        };
        private static string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spotify-Avalonia");
        private static string path = Path.Combine(directory, "Settings.json");
        private bool darkMode;
        private IBrush backColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#111111"));
        private IBrush foreColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#ababab"));
        private IBrush accentColour = new SolidColorBrush(Avalonia.Media.Color.Parse("#ababab"));
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
        //public ObservableCollection<SizeClass> Sizes { get; set; } = new ObservableCollection<SizeClass>(new List<SizeClass> { SizeClass.Tiny, SizeClass.Small, SizeClass.Medium, SizeClass.Large });
        [DataMember]
        public string ProgressBarStyle { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public IBrush BackColour { get => backColour; set => this.RaiseAndSetIfChanged(ref backColour, value); }
        [DataMember]
        public IBrush ForeColour { get => foreColour; set => this.RaiseAndSetIfChanged(ref foreColour, value); }
        public IBrush AccentColour { get => accentColour; set => this.RaiseAndSetIfChanged(ref accentColour, value); }
    }
}
