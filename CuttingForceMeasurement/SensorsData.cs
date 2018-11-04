using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    public abstract class SensorsData
    {
        public bool IsReading { get; set; }
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
            int count = 0;
            IsReading = true;
            while(IsReading)
            {
                Next(count);
                count++;
            }
        }

        public virtual void OnStop()
        {

        }

        protected abstract void Next(int count);
    }
}
