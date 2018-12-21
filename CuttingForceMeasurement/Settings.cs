using System;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Предоставляет объект настроек пользователя. Возможно клонирование.
    /// Настройки хранятся в директории данных приложений пользователя <c>AppData/Local/CuttingForceMeasurement</c>
    /// Формат хранения JSON
    /// </summary>
    public class Settings : ICloneable
    {

        /// <summary>
        /// Скорость передачи по сериальному порту
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
