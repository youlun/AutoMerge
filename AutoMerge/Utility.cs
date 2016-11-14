using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoMerge
{
    internal static class Utility
    {
        internal static bool ShowQuestionMessageBox(
            string messageBoxText,
            string caption,
            MessageBoxResult defaultResult = MessageBoxResult.Yes
        ) {
            MessageBoxResult mbResult = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, defaultResult);
            return MessageBoxResult.Yes == mbResult;
        }

        internal static void ShowInformationMessageBox(string messageBoxText, string caption = "提示") {
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
