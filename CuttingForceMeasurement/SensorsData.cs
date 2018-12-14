using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Класс для управления потоком чтения данных
    /// </summary>
    public abstract class SensorsData
    {
        /// <summary>
        /// Состояние чтения, читает или нет
        /// </summary>
        public bool IsReading { get; set; }
        /// <summary>
        /// Текущее окно, которое выводит промежуточный результат чтения
        /// </summary>
        protected MainWindow Main { get; set; }

        public SensorsData(MainWindow main)
        {
            Main = main;
            IsReading = false;
        }

        public void Stop()
        {
            IsReading = false;
            OnStop();
        }

        public void Read()
        {
            IsReading = true;
            while(IsReading)
            {
                Next();
            }
        }

        /// <summary>
        /// Событие принудительной остановки чтения
        /// </summary>
        public virtual void OnStop()
        {

        }

        /// <summary>
        /// Исолпользуется для выполнения операции чтения
        /// </summary>
        protected abstract void Next();
    }
}
