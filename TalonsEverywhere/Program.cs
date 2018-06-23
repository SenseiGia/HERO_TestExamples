using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;

namespace TalonsEverywhere
{
    public class Program
    {
        /* Values used to talons */
        static int kTimeout = 10;
        static int kNumOfTalons = 16;
        static TalonSRX[] _talonCollection = new TalonSRX[kNumOfTalons];
        static int _talonStartIndex = 0;

        /* Tracking variables */
        static int passCount = 0;
        static int failCount = 0;
        static int talonCount = 0;

        public static void Main()
        {
            /* Create Talons */
            for (int i = 0; i < (kNumOfTalons); i++)
            {
                _talonCollection[i] = new TalonSRX(i + _talonStartIndex);
            }
            ///* Set Status Frame 7, Check for Dropped Transmit */
            //foreach (TalonSRX _talon in _talonCollection)
            //    _talon.SetStatusFramePeriod(StatusFrameEnhanced.Status_7_CommStatus, 150, kTimeout);


            /* Init done, indicate current timeout value */
            Debug.Print("Teleop Init completed, timeout: " + kTimeout);
            int value = 0;

            while (true)
            {
                /* Reset errorDetected and error */
                CTRE.ErrorCode error;
                bool errorDetected = false;

                CTRE.Phoenix.Watchdog.Feed();

                /* Second indexing method */
                if (talonCount < kNumOfTalons)
                {
                    /* Get current Talon Device ID */
                    int talonNumber = _talonCollection[talonCount].GetDeviceID();

                    error = _talonCollection[talonCount].Config_kP(0, 1, kTimeout);                         // 310
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kI(0, 1, kTimeout);                         // 311
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kD(0, 1, kTimeout);                         // 312
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kF(0, 1, kTimeout);                         // 313
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_IntegralZone(0, 100, kTimeout);             // 314
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigAllowableClosedloopError(0, 100, kTimeout);  // 315
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigNominalOutputForward(0.5f, kTimeout);        // 306
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigNominalOutputReverse(-0.5f, kTimeout);       // 308
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigPeakOutputForward(1, kTimeout);              // 305
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigPeakOutputReverse(-1, kTimeout);             // 307
                    if (error != CTRE.ErrorCode.OK)
                        errorDetected = true;

                    if (!errorDetected)
                        passCount++;
                    else if (errorDetected)
                        failCount++;

                    if(talonNumber == 13)
                    {
                        /* test setselectesendosr*/
                        value++;
                        _talonCollection[talonCount].SetSelectedSensorPosition(value, 0, kTimeout);
                        _talonCollection[talonCount].GetSelectedSensorPosition(0);
                    }

                    byte[] temp1 = new byte[8];
                    ulong data1 = (ulong)BitConverter.ToUInt64(temp1, 0);
                    CTRE.Native.CAN.Send(0x0000A00A, data1, 8, 0);

                    Debug.Print("Success Count: " + passCount + " Fail Count: " + failCount + " Talon #: " + talonNumber);
                    talonCount++;
                }
                else
                {
                    talonCount = 0;
                }

                Thread.Sleep(10);
            }
        }
    }
}

