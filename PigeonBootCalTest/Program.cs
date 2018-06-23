using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.MotorControl;
namespace PigeonBootCalTest
{
    public class Program
    {
        /* Display Module */
        static Font bigFont = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);
        static Font smallFont = Properties.Resources.GetFont(Properties.Resources.FontResources.small);
        static CTRE.Gadgeteer.Module.DisplayModule display = new CTRE.Gadgeteer.Module.DisplayModule(CTRE.HERO.IO.Port8, CTRE.Gadgeteer.Module.DisplayModule.OrientationType.Portrait);

        /* On Board HERO */
        static InputPort HeroButton = new InputPort((Cpu.Pin)0x42, false, Port.ResistorMode.Disabled);
        static OutputPort digital1 = new OutputPort(CTRE.HERO.IO.Port3.Pin5, false);

        /* Hardware */
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX _talon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(0);
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX _testTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(1);   // Just used to connect to Pigeon
        static CTRE.Phoenix.Sensors.PigeonIMU _pidgey = new CTRE.Phoenix.Sensors.PigeonIMU(_talon);
        static CTRE.Phoenix.Sensors.PigeonIMU _testPidgey = new CTRE.Phoenix.Sensors.PigeonIMU(_testTalon);         // Customer Talon
        static CTRE.Phoenix.Sensors.GeneralStatus _pidgeyStatus = new CTRE.Phoenix.Sensors.GeneralStatus();
        static CTRE.Phoenix.Sensors.GeneralStatus _testPidgeyStatus = new CTRE.Phoenix.Sensors.GeneralStatus();

        /* Timing */
        static CTRE.Phoenix.Stopwatch myStopwatch = new CTRE.Phoenix.Stopwatch();

        /* Tracking Variables */
        static int state = 0;
        static uint goodCount = 0;
        static uint badCount = 0;

        static bool firstTime = false;
        static bool firstTime_test = false;

        /* Our Pigeon Time */
        static float longestTime = 0;
        static float shortestTime = 0;
        static float averageTime = 0;
        static double sum = 0;

        /* Test Pigeon time */
        static float longestTime_test = 0;
        static float shortestTime_test = 0;
        static float averageTime_test = 0;
        static double sum_test = 0;

        /* Constants */
        static float Timeout = 60; // 5 Minutes 

        /* Pigeon time */
        static int temp = 0;
        static float tempTime = 0;
        static bool checkReset = true;
        static bool isPigeonStale = false;

        /* Talon time */
        static float _motorStartTime = 0;
        static float _pigeonStartTime = 0;

        static int _originalBC = 0;
        static int _testBC = 0;

