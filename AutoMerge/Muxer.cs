using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMerge
{
    internal class Muxer
    {
        internal class MuxingConfiguration
        {
            public string MainDirectory { get; set; }
            public VideoSourceType VideoSourceType { get; set; }
            public List<AudioSourceType> AudioSourceTypes { get; set; }
            public OutputType OutputType { get; set; }
            public string VideoFps { get; set; }
            public string AudioLanguage { get; set; }
            public string SubtitleLanguage { get; set; }
            public bool MuxChapter { get; set; }
            public bool MuxSubtitle { get; set; }
        }

        internal void StartMux(MuxingConfiguration configuration)
        {
            string videoFileExtension = Settings.VideoSourceFileTypes[configuration.VideoSourceType].FileExtension;
            var videoFiles = EnumerateFiles($"*{videoFileExtension}", configuration.MainDirectory);
            var episodes = GenerateEpisodes(videoFiles, configuration);

            var mergeParameters = GenerateMergeParameters(episodes);

            Parallel.ForEach(mergeParameters, parameter => {
                string mainProcess = null;
                switch (parameter.Item1) {
                    case OutputType.Mkv:
                        mainProcess = Settings.MkvMergeFilePath;
                        break;
                    case OutputType.Mp4:
                        mainProcess = Settings.Mp4MuxerFilePath;
                        break;
                    default:
                        return;
                }

                Console.WriteLine($"{mainProcess} {parameter.Item2}\n");
                //StartProcess(mainProcess, parameter.Item2);
            });
        }

        private List<string> EnumerateFiles(string filePattern, string directory = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFiles(
                (string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : directory),
                filePattern,
                searchOption
            ).ToList();
        }

        private void StartProcess(string filename, string arguments, bool waitForExit = true, bool hidden = false, bool redirectStdout = false)
        {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = true
            };

            if (hidden) {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }
            if (redirectStdout) {
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
            }

            Process p = new Process() {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            p.Start();
            if (waitForExit)
                p.WaitForExit();
        }
        
        private List<Episode> GenerateEpisodes(List<string> videoFiles, MuxingConfiguration configuration)
        {
            var episodes = new List<Episode>();

            foreach (var file in videoFiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                string outputFile = baseName + Settings.OutputFileTypes[configuration.OutputType].FileExtension;
                string videoFile = file;
                string chapterFile = baseName + Settings.ChapterFileExtension;
                var audioFiles = new List<string>();
                var subtitleFiles = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                if (configuration.AudioSourceTypes != null && configuration.AudioSourceTypes.Count > 0) {
                    foreach (var audioType in configuration.AudioSourceTypes) {
                        string audioFileExtension = Settings.AudioSourceFileTypes[audioType].FileExtension;
                        audioFiles.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*{audioFileExtension}", directory, SearchOption.TopDirectoryOnly));
                    }
                    if (0 == audioFiles.Count) continue;
                }
                
                if (configuration.MuxSubtitle) {
                    subtitleFiles.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*{Settings.SubtitleFileExtension}", directory, SearchOption.TopDirectoryOnly));
                }

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = configuration.VideoFps,
                    OutputFile = outputFile,
                    OutputFileType = configuration.OutputType,
                    ChapterFile = File.Exists(chapterFile) && configuration.MuxChapter ? chapterFile : null,
                    AudioFiles = audioFiles,
                    AudioLanguage = configuration.AudioLanguage,
                    SubtitleFiles = subtitleFiles,
                    SubtitleLanguage = configuration.SubtitleLanguage
                };
                episodes.Add(ep);
            }

            return episodes;
        }
        
        private List<Tuple<OutputType, string>> GenerateMergeParameters(List<Episode> episodes)
        {
            var mergeParameters = new List<Tuple<OutputType, string>>();

            foreach (var ep in episodes) {
                string parameter;
                switch (ep.OutputFileType) {
                    case OutputType.Mkv:
                        parameter = GenerateMkvMergeParameter(ep);
                        break;
                    case OutputType.Mp4:
                        parameter = GenerateMp4MergeParameter(ep);
                        break;
                    default:
                        continue;
                }
                mergeParameters.Add(Tuple.Create(ep.OutputFileType, parameter));
            }

            return mergeParameters;
        }

        private string GenerateMkvMergeParameter(Episode episode)
        {
            string trackTemplate = "--language 0:{0} \"(\" \"{1}\" \")\"";

            var parameters = new List<string>();

            /* global options  */
            parameters.Add("--ui-language zh_CN");
            parameters.Add($"--output \"{episode.OutputFile}\"");

            /* video track */
            parameters.Add("--language 0:und");
            parameters.Add($"--default-duration 0:{episode.VideoFps}p \"(\" \"{episode.VideoFile}\" \")\" ");

            /* audio track */
            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add(string.Format(trackTemplate, episode.AudioLanguage, audioFile));
            }

            /* subtitle track */
            foreach (var subtitleFile in episode.SubtitleFiles) {
                parameters.Add(string.Format(trackTemplate, episode.SubtitleLanguage, subtitleFile));
            }

            /* chapter */
            if (episode.ChapterFile != null) {
                parameters.Add($"--chapters \"{episode.ChapterFile}\"");
            }

            /* track order */
            int trackCount = 1 + episode.AudioFiles.Count + episode.SubtitleFiles.Count;
            var trackOrder = new List<string>(trackCount);
            for (int i = 0; i < trackCount; i++) {
                trackOrder.Add($"{i}:0");
            }
            parameters.Add($"--track-order {string.Join(",", trackOrder)}");

            return string.Join(" ", parameters);
        }

        private string GenerateMp4MergeParameter(Episode episode)
        {
            var parameters = new List<string>();

            /* global options */
            parameters.Add("--file-format mp4");
            parameters.Add($"-o {episode.OutputFile}");

            /* video track */
            parameters.Add($"-i {episode.VideoFile}?fps={episode.VideoFps}");

            /* audio track */
            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add($"-i {audioFile}?language={episode.AudioLanguage}");
            }

            /* chapter */
            if (!string.IsNullOrEmpty(episode.ChapterFile)) {
                parameters.Add($"--chapter \"{episode.ChapterFile}\"");
            }

            return string.Join(" ", parameters);
        }
    }
}
