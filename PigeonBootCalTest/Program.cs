using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace PigeonBootCalTest
{
    public class Program
    {
        /* Display Module */
        static Font bigFont = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);
        static CTRE.Gadgeteer.Module.DisplayModule display = new CTRE.Gadgeteer.Module.DisplayModule(CTRE.HERO.IO.Port1, CTRE.Gadgeteer.Module.DisplayModule.OrientationType.Portrait);

        /* On Board HERO */
        static InputPort HeroButton = new InputPort((Cpu.Pin)0x42, false, Port.ResistorMode.Disabled);
        static OutputPort digital1 = new OutputPort(CTRE.HERO.IO.Port3.Pin5, false);

        /* Hardware */
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX _talon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(0);
        static CTRE.Phoenix.Sensors.PigeonIMU _pidgey = new CTRE.Phoenix.Sensors.PigeonIMU(_talon);
        static CTRE.Phoenix.Sensors.GeneralStatus _pidgeyStatus = new CTRE.Phoenix.Sensors.GeneralStatus();

        /* Timing */
        static CTRE.Phoenix.Stopwatch myStopwatch = new CTRE.Phoenix.Stopwatch();

        /* Variables */
        static int state = 0;
        static uint goodCount = 0;
        static uint badCount = 0;
        /* Time */
        static float longestTime = 0;
        static float shortestTime = 0;
        static float currentDuration = 0;
        static float averageTime = 0;
        static double sum = 0;
        /* Pigeon time */
        static int temp = 0;
        static int tempTime = 0;
        static bool checkReset = true;
        static bool isPigeonStale = false;

        public static void Main()
        {
            /* Initialize Display elements */
            CTRE.Gadgeteer.Module.DisplayModule.LabelSprite goodCountDisplay, badCountDisplay, statusDisplay, currentTimeDisplay, longestTimeDisplay, shortestTimeDisplay, averageTimeDisplay;
            /* Count and state */
            goodCountDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 0, 60, 15);
            badCountDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Red, 70, 0, 50, 15);
            statusDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Orange, 0, 20, 45, 15);
            /* Times */
            currentTimeDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.White, 0, 60, 70, 15);
            longestTimeDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Magenta, 0, 80, 70, 15);
            shortestTimeDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Magenta, 0, 100, 70, 15);
            averageTimeDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 0, 120, 70, 15);

            /* Track test version */
            Debug.Print("Ver. 1.3");

            /* Start the test with the Pigeon off */
            digital1.Write(true);
            Thread.Sleep(1000);     //Allow enough time for shut down to take effect

            while (true)
            {
                /* Always enable talon (No Motors connected) */
                CTRE.Phoenix.Watchdog.Feed();

                /* Update general pigeon status */
                _pidgey.GetGeneralStatus(_pidgeyStatus);

                if (checkReset)
                {
                    /* Reset/Update information used to check if Pigeon is disconnected */
                    temp = _pidgeyStatus.upTimeSec;
                    tempTime = (int)currentDuration;
                    isPigeonStale = false;
                    checkReset = false;
                }

                switch (state)
                {
                    case 0:
                        /* Start stopwatch */
                        myStopwatch.Start();
                        checkReset = true;
                        digital1.Write(false);

                        /* Ouput values on Display Module */
                        goodCountDisplay.SetText("GC: " + 1000);
                        badCountDisplay.SetText("BC: " + 20);
                        longestTimeDisplay.SetText("LT: " + longestTime);
                        shortestTimeDisplay.SetText("ST: " + shortestTime);
                        averageTimeDisplay.SetText("AT: " + averageTime);

                        state = 1;
                        break;
                    case 1:
                        /* Ensure Pigeon is Initializing, this tells us that we have entered a new reset */
                        if (_pidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Initializing)
                            state = 2;

                        /* Timeout */
                        if (currentDuration >= 20)
                            state = 5;

                        break;
                    case 2:
                        /* Wait until the pigeon finished boot/cal to decide pigeon connection is good */
                        if (_pidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Ready)
                            state = 3;      // Proceed to next state

                        /* Timeout */
                        if (currentDuration >= 20)
                            state = 5;
                        break;
                    case 3:
                        /* Track Time */

                        if (longestTime == 0 && shortestTime == 0)
                        {
                            /* No times stored, load up with currentDuration */
                            longestTime = currentDuration;
                            shortestTime = currentDuration;
                        }
                        else
                        {
                            /* Update longest/shortest time */
                            if (currentDuration > longestTime)
                                longestTime = currentDuration;
                            else if (currentDuration < shortestTime)
                                shortestTime = currentDuration;
                        }

                        /* Average must be done here to record the correct time */
                        sum += currentDuration;
                        averageTime = (float)sum / (goodCount + 1);

                        state = 4;
                        break;
                    case 4:
                        /* Increment good count */
                        goodCount++;

                        Thread.Sleep(2000);     // Allows us to view the pigeon after if has finished boot calibrating
                        digital1.Write(true);   // Turn on the relay to rest the Talon/Pigeon
                        Thread.Sleep(1000);


                        /* Safety check to ensure Pigeon has been turned off */
                        if (isPigeonStale)
                            state = 0;

                        break;
                    case 5:
                        /* Increment bad count */
                        badCount++;

                        digital1.Write(true);
                        Thread.Sleep(1000);

                        if (isPigeonStale)
                            state = 0;

                        break;
                }

                /** Continuous operations */

                /* Print current status 1 = Initialization, 2 = ready, other values only work on CAN */
                statusDisplay.SetText("PS: " + _pidgey.GetState());

                /* Print current cycle time */
                currentDuration = myStopwatch.Duration;
                currentTimeDisplay.SetText("CT: " + currentDuration);

                /* Pigeon Frame stale check */
                if(currentDuration > tempTime + 1)
                {
                    /* 1 second has passed with stopwatch */
                    if (_pidgeyStatus.upTimeSec > temp + 1)
                    {
                        /* Uptime with pigeon has increased, update */
                        temp = _pidgeyStatus.upTimeSec;
                        tempTime = (int)currentDuration;
                        isPigeonStale = false;
                    }
                    else
                    {
                        /* Pigeon stale, notify */
                        isPigeonStale = true;
                    }
                }

                /* The usual thread sleep */
                Thread.Sleep(5);
            }
        }
    }
}