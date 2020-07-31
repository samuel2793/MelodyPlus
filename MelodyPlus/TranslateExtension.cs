using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;

namespace MelodyPlus.Localization
{
    public class TranslateExtension : MarkupExtension
    {
        public TranslateExtension(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }

        public string FallBack { get; set; }

        public string Context { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var keyToUse = Key;
            if (!string.IsNullOrWhiteSpace(Context))
            {
                keyToUse = $"{Context}/{Key}";
            }

            var binding = new Binding($"[{keyToUse}]", BindingMode.OneWay)
            {
                
                Source = Translator.Instance,
            };

            return binding;
        }
    }
    public class Translator : INotifyPropertyChanged
    {
        private const string IndexerName = "Item";
        private const string IndexerArrayName = "Item[]";

        public string this[string key]
        {
            get
            {
                var result = Resources.MainWindow.ResourceManager.GetString(key);
                if(result == null)
                {
                    var culture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
                    result = Resources.MainWindow.ResourceManager.GetString(key);
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                return result;

                
            }
        }

        public static Translator Instance { get; set; } = new Translator();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
        }
    }
}
