using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CanifierPWMOutput
{
    public class Program
    {
        static CTRE.Phoenix.CANifier _canifier = new CTRE.Phoenix.CANifier(0);
        static CTRE.Phoenix.Controller.GameController _gamepad = new CTRE.Phoenix.Controller.GameController(CTRE.Phoenix.UsbHostDevice.GetInstance());

        /* Constants */
        static CTRE.Phoenix.CANifier.PWMChannel kMotorControllerCh = CTRE.Phoenix.CANifier.PWMChannel.PWMChannel2;
        static float _percentOutput = 0;

        public static void Main()
        {
            /* start transmitting neutral */
            _percentOutput = 0;
            _canifier.SetPWMOutput((uint)kMotorControllerCh, 0);

            _canifier.EnablePWMOutput(1, true);
            _canifier.EnablePWMOutput((int)kMotorControllerCh, true);
            while (true)
            {
                CTRE.Phoenix.Watchdog.Feed();
                /* just grab three axis and direct control the components */
                float axis = _gamepad.GetAxis(1);
                /* scale to typical pwm withds */
                float pulseUs = CTRE.Phoenix.LinearInterpolation.Calculate(axis, -1, 1000f, +1, 2000f); /* [-1,+1] => [1000,2000]us */
                                                                                                        /* scale to period */
                float periodUs = 4200; // hardcoded for now, this will be settable in future firmware update.
                _percentOutput = pulseUs / periodUs;
                /* set it */
                Debug.Print(": " + _percentOutput);
                _canifier.SetPWMOutput((uint)kMotorControllerCh, _percentOutput);
            }
        }
    }
}
