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
        static int kTimeout = 14;
        static int kNumOfTalons = 17;
        static TalonSRX[] _talonCollection = new TalonSRX[kNumOfTalons];
        static int _talonStartIndex = 1;

        /* Tracking variables */
        static int passCount = 0;
        static int failCount = 0;
        static int talonCount = 0;

        public static void Main()
        {
            /* Create our 12 Talons... */
            for (int i = 0; i < (kNumOfTalons); i++)
            {
                _talonCollection[i] = new TalonSRX(i + _talonStartIndex);
            }
            foreach (TalonSRX _talon in _talonCollection)
                _talon.SetStatusFramePeriod(StatusFrameEnhanced.Status_7_CommStatus, 250, kTimeout);
            /* Everything should be default */
            Debug.Print("Teleop Init completed, timeout: " + kTimeout);

            while (true)
            {
                CTRE.ErrorCode error;
                bool errorDetected = false;

                /* Second indexing method */
                if (talonCount < kNumOfTalons)
                {
                    int talonNumber = _talonCollection[talonCount].GetDeviceID();

                    /* Do 10 configs per talon */
                    error = _talonCollection[talonCount].Config_kP(0, 1, kTimeout);                         // 310
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kI(0, 1, kTimeout);                         // 311
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kD(0, 1, kTimeout);                         // 312
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_kF(0, 1, kTimeout);                         // 313
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].Config_IntegralZone(0, 100, kTimeout);             // 314
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigAllowableClosedloopError(0, 100, kTimeout);  // 315
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    //error = _talonCollection[talonCount].ConfigContinuousCurrentLimit(10, kTimeout);        // 360
                    //if (error != CTRE.ErrorCode.OK)
                    //    //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                    //    errorDetected = true;
                    //error = _talonCollection[talonCount].ConfigPeakCurrentDuration(10, kTimeout);           // 361
                    //if (error != CTRE.ErrorCode.OK)
                    //    //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                    //    errorDetected = true;
                    //error = _talonCollection[talonCount].ConfigPeakCurrentLimit(15, kTimeout);              // 362
                    //if (error != CTRE.ErrorCode.OK)
                    //    //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                    //    errorDetected = true;
                    error = _talonCollection[talonCount].ConfigNominalOutputForward(0.5f, kTimeout);        // 306
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigNominalOutputReverse(-0.5f, kTimeout);       // 308
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigPeakOutputForward(1, kTimeout);              // 305
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;
                    error = _talonCollection[talonCount].ConfigPeakOutputReverse(-1, kTimeout);             // 307
                    if (error != CTRE.ErrorCode.OK)
                        //if (error != CTRE.ErrorCode.CAN_TX_FULL)
                        errorDetected = true;

                    ///* just see how fast this loop goes */
                    //byte[] temp1 = new byte[8];
                    //ulong data1 = (ulong)BitConverter.ToUInt64(temp1, 0);
                    //CTRE.Native.CAN.Send(0x0000A00A, data1, 8, 0);

                    if (!errorDetected)
                        passCount++;
                    else if (errorDetected)
                        failCount++;
                    
                    Debug.Print("Config Success Count: " + passCount + " Config Fail Count: " + failCount + " Talon #: " + talonNumber);
                    talonCount++;
                }
                else
                {
                    talonCount = 0;
                }

                Thread.Sleep(5);
            }
        }
    }
}

