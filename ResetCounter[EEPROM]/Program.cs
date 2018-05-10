﻿using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace ReseCounter
{
    public class Program
    {
        /* Hardware */
        static CTRE.HERO.OnboardEEPROM _eeprom = CTRE.HERO.OnboardEEPROM.Instance;                  // HERO's EEPROM
        static InputPort _button = new InputPort((Cpu.Pin)0x42, false, Port.ResistorMode.Disabled); // HERO's Button

        /* Constants */
        static uint kHeader = 0x0A0A0A0A;
        static uint kChecksum = 0xFAAAAAAA;
        static uint kAddress = 0x000000;
        static byte variableCount = 1;      // Determines number of uints to be read, statically create local array

        /* Variables */
        static uint[] storageArray = new uint[variableCount];
        static ArrayList generalArray = new ArrayList();
        static uint resetCount = 0;

        public static void Main()
        {
            /* Read */
            ReadEEPROM();

            /* Update */
            resetCount = storageArray[0];
            resetCount++;

            /* Output */
            Debug.Print("Bootup Count: " + resetCount);

            /* Write */
            generalArray.Add(resetCount);
            WriteEEPROM(generalArray);

            while (true)
            {
                if (_button.Read())
                {
                    _eeprom.Erase4KB(kAddress);
                    ReadEEPROM();
                    resetCount = storageArray[0];
                    Debug.Print("Bootup Count: " + resetCount);
                }

                /* Clear array for future writes */
                generalArray.Clear();

                /* The usual sleep we have in programs */
                Thread.Sleep(10);
            }
        }
        public static void WriteEEPROM(ArrayList arrayToSend)
        {
            /* We must erase before we write */
            _eeprom.Erase4KB(kAddress);

            ArrayList uintArray = new ArrayList();
            ArrayList byteArray = new ArrayList();
            uintArray.Add(kHeader);
            foreach(uint value in arrayToSend)
            {
                uintArray.Add(value);
            }
            uintArray.Add(kChecksum);
            foreach (uint value in uintArray)
            {
                /* Convert each uint value */
                ValueToBytes(value, byteArray);
            }

            /* Write */
            _eeprom.Write(kAddress, (byte[])byteArray.ToArray(typeof(byte)));
        }

        public static void ReadEEPROM()
        {
            //Read Data out once on boot
            byte NumOfRows = (byte)(variableCount + 2);                     // Expected variable + Header and Checksum
            byte[] ReadBuffer = new byte[NumOfRows * 4];                    // NumOfRows * 4 because we read bytes and want ints
            _eeprom.Read(kAddress, ReadBuffer, ReadBuffer.Length);          // Read EEPROM data and store locally

            //Read Header
            uint Header = (uint)((ReadBuffer[0] << 24));
            Header |= (uint)((ReadBuffer[1] << 16));
            Header |= (uint)((ReadBuffer[2] << 8));
            Header |= (ReadBuffer[3]);

            //Read Checksum
            int indexFooter = (NumOfRows - 1) * 4;
            uint Checksum = (uint)((ReadBuffer[indexFooter + 0] << 24));
            Checksum |= (uint)((ReadBuffer[indexFooter + 1] << 16));
            Checksum |= (uint)((ReadBuffer[indexFooter + 2] << 8));
            Checksum |= (ReadBuffer[indexFooter + 3]);

            if ((Header == kHeader) && (Checksum == kChecksum))
            {
                //Header and checksum from location is good so store its elements into a local
                int indexer = 0;
                for (int i = 0; i < variableCount; i++)
                {
                    indexer = (i * 4) + 4; // each uint is 4 bytes so index by 4, skip 4 for header
                    storageArray[i] = (uint)((ReadBuffer[0 + indexer] << 24));
                    storageArray[i] |= (uint)((ReadBuffer[1 + indexer] << 16));
                    storageArray[i] |= (uint)((ReadBuffer[2 + indexer] << 8));
                    storageArray[i] |= (ReadBuffer[3 + indexer]);
                }
            }
            else
            {
                //Nothing was found so just carry on, should only happen on first boot after cleared EEPROM
                //Or this could be really bad and we just lost all shite
                /* Clear Local Array */
                for (int i = 0; i < variableCount; i++)
                    storageArray[i] = 0;
            }
        }

        /** Takes value, converts to 4 bytes, and adds it to an ArrayList */
        public static void ValueToBytes(uint value, ArrayList arrayAddedTo)
        {
            /* Convert float to bytes and add each byte to arraylist */
            byte[] Values = BitConverter.GetBytes(value);
            arrayAddedTo.Add(Values[3]);
            arrayAddedTo.Add(Values[2]);
            arrayAddedTo.Add(Values[1]);
            arrayAddedTo.Add(Values[0]);
        }
    }
}
