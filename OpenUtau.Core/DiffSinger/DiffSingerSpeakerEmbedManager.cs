using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using Serilog;

using OpenUtau.Core.Render;

namespace OpenUtau.Core.DiffSinger
{
    public class DiffSingerSpeakerEmbedManager
    {
        DsConfig dsConfig;
        string rootPath;
        public NDArray speakerEmbeds = null;
        const string VoiceColorHeader = DiffSingerUtils.VoiceColorHeader;

        public DiffSingerSpeakerEmbedManager(DsConfig dsConfig, string rootPath) {
            this.dsConfig = dsConfig;
            this.rootPath = rootPath;
        }
        public NDArray loadSpeakerEmbed(string speaker) {
            string path = Path.Join(rootPath, speaker + ".emb");
            if(!File.Exists(path)) {
                throw new FileNotFoundException($"Speaker embed file {path} not found");
            }
            using var reader = new BinaryReader(File.OpenRead(path));
            var fileSize = reader.BaseStream.Length;
            var expectedSize = dsConfig.hiddenSize * 4L;
            int actualDim;
            if (fileSize != expectedSize) {
                actualDim = (int)(fileSize / 4L);
                Log.Warning("Speaker embed file \"{0}\" has {1} floats ({2} bytes), but dsConfig.hiddenSize is {3}. " +
                    "Using actual file dimension {1}. This usually means the singer or subbank is from a different model version.",
                    path, actualDim, fileSize, dsConfig.hiddenSize);
            } else {
                actualDim = dsConfig.hiddenSize;
            }
            return np.array<float>(Enumerable.Range(0, actualDim)
                .Select(i => reader.ReadSingle()));
        }

        public NDArray getSpeakerEmbeds() {
            if(speakerEmbeds == null) {
                if(dsConfig.speakers == null) {
                    return null;
                }
                try {
                    var firstEmbed = loadSpeakerEmbed(dsConfig.speakers[0]);
                    int actualDim = firstEmbed.Shape[0];
                    var embeds = np.zeros<float>(actualDim, dsConfig.speakers.Count);
                    embeds[":", 0] = firstEmbed;
                    foreach(var spkId in Enumerable.Range(1, dsConfig.speakers.Count - 1)) {
                        var embed = loadSpeakerEmbed(dsConfig.speakers[spkId]);
                        if (embed.Shape[0] != actualDim) {
                            Log.Warning("Speaker \"{0}\" has dimension {1}, expected {2}. Padding with zeros.",
                                dsConfig.speakers[spkId], embed.Shape[0], actualDim);
                            for (int j = 0; j < Math.Min(embed.Shape[0], actualDim); j++) {
                                embeds[j, spkId] = embed[j];
                            }
                        } else {
                            embeds[":", spkId] = embed;
                        }
                    }
                    speakerEmbeds = embeds;
                } catch (Exception e) {
                    Log.Error(e, "Failed to load speaker embeddings for singer.");
                    return null;
                }
            }
            return speakerEmbeds;
        }

        public bool IsVoiceColorCurve(string abbr, out int subBankId) {
            subBankId = 0;
            if (abbr.StartsWith(VoiceColorHeader) && int.TryParse(abbr.Substring(2), out subBankId)) {;
                subBankId -= 1;
                return true;
            } else {
                return false;
            }
        }

        public int getSpeakerIndexBySuffix(string suffix) {
            var speakerIndex = dsConfig.speakers.IndexOf(suffix);
            if (speakerIndex >= 0) {
                return speakerIndex;
            }
            speakerIndex = dsConfig.speakers.FindIndex(s => {
                var spSegs = s.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var sfSegs = suffix.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return sfSegs.Length <= spSegs.Length
                    && spSegs[^sfSegs.Length..].SequenceEqual(sfSegs);
            });
            if (speakerIndex >= 0) {
                return speakerIndex;
            }
            if (dsConfig.speakers.Count == 0) {
                throw new InvalidOperationException(
                    "Subbanks are defined in character.yaml but \"speakers\" is empty in dsconfig.yaml.");
            }
            Log.Warning(
                $"Speaker suffix \"{suffix}\" not found in dsConfig.speakers, falling back to first speaker. " +
                $"Candidates: {string.Join(',', dsConfig.speakers)}.");
            return 0;
        }