        public static void Main()
        {
            /* Initialize Display elements */
            CTRE.Gadgeteer.Module.DisplayModule.LabelSprite goodCountDisplay, badCountDisplay, statusDisplay, currentTimeDisplay, longestTimeDisplay, shortestTimeDisplay, averageTimeDisplay,
                                    longestTimeDisplay_test, shortestTimeDisplay_test, averageTimeDisplay_test, OriginalGCDisplay, TestGCDisplay, myDisplay;
            /* Count and state */
            goodCountDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 0, 60, 15);
            badCountDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Red, 70, 0, 50, 15);
            statusDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Orange, 0, 20, 45, 15);
            currentTimeDisplay = display.AddLabelSprite(bigFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Orange, 50, 20, 70, 15);

            /* Instructions */
            myDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Cyan, 0, 40, 120, 15);

            /* Times */
            longestTimeDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Blue, 0, 80, 60, 15);
            shortestTimeDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Blue, 0, 100, 60, 15);
            averageTimeDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Blue, 0, 120, 60, 15);
            /* Test */
            longestTimeDisplay_test = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 60, 80, 60, 15);
            shortestTimeDisplay_test = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 60, 100, 60, 15);
            averageTimeDisplay_test = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 60, 120, 60, 15);

            /* Random */
            OriginalGCDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Blue, 0, 60, 60, 15);
            TestGCDisplay = display.AddLabelSprite(smallFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Yellow, 60, 60, 60, 15);

            myDisplay.SetText("Orig Blue : Test Yellow");

            /* Track test version */
            Debug.Print("Ver. 1.4");

            /* Start the test with the Pigeon off */
            digital1.Write(true);
            Thread.Sleep(1000);     //Allow enough time for shut down to take effect

            while (true)
            {
                /* Always enable talon (No Motors connected) */
                CTRE.Phoenix.Watchdog.Feed();

                /* Update general pigeon status' */
                _pidgey.GetGeneralStatus(_pidgeyStatus);
                _testPidgey.GetGeneralStatus(_testPidgeyStatus);

                if (checkReset)
                {
                    /* Reset/Update information used to check if Pigeon is disconnected */
                    temp = _pidgeyStatus.upTimeSec;
                    tempTime = myStopwatch.Duration;
                    isPigeonStale = false;
                    checkReset = false;

                    /* Reset/Update information used to check if Pigeon is disconnected */
                    temp = _pidgeyStatus.upTimeSec;
                    tempTime = myStopwatch.Duration;
                    isPigeonStale = false;
                    checkReset = false;
                }

                /* Ouput values on Display Module */
                /* Print current status 1 = Initialization, 2 = ready, other values only work on CAN */
                statusDisplay.SetText("PS: " + state);  // _pidgey.GetState());

                goodCountDisplay.SetText("GC: " + goodCount);
                badCountDisplay.SetText("BC: " + badCount);

                longestTimeDisplay.SetText("LT: " + longestTime);
                shortestTimeDisplay.SetText("ST: " + shortestTime);
                averageTimeDisplay.SetText("AT: " + averageTime);

                longestTimeDisplay_test.SetText("LT: " + longestTime_test);
                shortestTimeDisplay_test.SetText("ST: " + shortestTime_test);
                averageTimeDisplay_test.SetText("AT: " + averageTime_test);

                OriginalGCDisplay.SetText("BC: " + _originalBC);
                TestGCDisplay.SetText("BC: " + _testBC);

                /* Print current cycle time */
                currentTimeDisplay.SetText("CT: " + myStopwatch.Duration);

                /* Determine if Pigeon is OFF */
                if (myStopwatch.Duration > tempTime + 1)
                {
                    /* 1 second has passed with stopwatch */
                    if (_pidgeyStatus.upTimeSec > temp + 1)
                    {
                        /* Uptime with pigeon has increased, update */
                        temp = _pidgeyStatus.upTimeSec;
                        tempTime = myStopwatch.Duration;
                        isPigeonStale = false;
                    }
                    else
                    {
                        /* Pigeon stale, notify */
                        isPigeonStale = true;
                    }
                }

                if (HeroButton.Read())
                {
                    /* Pigeon One */
                    longestTime = 0;
                    shortestTime = 0;
                    averageTime = 0;
                    sum = 0;

                    /* Pigeon Two */
                    longestTime_test = 0;
                    shortestTime_test = 0;
                    averageTime_test = 0;
                    sum_test = 0;
                }

                switch (state)
                {
                    case 0:
                        /* Start stopwatch */
                        myStopwatch.Start();

                        /* Reset Information */
                        checkReset = true;

                        /* Turn On Talons */
                        digital1.Write(false);

                        state = 1;

                        break;
                    case 1:
                        uint numOfPigeonInit = 0;

                        /* Ensure Pigeons are Initializing, this tells us that we have entered a new reset */
                        if (_pidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Initializing)
                            numOfPigeonInit++;
                        if (_testPidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Initializing)
                            numOfPigeonInit++;

                        /* Proceed to next state if Pigeon Count is good */
                        if (numOfPigeonInit == 2)
                        {
                            _motorStartTime = myStopwatch.Duration;
                            state = 2;
                        }

                        /* Timeout */
                        if (myStopwatch.Duration >= Timeout)
                            state = 5;

                        break;
                    case 2:
                        /* Run motor for short time */
                        _talon.Set(ControlMode.PercentOutput, 0.08f);

                        /* Run motor for 5 seconds */
                        if ((myStopwatch.Duration - _motorStartTime) >= 5)
                        {
                            /* Stop Talon and proceed to next state */
                            _talon.Set(ControlMode.PercentOutput, 0);
                            firstTime = true;
                            firstTime_test = true;
                            _pigeonStartTime = myStopwatch.Duration;
                            state = 3;
                        }

                        break;
                    case 3:
                        uint _pigeonFinishCount = 0;
                        if (_pidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Ready)
                        {
                            /* Regular Pigeon Finisehd */
                            _pigeonFinishCount++;
                            if (firstTime)
                            {
                                float Duration = myStopwatch.Duration - _pigeonStartTime;
                                if (longestTime == 0 && shortestTime == 0)
                                {
                                    longestTime = Duration;
                                    shortestTime = Duration;
                                }
                                else
                                {
                                    if (Duration > longestTime)
                                        longestTime = Duration;
                                    else if (Duration < shortestTime)
                                        shortestTime = Duration;
                                }
                                sum += Duration;
                                averageTime = (float)sum / (goodCount + 1);

                                firstTime = false;
                            }
                        }
                        if (_testPidgey.GetState() == CTRE.Phoenix.Sensors.PigeonState.Ready)
                        {
                            /* Test Pigeon Finished */
                            _pigeonFinishCount++;
                            if (firstTime_test)
                            {
                                float Duration = myStopwatch.Duration - _pigeonStartTime;
                                if (longestTime_test == 0 && shortestTime_test == 0)
                                {
                                    longestTime_test = Duration;
                                    shortestTime_test = Duration;
                                }
                                else
                                {
                                    if (Duration > longestTime_test)
                                        longestTime_test = Duration;
                                    else if (Duration < shortestTime_test)
                                        shortestTime_test = Duration;
                                }
                                sum_test += Duration;
                                averageTime_test = (float)sum_test / (goodCount + 1);

                                firstTime_test = false;
                            }
                        }

                        /* Proceed to next state */
                        if (_pigeonFinishCount == 2)
                            state = 4;

                        /* Timeout */
                        if (myStopwatch.Duration >= Timeout)
                            state = 5;

                        break;
                    case 4:
                        /* Increment good count */
                        goodCount++;

                        Thread.Sleep(2000);     // Allows us to view the pigeon after if has finished boot calibrating
                        digital1.Write(true);   // Turn on the relay to rest the Talon/Pigeon

                        state = 6;
                        break;
                    case 5:
                        /* Increment bad count */
                        badCount++;

                        if (firstTime)
                            _originalBC++;
                        if (firstTime_test)
                            _testBC++;

                        digital1.Write(true);

                        state = 6;
                        break;
                    case 6:
                        /* Wait for Pigeons to turn off */
                        Thread.Sleep(1000);
                        if (isPigeonStale)
                            state = 0;
                        break;
                    default:
                        /* Do nothing by default */
                        break;
                }

                /* The usual thread sleep */
                Thread.Sleep(5);
            }
        }
    }
}