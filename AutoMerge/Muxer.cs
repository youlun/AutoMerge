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
using AutoMerge.Utility;

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

        private MuxingConfiguration _muxingConfiguration;

        private Muxer() { }

        internal Muxer(MuxingConfiguration muxingConfiguration)
        {
            _muxingConfiguration = muxingConfiguration;
        }

        internal void StartMux(
            Action<List<Episode>> fileListBuiltCallback,
            Action<Guid> taskStartedCallback,
            Action<Guid, int> taskProgressChangedCallback,
            Action<Guid> taskCompletedCallback,
            Action allTaskCompletedCallback
        ) {
            string videoFileExtension = Settings.VideoSourceFileTypes[_muxingConfiguration.VideoSourceType].FileExtension;
            var videoFiles = FileSystemUtility.EnumerateFiles($"*{videoFileExtension}", _muxingConfiguration.MainDirectory);
            var episodes = GenerateEpisodes(videoFiles);

            var mergeParameters = GenerateMergeParameters(episodes);

            fileListBuiltCallback(episodes);

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

        private List<Episode> GenerateEpisodes(List<string> videoFiles)
        {
            var episodes = new List<Episode>();

            foreach (var file in videoFiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                long totalSize = 0;

                string outputFile = baseName + Settings.OutputFileTypes[_muxingConfiguration.OutputType].FileExtension;
                string videoFile = file;
                string chapterFile = baseName + Settings.ChapterFileExtension;
                var audioFiles = new List<string>();
                var subtitleFiles = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                totalSize += FileSystemUtility.GetFileSize(videoFile);

                if (_muxingConfiguration.AudioSourceTypes != null && _muxingConfiguration.AudioSourceTypes.Count > 0) {
                    foreach (var audioType in _muxingConfiguration.AudioSourceTypes) {
                        string audioFileExtension = Settings.AudioSourceFileTypes[audioType].FileExtension;
                        string filePattern = $"{fileNameWithoutExtension}*{audioFileExtension}";
                        audioFiles.AddRange(FileSystemUtility.EnumerateFiles(filePattern, directory, SearchOption.TopDirectoryOnly));
                    }
                    if (0 == audioFiles.Count) continue;

                    foreach (var audioFile in audioFiles) {
                        totalSize += FileSystemUtility.GetFileSize(audioFile);
                    }
                }
                
                if (_muxingConfiguration.MuxSubtitle) {
                    string filePattern = $"{fileNameWithoutExtension}*{Settings.SubtitleFileExtension}";
                    subtitleFiles.AddRange(FileSystemUtility.EnumerateFiles(filePattern, directory, SearchOption.TopDirectoryOnly));
                }

                if (!File.Exists(chapterFile) || !_muxingConfiguration.MuxChapter) {
                    chapterFile = null;
                }

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = _muxingConfiguration.VideoFps,
                    OutputFile = outputFile,
                    OutputFileType = _muxingConfiguration.OutputType,
                    ChapterFile = chapterFile,
                    AudioFiles = audioFiles,
                    AudioLanguage = _muxingConfiguration.AudioLanguage,
                    SubtitleFiles = subtitleFiles,
                    SubtitleLanguage = _muxingConfiguration.SubtitleLanguage,
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