        //used by phonemizer (duration model)
        public Tensor<float> PhraseSpeakerEmbedByPhone(string[] speakerByPhone){
            var speakerEmbeds = getSpeakerEmbeds();
            if (speakerEmbeds == null) {
                return new DenseTensor<float>(new float[0], new int[] { 1, speakerByPhone.Length, 0 });
            }
            var actualDim = speakerEmbeds.Shape[0];
            var totalPhones = speakerByPhone.Length;
            NDArray spkCurves = np.zeros<float>(totalPhones, dsConfig.speakers.Count);
            foreach(int phoneId in Enumerable.Range(0,totalPhones)) {
                var spkId = getSpeakerIndexBySuffix(speakerByPhone[phoneId]);
                spkCurves[phoneId, spkId] = 1;
            }
            var spkEmbedResult = np.dot(spkCurves, speakerEmbeds.T);
            var spkEmbedTensor = new DenseTensor<float>(spkEmbedResult.ToArray<float>(), 
                new int[] { totalPhones, actualDim })
                .Reshape(new int[] { 1, totalPhones, actualDim });
            return spkEmbedTensor;
        }

        //used by variance, pitch and acoustic
        public Tensor<float> PhraseSpeakerEmbedByFrame(RenderPhrase phrase, IList<int> durations, float frameMs, int totalFrames, int headFrames, int tailFrames){
            var singer = phrase.singer;
            var speakerEmbeds = getSpeakerEmbeds();
            if (speakerEmbeds == null) {
                return new DenseTensor<float>(new float[0], new int[] { 1, totalFrames, 0 });
            }
            var actualDim = speakerEmbeds.Shape[0];
            var headDefaultSpk = getSpeakerIndexBySuffix(phrase.phones[0].suffix);
            var tailDefaultSpk = getSpeakerIndexBySuffix(phrase.phones[^1].suffix);
            var defaultSpkByFrame = Enumerable.Repeat(headDefaultSpk, headFrames).ToList();
            defaultSpkByFrame.AddRange(Enumerable.Range(0, phrase.phones.Length)
                .SelectMany(phIndex => Enumerable.Repeat(getSpeakerIndexBySuffix(phrase.phones[phIndex].suffix), durations[phIndex+1])));
            defaultSpkByFrame.AddRange(Enumerable.Repeat(tailDefaultSpk, tailFrames));
            NDArray spkCurves = np.zeros<float>(totalFrames, dsConfig.speakers.Count);
            foreach(var curve in phrase.curves) {
                if(singer.Subbanks != null && IsVoiceColorCurve(curve.Item1,out int subBankId) && subBankId < singer.Subbanks.Count) {
                    var spkId = getSpeakerIndexBySuffix(singer.Subbanks[subBankId].Suffix);
                    spkCurves[":", spkId] += DiffSingerUtils.SampleCurve(phrase, curve.Item2, 0, 
                        frameMs, totalFrames, headFrames, tailFrames, x => x * 0.01f)
                        .Select(f => (float)f).ToArray();
                }
            }
            foreach(int frameId in Enumerable.Range(0,totalFrames)) {
                var spkSum = spkCurves[frameId, ":"].ToArray<float>().Sum();
                if (spkSum > 1) {
                    spkCurves[frameId, ":"] /= spkSum;
                } else {
                    spkCurves[frameId, defaultSpkByFrame[frameId]] += 1 - spkSum;
                }
            }
            var spkEmbedResult = np.dot(spkCurves, speakerEmbeds.T);
            var spkEmbedTensor = new DenseTensor<float>(spkEmbedResult.ToArray<float>(), 
                new int[] { totalFrames, actualDim })
                .Reshape(new int[] { 1, totalFrames, actualDim });
            return spkEmbedTensor;
        }
    }
}
