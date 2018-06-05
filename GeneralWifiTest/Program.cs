using System;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CTRE.HERO;
using CTRE.Gadgeteer.Module;

namespace GeneralWifiTest
{
    public class Program
    {
        /** Hardware */
        static WiFiESP12F _wifi = new WiFiESP12F(IO.Port1);
        static InputPort _HEROButton = new InputPort((Cpu.Pin)0x42, false, Port.ResistorMode.Disabled);

        /** entry point of the application */
        public static void Main()
        {
            /* Iniitalize wifi module */
            //_wifi.enableDebugPrints(false);
            Debug.Print("Init Start!");
            _wifi.reset();
            Thread.Sleep(1000);
            Thread.Sleep(1000);
            _wifi.setWifiMode(WiFiESP12F.wifiMode.SOFTAP_STATION);
            Thread.Sleep(1000);
            _wifi.setAP("G_WifiModule", "Chickens", 1, WiFiESP12F.SecurityType.WPA_WPA2_PSK);
            _wifi.startUDP(4, "192.168.4.2", 11000, 11001);
            Debug.Print("Init Done!");
            _wifi.test();       //This is done to skip the cipstart... need to learn more about this

            int counter = 0;
            bool state = true;
            bool lastHeroRead = false;
            while (true)
            {
                bool button = _HEROButton.Read();
                if (button)
                {
                    counter++;
                    if(counter >= 100)
                    {
                        state = !state;
                        _wifi.enableDebugPrints(state);
                        Debug.Print(state ? "Prints Enabled" : "Prints Disabled");
                        counter = 0;
                    }
                }
                else
                {
                    counter = 0;
                }

                if (button && !lastHeroRead) {
                    /* Test the various modes */
                    String APIP = _wifi.getAccessPointIP();
                    String StationIP = _wifi.getStationIP();
                    Debug.Print("IP : " + StationIP + " || " + APIP);
                }
                lastHeroRead = button;

                Thread.Sleep(10);
            }
        }
    }
}