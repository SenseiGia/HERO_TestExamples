using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.Controller;

namespace GamepadTesting
{
    public class Program
    {
        public static GameController _gamepad = new GameController(CTRE.Phoenix.UsbHostDevice.GetInstance(1), 0);

        public const int kNumButtonsPlusOne = 13;

        public static void Main()
        {

            /* CHANGE THIS ARRAY SIZE TO TEST VARIOUS THINGS ( + null) */
            bool[] btns = null;

            /* Latched values to detect on-press events for buttons */
            bool[] _btns = new bool[kNumButtonsPlusOne];
            bool[] btnStatus = new bool[kNumButtonsPlusOne];

            while (true)
            {
                _gamepad.GetButtons(btns);
                if(btns != null){

                    /* Print buttons */
                    int j = 0;
                    String testPrint = "";
                    foreach (bool btn in btns)
                    {
                        j++;
                        testPrint += " " + j + ": " + btn;
                    }
                    Debug.Print(testPrint);
                }

                Thread.Sleep(100);
            }
        }
    }
}
