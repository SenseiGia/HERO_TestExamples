using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace DriveModuleSimulate
{
    public class Program
    {
        /* Change this */
        static int kAdjustableTime = 10;    /* Seconds */
        
        /* display module */
        static CTRE.Gadgeteer.Module.DisplayModule _display = new CTRE.Gadgeteer.Module.DisplayModule(CTRE.HERO.IO.Port8, CTRE.Gadgeteer.Module.DisplayModule.OrientationType.Portrait);
        static CTRE.Gadgeteer.Module.DisplayModule.LabelSprite _firstline, _secondline;
        
        /* GPIO's for drive module */
        static OutputPort Pin5 = new OutputPort(CTRE.HERO.IO.Port3.Pin5, false);
        static OutputPort Pin6 = new OutputPort(CTRE.HERO.IO.Port3.Pin6, false);
        static OutputPort Pin7 = new OutputPort(CTRE.HERO.IO.Port3.Pin7, false);
        static OutputPort Pin8 = new OutputPort(CTRE.HERO.IO.Port3.Pin8, false);

        public static void Main()
        {
            /* On/Off count */
            uint count = 0;
            /* Timing */
            long switchTime = 0;
            long now = DateTime.Now.Ticks;
            long stateTime = kAdjustableTime * TimeSpan.TicksPerSecond;
            /* swtich variable */
            bool switchState = true;

            /* Display elemnts */
            Font bigFont = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);
            _firstline = _display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 0, 120, 20);
            _secondline = _display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 20, 120, 20);

            while (true)
            {
                /* Update time */
                now = DateTime.Now.Ticks;

                /* State decider */
                switch (switchState)
                {
                    case true:
                        if (now - switchTime > stateTime)
                        {
                            switchState = false;
                            switchTime = now;
                        }
                        break;
                    case false:
                        if (now - switchTime > stateTime)
                        {
                            switchState = true;
                            switchTime = now;
                            count++;
                        }
                        break;
                    default:
                        break;
                }

                /* State implementer */
                if (switchState)
                {
                    /* On */
                    Pin5.Write(true);
                    Pin6.Write(true);
                    Pin7.Write(true);
                    Pin8.Write(true);
                }
                else
                {
                    Pin5.Write(false);
                    Pin6.Write(false);
                    Pin7.Write(false);
                    Pin8.Write(false);
                }

                /* Show stuff on display and sleep a lil */
                _firstline.SetText(switchState ? "On" : "Off");
                _secondline.SetText("Count: " + count);
                Thread.Sleep(5);
            }
        }
    }
}
