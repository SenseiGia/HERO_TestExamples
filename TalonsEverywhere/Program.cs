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
        static int kNumOfTalons = 12;
        static TalonSRX[] _talonCollection = new TalonSRX[kNumOfTalons];

        /* Tracking variables */
        static int passCount = 0;
        static int failCount = 0;

        public static void Main()
        {
            /* Create our 12 Talons... */
            for (int i = 0; i < kNumOfTalons ; i++)
            {
                _talonCollection[i] = new TalonSRX(i);
            }

            /* Set all _talons to off */
            foreach (TalonSRX _talon in _talonCollection)
            {
                _talon.Set(ControlMode.PercentOutput, 0);

                _talon.ConfigContinuousCurrentLimit(10, kTimeout);
                _talon.Config_kF(0, 1, kTimeout);
                _talon.Config_kP(0, 1, kTimeout);
                _talon.Config_kI(0, 1, kTimeout);
                _talon.Config_kD(0, 1, kTimeout);
                _talon.ConfigPeakCurrentDuration(10, kTimeout);
                _talon.ConfigPeakCurrentLimit(15, kTimeout);
                _talon.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Absolute, 1, kTimeout);
                _talon.ConfigNominalOutputForward(0.5f, kTimeout);
                _talon.ConfigNominalOutputReverse(-0.5f, kTimeout);
                _talon.ConfigPeakOutputForward(1, kTimeout);
                _talon.ConfigPeakOutputReverse(-1, kTimeout);
                _talon.ConfigAllowableClosedloopError(1, 100, kTimeout);
                _talon.ConfigOpenloopRamp(2, kTimeout);
                _talon.ConfigClosedloopRamp(2, kTimeout);
            }
            Debug.Print("Initial Configuration Completed.");

            passCount = 0;
            failCount = 0;
            foreach (TalonSRX _talon in _talonCollection)
            {
                _talon.Config_kF(0, 1, kTimeout);
                _talon.Config_kP(0, 1, kTimeout);
                _talon.Config_kI(0, 1, kTimeout);
                _talon.Config_kD(0, 1, kTimeout);
            }
            Debug.Print("Teleop Init completed, timeout: " + kTimeout);

            while (true)
            {
                /* Not sure if we need to enable talons for this test... */
                CTRE.Phoenix.Watchdog.Feed();

                int errorDetected = 0;
                foreach (TalonSRX _talon in _talonCollection)
                {
                    /* Do  bunch of configs */
                    errorDetected += (int)_talon.ConfigContinuousCurrentLimit(10, kTimeout);
                    errorDetected += (int)_talon.Config_kF(0, 1, kTimeout);
                    errorDetected += (int)_talon.Config_kP(0, 1, kTimeout);
                    errorDetected += (int)_talon.Config_kI(0, 1, kTimeout);
                    errorDetected += (int)_talon.Config_kD(0, 1, kTimeout);
                    errorDetected += (int)_talon.ConfigPeakCurrentDuration(10, kTimeout);
                    errorDetected += (int)_talon.ConfigPeakCurrentLimit(15, kTimeout);
                    errorDetected += (int)_talon.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Absolute, 1, kTimeout);
                    errorDetected += (int)_talon.ConfigNominalOutputForward(0.5f, kTimeout);
                    errorDetected += (int)_talon.ConfigNominalOutputReverse(-0.5f, kTimeout);
                    errorDetected += (int)_talon.ConfigPeakOutputForward(1, kTimeout);
                    errorDetected += (int)_talon.ConfigPeakOutputReverse(-1, kTimeout);
                    errorDetected += (int)_talon.ConfigAllowableClosedloopError(1, 100, kTimeout);
                    errorDetected += (int)_talon.ConfigOpenloopRamp(2, kTimeout);
                    errorDetected += (int)_talon.ConfigClosedloopRamp(2, kTimeout);

                }
                if (errorDetected == 0)
                    passCount++;
                else if (errorDetected != 0)
                    failCount++;

                Debug.Print("Config Success Count: " + passCount + " Config Fail Count: " + failCount);
            }
        }
    }
}
