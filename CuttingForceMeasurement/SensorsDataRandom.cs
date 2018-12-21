using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Генерирует псевдослучайные данные датчиков. Используется для демоснстрации и тестирования поведения представления
    /// </summary>
    public class SensorsDataRandom : SensorsData
    {
        private Random Rand;
        private int count = 0;

        public SensorsDataRandom(MainWindow main) : base(main)
        {
            Rand = new Random();
        }

        protected override void Next()
        {
            SensorDataItem se = new SensorDataItem
            {
                Time = count,
                Acceleration = (Rand.Next(0, 100)/100) * Main.settings.AccelerationCoef,
                Force = Rand.Next(0, 5) * Main.settings.ForceCoef,
                Voltage = 220 * Main.settings.VoltageCoef,
                Amperage = (3.5 + Math.Round(Rand.NextDouble(), 2) * 2.5) * Main.settings.AmperageCoef,
                Rpm = Rand.Next(1300, 1475) * Main.settings.RpmCoef
            };
            Main.UpdateSensorsData(se);
            count++;
            Main.SetTimeReading((double)count);
            Task.Delay(100).Wait();
        }
    }
}
