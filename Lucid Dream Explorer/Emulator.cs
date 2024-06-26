﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lucid_Dream_Explorer
{
    static class Emulator
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

        public static readonly string EPSXE = @"pcsxr";
        public static readonly string PSXFIN = @"psxfin";
        public static readonly string XEBRA = @"XEBRA";
        public static readonly string NOCASH = @"NO$PSX";

        //About window text strings
        public static readonly string PSXFIN_VERSION_CHECK = "pSX v1.13\0";
        public static readonly string EPSXE_VERSION_CHECK = @"Playstation Emulator based on PCSX-df";
        public static readonly string XEBRA_VERSION_CHECK = @""; //TODO
        public static readonly string NOCASH_VERSION_CHECK = "2.2 ";

        public static string emulator;
        private static List<string> badVersionsMessaged = new List<string>();
        private static IntPtr? emulatorPtr;
        public static IntPtr? baseAddr
        {
            get
            {
                if (emulatorPtr == null)
                {
                    emulatorPtr = HookEmulator();
                }
                return emulatorPtr;
            }
        }

        private static IntPtr? HookEmulator()
        {
            IntPtr? DEbaseAddress;
            //Look for slow emulators first
            //Try XEBRA
            DEbaseAddress = DetectXEBRA();
            emulator = XEBRA;
            if (DEbaseAddress != null) return DEbaseAddress;
            //Try ePSXe
            DEbaseAddress = DetectEPSXe();
            emulator = EPSXE;
            if (DEbaseAddress != null) return DEbaseAddress;
            //Try psxfin
            DEbaseAddress = DetectPsxfin();
            emulator = PSXFIN;
            if (DEbaseAddress != null) return DEbaseAddress;
            DEbaseAddress = DetectNocash();
            emulator = NOCASH;
            if (DEbaseAddress != null) return DEbaseAddress;
            emulator = null;
            return null;
        }

        //Returns main memory start
        private static IntPtr? DetectEPSXe()
        {
            var mioRelative = new MemoryIO(EPSXE, true); //Program start = 0
            if (!mioRelative.processOK()) return null;

            return versionOk(EPSXE_VERSION_CHECK, mioRelative, (IntPtr)Offset.ePSXeVersion)
                ? (IntPtr?)Offset.ePSXeMemstart : null; //Fixed pos, no pointer needed)
        }

        //Returns main memory start
        private static IntPtr? DetectXEBRA()
        {
            var mioRelative = new MemoryIO(XEBRA, true); //Program start = 0
            if (!mioRelative.processOK()) return null;

            byte[] readBuf = new byte[4];
            var baseAddrPointer = new IntPtr(Offset.xebraMemstart); //XEBRA.EXE+54920, memstart ptr
            mioRelative.MemoryRead(baseAddrPointer, readBuf);

            return versionOk(XEBRA_VERSION_CHECK, mioRelative, (IntPtr)Offset.xebraVersion)
                ? (IntPtr?)BitConverter.ToInt32(readBuf, 0) : null;
        }

        //Returns main memory start
        private static IntPtr? DetectPsxfin()
        {
            var mioRelative = new MemoryIO(PSXFIN, true); //Program start = 0
            if (!mioRelative.processOK()) return null;

            byte[] readBuf = new byte[4];
            var baseAddrPointer = new IntPtr(Offset.psxfinMemstart); //psxfin.exe+171A5C, memstart ptr 
            mioRelative.MemoryRead(baseAddrPointer, readBuf);

            return versionOk(PSXFIN_VERSION_CHECK, mioRelative, (IntPtr)Offset.psxfinVersion)
                ? (IntPtr?)BitConverter.ToInt32(readBuf, 0) : null;
        }

        private static IntPtr? DetectNocash()
        {
            var mioRelative = new MemoryIO(NOCASH, true); //Program start = 0
            if (!mioRelative.processOK()) return null;

            byte[] readBuf = new byte[4];
            IntPtr secretText = new IntPtr(Offset.nocashEdition);
            mioRelative.MemoryRead(secretText, readBuf);

            IntPtr baseAddrPointer;
            uint crimes = BitConverter.ToUInt32(readBuf, 0);
            if (crimes == 0x171720f1) //"lluF" in bad ACSII
            {
                baseAddrPointer = new IntPtr(Offset.nocashMemstart); //Load pointer for debug version
            }
            else
            {
                baseAddrPointer = new IntPtr(Offset.nocashMemstartLite); //Load pointer for gaming version
            }
            mioRelative.MemoryRead(baseAddrPointer, readBuf);
            int baseAddr = BitConverter.ToInt32(readBuf,0);

            if (baseAddr == 0)
            {
                return null; //Memory table is "Not Ready"
            }

            //NO$PSX version number is manually slapped thogether from machine code, fun.
            byte[] versionArray = System.Text.Encoding.Unicode.GetBytes("X.Xx");
            byte[] replaceBuf = new byte[1];
            mioRelative.MemoryRead((IntPtr)Offset.nocashVersion, replaceBuf);
            versionArray[0] = replaceBuf[0];
            mioRelative.MemoryRead((IntPtr)Offset.nocashVersion + 10, replaceBuf);
            versionArray[4] = replaceBuf[0];
            mioRelative.MemoryRead((IntPtr)Offset.nocashVersion + 20, replaceBuf);
            versionArray[6] = replaceBuf[0];

            return versionOk(NOCASH_VERSION_CHECK, versionArray)
                ? (IntPtr?)baseAddr : null;
        }

        //Detect if supported version
        private static bool versionOk(string checkVersion, MemoryIO mioRelative, IntPtr versionPtr)
        {
            byte[] expected = System.Text.Encoding.Unicode.GetBytes(checkVersion);
            var versionBuf = new Byte[expected.Length];
            mioRelative.MemoryRead(versionPtr, versionBuf);
            return versionOk(checkVersion, versionBuf);
        }

        private static bool versionOk(string checkVersion, byte[] versionBuf)
        {
            byte[] expected = System.Text.Encoding.Unicode.GetBytes(checkVersion);
            if (memcmp(expected, versionBuf, new UIntPtr((uint)expected.Length)) == 0)
                return true; //Identical

            bool empty = true;
            foreach (byte b in versionBuf) if (b != 0) empty = false; //All bytes == 0?
            if (!empty) MessageWrongVersion(checkVersion); //Read error, maybe starting up
            return false; //Wrong version => Fail
        }

        private static void MessageWrongVersion(string emulator)
        {
            if (badVersionsMessaged.Contains(emulator)) return; //Message already shown
            badVersionsMessaged.Add(emulator);
            System.Windows.Forms.MessageBox.Show("Only " + emulator + " is supported",
                "Emulator version not supported",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Exclamation,
                System.Windows.Forms.MessageBoxDefaultButton.Button1);
        }
    }
}
