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
        static PneumaticControlModule _pcm = new PneumaticControlModule(0);
        static GameController _gamepad = new GameController(UsbHostDevice.GetInstance());
        static DisplayModule _displayModule = new DisplayModule(CTRE.HERO.IO.Port8, DisplayModule.OrientationType.Landscape);

        /* Font Resources */
        static Font _smallFont = Properties.Resources.GetFont(Properties.Resources.FontResources.small);
        static Font _bigFont = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);

        public static void Main()
        {
            /* Start the compressor */
            _pcm.StartCompressor();

            /* Tracking variables */
            bool lastCompState = false;
            bool CompState = false;
            int compStartCount = 0;

            long now = DateTime.Now.Ticks;

            /* Display Module Elements */
            DisplayModule.LabelSprite _labelTitle = _displayModule.AddLabelSprite(_bigFont, DisplayModule.Color.White, 0, 0, 120, 16);
            DisplayModule.LabelSprite _labelCnt = _displayModule.AddLabelSprite(_bigFont, DisplayModule.Color.White, 0, 16, 120, 16);

            while (true)
            {
                /* Always enable actuators during this test */
                CTRE.Phoenix.Watchdog.Feed();

                /* Simple gamepad control */
                if (_gamepad.GetButton(2))
                    _talon.Set(ControlMode.PercentOutput, -1.0f);
                else if (_gamepad.GetButton(4))
                    _talon.Set(ControlMode.PercentOutput, +1.0f);
                else
                {
                    float y = _gamepad.GetAxis(1);
                    CTRE.Phoenix.Util.Deadband(ref y);
                    _talon.Set(ControlMode.PercentOutput, y);
                }

#if false
                //Switch the compressor on/off
                //now = DateTime.Now.Ticks;
                //switch (switchState)
                //{
                //    case true:
                //        if (now - lastSwitch > kOnTime)
                //        {
                //            switchState = false;
                //            lastSwitch = now;
                //        }
                //        break;
                //    case false:
                //        if (now - lastSwitch > kOffTime)
                //        {
                //            switchState = true;
                //            lastSwitch = now;
                //        }
                //        break;
                //    default:
                //        break;
                //}
                //_pcm.SetSolenoidOutput(0, switchState);
#endif
                /* Compressor Control */
                _pcm.SetSolenoidOutput(0, _pcm.GetPressureSwitchValue());
                //Debug.Print("" + _pcm.GetPressureSwitchValue());

                /* Compressor Check */
                _pcm.GetLowLevelObject().GetCompressorOn(out CompState);
                if (CompState && !lastCompState) { compStartCount++; }
                lastCompState = CompState;

                /* Display Module Output */
                _labelTitle.SetText("Comp Start Count:");
                _labelCnt.SetText("" + compStartCount);

                Thread.Sleep(5);
            }
        }
    }
}
