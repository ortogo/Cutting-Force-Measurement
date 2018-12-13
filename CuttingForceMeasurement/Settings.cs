using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CuttingForceMeasurement
{
    public class Settings: ICloneable
    {
        private const string FILENAME = "settings.json";
        private const string LOCALAPPDIR = "CuttingForceMeasurement";
        readonly string PathLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // Скорсть передачи по сериальному порту
        public int BaudRate { get; set; }
        // Количество бит в передаваемом байте
        public int DataBits { get; set; }
        // Ускорение м/с^2
        public double AccelerationCoef { get; set; }
        // Усилие кН
        public double ForceCoef { get; set; }
        // Напряжение, В
        public double VoltageCoef { get; set; }
        // Ток, А
        public double AmperageCoef { get; set; }
        // Частота оборотов, об/микросек
        public double RpmCoef { get; set; }
        // Демо режим
        public bool DemoMode { get; set; }

        public void Save()
        {
            string serialized = JsonConvert.SerializeObject(this);
            
            File.WriteAllText(GetFilePath(), serialized);
        }

        private string GetFilePath()
        {
            string filePath = $"{PathLocal}\\{LOCALAPPDIR}\\{FILENAME}";
            return filePath;
        }

        private string GetDirectoryPath()
        {
            
            string directoryPath = $"{PathLocal}\\{LOCALAPPDIR}";
            return directoryPath;
        }

        public void Load()
        {
            var fi = new FileInfo(GetFilePath());
            if (!fi.Exists)
            {
                DefaultSave();
            } else
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

        public object Clone()
        {
            return new Settings {
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
