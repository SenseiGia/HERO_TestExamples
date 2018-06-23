using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix;

namespace PersistentCustomParamsTest
{
    public class Program
    {
        /* Hardware */
        static TalonSRX _talon = new TalonSRX(1);
        static PigeonIMU _pidgey = new PigeonIMU(_talon);
        static CANifier _canifier = new CANifier(1);
        static GameController _gamepad = new GameController(UsbHostDevice.GetInstance());

        static bool[] _currentBtns = new bool[13];
        static bool[] _previousBtns = new bool[13];
        static int kTimeout = 20;

        public static void Main()
        {
            /* Don't initalize anything */
            while (true)
            {
                _gamepad.GetButtons(_currentBtns);
                /* Set custom configs based on which shoulder buttons is pressed */
                if(_currentBtns[5] && !_previousBtns[5])
                {
                    /* set custom config 0 on left bumper */
                    _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 10, 0, 0, kTimeout);
                    _pidgey.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 20, 0, 0, kTimeout);
                    _canifier.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 30, 0, 0, kTimeout);
                    Debug.Print("First Configs Completed 0");
                }
                else if(_currentBtns[6] && !_previousBtns[6])
                {
                    /* set custom config 0 on right bumper */
                    _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 40, 0, 0, kTimeout);
                    _pidgey.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 50, 0, 0, kTimeout);
                    _canifier.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 60, 0, 0, kTimeout);
                    Debug.Print("Second Configs Completed 0");
                }
                else if(_currentBtns[7] && !_previousBtns[7])
                {
                    /* set custom config 1 on left trigger */
                    _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 20f, 0, 1, kTimeout);
                    _pidgey.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 25000, 0, 1, kTimeout);
                    _canifier.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 987654, 0, 1, kTimeout);
                    Debug.Print("Second Configs Completed 1");
                }
                else if (_currentBtns[8] && !_previousBtns[8])
                {
                    /* set custom config 1 on right trigger */
                    _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 2.123f, 0, 1, kTimeout);
                    _pidgey.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 575, 0, 1, kTimeout);
                    _canifier.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 33f, 0, 1, kTimeout);
                    Debug.Print("Second Configs Completed 1");
                }

                /* Get Custom config */
                if (_currentBtns[1] && !_previousBtns[1])
                {
                    /* Set Custom config on X-Press */
                    float[] ConfigValues = new float[3];
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, out ConfigValues[0], 0, kTimeout);
                    ConfigValues[1] = _pidgey.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 0, kTimeout);
                    ConfigValues[2] = (float)_canifier.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 0, kTimeout);

                    Debug.Print(" Talon's config: " + ConfigValues[0] + " Pigeon's config: " + ConfigValues[1] + " Canifier's config: " + ConfigValues[2]);
                }
                else if (_currentBtns[3] && !_previousBtns[3])
                {
                    /* Set Custom config on B-Press */
                    float[] ConfigValues = new float[3];
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, out ConfigValues[0], 1, kTimeout);
                    ConfigValues[1] = _pidgey.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 1, kTimeout);
                    ConfigValues[2] = (float)_canifier.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eCustomParam, 1, kTimeout);

                    Debug.Print(" Talon's config: " + ConfigValues[0] + " Pigeon's config: " + ConfigValues[1] + " Canifier's config: " + ConfigValues[2]);
                }
                System.Array.Copy(_currentBtns, _previousBtns, _previousBtns.Length);

                /* wait a bit */
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
