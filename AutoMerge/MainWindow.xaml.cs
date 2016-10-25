using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace AutoMerge
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<VideoSourceType, FileType> VideoSourceFileTypes = new Dictionary<VideoSourceType, FileType>
        {
            { VideoSourceType.Avc, new FileType { FileExtension = ".avc" } },
            { VideoSourceType.Hevc, new FileType { FileExtension = ".hevc" } }
        };
        private Dictionary<AudioSourceType, FileType> AudioSourceFileTypes = new Dictionary<AudioSourceType, FileType>
        {
            { AudioSourceType.Flac, new FileType { FileExtension = ".flac", UIButtonType = ButtonType.Check } },
            { AudioSourceType.M4a, new FileType { FileExtension = ".m4a", UIButtonType = ButtonType.Check } },
            { AudioSourceType.Aac, new FileType { FileExtension = ".aac", UIButtonType = ButtonType.Check } },
            { AudioSourceType.Ac3, new FileType { FileExtension = ".ac3", UIButtonType = ButtonType.Check } }
        };
        private Dictionary<OutputType, FileType> OutputFileTypes = new Dictionary<OutputType, FileType>
        {
            { OutputType.Mkv, new FileType { FileExtension = ".mkv" } },
            { OutputType.Mp4, new FileType { FileExtension = ".mp4" } }
        };

        private TextBox AudioLanguage = null;
        private CheckBox UsingAllAudioSourceType = null;

        private string MkvMergeFilePath = null;
        private string Mp4MuxerFilePath = null;

        private Configuration Config = null;
        private KeyValueConfigurationCollection Settings = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeConfiguration();
            SelectBinariesPath();
            InitializeToggleButtons();
        }

        private void InitializeToggleButtons()
        {
            Action<object, FileType, WrapPanel> generateButton = (key, fileType, panel) => {
                ToggleButton btn = null;
                switch (fileType.UIButtonType) {
                    case ButtonType.Check: btn = new CheckBox(); break;
                    case ButtonType.Radio: btn = new RadioButton(); break;
                    default: return;
                }
                btn.Content = fileType.UIDisplayName;
                btn.Tag = key;
                btn.Margin = new Thickness(0, 0, 10, 0);
                btn.VerticalAlignment = VerticalAlignment.Center;

                panel.Children.Add(btn);
            };

            foreach (var pair in VideoSourceFileTypes) generateButton(pair.Key, pair.Value, VideoSourcePanel);
            foreach (var pair in AudioSourceFileTypes) generateButton(pair.Key, pair.Value, AudioSourcePanel);
            foreach (var pair in OutputFileTypes) generateButton(pair.Key, pair.Value, OutputPanel);

            AudioLanguage = new TextBox {
                Text = "jpn",
                Width = 30,
                Height = 20
            };

            UsingAllAudioSourceType = new CheckBox {
                Content = "全部",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            UsingAllAudioSourceType.Checked += UsingAllAudioSource_CheckedChanged;
            UsingAllAudioSourceType.Unchecked += UsingAllAudioSource_CheckedChanged;

            AudioSourcePanel.Children.Add(UsingAllAudioSourceType);
            AudioSourcePanel.Children.Add(new TextBlock { Text = "语言", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            AudioSourcePanel.Children.Add(AudioLanguage);
        }

        private void InitializeConfiguration()
        {/*
            Configuration localConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = localConfig.FilePath };
            Config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.PerUserRoamingAndLocal);

            string currentSection = null;
            try {
                currentSection =
           (string)Config.GetSection("FilePath");
            }

            Settings = Config.AppSettings.Settings;

            if (null == Settings["MkvMergeFilePath"]) Settings.Add("MkvMergeFilePath", string.Empty);
            if (null == Settings["Mp4MuxerFilePath"]) Settings.Add("Mp4MuxerFilePath", string.Empty);

            MkvMergeFilePath = Settings["MkvMergeFilePath"].Value;
            MkvMergeFilePath = Settings["Mp4MuxerFilePath"].Value;*/
        }

        private void SelectBinariesPath()
        {
            Func<string, string> selectFile = (title) => {
                var dlg = new OpenFileDialog();
                dlg.Filter = "exe(*.exe)|*.exe";
                dlg.Title = title;
                dlg.Multiselect = false;
                if (true == dlg.ShowDialog()) {
                    return dlg.FileName;
                }
                return null;
            };

            while (string.IsNullOrEmpty(MkvMergeFilePath) || !File.Exists(MkvMergeFilePath)) {
                MkvMergeFilePath = selectFile("选择 MKVToolNix mkvmerge.exe");
                if (string.IsNullOrEmpty(MkvMergeFilePath) && MessageBoxResult.Yes == MessageBox.Show("不使用 mkv 合并功能？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)) {
                    OutputFileTypes.Remove(OutputType.Mkv);
                    break;
                }
            }

            while (string.IsNullOrEmpty(Mp4MuxerFilePath) || !File.Exists(Mp4MuxerFilePath)) {
                Mp4MuxerFilePath = selectFile("选择 L-SMASH muxer.exe");
                if (string.IsNullOrEmpty(Mp4MuxerFilePath) && MessageBoxResult.Yes == MessageBox.Show("不使用 mp4 合并功能？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)) {
                    OutputFileTypes.Remove(OutputType.Mp4);
                    break;
                }
            }
            /*
            Settings["MkvMergeFilePath"].Value = MkvMergeFilePath;
            Settings["Mp4MuxerFilePath"].Value = Mp4MuxerFilePath;
            try {
                Config.Save(ConfigurationSaveMode.Modified);
            } catch (Exception e) {

                throw;
            }*/
        }

        private void ForceChecked(object sender, RoutedEventArgs e)
        {
            (sender as CheckBox).IsChecked = true;
        }

        private void UsingAllAudioSource_CheckedChanged(object sender, RoutedEventArgs e)
        {
            foreach (var element in AudioSourcePanel.Children) {
                CheckBox chk = element as CheckBox;
                if (null == chk || UsingAllAudioSourceType == chk) continue;

                if (true == UsingAllAudioSourceType.IsChecked) {
                    chk.Unchecked += ForceChecked;
                } else {
                    chk.Unchecked -= ForceChecked;
                }
                chk.IsChecked = UsingAllAudioSourceType.IsChecked;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private List<string> EnumerateFiles(string filePattern, string directory = null, int depth = 1)
        {
            var result = new List<string>();

            if (0 == depth && directory != null) {
                var files = Directory.EnumerateFiles(directory, filePattern);
                foreach (var file in files) {
                    result.Add(file);
                }
                return result;
            }

            var dirs = Directory.EnumerateDirectories((null == directory ? Directory.GetCurrentDirectory() : directory));
            foreach (var dir in dirs) {
                var files = Directory.EnumerateFiles(dir, filePattern);
                foreach (var file in files) {
                    result.Add(file);
                    Console.WriteLine(file);
                }
            }
            return result;
        }

        private List<Episode> MakeEpisodes(
            List<string> m2tsfiles,
            List<AudioSourceType> audioTypes,
            VideoSourceType videoType,
            OutputType outputType,
            string videoFps,
            string audioLanguage,
            string subtitleLanguage,
            bool mergeChapter,
            bool mergeSubtitle
        )
        {

            List<Episode> episodes = new List<Episode>();

            foreach (var file in m2tsfiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                string outputFile = baseName + this.OutputFileTypes[outputType].FileExtension;
                string videoFile = baseName + this.VideoSourceFileTypes[videoType].FileExtension;
                string chapterFile = baseName + ".txt";
                var audioFiles = new List<string>();
                var subtitleFiels = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                foreach (var audio in audioTypes) {
                    audioFiles.AddRange(this.EnumerateFiles($"{fileNameWithoutExtension}*{this.AudioSourceFileTypes[audio].FileExtension}", directory, 0));
                }

                if (0 == audioFiles.Count) continue;

                subtitleFiels.AddRange(this.EnumerateFiles($"{fileNameWithoutExtension}*.sup", directory, 0));

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = videoFps,
                    OutputFile = outputFile,
                    OutputFileType = outputType,
                    ChapterFile = File.Exists(chapterFile) && mergeChapter ? chapterFile : null,
                    AudioFiles = audioFiles,
                    AudioLanguage = audioLanguage,
                    SubtitleFiles = mergeSubtitle ? subtitleFiels : null,
                    SubtitleLanguage = subtitleLanguage
                };
                episodes.Add(ep);
            }

            return episodes;
        }

        private string MkvMergeParameter(Episode episode)
        {
            string trackTemplate = "--language 0:{0} \"(\" \"{1}\" \")\"";

            var parameters = new List<string>();
            var trackOrder = new List<string>();

            int fileID = 0;

            parameters.Add("--ui-language zh_CN");
            parameters.Add($"--output \"{episode.OutputFile}\"");

            parameters.Add(string.Format(trackTemplate, "und", episode.VideoFile));
            trackOrder.Add($"{fileID++}:0");

            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add(string.Format(trackTemplate, episode.AudioLanguage, audioFile));
                trackOrder.Add($"{fileID++}:0");
            }

            if (episode.SubtitleFiles != null) {
                foreach (var subtitleFile in episode.SubtitleFiles) {
                    parameters.Add(string.Format(trackTemplate, episode.SubtitleLanguage, subtitleFile));
                    trackOrder.Add($"{fileID++}:0");
                }
            }

            if (episode.ChapterFile != null) parameters.Add($"--chapters \"{episode.ChapterFile}\"");

            parameters.Add(string.Format("--track-order {0}", string.Join(",", trackOrder)));

            return string.Join(" ", parameters);
        }

        private string Mp4MergeParameter(Episode episode)
        {
            var parameters = new List<string>();
            //mp4muxer --interleave 500 --file-format mp4 --chapter {0} -i{1}?fps=24000/1001 -i{2}?language=jpn -o {3}
            parameters.Add("--file-format mp4");

            if (episode.ChapterFile != null) parameters.Add($"--chapter \"{episode.ChapterFile}\"");

            parameters.Add($"-i {episode.VideoFile}?fps={episode.VideoFps}");

            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add($"-i {audioFile}?language={episode.AudioLanguage}");
            }

            parameters.Add($"-o {episode.OutputFile}");

            return string.Join(" ", parameters);
        }

        private List<Tuple<OutputType, string>> MakeMergeParameters(List<Episode> episodes)
        {
            var mergeParameters = new List<Tuple<OutputType, string>>();

            foreach (var ep in episodes) {
                string parameter = null;
                switch (ep.OutputFileType) {
                    case OutputType.Mkv:
                        parameter = this.MkvMergeParameter(ep);
                        break;
                    case OutputType.Mp4:
                        parameter = this.Mp4MergeParameter(ep);
                        break;
                    default:
                        continue;
                }
                mergeParameters.Add(Tuple.Create(ep.OutputFileType, parameter));
            }

            return mergeParameters;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            List<AudioSourceType> audioType;
            VideoSourceType videoType;
            OutputType outputType;
            #region UI
            /*
            if (this.rdoFlac.Checked) {
                audioType = AudioSourceType.Flac;
            } else if (this.rdoM4a.Checked) {
                audioType = AudioSourceType.M4a;
            } else if (this.rdoAudioBoth.Checked) {
                audioType = AudioSourceType.Both;
            } else {
                MessageBox.Show("需要选择输入音频格式");
                return;
            }

            if (this.rdo264.Checked) {
                videoType = VideoSourceType.Avc;
            } else if (this.rdoHevc.Checked) {
                videoType = VideoSourceType.Hevc;
            } else {
                MessageBox.Show("需要选择输入视频格式");
                return;
            }

            if (this.rdoMkv.Checked) {
                outputType = OutputType.Mkv;
            } else if (this.rdoMp4.Checked) {
                outputType = OutputType.Mp4;
            } else {
                MessageBox.Show("需要选择输出视频格式");
                return;
            }
            */
            #endregion

            audioType = new List<AudioSourceType>();
            videoType = VideoSourceType.Avc;
            outputType = OutputType.Mkv;

            var m2tsFiles = EnumerateFiles("*.m2ts");
            var episodes = MakeEpisodes(m2tsFiles, audioType, videoType, outputType,
                Fps.Text, AudioLanguage.Text, SubtitleLanguage.Text, true == ChapterCheck.IsChecked, true == SubtitleCheck.IsChecked);
            var mergeParameters = MakeMergeParameters(episodes);

            Parallel.ForEach(mergeParameters, new ParallelOptions {
                MaxDegreeOfParallelism = 4
            }, parameter => {
                string mainProcess = null;
                switch (parameter.Item1) {
                    case OutputType.Mkv:
                        mainProcess = this.MkvMergeFilePath;
                        break;
                    case OutputType.Mp4:
                        mainProcess = this.Mp4MuxerFilePath;
                        break;
                    default:
                        return;
                }

                Console.WriteLine($"{mainProcess} {parameter.Item2}");
                StartProcess(mainProcess, parameter.Item2);
            });
        }
    }
}
