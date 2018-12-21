using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Логика взаимодействия для InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
            SetContent();
            Closing += OnClosing;
        }

        private void SetContent()
        {
            string path = @".\Readme.txt";

            // Если ридми не существует, то заполнить данными по умолчанию
            if (!File.Exists(path))
            {
                Console.WriteLine("Readme not exists, create him");
                File.WriteAllLines(path, defaultReadme);
            }

            string[] readText = File.ReadAllLines(path);
            StackPanel panel = new StackPanel
            {
                Margin = new Thickness(8)
            };
            for (int i = 0; i < readText.Length; i++)
            {
                var line = readText[i].Trim();
                if (line.Length <= 0) continue;
                // пропускаем комментарий
                if (line.StartsWith(";")) continue;
                // выводим заголовок
                if (line.StartsWith("#"))
                {
                    if (panel.Children.Count > 0)
                    {
                        InformationContent.Children.Add(panel);
                        panel = new StackPanel
                        {
                            Margin = new Thickness(8)
                        };
                    }
                    InformationContent.Children.Add(CreateHeader(line));
                    
                }
                else
                {
                    panel.Children.Add(TextLine(line));
                }
            }
            if (panel.Children.Count > 0)
            {
                InformationContent.Children.Add(panel);
            }
        }

        private TextBlock CreateHeader(string text)
        {
            /*
             * Style="{StaticResource MaterialDesignTitleTextBlock}" Foreground="{DynamicResource PrimaryHueMidBrush}"
             */
            TextBlock block = new TextBlock
            {
                Text = text,
                Style = (Style)TryFindResource("MaterialDesignTitleTextBlock"),
                Foreground = (Brush)TryFindResource("PrimaryHueMidBrush")
            };
            return block;
        }

        private TextBlock TextLine(string text)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap
            };
            return block;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            Owner.Activate();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NavigationBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private string[] defaultReadme =
        {
            "; Заголовок",
            "# Cutting Force Measurement v1.0.1.1",
            "",
            "Приложение позволит Вам записать в реальном времени показания датчиков.",
            "",
            "# Инструкция",
            "",
            "1. Для начала измерений запустите приложение.",
            "2. Введите ваши данные.",
            "3. В поле для выбора COM выберите тот, к которому подключено устройство. В большинстве случаев он будет выбран по умолчанию. Если оборудование переподключено или подключено позже нажмите кнопку \"Обновить\" (фиолетовая стрелка по кругу).",
            "4. Запустите станок.",
            "5. Нажмите кнопку \"Записать\" и получите необходимое количество информации.",
            "6. Нажмите кнопку \"Остановить\".",
            "7. Сохраните информацию с помощью кнопки \"Сохранить\". Имя файла будет создано автоматически и расположения сохранения будет рабочий стол.Эти данные можно изменить.",
            "8. После сохранени убедитесь, что все прошло успешно.",
            "9. Для повторной записи можно нажать кнопку \"Записать\", тогда таблица результатов будет очищена и запись начнется сначала.",
        };
    }
}
