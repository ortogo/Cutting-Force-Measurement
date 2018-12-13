using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    public class SensorsDataRandom : SensorsData
    {
        private Random Rand;

        public SensorsDataRandom(MainWindow main) : base(main)
        {
            Rand = new Random();
        }

        protected override void Next(int count)
        {
            SensorDataItem se = new SensorDataItem
            {
                Time = count,
                Acceleration = Rand.Next(0, 100) * Main.CurrentSettings.AccelerationCoef,
                Force = Rand.Next(0, 100) * Main.CurrentSettings.ForceCoef,
                Voltage = Rand.Next(200, 220) * Main.CurrentSettings.VoltageCoef,
                Amperage = (3.5 + Math.Round(Rand.NextDouble(), 2) * 2.5) * Main.CurrentSettings.AmperageCoef,
                Rpm = Rand.Next(2800, 2975) * Main.CurrentSettings.RpmCoef
            };
            Main.UpdateSensorsData(se);
            Main.SetTimeReading(count);
            Task.Delay(100).Wait();
        }
    }
}
