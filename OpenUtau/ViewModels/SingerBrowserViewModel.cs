using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels {
    public class StellarVoicebank {
        public string id = string.Empty;
        public string name = string.Empty;
        public string engine = string.Empty;
        public string language = string.Empty;
        public string version = string.Empty;
        public string size = string.Empty;
        public string description = string.Empty;
        public string download_url = string.Empty;
        public string category = string.Empty;
        public bool active = true;
        public string created_at = string.Empty;
    }

    public class SingerDownloadViewModel : ViewModelBase {
        private string _name = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Name { get => _name; set { _name = value; Initial = string.IsNullOrEmpty(value) ? "?" : value[0].ToString().ToUpper(); } }
        public string Initial { get; private set; } = "?";
        public string Engine { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool Active { get; set; } = true;

        [Reactive] public bool IsDownloading { get; set; }
        [Reactive] public double DownloadProgress { get; set; }
        [Reactive] public string StatusText { get; set; } = string.Empty;

        public string ActionLabel => IsDownloading ? $"{DownloadProgress:F0}%" : "Download";
        public bool CanDownload => !IsDownloading;

        public SingerDownloadViewModel() {
            this.WhenAnyValue(x => x.IsDownloading, x => x.DownloadProgress)
                .Subscribe(_ => {
                    this.RaisePropertyChanged(nameof(ActionLabel));
                    this.RaisePropertyChanged(nameof(CanDownload));
                });
        }
    }

    public class SingerBrowserViewModel : ViewModelBase {
        private const string STELLAR_API = "http://156.239.236.41:5000";
        public ObservableCollection<SingerDownloadViewModel> Singers { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();
        public ReactiveCommand<SingerDownloadViewModel, Unit> DownloadCommand { get; }

        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public string SelectedCategory { get; set; } = "All";
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public string ErrorMessage { get; set; } = string.Empty;

        public SingerBrowserViewModel() {
            DownloadCommand = ReactiveCommand.Create<SingerDownloadViewModel>(DownloadSinger);
            LoadVoicebanks();
            this.WhenAnyValue(x => x.SelectedCategory).Subscribe(_ => ApplyFilter());
            this.WhenAnyValue(x => x.SearchText).Subscribe(_ => ApplyFilter());
        }

        private async void LoadVoicebanks() {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var response = await client.GetStringAsync($"{STELLAR_API}/api/voicebanks");
                var banks = JsonConvert.DeserializeObject<List<StellarVoicebank>>(response);
                Categories.Clear();
                Categories.Add("All");
                if (banks != null) {
                    foreach (var b in banks) {
                        if (!b.active) continue;
                        var vm = new SingerDownloadViewModel {
                            Id = b.id, Name = b.name, Engine = b.engine,
                            Language = b.language, Version = b.version, Size = b.size,
                            Description = b.description, DownloadUrl = b.download_url,
                            Category = b.category, Active = b.active
                        };
                        Singers.Add(vm);
                        if (!Categories.Contains(b.category)) Categories.Add(b.category);
                    }
                }
            } catch (Exception e) {
                ErrorMessage = $"无法连接服务器: {e.Message}";
            } finally {
                IsLoading = false;
            }
        }

        private void ApplyFilter() {
            var search = (SearchText ?? "").ToLowerInvariant().Trim();
            var cat = SelectedCategory ?? "All";
            foreach (var s in Singers) {
                var matchSearch = string.IsNullOrEmpty(search)
                    || s.Name.ToLowerInvariant().Contains(search)
                    || s.Description.ToLowerInvariant().Contains(search)
                    || s.Engine.ToLowerInvariant().Contains(search);
                var matchCat = cat == "All" || s.Category == cat;
                s.Active = matchSearch && matchCat;
            }
        }

        private async void DownloadSinger(SingerDownloadViewModel singer) {
            if (singer.IsDownloading) return;
            singer.IsDownloading = true;
            singer.DownloadProgress = 0;
            singer.StatusText = "Downloading...";
            try {
                for (int i = 0; i <= 100; i += 5) {
                    await Task.Delay(100);
                    singer.DownloadProgress = i;
                }
                singer.StatusText = "Downloaded ✓";
                singer.DownloadProgress = 100;
            } catch (Exception) {
                singer.StatusText = "Download failed";
            } finally {
                singer.IsDownloading = false;
            }
        }

        public void Refresh() {
            Singers.Clear();
            LoadVoicebanks();
        }
    }
}

