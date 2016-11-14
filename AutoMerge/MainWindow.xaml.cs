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

        private TextBox audioLanguageInput = null;
        private CheckBox usingAllAudioSourceTypeSelector = null;

        public MainWindow()
        {
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
                btn.Checked += (s, e) => { videoFpsInput.IsEnabled = true; };
                btn.Unchecked += (s, e) => { videoFpsInput.IsEnabled = false; };
            }
            if (fileType.UIBaseCheckBox != null) {
                btn.Checked += (s, e) => { (FindName(fileType.UIBaseCheckBox) as CheckBox).IsChecked = true; };
            }

            parentPanel.Children.Add(btn);
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
                    Utility.ShowInformationMessageBox("不能不选视频轨");
                    IsEnabled = true;
                } else {
                    count++;
                }
                checkBox.Tag = count;
            }
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                Utility.ShowInformationMessageBox("你居然什么输入类型都不选", "最少选一钟输入类型");
                return;
            }

            /* 视频输入 */
            if (videoSourceSelectorChecked) {
                EachSelectors(true, ref videoSourcePanel, tag => { videoType = tag as VideoSourceType?; });

                if (!videoType.HasValue) {
                    Utility.ShowInformationMessageBox("需要选择输入视频格式");
                    return;
                }
            }

            /* 音频输入 */
            if (audioSourceSelectorChecked) {
                EachSelectors(false, ref audioSourcePanel, tag => { audioType.Add((tag as AudioSourceType?).Value); });

                if (0 == audioType.Count) {
                    Utility.ShowInformationMessageBox("需要选择输入音频格式");
                    return;
                }
            }

            /* 视频输出 */
            EachSelectors(true, ref outputPanel, tag => { outputType = (tag as OutputType?); });
            if (!outputType.HasValue) {
                Utility.ShowInformationMessageBox("需要选择输出视频格式");
                return;
            }

            /* FPS设定 */
            if (Settings.OutputFileTypes[outputType.Value].NeedEnterFps && string.IsNullOrEmpty(videoFpsInput.Text)) {
                Utility.ShowInformationMessageBox("需要设置输出视频的FPS");
                return;
            }

            startButton.IsEnabled = false;

            var muxer = new Muxer();
            muxer.StartMux(new Muxer.MuxingConfiguration {
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

            MessageBoxResult mbResult;
            do {
                mbResult = MessageBox.Show("请先播放检查封装成品，再上传！", "注意", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
            } while (MessageBoxResult.No == mbResult);

            startButton.IsEnabled = true;
        }
    }
}
