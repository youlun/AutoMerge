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
            Action<Guid, int, string> taskProgressChangedCallback,
            Action<Guid> taskCompletedCallback,
            Action allTaskCompletedCallback,
            bool putCrc32ToFilename,
            bool moveToCompletedFolder
        ) {
            string videoFileExtension = Settings.VideoSourceFileTypes[_muxingConfiguration.VideoSourceType].FileExtension;
            var videoFiles = FileSystemUtility.EnumerateFiles($"*{videoFileExtension}", _muxingConfiguration.MainDirectory);
            var episodes = GenerateEpisodes(videoFiles);

            var mergeParameters = GenerateMergeParameters(episodes);

            fileListBuiltCallback(episodes);

            new Thread(new ThreadStart(() => {
                Parallel.ForEach(mergeParameters, parameter => {
                    var episode = parameter.Value.Item2;

                    OutputType outputType = episode.OutputFileType;
                    Guid taskId = parameter.Key;
                    string args = parameter.Value.Item1;
                    long totalFileSize = episode.TotalFileSize;

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

                        taskProgressChangedCallback(taskId, progress, "封装中");
                    }, () => {
                        string file = episode.OutputFile;

                        if (!File.Exists(file)) {
                            taskCompletedCallback(taskId);
                            return;
                        }

                        if (putCrc32ToFilename) {
                            uint crc = 0;
                            int progress = 0;
                            long completedSize = 0, totalSize = 0;

                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 16777216)) {
                                byte[] buffer = new byte[16777216];
                                totalSize = fs.Length;

                                int n = 0;

                                while ((n = fs.Read(buffer, 0, 16777216)) > 0) {
                                    completedSize += n;
                                    crc = Force.Crc32.Crc32Algorithm.Append(crc, buffer, 0, n);
                                    progress = Convert.ToInt32(completedSize * 100d / totalSize);
                                    taskProgressChangedCallback(taskId, progress, "校验中");
                                }
                            }

                            string newName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileNameWithoutExtension(file)} [{crc.ToString("X8")}]{Path.GetExtension(file)}";
                            File.Move(file, newName);
                            file = newName;
                        }

                        if (moveToCompletedFolder) {
                            string curDir = _muxingConfiguration.MainDirectory;
                            string newPath = $"{curDir}\\completed\\{file.Replace(curDir, "")}";
                            string newDir = Path.GetDirectoryName(newPath);
                            if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
                            File.Move(file, newPath);
                        }

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

                string outputFileExtension = Settings.OutputFileTypes[_muxingConfiguration.OutputType].FileExtension;
                string outputFile = baseName + outputFileExtension;
                string videoFile = file;
                string chapterFile = baseName + Settings.ChapterFileExtension;
                var audioFiles = new List<string>();
                var subtitleFiles = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                var existingOutputFiles = FileSystemUtility.EnumerateFiles($@"{fileNameWithoutExtension} [*]{outputFileExtension}", directory, SearchOption.TopDirectoryOnly);
                if (1 == existingOutputFiles.Count && Regex.IsMatch(existingOutputFiles[0], Regex.Escape(baseName) + @" \[[0-9a-fA-F]{8}\]\.mkv")) {
                    continue;
                }

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

                string chapterLanguage = "eng";

                if (!File.Exists(chapterFile) || !_muxingConfiguration.MuxChapter) {
                    chapterFile = null;
                } else {
                    string chapterFileContent = null;
                    using (var sr = new StreamReader(chapterFile, Encoding.UTF8)) {
                        chapterFileContent = sr.ReadToEnd();
                    }
                    if (Regex.IsMatch(chapterFileContent, @"[\u3041-\u30ff]")) {
                        chapterLanguage = "jpn";
                    }
                }

                string videoFps = _muxingConfiguration.VideoFps;

                if ("AUTO" == videoFps) {
                    var info = new MediaInfo.DotNetWrapper.MediaInfo();
                    info.Open(videoFile);
                    var num = info.Get(MediaInfo.DotNetWrapper.StreamKind.Video, 0, "FrameRate_Num");
                    var den = info.Get(MediaInfo.DotNetWrapper.StreamKind.Video, 0, "FrameRate_Den");
                    videoFps = $"{num}/{den}";
                }

                Episode ep = new Episode {
                    VideoFile = videoFile,
                    VideoFps = videoFps,
                    OutputFile = outputFile,
                    OutputFileType = _muxingConfiguration.OutputType,
                    ChapterFile = chapterFile,
                    AudioFiles = audioFiles,
                    AudioLanguage = _muxingConfiguration.AudioLanguage,
                    SubtitleFiles = subtitleFiles,
                    SubtitleLanguage = _muxingConfiguration.SubtitleLanguage,
                    TaskId = Guid.NewGuid(),
                    TotalFileSize = totalSize,
                    ChapterLanguage = chapterLanguage
                };
                episodes.Add(ep);
            }

            return episodes;
        }
        
        private Dictionary<Guid, Tuple<string, Episode>> GenerateMergeParameters(List<Episode> episodes)
        {
            var mergeParameters = new Dictionary<Guid, Tuple<string, Episode>>();

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
                mergeParameters.Add(ep.TaskId, Tuple.Create(parameter, ep));
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
                parameters.Add($"--chapter-language {episode.ChapterLanguage}");
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
            parameters.Add($"-o \"{episode.OutputFile}\"");

            /* video track */
            parameters.Add($"-i \"{episode.VideoFile}?fps={episode.VideoFps}\"");

            /* audio track */
            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add($"-i \"{audioFile}?language={episode.AudioLanguage}\"");
            }

            /* chapter */
            if (!string.IsNullOrEmpty(episode.ChapterFile)) {
                parameters.Add($"--chapter \"{episode.ChapterFile}\"");
            }

            return string.Join(" ", parameters);
        }
    }
}
