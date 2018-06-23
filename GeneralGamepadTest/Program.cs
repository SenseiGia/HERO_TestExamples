using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace GeneralGamepadTest
{
    public class Program
    {
        //Create Gamepad
        public static CTRE.Phoenix.Controller.GameController Gamepad = new CTRE.Phoenix.Controller.GameController(CTRE.Phoenix.UsbHostDevice.GetInstance(0), 0);

        //Create display module for debugging
        static CTRE.Gadgeteer.Module.DisplayModule Display = new CTRE.Gadgeteer.Module.DisplayModule(CTRE.HERO.IO.Port8, CTRE.Gadgeteer.Module.DisplayModule.OrientationType.Landscape);
        static Microsoft.SPOT.Font smallFont = Properties.Resources.GetFont(Properties.Resources.FontResources.small);
        static Microsoft.SPOT.Font bigFont = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);

        static bool[] _currentBtns = new bool[11];
        static bool[] _previousBtns = new bool[11];
        static uint[] _btnCount = new uint[11];

        public static void Main()
        {
            /* Initialize Display */
            CTRE.Gadgeteer.Module.DisplayModule.LabelSprite lineOne, lineTwo, lineThree, lineFour, lineFive,
                lineSix, lineSeven, lineEight, lineNine, lineTen;

            /* State and battery display in the 1st row */
            lineOne = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Red, 0, 0, 100, 10);
            lineTwo = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 10, 100, 10);
            lineThree = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Cyan, 0, 20, 100, 10);
            lineFour = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Cyan, 0, 30, 100, 10);
            lineFive = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 0, 40, 100, 10);
            lineSix = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Blue, 0, 50, 100, 10);
            lineSeven = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Orange, 0, 60, 100, 10);
            lineEight = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.White, 0, 70, 100, 10);
            lineNine = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.White, 0, 80, 100, 10);
            lineTen = Display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.White, 0, 90, 100, 10);
            while (true)
            {
                Gamepad.GetButtons(_currentBtns);
                for (int i = 1; i < _currentBtns.Length; i++)
                {
                    if (_currentBtns[i] && !_previousBtns[i])
                        ++_btnCount[i];
                }

                /* Velocity CAN Frame */
                byte[] frame2 = new byte[8];
                ulong data2 = (ulong)BitConverter.ToUInt64(frame2, 0);
                CTRE.Native.CAN.Send(8, data2, 8, 0);

                /* State and battery display in the 1st row */
                lineOne.SetText("Btn 1: " + Gamepad.GetButton(1) + "  " + _btnCount[1]);
                lineTwo.SetText("Btn 2: " + Gamepad.GetButton(2) + "  " + _btnCount[2]);
                lineThree.SetText("Btn 3: " + Gamepad.GetButton(3) + "  " + _btnCount[3]);
                lineFour.SetText("Btn 4: " + Gamepad.GetButton(4) + "  " + _btnCount[4]);
                lineFive.SetText("Btn 5: " + Gamepad.GetButton(5) + "  " + _btnCount[5]);
                lineSix.SetText("Btn 6: " + Gamepad.GetButton(6) + "  " + _btnCount[6]);
                lineSeven.SetText("Btn 7: " + Gamepad.GetButton(7) + "  " + _btnCount[7]);
                lineEight.SetText("Btn 8: " + Gamepad.GetButton(8) + "  " + _btnCount[8]);
                lineNine.SetText("Btn 9: " + Gamepad.GetButton(9) + "  " + _btnCount[9]);
                lineTen.SetText("Btn 10: " + Gamepad.GetButton(10) + "  " + _btnCount[10]);

                System.Array.Copy(_currentBtns, _previousBtns, _previousBtns.Length);

                Thread.Sleep(110);
            }
        }
    }
}
