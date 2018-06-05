using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GeneralPigeonTest
{
    public class Program
    {
        static CTRE.Phoenix.Sensors.PigeonIMU _pidgey = new CTRE.Phoenix.Sensors.PigeonIMU(8);


        public static void Main()
        {
            _pidgey.SetStatusFramePeriod(CTRE.Phoenix.Sensors.PigeonIMU_StatusFrame.RawStatus_4_Mag, 5, 10);
            _pidgey.SetStatusFramePeriod(CTRE.Phoenix.Sensors.PigeonIMU_StatusFrame.CondStatus_3_GeneralAccel, 5, 10);
            _pidgey.SetStatusFramePeriod(CTRE.Phoenix.Sensors.PigeonIMU_StatusFrame.CondStatus_10_SixDeg_Quat, 5, 10);
            _pidgey.SetStatusFramePeriod(CTRE.Phoenix.Sensors.PigeonIMU_StatusFrame)
            while (true)
            {
                /* print the three analog inputs as three columns */
                Debug.Print("Counter Value: " + counter);

                /* increment counter */
                ++counter; /* try to land a breakpoint here and hover over 'counter' to see it's current value.  Or add it to the Watch Tab */

                /* wait a bit */
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
