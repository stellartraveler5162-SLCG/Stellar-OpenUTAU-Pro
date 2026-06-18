using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace OpenUtau.Core.Util {
    public static class DependencyCheckService {
        private static readonly string[] RequiredDependencies = new[] {
            "pc_nsf_hifigan_44.1k_hop512_128bin_2025.02",
        };

        public static async Task CheckAndDownloadAsync(IProgress<string>? progress = null) {
            try {
                var registry = await PackageManager.Inst.FetchRegistryAsync().ConfigureAwait(false);
                foreach (var depId in RequiredDependencies) {
                    if (PackageManager.Inst.GetInstalledPath(depId) != null) {
                        progress?.Report($"✓ {depId}");
                        continue;
                    }
                    var bundledPath = Path.Combine(PathManager.Inst.RootPath, "Dependencies", depId);
                    if (Directory.Exists(bundledPath) && File.Exists(Path.Combine(bundledPath, "vocoder.yaml"))) {
                        progress?.Report($"✓ {depId} (bundled)");
                        continue;
                    }
                    var software = registry.FirstOrDefault(s => s.id == depId);
                    if (software == null) {
                        Log.Warning($"Dependency {depId} not found in registry");
                        progress?.Report($"⚠ {depId} not found");
                        continue;
                    }
                    progress?.Report($"↓ Downloading {depId}...");
                    try {
                        await PackageManager.Inst.InstallAsync(software, null).ConfigureAwait(false);
                        progress?.Report($"✓ {depId}");
                    } catch (Exception e) {
                        Log.Error(e, $"Failed to download dependency {depId}");
                        progress?.Report($"✗ {depId} failed");
                    }
                }
            } catch (Exception e) {
                Log.Error(e, "Dependency check failed");
                progress?.Report("⚠ Network unavailable, skipping dependency check");
            }
        }
    }
}
