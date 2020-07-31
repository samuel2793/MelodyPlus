using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MelodyPlus
{
    public class Browser : Window
    {
        public Browser()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
