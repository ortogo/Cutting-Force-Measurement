using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    public class SensorDataItem
    {
        // цикл 50 мидисек
        // милисекунды
        public double Time { get; set; }
        // Ускорение м/с^2
        public double Acceleration { get; set; }
        // Усилие кН
        public double Force { get; set; }
        // Напряжение, В
        public double Voltage { get; set; }
        // Ток, А
        public double Amperage { get; set; }
        // Частота оборотов, об/микросек
        public double Rpm { get; set; }

        public override string ToString()
        {
            return $"{Time}\t{Acceleration}\t{Force}\t{Voltage}\t{Amperage}\t{Rpm}";
        }
    }
}
