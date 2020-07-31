﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using MelodyPlus;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MelodyPlus
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            UserSettings.Load();
            DataContext = UserSettings.Settings;
            InitializeComponent();

            //var sizeCombo = this.FindControl<ComboBox>("size");
            //sizeCombo.SelectedIndex = UserSettings.Settings.Size.ID;
            Closed += (a,e) => viewer?.Close();
            //new Browser().Show();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var location = e.GetPosition(this);
                if (location.Y <= 15)
                {
                    BeginMoveDrag(e);
                }
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }
        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        Viewer viewer;
        private async void OnButtonClick(object sender, RoutedEventArgs e)
        {
            viewer?.Close();
            viewer = new Viewer();
            //viewer.SetSize(UserSettings.Settings.Size);
            await viewer.ConnectToSpotify();
            viewer.Show();
            viewer.UpdateImageHeight();
            await viewer.CheckTrack();
        }
        private void SettingChanged(object sender,RoutedEventArgs e)
        {
            UserSettings.Save();
            Task.Delay(1).ContinueWith(t => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => viewer?.UpdateImageHeight(), Avalonia.Threading.DispatcherPriority.Loaded));
        }
        private void DarkModeOn(object sender, RoutedEventArgs e)
        {
            if (viewer != null)
            {
                viewer.Model.BackColour = new SolidColorBrush(Color.Parse("#111111"));
                viewer.Model.ForeColour = new SolidColorBrush(Color.Parse("#ababab"));
                if (viewer.QRCode != null)
                {
                    viewer.Model.PlaylistCode = Helpers.GetBitmapFromImage(viewer.QRCode.GetGraphic(6, System.Drawing.Color.FromArgb(171, 171, 171), System.Drawing.Color.FromArgb(17, 17, 17), false));
                }
            }
            UserSettings.Settings.BackColour = new SolidColorBrush(Color.Parse("#111111"));
            UserSettings.Settings.ForeColour = new SolidColorBrush(Color.Parse("#ababab"));
            UserSettings.Settings.DarkMode = true;
            UserSettings.Save();
        }
        private void LightModeOn(object sender, RoutedEventArgs e)
        {
            if (viewer != null)
            {
                viewer.Model.BackColour = new SolidColorBrush(Color.Parse("#ffffff"));
                viewer.Model.ForeColour = new SolidColorBrush(Color.Parse("#404040"));
                if (viewer.QRCode != null)
                {
                    viewer.Model.PlaylistCode = Helpers.GetBitmapFromImage(viewer.QRCode.GetGraphic(6, System.Drawing.Color.FromArgb(64, 64, 64), System.Drawing.Color.FromArgb(255, 255, 255), false));
                }
            }
            UserSettings.Settings.BackColour = new SolidColorBrush(Color.Parse("#ffffff"));
            UserSettings.Settings.ForeColour = new SolidColorBrush(Color.Parse("#404040"));
            UserSettings.Settings.DarkMode = false;
            UserSettings.Save();
        }
    }
}