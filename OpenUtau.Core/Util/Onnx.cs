using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.Core {
    public class GpuInfo {
        public int deviceId;
        public string description = "";

        override public string ToString() {
            return $"[{deviceId}] {description}";
        }
    }

    public enum OnnxRunnerChoice {
        Default,
        CPU,
        CPUForCoreML,
    }

    public class Onnx {
        private static readonly ConcurrentDictionary<int, OrtEpDevice> devices = new();
        private static volatile bool dmlDisabled;
        private static volatile bool dmlEnumerated;

        private static void ensureDmlEnumerated() {
            if (dmlEnumerated || dmlDisabled) return;
            lock (devices) {
                if (dmlEnumerated || dmlDisabled) return;
                try {
                    var env = OrtEnv.Instance();
                    var ortDevices = env.GetEpDevices();
                    int i = 0;
                    foreach (var device in ortDevices.Where(d => d.EpName.ToLower().Contains("dml"))) {
                        devices[i++] = device;
                    }
                    if (devices.Count == 0) {
                        Log.Warning("DirectML: no GPU devices found. DirectML will be unavailable.");
                        dmlDisabled = true;
                    } else {
                        Log.Information("DirectML: found {0} GPU device(s) including [{1}] {2}",
                            devices.Count,
                            devices.TryGetValue(0, out var d0) ? d0.HardwareDevice.Type : "?",
                            getGpuDescription(devices[0]));
                    }
                } catch (Exception e) {
                    Log.Warning(e, "DirectML: device enumeration failed. DirectML will be unavailable.");
                    dmlDisabled = true;
                }
                dmlEnumerated = true;
            }
        }

        private static string getGpuDescription(OrtEpDevice device) {
            try {
                foreach (var item in device.HardwareDevice.Metadata.Entries) {
                    if (item.Key.ToLower() == "description")
                        return item.Value;
                }
            } catch { }
            return $"{device.HardwareDevice.Vendor} ({device.HardwareDevice.Type})";
        }

        public static List<string> getRunnerOptions() {
            if (OS.IsWindows()) {
                return new List<string> {
                "CPU",
                "DirectML"
                };
            } else if (OS.IsMacOS()) {
                return new List<string> {
                "CPU",
                "CoreML"
                };
            } else if (OS.IsAndroid()) {
                return new List<string> {
                "CPU",
                "NNAPI"
                };
            }
            return new List<string> {
                "CPU"
            };
        }

        public static List<GpuInfo> getGpuInfo() {
            if (OS.IsAndroid()) {
                return new List<GpuInfo>{new GpuInfo {
                    deviceId = 0,
                }};
            }
            List<GpuInfo> gpuList = new List<GpuInfo>();
            try {
                var env = OrtEnv.Instance();
                var ortDevices = env.GetEpDevices();

                int i = 0;
                foreach (var device in ortDevices.Where(d => d.EpName.ToLower().Contains("dml"))) {
                    var description = getGpuDescription(device);
                    devices[i] = device;
                    gpuList.Add(new GpuInfo {
                        deviceId = i++,
                        description = description
                    });
                }
                if (gpuList.Count > 0) {
                    dmlEnumerated = true;
                    dmlDisabled = false;
                }
            } catch (Exception e) {
                Log.Warning(e, "Failed to enumerate GPU info, DirectML may be unavailable.");
            }
            if (gpuList.Count == 0) {
                gpuList.Add(new GpuInfo {
                    deviceId = 0,
                });
            }
            return gpuList;
        }

        private static SessionOptions getOnnxSessionOptions(bool coremlEnableOnSubgraphs = false) {
            SessionOptions options = new SessionOptions();
            List<string> runnerOptions = getRunnerOptions();
            string runner = Preferences.Default.OnnxRunner;
            if (String.IsNullOrEmpty(runner)) {
                runner = runnerOptions[0];
            }
            if (!runnerOptions.Contains(runner)) {
                runner = "CPU";
            }
            switch (runner) {
                case "DirectML":
                    ensureDmlEnumerated();
                    if (dmlDisabled || devices.Count == 0) {
                        Log.Information("DirectML unavailable, using CPU.");
                        break;
                    }
                    if (devices.TryGetValue(Preferences.Default.OnnxGpu, out var d)) {
                        options.AppendExecutionProvider(
                            OrtEnv.Instance(),
                            new List<OrtEpDevice> { d },
                            new Dictionary<string, string> { }
                        );
                    } else {
                        Log.Warning("DirectML device {0} not found in {1} available devices, using CPU",
                            Preferences.Default.OnnxGpu, devices.Count);
                    }
                    break;
                case "CoreML":
                    options.AppendExecutionProvider("CoreML", new Dictionary<string, string> {
                        { "MLComputeUnits", "ALL" },
                        { "RequireStaticInputShapes", "1"},
                        { "ModelFormat", "NeuralNetwork"},
                        { "EnableOnSubgraphs", coremlEnableOnSubgraphs ? "1" : "0" }
                    });
                    break;
                case "NNAPI":
                    options.AppendExecutionProvider_Nnapi();
                    break;
            }
            return options;
        }

        private static InferenceSession createCpuSession(byte[] model) {
            var options = new SessionOptions();
            options.AppendExecutionProvider_CPU();
            return new InferenceSession(model, options);
        }

        private static InferenceSession createCpuSession(string modelPath) {
            var options = new SessionOptions();
            options.AppendExecutionProvider_CPU();
            return new InferenceSession(modelPath, options);
        }

        public static InferenceSession getInferenceSession(byte[] model, OnnxRunnerChoice runnerChoice = OnnxRunnerChoice.Default) {
            if (runnerChoice == OnnxRunnerChoice.CPU ||
                (runnerChoice == OnnxRunnerChoice.CPUForCoreML && Preferences.Default.OnnxRunner == "CoreML")) {
                return createCpuSession(model);
            }

            if (OS.IsMacOS() && Preferences.Default.OnnxRunner == "CoreML") {
                try {
                    return new InferenceSession(model, getOnnxSessionOptions(coremlEnableOnSubgraphs: true));
                } catch (Exception e) {
                    Log.Warning(e, "Failed to create CoreML session with subgraphs, falling back");
                }
            }

            try {
                return new InferenceSession(model, getOnnxSessionOptions());
            } catch (Exception e) {
                Log.Warning(e, "Failed to create session with {0}, falling back to CPU", Preferences.Default.OnnxRunner);
                if (Preferences.Default.OnnxRunner == "DirectML") {
                    dmlDisabled = true;
                    Log.Warning("Disabling DirectML for the rest of this session. Restart to retry.");
                }
                return createCpuSession(model);
            }
        }

        public static InferenceSession getInferenceSession(string modelPath, OnnxRunnerChoice runnerChoice = OnnxRunnerChoice.Default) {
            if (runnerChoice == OnnxRunnerChoice.CPU ||
                (runnerChoice == OnnxRunnerChoice.CPUForCoreML && Preferences.Default.OnnxRunner == "CoreML")) {
                return createCpuSession(modelPath);
            }

            if (OS.IsMacOS() && Preferences.Default.OnnxRunner == "CoreML") {
                try {
                    return new InferenceSession(modelPath, getOnnxSessionOptions(coremlEnableOnSubgraphs: true));
                } catch (Exception e) {
                    Log.Warning(e, "Failed to create CoreML session with subgraphs, falling back");
                }
            }

            try {
                return new InferenceSession(modelPath, getOnnxSessionOptions());
            } catch (Exception e) {
                Log.Warning(e, "Failed to create session with {0}, falling back to CPU", Preferences.Default.OnnxRunner);
                if (Preferences.Default.OnnxRunner == "DirectML") {
                    dmlDisabled = true;
                    Log.Warning("Disabling DirectML for the rest of this session. Restart to retry.");
                }
                return createCpuSession(modelPath);
            }
        }

        public static void VerifyInputNames(InferenceSession session, IEnumerable<NamedOnnxValue> inputs) {
            var sessionInputNames = session.InputNames.ToHashSet();
            var givenInputNames = inputs.Select(v => v.Name).ToHashSet();
            var missing = sessionInputNames
                .Except(givenInputNames)
                .OrderBy(s => s, StringComparer.InvariantCulture)
                .ToArray();
            if (missing.Length > 0) {
                throw new ArgumentException("Missing input(s) for the inference session: " + string.Join(", ", missing));
            }
            var unexpected = givenInputNames
                .Except(sessionInputNames)
                .OrderBy(s => s, StringComparer.InvariantCulture)
                .ToArray();
            if (unexpected.Length > 0) {
                throw new ArgumentException("Unexpected input(s) for the inference session: " + string.Join(", ", unexpected));
            }
        }
    }
}
