﻿using System;
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
                Acceleration = (Rand.Next(0, 100)/100.0),
                Force = Rand.Next(0, 5),
                Voltage = 220,
                Amperage = (3.5 + Math.Round(Rand.NextDouble(), 2) * 2.5),
                Rpm = Rand.Next(1300, 1475)
            };
            Main.UpdateSensorsData(se);
            count++;
            Main.SetTimeReading((double)count);
            Task.Delay(100).Wait();
        }
    }
}
