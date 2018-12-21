using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CuttingForceMeasurement.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : UserControl
    {

        private Regex doubleRegex = new Regex(@"^-?(\d*)\.?(\d*)$");
        private Regex numberRegex = new Regex(@"\d");

        public SettingsDialog()
        {
            InitializeComponent();
        }

        public delegate void OnDemoModeEvent(object sender, RoutedEventArgs e);
        public delegate void OffDemoModeEvent(object sender, RoutedEventArgs e);
        public event OnDemoModeEvent OnDemoMode;
        public event OffDemoModeEvent OffDemoMode;

        /// <summary>
        /// Ограничение ввода. Разрешает вводить только дробные числа с точкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Double_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = ((TextBox)sender);
            // то что сейчас введено
            string currentInput = tb.Text;
            // положение каретки
            int caretIndex = tb.CaretIndex;
            // текущий символ ввода
            string inputedSymbol = e.Text;
            // Замена запятой на точку
            if (inputedSymbol == ",")
            {
                inputedSymbol = ".";
            }
            // если есть выбраный текст, то удалить его и изменить положение каретки
            if (tb.SelectedText.Length > 0)
            {
                currentInput = currentInput.Remove(tb.SelectionStart, tb.SelectionLength);
                if (tb.SelectionStart != caretIndex)
                {
                    tb.CaretIndex = tb.SelectionStart;
                    caretIndex = tb.SelectionStart;
                }
            }
            // вставляем введенный символ в строку
            currentInput = currentInput.Insert(caretIndex, inputedSymbol);
            // проверка на валидность ввода, замена текста в поле, установка каретки на нужную позицию
            if (doubleRegex.IsMatch(currentInput))
            {

                tb.Text = currentInput;
                tb.CaretIndex = caretIndex + 1;
            }
            // прерываем ввод, так как текст в поле изменяется вручную
            e.Handled = true;
        }

        /// <summary>
        /// Ограничение ввода. Разрешает вводить только числа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = ((TextBox)sender);
            if (numberRegex.IsMatch(e.Text))
            {
                var pos = tb.CaretIndex;
                tb.Text = tb.Text.Insert(pos, e.Text);
                tb.CaretIndex = pos + 1;
            }

            e.Handled = true;
        }

        private void DemoMode_Checked(object sender, RoutedEventArgs e)
        {
            OnDemoMode?.Invoke(sender, e);
        }

        private void DemoMode_Unchecked(object sender, RoutedEventArgs e)
        {
            OffDemoMode?.Invoke(sender, e);
        }
    }
}
