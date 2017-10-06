using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace AutoMerge
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private TextBox audioLanguageInput = null;
        private CheckBox usingAllAudioSourceTypeSelector = null;

        private ObservableCollection<MuxingTask> _taskList = new ObservableCollection<MuxingTask>();
        private object _taskListLock = new object();
        public ObservableCollection<MuxingTask> TaskList { get { return _taskList; } set { _taskList = value; } }

        public MainWindow()
        {
            BindingOperations.EnableCollectionSynchronization(TaskList, _taskListLock);
            DataContext = this;

            InitializeComponent();
            InitializeLogWindow();
            InitializeSelectors();
            InitializeOtherComponent();
        }

        private void InitializeLogWindow()
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
            Console.WriteLine("(log window)");
        }

        private void InitializeSelectors()
        {
            foreach (var pair in Settings.VideoSourceFileTypes) {
                GenerateSelector(pair.Key, pair.Value, ref videoSourcePanel);
            }
            foreach (var pair in Settings.AudioSourceFileTypes) {
                GenerateSelector(pair.Key, pair.Value, ref audioSourcePanel);
            }
            foreach (var pair in Settings.OutputFileTypes) {
                GenerateSelector(pair.Key, pair.Value, ref outputPanel);
            }
        }

        private void InitializeOtherComponent()
        {
            audioLanguageInput = new TextBox {
                Text = "jpn",
                Width = 30,
                Height = 22
            };

            usingAllAudioSourceTypeSelector = new CheckBox {
                Content = "全部",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            usingAllAudioSourceTypeSelector.Checked += usingAllAudioSourceTypeSelector_CheckedChanged;
            usingAllAudioSourceTypeSelector.Unchecked += usingAllAudioSourceTypeSelector_CheckedChanged;

            audioSourcePanel.Children.Add(usingAllAudioSourceTypeSelector);
            audioSourcePanel.Children.Add(new TextBlock { Text = "语言", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            audioSourcePanel.Children.Add(audioLanguageInput);
        }

        private void UpdateTaskProgress(Guid taskId, int progress, string statusText)
        {
            lock (_taskList) {
                foreach (var task in _taskList) {
                    if (task.TaskId != taskId) continue;
                    task.Percent = progress;
                    task.StatusText = statusText;
                }
            }
        }

        private void UpdateTaskStatusText(Guid taskId, string statusText)
        {
            lock (_taskList) {
                foreach (var task in _taskList) {
                    if (task.TaskId != taskId) continue;
                    task.StatusText = statusText;
                }
            }
        }

        private void RemoveTask(Guid taskId)
        {
            lock (_taskList) {
                MuxingTask muxingTask = null;
                foreach (var task in _taskList) {
                    if (task.TaskId != taskId) continue;
                    muxingTask = task;
                }
                _taskList.Remove(muxingTask);
            }
        }

        private void MuxingFileListBuilt(List<Episode> episodes)
        {
            foreach (var episode in episodes) {
                TaskList.Add(new MuxingTask {
                    OutputFileName = episode.OutputFile,
                    Percent = 0,
                    StatusText = "等待中",
                    TaskId = episode.TaskId
                });
            }
            if (episodes.Count > 0) {
                taskProgressPage.IsSelected = true;
            }
        }

        private void MuxingTaskStarted(Guid taskId) => UpdateTaskStatusText(taskId, "读取中");

        private void MuxingTasksCompleted()
        {
            MessageBoxResult mbResult;
            do {
                mbResult = MessageBox.Show("请先播放检查封装成品，再上传！", "注意", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
            } while (MessageBoxResult.No == mbResult);

            startButton.Dispatcher.BeginInvoke(new Action(() => {
                startButton.IsEnabled = true;
            }), DispatcherPriority.Normal);
        }

        private void GenerateSelector(object tag, FileType fileType, ref WrapPanel parentPanel)
        {
            ToggleButton btn = null;
            switch (fileType.UIButtonType) {
                case ButtonType.Check: btn = new CheckBox(); break;
                case ButtonType.Radio: btn = new RadioButton(); break;
                default: return;
            }
            btn.Content = fileType.UIDisplayName;
            btn.Tag = tag;
            btn.Margin = new Thickness(0, 0, 10, 0);
            btn.VerticalAlignment = VerticalAlignment.Center;

            if (fileType.NeedEnterFps) {
                RoutedEventHandler checkChanged = (o, e) => {
                    autoDetectVideoFpsSelector.IsEnabled = true;
                    autoDetectVideoFpsSelector.IsChecked = true;
                    videoFpsInput.IsEnabled = btn.IsEnabled && !autoDetectVideoFpsSelector.IsChecked.GetValueOrDefault();
                };
                btn.Checked += checkChanged;
                btn.Unchecked += checkChanged;
            }
            if (fileType.UIBaseCheckBox != null) {
                btn.Checked += (s, e) => { (FindName(fileType.UIBaseCheckBox) as CheckBox).IsChecked = true; };
            }

            if (fileType.DefaultSelected)
            {
                btn.IsChecked = true;
            }

            parentPanel.Children.Add(btn);
        }

        private void EachSelectors(bool onlyOne, ref WrapPanel panel, Action<object> configCallback)
        {
            foreach (var children in panel.Children) {
                var btn = children as ToggleButton;
                if (null == btn || !btn.IsChecked.GetValueOrDefault() || usingAllAudioSourceTypeSelector == btn) {
                    continue;
                }
                configCallback(btn.Tag);
                if (onlyOne) {
                    break;
                }
            }
        }

        private void ForceChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (sender as CheckBox);
            checkBox.IsChecked = true;

            /* 搞事 */
            if (sender == videoSourceSelector) {
                int count = Convert.ToInt16(checkBox.Tag);
                if (count >= 5) {
                    count = 0;
                    IsEnabled = false;
                    for (int i = 0; i < 100; i++) {
                        Left += ((0 == i % 2) ? 5 : -5);
                        Thread.Sleep(5);
                    }
                    MessageBoxUtility.ShowInformationMessageBox("不能不选视频轨");
                    IsEnabled = true;
                } else {
                    count++;
                }
                checkBox.Tag = count;
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            var audioType = new List<AudioSourceType>();
            VideoSourceType? videoType = null;
            OutputType? outputType = null;

            bool videoSourceSelectorChecked = videoSourceSelector.IsChecked.GetValueOrDefault();
            bool audioSourceSelectorChecked = audioSourceSelector.IsChecked.GetValueOrDefault();
            bool chapterSelectorChecked = chapterSelector.IsChecked.GetValueOrDefault();
            bool subtitleSelectorChecked = subtitleSelector.IsChecked.GetValueOrDefault();

            /* 搞事 */
            if (!videoSourceSelectorChecked && !audioSourceSelectorChecked && !chapterSelectorChecked && !subtitleSelectorChecked) {
                for (int i = 0; i < 100; i++) {
                    Left += ((0 == i % 2) ? 5 : -5);
                    Thread.Sleep(5);
                }
                MessageBoxUtility.ShowInformationMessageBox("你居然什么输入类型都不选", "最少选一钟输入类型");
                return;
            }

            /* 视频输入 */
            if (videoSourceSelectorChecked) {
                EachSelectors(true, ref videoSourcePanel, tag => { videoType = tag as VideoSourceType?; });

                if (!videoType.HasValue) {
                    MessageBoxUtility.ShowInformationMessageBox("需要选择输入视频格式");
                    return;
                }
            }

            /* 音频输入 */
            if (audioSourceSelectorChecked) {
                EachSelectors(false, ref audioSourcePanel, tag => { audioType.Add((tag as AudioSourceType?).Value); });

                if (0 == audioType.Count) {
                    MessageBoxUtility.ShowInformationMessageBox("需要选择输入音频格式");
                    return;
                }
            }

            /* 视频输出 */
            EachSelectors(true, ref outputPanel, tag => { outputType = (tag as OutputType?); });
            if (!outputType.HasValue) {
                MessageBoxUtility.ShowInformationMessageBox("需要选择输出视频格式");
                return;
            }

            /* FPS设定 */
            if (Settings.OutputFileTypes[outputType.Value].NeedEnterFps && string.IsNullOrEmpty(videoFpsInput.Text)) {
                MessageBoxUtility.ShowInformationMessageBox("需要设置输出视频的FPS");
                return;
            }

            startButton.IsEnabled = false;

            var muxer = new Muxer(new Muxer.MuxingConfiguration {
                AudioLanguage = audioLanguageInput.Text,
                AudioSourceTypes = audioType,
                MainDirectory = Directory.GetCurrentDirectory(),
                MuxChapter = chapterSelector.IsChecked.GetValueOrDefault(),
                MuxSubtitle = subtitleSelector.IsChecked.GetValueOrDefault(),
                OutputType = outputType.Value,
                SubtitleLanguage = subtitleLanguageInput.Text,
                VideoFps = videoFpsInput.Text,
                VideoSourceType = videoType.Value
            });

            muxer.StartMux(MuxingFileListBuilt, MuxingTaskStarted, UpdateTaskProgress, RemoveTask, MuxingTasksCompleted,
                putCrc32ToFilenameSelector.IsChecked.GetValueOrDefault(), moveToCompletedFolderSelector.IsChecked.GetValueOrDefault());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void taskList_SizeChanged(object sender, SizeChangedEventArgs e) => fileNameColumnHeader.Width = taskList.ActualWidth - 220;

        private void usingAllAudioSourceTypeSelector_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool selectorChecked = usingAllAudioSourceTypeSelector.IsChecked.GetValueOrDefault();

            foreach (var element in audioSourcePanel.Children) {
                CheckBox chk = element as CheckBox;
                if (null == chk || usingAllAudioSourceTypeSelector == chk) continue;

                if (selectorChecked) {
                    chk.Unchecked += ForceChecked;
                } else {
                    chk.Unchecked -= ForceChecked;
                }
                chk.IsChecked = usingAllAudioSourceTypeSelector.IsChecked;
            }

            if (selectorChecked) {
                audioSourceSelector.Unchecked += ForceChecked;
            } else {
                audioSourceSelector.Unchecked -= ForceChecked;
            }
        }

        private void autoDetectVideoFpsSelector_Checked(object sender, RoutedEventArgs e)
        {
            videoFpsInput.IsEnabled = false;
            videoFpsInput.Text = "AUTO";
        }

        private void autoDetectVideoFpsSelector_UnChecked(object sender, RoutedEventArgs e)
        {
            videoFpsInput.IsEnabled = true;
            videoFpsInput.Text = "24000/1001";
        }
    }
}
