using System;
using Newtonsoft.Json;
using System.IO;

using CuttingForceMeasurement;

namespace CuttingForceMeasurement.ViewModels
{
    public class SettingsDialogViewModel : BaseViewModel
    {
        public Settings CurrentSettings = new Settings();
        public Settings PrevioslySettings;
        /// <summary>
        /// Название файла настроек
        /// </summary>
        private const string FILENAME = "settings.json";
        /// <summary>
        /// Имя директории настроек
        /// </summary>
        private const string LOCALAPPDIR = "CuttingForceMeasurement";
        /// <summary>
        /// Полный путь к директории <c>Local</c>
        /// </summary>
        readonly string PathLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Скорость передачи по сериальному порту
        /// </summary>
        public int BaudRate {
            get => CurrentSettings.BaudRate;
            set {
                CurrentSettings.BaudRate = value;
                OnPropertyChanged(nameof(BaudRate));
            }
        }
        /// <summary>
        /// Количество бит в передаваемом байте
        /// </summary>
        public int DataBits {
            get => CurrentSettings.DataBits;
            set
            {
                CurrentSettings.DataBits = value;
                OnPropertyChanged(nameof(DataBits));
            }
        }
        /// <summary>
        /// Ускорение м/с^2
        /// </summary>
        public double AccelerationCoef {
            get => CurrentSettings.AccelerationCoef;
            set
            {
                CurrentSettings.AccelerationCoef = value;
                OnPropertyChanged(nameof(AccelerationCoef));
            }
        }
        /// <summary>
        /// Усилие кН
        /// </summary>
        public double ForceCoef {
            get => CurrentSettings.ForceCoef;
            set
            {
                CurrentSettings.ForceCoef = value;
                OnPropertyChanged(nameof(ForceCoef));
            }
        }
        /// <summary>
        /// Напряжение, В
        /// </summary>
        public double VoltageCoef {
            get => CurrentSettings.VoltageCoef;
            set
            {
                CurrentSettings.VoltageCoef = value;
                OnPropertyChanged(nameof(VoltageCoef));
            }
        }
        /// <summary>
        /// Ток, А
        /// </summary>
        public double AmperageCoef {
            get => CurrentSettings.AmperageCoef;
            set
            {
                CurrentSettings.AmperageCoef = value;
                OnPropertyChanged(nameof(AmperageCoef));
            }
        }
        /// <summary>
        /// Частота оборотов, об/микросек
        /// </summary>
        public double RpmCoef {
            get => CurrentSettings.RpmCoef;
            set
            {
                CurrentSettings.RpmCoef = value;
                OnPropertyChanged(nameof(RpmCoef));
            }
        }
        /// <summary>
        /// Демо режим
        /// </summary>
        public bool DemoMode {
            get => CurrentSettings.DemoMode;
            set
            {
                CurrentSettings.DemoMode = value;
                OnPropertyChanged(nameof(DemoMode));
            }
        }

        /// <summary>
        /// Сохраняет настройки
        /// </summary>
        public void Save()
        {
            string serialized = JsonConvert.SerializeObject(CurrentSettings);

            File.WriteAllText(GetFilePath(), serialized);
        }

        /// <summary>
        /// Возыращает путь к файлу настроек
        /// </summary>
        /// <returns></returns>
        private string GetFilePath()
        {
            string filePath = $"{PathLocal}\\{LOCALAPPDIR}\\{FILENAME}";
            return filePath;
        }

        /// <summary>
        /// Возвращает путь к директории хранения настроек
        /// </summary>
        /// <returns></returns>
        private string GetDirectoryPath()
        {

            string directoryPath = $"{PathLocal}\\{LOCALAPPDIR}";
            return directoryPath;
        }

        /// <summary>
        /// Читает настройки из хранилища. В случае отсутствия файла настроек, он создается
        /// </summary>
        public void Load()
        {
            var fi = new FileInfo(GetFilePath());
            if (!fi.Exists)
            {
                DefaultSave();
            }
            else
            {
                string filePath = GetFilePath();
                string serialized = File.ReadAllText(filePath);
                // if settings file empty
                if (string.IsNullOrEmpty(serialized))
                {
                    DefaultSave();
                    return;
                }
                Settings s = JsonConvert.DeserializeObject<Settings>(serialized);
                Set(s);
            }
        }

        public void Set(Settings s)
        {
            BaudRate = s.BaudRate;
            DataBits = s.DataBits;
            AccelerationCoef = s.AccelerationCoef;
            ForceCoef = s.ForceCoef;
            VoltageCoef = s.VoltageCoef;
            AmperageCoef = s.AmperageCoef;
            RpmCoef = s.RpmCoef;
            DemoMode = s.DemoMode;
        }

        /// <summary>
        /// Создает новый файл настроек
        /// </summary>
        private void DefaultSave()
        {
            var fi = new FileInfo(GetFilePath());
            DefaultInit();
            var dirInfo = new DirectoryInfo(GetDirectoryPath());
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            fi.Create().Close();
            Save();
        }

        /// <summary>
        /// Настройки по умолчанию
        /// </summary>
        private void DefaultInit()
        {
            BaudRate = 115200;
            DataBits = 8;
            AccelerationCoef = 1;
            ForceCoef = 1;
            VoltageCoef = 1;
            AmperageCoef = 1;
            RpmCoef = 1;
            DemoMode = false;
        }
    }
}
