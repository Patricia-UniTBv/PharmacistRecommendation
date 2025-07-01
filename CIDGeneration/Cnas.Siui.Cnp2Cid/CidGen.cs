#region Copyright ©2011-2013, SIVECO Romania SA - All Rights Reserved
// ======================================================================
// Copyright ©2011-2013 SIVECO Romania SA - All Rights Reserved
// ======================================================================
// This file and its contents are protected by Romanian and International
// copyright laws. Unauthorized reproduction and/or distribution of all
// or any portion of the code contained herein is strictly prohibited
// and will result in severe civil and criminal penalties.
// Any violations of this copyright will be prosecuted
// to the fullest extent possible under law.
// ======================================================================
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.
// ======================================================================
#endregion

#region References
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cnas.Siui.CidGen;
#endregion

namespace Cnas.Siui.Cnp2Cid
{
    public static class CidGen
    {
        // 1) Declare P/Invoke pentru 32-biți
        [DllImport("CidGen32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCid32(
            [MarshalAs(UnmanagedType.LPStr)] string pid,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder cid);

        // 2) Declare P/Invoke pentru 64-biți
        [DllImport("CidGen64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCid64(
            [MarshalAs(UnmanagedType.LPStr)] string pid,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder cid);

        // delegate comun
        private delegate string CidGenDelegate(string pid);
        private static readonly CidGenDelegate CidGenFunc;

        static CidGen()
        {
            bool nativeOk = false;
            bool is64 = IntPtr.Size == 8;

            if (is64 && File.Exists("CidGen64.dll"))
            {
                // înlocuim TestNative(GetCid64) cu un lambda
                nativeOk = TestNative((pid, sb) => GetCid64(pid, sb));
                if (nativeOk) CidGenFunc = GetCidHashWin64;
            }
            else if (!is64 && File.Exists("CidGen32.dll"))
            {
                nativeOk = TestNative((pid, sb) => GetCid32(pid, sb));
                if (nativeOk) CidGenFunc = GetCidHashWin32;
            }

            if (!nativeOk)
                CidGenFunc = GetCidHashNet;
        }


        public static string GetCidHash(string pid)
            => CidGenFunc != null ? CidGenFunc(pid) : null;

        // metoda managed din sursa CNAS
        private static string GetCidHashNet(string pid)
            => CryptoHash.GetCidHash(pid);

        // adaptor pentru Win32
        private static string GetCidHashWin32(string pid)
        {
            var sb = new StringBuilder(256);
            return GetCid32(pid, sb) ? sb.ToString() : null;
        }

        // adaptor pentru Win64
        private static string GetCidHashWin64(string pid)
        {
            var sb = new StringBuilder(256);
            return GetCid64(pid, sb) ? sb.ToString() : null;
        }

        // test rapid ca să nu arunce excepție ValveNotFound
        private static bool TestNative(Func<string, StringBuilder, bool> fn)
        {
            try
            {
                var sb = new StringBuilder(1);
                return fn("", sb); // un apel simplu
            }
            catch
            {
                return false;
            }
        }
    }
}