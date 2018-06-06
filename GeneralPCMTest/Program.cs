using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Controller;
using CTRE.Gadgeteer.Module;
using CTRE.Phoenix;

/* Jacob's test upadated to Phoenix Framwork */
namespace GeneralPCMTest
{
    public class Program
    {
        /* Hardware */
        static TalonSRX _talon = new TalonSRX(0);
        static GameController _gamepad = new GameController(UsbHostDevice.GetInstance());

        /* All buttons */
        static bool[] _currentBtns = new bool[13];
        static bool[] _previousBtns = new bool[13];

        public static void Main()
        {
            /* Enable the status frame used to test configFactorDefault */
            _talon.SetStatusFramePeriod(StatusFrameEnhanced.Status_7_CommStatus, 10, 10);

            while (true)
            {
                /* Always enable actuators during this test */
                CTRE.Phoenix.Watchdog.Feed();

                /* Just get all buttons */
                _gamepad.GetButtons(_currentBtns);

                CTRE.ErrorCode _error;
                bool errorState = false;
                if (_currentBtns[2] && !_previousBtns[2])
                {
                    Debug.Print("Peforming random sets...");
                    /* Do a bunch of sets */
                    _error = _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eContinuousCurrentLimitAmps, 10, 0, 0, 10);
                    if (_error != 0)
                        errorState = true;
                    //Thread.Sleep(1000);
                    _error = _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.ePeakCurrentLimitAmps, 17, 0, 0, 10);
                    if (_error != 0)
                        errorState = true;
                    //Thread.Sleep(1000);
                    //_error = _talon.ConfigOpenloopRamp(2.25f, 10);
                    _error = _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eOpenloopRamp, 2.25f, 0, 0, 10);
                    if (_error != 0)
                        errorState = true;
                    //Thread.Sleep(1000);
                    _error = _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.ePeakCurrentLimitMs, 100, 0, 0, 10);
                    if (_error != 0)
                        errorState = true;
                    //Thread.Sleep(1000);
                    _error = _talon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eMotMag_VelCruise, 1000, 0, 0, 10);
                    if (_error != 0)
                        errorState = true;
                    //Thread.Sleep(1000);
                    Debug.Print("Sets completed: " + (errorState ? "Error Somewhere" : "No Errors"));
                }
                else if (_currentBtns[9] && !_previousBtns[9])
                {
                    /* This does not work, as expected */
                    Debug.Print("All configurations cleared with whatevercode");
                    _talon.ConfigSetParameter((CTRE.Phoenix.LowLevel.ParamEnum)500, 0, 0, 0, 10);
                }
                else if (_currentBtns[10] && !_previousBtns[10])
                {
                    /* This should clear all configurations */
                    Debug.Print("All configurations cleared with 0xA5A5");
                    _talon.ConfigSetParameter((CTRE.Phoenix.LowLevel.ParamEnum)500, 0xA5A5, 0, 0, 10);
                }
                else if (_currentBtns[6] && !_previousBtns[6])
                {
                    /* Read all configurations */
                    Debug.Print("Reading all configurations");
                    float[] Configurations = new float[5];
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eContinuousCurrentLimitAmps, out Configurations[0]);
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.ePeakCurrentLimitAmps, out Configurations[1]);
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eOpenloopRamp, out Configurations[2]);
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.ePeakCurrentLimitMs, out Configurations[3]);
                    _talon.ConfigGetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eMotMag_VelCruise, out Configurations[4]);
                    Debug.Print("CCLA: " + Configurations[0] + " PCLA: " + Configurations[1] + " OLR: " + Configurations[2] + " PCLM: " + Configurations[3] + " MMVC: " + Configurations[4]);
                }
                System.Array.Copy(_currentBtns, _previousBtns, _currentBtns.Length);

                Thread.Sleep(5);
            }
        }
    }
}
