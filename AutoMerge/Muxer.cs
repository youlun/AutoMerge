using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

        internal void StartMux(
            MuxingConfiguration configuration,
            Action<List<Episode>> fileListBuildedCallback,
            Action<Guid> taskStartedCallback,
            Action<Guid, int> taskProgressChangedCallback,
            Action<Guid> taskCompletedCallback,
            Action allTaskCompletedCallback
        ) {
            string videoFileExtension = Settings.VideoSourceFileTypes[configuration.VideoSourceType].FileExtension;
            var videoFiles = EnumerateFiles($"*{videoFileExtension}", configuration.MainDirectory);
            var episodes = GenerateEpisodes(videoFiles, configuration);

            var mergeParameters = GenerateMergeParameters(episodes);

            fileListBuildedCallback(episodes);

            new Thread(new ThreadStart(() => {
                Parallel.ForEach(mergeParameters, parameter => {
                    var outputType = parameter.Value.Item1;
                    var taskId = parameter.Key;
                    var args = parameter.Value.Item2;
                    long totalFileSize = parameter.Value.Item3;

                    string mainProcess = null;
                    switch (outputType) {
                        case OutputType.Mkv:
                            mainProcess = Settings.MkvMergeFilePath;
                            break;
                        case OutputType.Mp4:
                            mainProcess = Settings.Mp4MuxerFilePath;
                            break;
                        default:
                            return;
                    }

                    Console.WriteLine($"{mainProcess} {args}\n");
                    taskStartedCallback(taskId);
                    ProcessUtility.StartProcess(mainProcess, args, true, true, true, () => {
                        taskStartedCallback(taskId);
                    }, stdout => {
                        if (null == stdout) return;

                        Match progressMatch = null;
                        int progress = 0;
                        switch (outputType) {
                            case OutputType.Mkv:
                                //Progress: 100%
                                progressMatch = Regex.Match(stdout, @"Progress: (\d*?)%", RegexOptions.Compiled);
                                if (progressMatch.Groups.Count < 2) return;
                                progress = Convert.ToInt32(progressMatch.Groups[1].Value);
                                break;
                            case OutputType.Mp4:
                                //Importing: 410017308 bytes
                                progressMatch = Regex.Match(stdout, @"Importing: (\d*?) bytes", RegexOptions.Compiled);
                                if (progressMatch.Groups.Count < 2) return;
                                progress = Convert.ToInt32(Convert.ToDouble(progressMatch.Groups[1].Value) / totalFileSize * 100d);
                                break;
                        }

                        taskProgressChangedCallback(taskId, progress);
                    }, () => {
                        taskCompletedCallback(taskId);
                    });
                });
                allTaskCompletedCallback();
            })).Start();
        }

        private List<string> EnumerateFiles(string filePattern, string directory = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Directory.EnumerateFiles(
                (string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : directory),
                filePattern,
                searchOption
            ).ToList();
        }

        
        private List<Episode> GenerateEpisodes(List<string> videoFiles, MuxingConfiguration configuration)
        {
            var episodes = new List<Episode>();

            foreach (var file in videoFiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                long totalSize = 0;

                string outputFile = baseName + Settings.OutputFileTypes[configuration.OutputType].FileExtension;
                string videoFile = file;
                string chapterFile = baseName + Settings.ChapterFileExtension;
                var audioFiles = new List<string>();
                var subtitleFiles = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                totalSize += new FileInfo(videoFile).Length;

                if (configuration.AudioSourceTypes != null && configuration.AudioSourceTypes.Count > 0) {
                    foreach (var audioType in configuration.AudioSourceTypes) {
                        string audioFileExtension = Settings.AudioSourceFileTypes[audioType].FileExtension;
                        audioFiles.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*{audioFileExtension}", directory, SearchOption.TopDirectoryOnly));
                    }
                    if (0 == audioFiles.Count) continue;

                    foreach (var audioFile in audioFiles) {
                        totalSize += new FileInfo(audioFile).Length;
                    }
                }
                
                if (configuration.MuxSubtitle) {
                    subtitleFiles.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*{Settings.SubtitleFileExtension}", directory, SearchOption.TopDirectoryOnly));
                }

                if (!File.Exists(chapterFile) || !configuration.MuxChapter) {
                    chapterFile = null;
                }

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = configuration.VideoFps,
                    OutputFile = outputFile,
                    OutputFileType = configuration.OutputType,
                    ChapterFile = chapterFile,
                    AudioFiles = audioFiles,
                    AudioLanguage = configuration.AudioLanguage,
                    SubtitleFiles = subtitleFiles,
                    SubtitleLanguage = configuration.SubtitleLanguage,
                    TaskId = Guid.NewGuid(),
                    TotalFileSize = totalSize
                };
                episodes.Add(ep);
            }

            return episodes;
        }
        
        private Dictionary<Guid, Tuple<OutputType, string, long>> GenerateMergeParameters(List<Episode> episodes)
        {
            var mergeParameters = new Dictionary<Guid, Tuple<OutputType, string, long>>();

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
                mergeParameters.Add(ep.TaskId, Tuple.Create(ep.OutputFileType, parameter, ep.TotalFileSize));
            }

            return mergeParameters;
        }

        private string GenerateMkvMergeParameter(Episode episode)
        {
            string trackTemplate = "--language 0:{0} \"(\" \"{1}\" \")\"";

            var parameters = new List<string>();

            /* global options  */
            parameters.Add("--ui-language en");
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
