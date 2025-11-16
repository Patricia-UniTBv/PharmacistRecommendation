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
        // Native declarations (unchanged signature, just safer usage)
        [DllImport("CidGen32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCid32(
            [MarshalAs(UnmanagedType.LPStr)] string pid,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder cid);

        [DllImport("CidGen64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCid64(
            [MarshalAs(UnmanagedType.LPStr)] string pid,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder cid);

        private delegate string? CidGenDelegate(string pid);
        private static readonly CidGenDelegate CidGenFunc;

        // Maximum expected CID length (increase from256 ->512 for safety)
        private const int BufferSize =512;

        static CidGen()
        {
            try
            {
                bool is64 = IntPtr.Size ==8;
                bool nativeOk = false;

                if (is64 && File.Exists(Path.Combine(AppContext.BaseDirectory, "CidGen64.dll")))
                {
                    nativeOk = TestNative((pid, sb) => GetCid64(pid, sb));
                    if (nativeOk) CidGenFunc = GetCidHashWin64;
                }
                else if (!is64 && File.Exists(Path.Combine(AppContext.BaseDirectory, "CidGen32.dll")))
                {
                    nativeOk = TestNative((pid, sb) => GetCid32(pid, sb));
                    if (nativeOk) CidGenFunc = GetCidHashWin32;
                }

                if (!nativeOk)
                {
                    // Fallback to managed implementation
                    CidGenFunc = GetCidHashNet;
                }
            }
            catch
            {
                CidGenFunc = GetCidHashNet;
            }
        }

        public static string? GetCidHash(string pid) => SafeInvoke(pid);

        // Unified safe wrapper (returns null if all strategies fail)
        public static string? SafeInvoke(string pid)
        {
            if (string.IsNullOrWhiteSpace(pid)) return null;
            try
            {
                return CidGenFunc(pid) ?? GetCidHashNet(pid);
            }
            catch
            {
                return GetCidHashNet(pid); // final fallback
            }
        }

        private static string? GetCidHashNet(string pid)
            => CryptoHash.GetCidHash(pid);

        private static string? GetCidHashWin32(string pid)
        {
            var sb = new StringBuilder(BufferSize);
            return GetCid32(pid, sb) ? sb.ToString() : null;
        }

        private static string? GetCidHashWin64(string pid)
        {
            var sb = new StringBuilder(BufferSize);
            return GetCid64(pid, sb) ? sb.ToString() : null;
        }

        // Previous implementation used a1 char buffer which could provoke native overwrite ⇒ potential heap corruption.
        private static bool TestNative(Func<string, StringBuilder, bool> fn)
        {
            try
            {
                var sb = new StringBuilder(BufferSize);
                // Use a harmless test input with expected shape; native should fill buffer safely.
                return fn("TEST", sb);
            }
            catch
            {
                return false;
            }
        }
    }
}