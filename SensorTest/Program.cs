using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace SensorTest
{
    public class Program
    {
        /* Hardware */
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX magTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(0);
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX anaTalon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(1);
        static CTRE.Phoenix.Controller.GameController gamepad = new CTRE.Phoenix.Controller.GameController(CTRE.Phoenix.UsbHostDevice.GetInstance(1), 0);

        /* Constants */
        static int kTimeout = 20;

        public static void Main()
        {
            /* Current setup is a CTRE MAG Encoder and a 128 CPR Opitcal Encoder, where printf's can be used to compare accuracy */
            magTalon.ConfigSelectedFeedbackSensor(CTRE.Phoenix.MotorControl.FeedbackDevice.QuadEncoder, 0, kTimeout);
            anaTalon.ConfigSelectedFeedbackSensor(CTRE.Phoenix.MotorControl.FeedbackDevice.QuadEncoder, 0, kTimeout);
            magTalon.ConfigSelectedFeedbackCoefficient(.125f, 0, kTimeout); /* Mag Encoder 4096 CPR * 0.125 = 128 == 128 CPR Opitcal Encoder, could scale up */

            /* Reduce the velocity averaging to generate useful plots in Vehicle spy */
            magTalon.ConfigVelocityMeasurementWindow(1, kTimeout);
            magTalon.ConfigVelocityMeasurementPeriod(CTRE.Phoenix.MotorControl.VelocityMeasPeriod.Period_1Ms, kTimeout);

            anaTalon.ConfigVelocityMeasurementWindow(1, kTimeout);
            anaTalon.ConfigVelocityMeasurementPeriod(CTRE.Phoenix.MotorControl.VelocityMeasPeriod.Period_1Ms, kTimeout);

            /* Increase the rate of the CAN frame */
            magTalon.SetStatusFramePeriod(CTRE.Phoenix.MotorControl.StatusFrameEnhanced.Status_2_Feedback0, 0, kTimeout);
            anaTalon.SetStatusFramePeriod(CTRE.Phoenix.MotorControl.StatusFrameEnhanced.Status_2_Feedback0, 0, kTimeout);

            bool lastBtn = false;
            while (true)
            {
                /* CTRE Output enable */
                if (gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                    CTRE.Phoenix.Watchdog.Feed();

                /* Encoder Postion taring */
                bool btn = gamepad.GetButton(2);
                if(btn && !lastBtn)
                {
                    magTalon.SetSelectedSensorPosition(0, 0, kTimeout);
                    anaTalon.SetSelectedSensorPosition(0, 0, kTimeout);
                }
                lastBtn = btn;
                float Y = gamepad.GetAxis(1);

                /* Reduce speed if right trigger is held down */
                if (gamepad.GetButton(6))
                    Y *= 0.10f;

                /* Talon with CTRE Mag Encoder drives the Talon */
                magTalon.Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, Y);

                /* Telemetry */
                int magTemp = magTalon.GetSelectedSensorPosition(0);
                int anaTemp = anaTalon.GetSelectedSensorPosition(0);
                Debug.Print("mag: " + magTemp + " ana: " + anaTemp + " dif: " + (magTemp - anaTemp));

                /* Allow some breathing room for the CAN Frames */
                Thread.Sleep(10);
            }
        }
    }
}
