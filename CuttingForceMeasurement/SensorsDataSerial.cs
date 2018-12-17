using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Реализует интерфейс для чтения данных <c>SensorsData</c>
    /// Читает данные от аналоговых датчиков
    /// </summary>
    public class SensorsDataSerial : SensorsData
    {
        const int SDI_PARAMS_COUNT = 6;
        const int AMPERAGE_INDEX = 4;

        private string SerialPortName { get; set; }
        private SerialPort Sensors { get; set; }
        
        private double? SensorsTime = null;

        public SensorsDataSerial(MainWindow main, string serial) : base(main)
        {
            SerialPortName = serial;
            Sensors = new SerialPort
            {
                PortName = SerialPortName,
                BaudRate = main.CurrentSettings.BaudRate,
                DataBits = main.CurrentSettings.DataBits,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = Encoding.Default,
                ReadTimeout = 10000
            };
            try
            {
                Sensors.Open();
            } catch (Exception e)
            {
                Main.TriggerErrorReading(e);
            }
        }

        protected override void Next()
        {
            if (!IsReading) return;

            string[] separatingChars = { " ", "\t"};
            try
            {
                if (!Sensors.IsOpen) return;

                string input = Sensors.ReadLine();
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                {
                   throw new Exception("Пустая строка");
                }
                // В системе используется русский разделитель целой и дробной части
                input = input.Replace('.', ',');

                string[] sdiParamsString = input.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                double[] sdiParams = new double[SDI_PARAMS_COUNT];

                bool machineStoped = true;
                
                for (int i = 0; i < SDI_PARAMS_COUNT; i ++)
                {
                    sdiParams[i] = Double.Parse(sdiParamsString[i]);
                    
                }
                if (sdiParams[AMPERAGE_INDEX] != 0)
                {
                    machineStoped = false;
                }
                if (machineStoped)
                {
                    return;
                }
                if (SensorsTime == null)
                {
                    SensorsTime = sdiParams[0];
                }

                var curTime = sdiParams[0] - (double)SensorsTime;
                SensorDataItem sdi = new SensorDataItem()
                {
                    Time = curTime,
                    Acceleration = sdiParams[1] * Main.CurrentSettings.AccelerationCoef,
                    Force = sdiParams[2] * Main.CurrentSettings.ForceCoef,
                    Voltage = sdiParams[3] * Main.CurrentSettings.VoltageCoef,
                    Amperage = sdiParams[4] * Main.CurrentSettings.AmperageCoef,
                    Rpm = sdiParams[5] * Main.CurrentSettings.RpmCoef,
                };
                Main.UpdateSensorsData(sdi);
                Main.SetTimeReading(curTime);
            }
            catch (Exception e)
            {
                /* 
                Console.WriteLine("Ошибка при чтении");
                Main.TriggerErrorReading(e);
                */
            }
            
        }

        public override void OnStop()
        {
            try
            {
                Sensors.Close();
            } catch(Exception e)
            {
                Console.WriteLine("Ошибкапри закрытии");
                Main.TriggerErrorReading(e);
            }
        }
    }
}
