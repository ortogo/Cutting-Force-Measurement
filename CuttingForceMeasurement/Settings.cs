using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Хранит настройки пользователя. Возможно клонирование.
    /// Настройки хранятся в директории данных приложений пользователя <c>AppData/Local/CuttingForceMeasurement</c>
    /// Формат хранения JSON
    /// </summary>
    /// <remarks>Включает в себя методы для сохранения и чтения настроек</remarks>
    public class Settings : ICloneable
    {
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
        /// Скорсть передачи по сериальному порту
        /// </summary>
        public int BaudRate { get; set; }
        /// <summary>
        /// Количество бит в передаваемом байте
        /// </summary>
        public int DataBits { get; set; }
        /// <summary>
        /// Ускорение м/с^2
        /// </summary>
        public double AccelerationCoef { get; set; }
        /// <summary>
        /// Усилие кН
        /// </summary>
        public double ForceCoef { get; set; }
        /// <summary>
        /// Напряжение, В
        /// </summary>
        public double VoltageCoef { get; set; }
        /// <summary>
        /// Ток, А
        /// </summary>
        public double AmperageCoef { get; set; }
        /// <summary>
        /// Частота оборотов, об/микросек
        /// </summary>
        public double RpmCoef { get; set; }
        /// <summary>
        /// Демо режим
        /// </summary>
        public bool DemoMode { get; set; }

        /// <summary>
        /// Сохраняет настройки
        /// </summary>
        public void Save()
        {
            string serialized = JsonConvert.SerializeObject(this);

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
                BaudRate = s.BaudRate;
                DataBits = s.DataBits;
                AccelerationCoef = s.AccelerationCoef;
                ForceCoef = s.ForceCoef;
                VoltageCoef = s.VoltageCoef;
                AmperageCoef = s.AmperageCoef;
                RpmCoef = s.RpmCoef;
                DemoMode = s.DemoMode;
            }

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
            BaudRate = 9600;
            DataBits = 8;
            AccelerationCoef = 1;
            ForceCoef = 1;
            VoltageCoef = 1;
            AmperageCoef = 1;
            RpmCoef = 1;
            DemoMode = false;
        }

        /// <summary>
        /// Копирование объекта настроек
        /// </summary>
        /// <returns>копия настроек</returns>
        public object Clone()
        {
            return new Settings
            {
                BaudRate = this.BaudRate,
                DataBits = this.DataBits,
                AccelerationCoef = this.AccelerationCoef,
                ForceCoef = this.ForceCoef,
                VoltageCoef = this.VoltageCoef,
                AmperageCoef = this.AmperageCoef,
                RpmCoef = this.RpmCoef,
                DemoMode = this.DemoMode,
            };
        }
    }
}
