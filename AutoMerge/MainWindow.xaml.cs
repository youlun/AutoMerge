using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Win32;

namespace AutoMerge
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<VideoSourceType, FileType> _videoSourceFileTypes = new Dictionary<VideoSourceType, FileType>
        {
            { VideoSourceType.Avc, new FileType { FileExtension = ".264", UIBaseCheckBox = "VideoSourceCheck" } },
            { VideoSourceType.Hevc, new FileType { FileExtension = ".hevc", UIBaseCheckBox = "VideoSourceCheck" } }
        };
        private Dictionary<AudioSourceType, FileType> _audioSourceFileTypes = new Dictionary<AudioSourceType, FileType>
        {
            { AudioSourceType.Flac, new FileType { FileExtension = ".flac", UIButtonType = ButtonType.Check, UIBaseCheckBox = "AudioSourceCheck" } },
            { AudioSourceType.M4a, new FileType { FileExtension = ".m4a", UIButtonType = ButtonType.Check, UIBaseCheckBox = "AudioSourceCheck" } },
            { AudioSourceType.Aac, new FileType { FileExtension = ".aac", UIButtonType = ButtonType.Check, UIBaseCheckBox = "AudioSourceCheck" } },
            { AudioSourceType.Ac3, new FileType { FileExtension = ".ac3", UIButtonType = ButtonType.Check, UIBaseCheckBox = "AudioSourceCheck" } }
        };
        private Dictionary<OutputType, FileType> _outputFileTypes = new Dictionary<OutputType, FileType>
        {
            { OutputType.Mkv, new FileType { FileExtension = ".mkv", NeedEnterFps = true } },
            { OutputType.Mp4, new FileType { FileExtension = ".mp4", NeedEnterFps = true } }
        };

        public string MkvMergeFilePath { get; set; } = @"MKVToolNix\mkvmerge.exe";
        public string Mp4MuxerFilePath { get; set; } = @"bin\mp4muxer.exe";

        private TextBox AudioLanguage = null;
        private CheckBox UsingAllAudioSourceType = null;

        public MainWindow()
        {
            int width = 210;
            int height = 30;
            bool success = false;
            while (!success) {
                try {
                    Console.SetWindowSize(width, height);
                    success = true;
                } catch {
                    width -= 14;
                    height -= 2;
                }
            }
            Console.WriteLine("这里是日志，不能关，可以最小化");

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

                if (fileType.NeedEnterFps) {
                    btn.Checked += (s, e) => { Fps.IsEnabled = true; };
                    btn.Unchecked += (s, e) => { Fps.IsEnabled = false; };
                }
                if (fileType.UIBaseCheckBox != null) {
                    btn.Checked += (s, e) => { (FindName(fileType.UIBaseCheckBox) as CheckBox).IsChecked = true; };
                }

                panel.Children.Add(btn);
            };

            foreach (var pair in _videoSourceFileTypes) generateButton(pair.Key, pair.Value, VideoSourcePanel);
            foreach (var pair in _audioSourceFileTypes) generateButton(pair.Key, pair.Value, AudioSourcePanel);
            foreach (var pair in _outputFileTypes) generateButton(pair.Key, pair.Value, OutputPanel);

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
        {
_open:
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AutoMerge")) {
                if (null == key) {
                    Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AutoMerge");
                    goto _open;
                }
                MkvMergeFilePath = key.GetValue("MkvMergeFilePath", string.Empty).ToString();
                Mp4MuxerFilePath = key.GetValue("Mp4MuxerFilePath", string.Empty).ToString();
            }
        }

        private void SelectBinariesPath()
        {
            Func<string, string> selectFile = title => {
                var dlg = new OpenFileDialog();
                dlg.Filter = "exe(*.exe)|*.exe";
                dlg.Title = title;
                dlg.Multiselect = false;
                if (true == dlg.ShowDialog()) {
                    return dlg.FileName;
                }
                return null;
            };

            var taskList = new List<Tuple<string, string, string, OutputType>> {
                Tuple.Create("MkvMergeFilePath", "MKVToolNix mkvmerge.exe", "mkv", OutputType.Mkv),
                Tuple.Create("Mp4MuxerFilePath", "L-SMASH muxer.exe", "mp4", OutputType.Mp4)
            };
            foreach (var task in taskList) {
                MessageBoxResult mbResult;
                var filePathProperty = GetType().GetProperty(task.Item1);

                while (!File.Exists(filePathProperty.GetValue(this).ToString())) {
                    var newFilePath = selectFile($"选择 {task.Item2}");
                    filePathProperty.SetValue(this, newFilePath);

                    if (string.IsNullOrEmpty(newFilePath)) {
                        mbResult = MessageBox.Show($"不使用 {task.Item3} 封装功能？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (MessageBoxResult.Yes == mbResult) {
                            filePathProperty.SetValue(this, string.Empty);
                            _outputFileTypes.Remove(task.Item4);
                            break;
                        }
                        continue;
                    }

                    mbResult = MessageBox.Show($"确定选对了？选错了可不好改哟\r\n{newFilePath}", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (MessageBoxResult.No == mbResult) {
                        filePathProperty.SetValue(this, string.Empty);
                        continue;
                    }

                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AutoMerge", true)) {
                        key.SetValue(task.Item1, newFilePath);
                    }
                }
            }
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

            result.AddRange(Directory.EnumerateFiles((null == directory ? Directory.GetCurrentDirectory() : directory), filePattern, SearchOption.AllDirectories));
            return result;
        }

        private List<Episode> GenerateEpisodes(
            List<string> videoFiles,
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

            foreach (var file in videoFiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                string outputFile = baseName + _outputFileTypes[outputType].FileExtension;
                string videoFile = file;
                string chapterFile = baseName + ".txt";
                var audioFiles = new List<string>();
                var subtitleFiels = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                foreach (var audio in audioTypes) {
                    audioFiles.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*{_audioSourceFileTypes[audio].FileExtension}", directory, 0));
                }

                if (0 == audioFiles.Count) continue;

                if (mergeSubtitle) {
                    subtitleFiels.AddRange(EnumerateFiles($"{fileNameWithoutExtension}*.sup", directory, 0));
                }

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = videoFps,
                    OutputFile = outputFile,
                    OutputFileType = outputType,
                    ChapterFile = File.Exists(chapterFile) && mergeChapter ? chapterFile : null,
                    AudioFiles = audioFiles,
                    AudioLanguage = audioLanguage,
                    SubtitleFiles = subtitleFiels,
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

        private string Mp4MergeParameter(Episode episode)
        {
            var parameters = new List<string>();

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
                        parameter = MkvMergeParameter(ep);
                        break;
                    case OutputType.Mp4:
                        parameter = Mp4MergeParameter(ep);
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

            var audioType = new List<AudioSourceType>();
            VideoSourceType? videoType = null;
            OutputType? outputType = null;
            #region UI
            if (true == VideoSourceCheck.IsChecked) {
                foreach (var children in VideoSourcePanel.Children) {
                    var btn = children as ToggleButton;
                    if (null == btn
                        || false == btn.IsChecked.GetValueOrDefault()
                    ) {
                        continue;
                    }
                    videoType = (btn as ToggleButton).Tag as VideoSourceType?;
                    break;
                }

                if (!videoType.HasValue) {
                    MessageBox.Show("需要选择输入视频格式", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (true == AudioSourceCheck.IsChecked) {
                foreach (var children in AudioSourcePanel.Children) {
                    var btn = children as ToggleButton;
                    if (null == btn
                        || false == btn.IsChecked.GetValueOrDefault()
                        || UsingAllAudioSourceType == btn
                    ) {
                        continue;
                    }
                    audioType.Add((btn.Tag as AudioSourceType?).Value);
                }

                if (!videoType.HasValue) {
                    MessageBox.Show("需要选择输入音频格式", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            foreach (var btn in OutputPanel.Children) {
                if (!(btn is ToggleButton)) continue;
                if (!(true == (btn as ToggleButton).IsChecked)) continue;
                outputType = (btn as ToggleButton).Tag as OutputType?;
                break;
            }
            if (!outputType.HasValue) {
                MessageBox.Show("需要选择输出视频格式", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (_outputFileTypes[outputType.Value].NeedEnterFps && string.IsNullOrEmpty(Fps.Text)) {
                MessageBox.Show("需要设置输出视频的FPS", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!(true == VideoSourceCheck.IsChecked)
                && !(true == AudioSourceCheck.IsChecked)
                && !(true == ChapterCheck.IsChecked)
                && !(true == SubtitleCheck.IsChecked)
            ) {
                for (int i = 0; i < 100; i++) {
                    Left += ((0 == i % 2) ? 5 : -5);
                    Thread.Sleep(5);
                }
                MessageBox.Show("什么输入类型都不选，我好慌啊", "倒是选一个呀", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            #endregion

            var videoFiles = EnumerateFiles($"*{_videoSourceFileTypes[videoType.Value].FileExtension}");
            var episodes = GenerateEpisodes(
                videoFiles,
                audioType,
                videoType.Value,
                outputType.Value,
                Fps.Text,
                AudioLanguage.Text,
                SubtitleLanguage.Text,
                ChapterCheck.IsChecked.GetValueOrDefault(false),
                SubtitleCheck.IsChecked.GetValueOrDefault(false)
            );
            var mergeParameters = MakeMergeParameters(episodes);

            Start.IsEnabled = false;

            Parallel.ForEach(mergeParameters, parameter => {
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

                Console.WriteLine($"{mainProcess} {parameter.Item2}\n");
                StartProcess(mainProcess, parameter.Item2);
            });

            MessageBoxResult mbResult;
            do {
                mbResult = MessageBox.Show("请先播放检查封装成品，再上传！", "注意", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
            } while (MessageBoxResult.No == mbResult);
            Start.IsEnabled = true;
        }
    }
}
