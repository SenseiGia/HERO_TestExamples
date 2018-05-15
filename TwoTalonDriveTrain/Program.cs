/* Comment/uncomment define based on Talons used, Tank Drive */
/* Talons ID should be changed to match */
/* Tests: Voltage Compensation, Current Limit, Ramping, Neutral Modes */

#define fourWheeled

using System;
using System.Threading;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl.CAN;


namespace ArcadeDriveAuxiliary
{
    public class Program
    {

        /* Create our drivetrain in here */
        static TalonSRX _rightTalon = new TalonSRX(1);
        static TalonSRX _leftTalon = new TalonSRX(0);
#if (fourWheeled)
        static TalonSRX _rightFollower = new TalonSRX(2);
        static TalonSRX _leftFollower = new TalonSRX(5);
#endif

        /* Gamepad */
        static GameController _gamepad = new GameController(CTRE.Phoenix.UsbHostDevice.GetInstance(1), 0);

        public static void Main()
        {
            /* Disable drivetrain/motors */
            _rightTalon.Set(ControlMode.PercentOutput, 0);
            _leftTalon.Set(ControlMode.PercentOutput, 0);

#if (fourWheeled)
            _rightFollower.Follow(_rightTalon);
            _leftFollower.Follow(_leftTalon);
#endif

            /* Configure output and sensor direction */
            _rightTalon.SetInverted(true);
            _leftTalon.SetInverted(false);

#if (fourWheeled)
            _rightFollower.SetInverted(true);
            _leftFollower.SetInverted(false);
#endif

            /* Mode print */
            Debug.Print("This is arcade drive using Arbitrary Feedforward");

            bool Btn1 = false;
            bool Btn2 = false;
            bool Btn3 = false;
            bool Btn4 = false;
            bool Btn10 = false;

            bool VoltageComp = false;
            bool CurrentLimit = false;
            bool NeutralState = false;
            bool RampRate = false;

            bool FirstCall = true;

            while (true)
            {
                /* Enable motor controllers if gamepad connected */
                if (_gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                    CTRE.Phoenix.Watchdog.Feed();

                /* Gamepad Stick Control */
                float forward = -1 * _gamepad.GetAxis(1);
                float turn = 1 * _gamepad.GetAxis(2);
                CTRE.Phoenix.Util.Deadband(ref forward);
                CTRE.Phoenix.Util.Deadband(ref turn);

                bool btn1 = _gamepad.GetButton(1);
                bool btn2 = _gamepad.GetButton(2);
                bool btn3 = _gamepad.GetButton(3);
                bool btn4 = _gamepad.GetButton(4);
                bool btn10 = _gamepad.GetButton(10);
                if (btn1 && !Btn1)
                {
                    VoltageComp = !VoltageComp;
                    FirstCall = true;
                }
                else if (btn2 && !Btn2)
                {
                    CurrentLimit = !CurrentLimit;
                    FirstCall = true;
                }
                else if (btn3 && !Btn3)
                {
                    NeutralState = !NeutralState;
                    FirstCall = true;
                }
                else if (btn4 && !Btn4)
                {
                    RampRate = !RampRate;
                    FirstCall = true;
                }
                else if (btn10 && !Btn10)
                {
                    VoltageComp = false;
                    CurrentLimit = false;
                    NeutralState = false;
                    RampRate = false;

                    FirstCall = true;
                }
                Btn1 = btn1;
                Btn2 = btn2;
                Btn3 = btn3;
                Btn4 = btn4;
                Btn10 = btn10;

                if (VoltageComp)
                {
                    _rightTalon.ConfigVoltageCompSaturation(10, 10);
                    _rightTalon.ConfigVoltageMeasurementFilter(16, 10);
                    _rightTalon.EnableVoltageCompensation(true);

                    _leftTalon.ConfigVoltageCompSaturation(10, 10);
                    _leftTalon.ConfigVoltageMeasurementFilter(16, 10);
                    _leftTalon.EnableVoltageCompensation(true);

                    if(FirstCall)
                        Debug.Print("Voltage Compensation: On");
                }
                else
                {
                    _rightTalon.EnableVoltageCompensation(false);
                    _leftTalon.EnableVoltageCompensation(false);

                    if (FirstCall)
                        Debug.Print("Voltage Compensation: Off");
                }

                if (CurrentLimit)
                {
                    _rightTalon.ConfigContinuousCurrentLimit(10, 10);
                    _rightTalon.ConfigPeakCurrentLimit(10, 10);
                    _rightTalon.ConfigPeakCurrentDuration(0, 10);
                    _rightTalon.EnableCurrentLimit(true);

                    _leftTalon.ConfigContinuousCurrentLimit(10, 10);
                    _leftTalon.ConfigPeakCurrentLimit(10, 10);
                    _leftTalon.ConfigPeakCurrentDuration(0, 10);
                    _leftTalon.EnableCurrentLimit(true);

                    if (FirstCall)
                        Debug.Print("Current Limit: On");
                }
                else
                {
                    _rightTalon.EnableCurrentLimit(false);
                    _leftTalon.EnableCurrentLimit(false);

                    if (FirstCall)
                        Debug.Print("Current Limit: Off");
                }


                if (NeutralState)
                {
                    _rightTalon.SetNeutralMode(NeutralMode.Coast);
                    _leftTalon.SetNeutralMode(NeutralMode.Coast);
#if (fourWheeled)
                    _rightFollower.SetNeutralMode(NeutralMode.Coast);
                    _leftFollower.SetNeutralMode(NeutralMode.Coast);
#endif

                    if (FirstCall)
                        Debug.Print("Neutral Mode: Coast");
                }
                else
                {
                    _rightTalon.SetNeutralMode(NeutralMode.Brake);
                    _leftTalon.SetNeutralMode(NeutralMode.Brake);
#if (fourWheeled)
                    _rightFollower.SetNeutralMode(NeutralMode.Brake);
                    _leftFollower.SetNeutralMode(NeutralMode.Brake);
#endif

                    if (FirstCall)
                        Debug.Print("Neutral Mode: Brake");
                }

                if (RampRate)
                {
                    _rightTalon.ConfigOpenloopRamp(3, 0);
                    _leftTalon.ConfigOpenloopRamp(3, 0);

                    if (FirstCall)
                        Debug.Print("Ramp Rate: On, 3 Seconds");
                }
                else
                {
                    _rightTalon.ConfigOpenloopRamp(0.0f, 0);
                    _leftTalon.ConfigOpenloopRamp(0.0f, 0);

                    if (FirstCall)
                        Debug.Print("Ramp Rate: Off, 0 Seconds");
                }

                /* Use Arbitrary FeedForward to create an Arcade Drive Control by modifying the forward output */
                _rightTalon.Set(ControlMode.PercentOutput, forward, DemandType.ArbitraryFeedForward, -turn);
                _leftTalon.Set(ControlMode.PercentOutput, forward, DemandType.ArbitraryFeedForward, +turn);

                if (FirstCall)
                    Debug.Print("");

                FirstCall = false;

                Thread.Sleep(5);
            }
        }
    }
}