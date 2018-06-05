using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix;
using CTRE.Phoenix.Controller;

namespace GamepadTesting
{
    public class Program
    {
        /** Hardware */
        public static GameController _gamepad = new GameController(UsbHostDevice.GetInstance());

        /** Constants */
        /* Arrays start at index of 1 and Logitech Gamepad has 12 buttons, use buttons 1-12 with the index of 13 */
        public const int kNumButtonsPlusOne = 13;

        public static void Main()
        {
            /* Arrays start at index of 1 and Logitech Gamepad has 12 buttons, use buttons 1-12 with the index of 13 */
            bool[] _currentBtns = new bool[kNumButtonsPlusOne];
            bool[] _previousBtns = new bool[kNumButtonsPlusOne];

            while (true)
            {
                /* Store all buttons into current buttons array */
                //_gamepad.GetButtons(_currentBtns);

                /* Skip Button 0 to prevent constantly throwing errors */
                for (uint i = 1; i < 12; i++)
                    _currentBtns[i] = _gamepad.GetButton(i);

                if (_currentBtns != null)
                {
                    /* If either back or start button held, print values */
                    if (_currentBtns[9] || _currentBtns[10])
                    {
                        /* Comment out to remove error */
                        _currentBtns[0] = _gamepad.GetButton(0);
                        String testPrint = "";

                        /* Print current status of all buttons */
                        for (int i = 0; i < kNumButtonsPlusOne; i++)
                            testPrint += "Btn " + (i) + ": " + _currentBtns[i] + "  ";

                        Debug.Print(testPrint);
                    }
                }
                System.Array.Copy(_currentBtns, _previousBtns, kNumButtonsPlusOne);

                Thread.Sleep(10);
            }
        }
    }
}
