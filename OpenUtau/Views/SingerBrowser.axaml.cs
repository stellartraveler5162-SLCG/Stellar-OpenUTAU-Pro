using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenUtau.App.ViewModels;

namespace OpenUtau.App.Views {
    public partial class SingerBrowser : Window {
        public SingerBrowser() {
            InitializeComponent();
            DataContext = new SingerBrowserViewModel();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnRefresh(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            if (DataContext is SingerBrowserViewModel vm) {
                vm.Refresh();
            }
        }
    }
}
