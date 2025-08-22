using System.Runtime.InteropServices;
using System.Text;

namespace PharmacistRecommendation.Helpers
{
    public static class CidGen
    {
        [DllImport(@"\Libraries\CidGen64.dll", CharSet = CharSet.Ansi,
    CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCid(
            [MarshalAs(UnmanagedType.LPStr)] string pid,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder cid);

        public static string GetCidHash(string cnp)
        {
            var cid = new StringBuilder();
            return GetCid(cnp, cid) ? cid.ToString() : null;
        }
    }
}
