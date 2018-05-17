using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace PWMSensorTest
{
    public class Program
    {
        /* Hardware */
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX magTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(0);
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX anaTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(6);
        static CTRE.Phoenix.Controller.GameController gamepad = new CTRE.Phoenix.Controller.GameController(CTRE.Phoenix.UsbHostDevice.GetInstance(1), 0);

        static CTRE.Gadgeteer.Module.DisplayModule displayModule = new CTRE.Gadgeteer.Module.DisplayModule(CTRE.HERO.IO.Port1, CTRE.Gadgeteer.Module.DisplayModule.OrientationType.Portrait);
        static CTRE.Phoenix.Stopwatch stopwatch = new CTRE.Phoenix.Stopwatch();

        static CTRE.Gadgeteer.Module.DisplayModule.LabelSprite differenceDisplay, statusDisplay, errorDisplay;
        static Font gFont = Properties.Resources.GetFont(Properties.Resources.FontResources.Century__14);

        /* Constants */
        static int kTimeout = 20;

        public static void Main()
        {
            /* Current setup is a CTRE MAG Encoder and a 128 CPR Opitcal Encoder, where printf's can be used to compare accuracy */
            magTalon.ConfigSelectedFeedbackSensor(CTRE.Phoenix.MotorControl.FeedbackDevice.PulseWidthEncodedPosition, 0, kTimeout);
            anaTalon.ConfigSelectedFeedbackSensor(CTRE.Phoenix.MotorControl.FeedbackDevice.QuadEncoder, 0, kTimeout);
            magTalon.ConfigSelectedFeedbackCoefficient(0.125f, 0, kTimeout); /* Mag Encoder 4096 CPR * 0.125 = 128 == 128 CPR Opitcal Encoder, could scale up */
            anaTalon.ConfigSelectedFeedbackCoefficient(1, 0, kTimeout);

            anaTalon.SetSensorPhase(false);
            magTalon.SetInverted(true);
            magTalon.SetSensorPhase(true);

            /* Reduce the velocity averaging to generate useful plots in Vehicle spy */
            magTalon.ConfigVelocityMeasurementWindow(1, kTimeout);
            magTalon.ConfigVelocityMeasurementPeriod(CTRE.Phoenix.MotorControl.VelocityMeasPeriod.Period_1Ms, kTimeout);

            anaTalon.ConfigVelocityMeasurementWindow(1, kTimeout);
            anaTalon.ConfigVelocityMeasurementPeriod(CTRE.Phoenix.MotorControl.VelocityMeasPeriod.Period_1Ms, kTimeout);

            /* Increase the rate of the CAN frame */
            magTalon.SetStatusFramePeriod(CTRE.Phoenix.MotorControl.StatusFrameEnhanced.Status_2_Feedback0, 4, kTimeout);
            anaTalon.SetStatusFramePeriod(CTRE.Phoenix.MotorControl.StatusFrameEnhanced.Status_2_Feedback0, 4, kTimeout);

            /* For PWM test, scale down motor so optical has a chance to keep up? */
            float peakOutput = 1f;
            float nominalOutput = 0;
            magTalon.ConfigPeakOutputForward(peakOutput, kTimeout);
            magTalon.ConfigPeakOutputReverse(-peakOutput, kTimeout);
            magTalon.ConfigNominalOutputForward(nominalOutput, kTimeout);
            magTalon.ConfigNominalOutputReverse(-nominalOutput, kTimeout);

            differenceDisplay = displayModule.AddLabelSprite(gFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Orange, 0, 0, 100, 15);
            statusDisplay = displayModule.AddLabelSprite(gFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Green, 0, 20, 100, 15);
            errorDisplay = displayModule.AddLabelSprite(gFont, CTRE.Gadgeteer.Module.DisplayModule.Color.Red, 0, 60, 100, 15);

            statusDisplay.SetText("Gamepad Mode");
            errorDisplay.SetText("None");

            /* Clear Position */
            SetPositon(0);


            /* Variables to test various features */
            bool lastBtn = false;
            bool lastBtn3 = false;
            bool state = false;
            float output = 0;

            while (true)
            {
                /* CTRE Output enable */
                if (gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                    CTRE.Phoenix.Watchdog.Feed();

                /* Encoder Postion taring */
                bool btn = gamepad.GetButton(2);
                bool btn3 = gamepad.GetButton(3);
                if(btn && !lastBtn)
                {
                    SetPositon(0);
                    errorDisplay.SetText("Error: " + 0);
                }
                if (btn3 && !lastBtn3)
                {
                    state = !state;
                    if (state)
                    {
                        stopwatch.Start();
                        statusDisplay.SetText("Sine Mode");
                    }
                    else
                    {
                        statusDisplay.SetText("Gamepad Mode");
                    }
                }
                lastBtn = btn;
                lastBtn3 = btn3;

                if (!state)
                {
                    /* Joypad value */
                    output = gamepad.GetAxis(1);

                    /* Reduce speed if right trigger is held down */
                    if (gamepad.GetButton(6))
                        output *= 0.50f;
                }
                else
                {
                    float amplitude = 1;
                    float frequencyHZ = 0.2f;
                    float time = stopwatch.Duration;
                    output = amplitude * (float)System.Math.Sin((2 * System.Math.PI) * frequencyHZ * time);
                }

                byte[] Frame = new byte[8];
                Frame[0] = (byte)((int)(output * 1000) >> 8);
                Frame[1] = (byte)((int)(output * 1000) & 0xFF);
                ulong data = (ulong)BitConverter.ToUInt64(Frame, 0);
                CTRE.Native.CAN.Send(0x09, data, 8, 0);

                /* (scale for maximum rpm @3000 - 6900ish) */
                output *= (1/5f);

                /* Talon with CTRE Mag Encoder drives the Talon */
                magTalon.Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, output);

                /* Telemetry */
                int magTemp = magTalon.GetSelectedSensorPosition(0);
                int anaTemp = anaTalon.GetSelectedSensorPosition(0);
                int difference = magTemp - anaTemp;

                Debug.Print("mag: " + magTemp + " ana: " + anaTemp + " dif: " + difference);
                differenceDisplay.SetText("Dif: " + difference);

                if (output > -0.05 && output < 0.05)
                {
                    /* We are slow enough to check */
                    if (System.Math.Abs(difference) > 50)
                    {
                        /* Disruption in position */
                        state = false;
                        statusDisplay.SetText("Bad Postion");
                        errorDisplay.SetText("Error: " + difference);
                    }
                }else if(System.Math.Abs(difference) > 500)
                {
                    /* Disruption in position */
                    state = false;
                    statusDisplay.SetText("Bad Postion");
                    errorDisplay.SetText("Error: " + difference);
                }

                /* Allow some breathing room for the CAN Frames */
                Thread.Sleep(5);
            }
        }

        public static void SetPositon(int position)
        {
            magTalon.SetSelectedSensorPosition(position, 0, kTimeout);
            anaTalon.SetSelectedSensorPosition(position, 0, kTimeout);
        }
    }
}
