using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenUtau.Core;

namespace OpenUtau.App.Views {
    public partial class AboutDialog : Window {
        public AboutDialog() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        void OnBilibili(object sender, RoutedEventArgs args) {
            OS.OpenWeb("https://space.bilibili.com/3546682291652746?spm_id_from=333.1007.0.0");
        }
    }
}
