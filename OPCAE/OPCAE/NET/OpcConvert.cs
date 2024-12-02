using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPCAE.NET
{


    public class OpcConvert
    {
        public static FILETIME LongToFILETIME(long lt)
        {
            FILETIME filetime;
            LongBits bits = new LongBits {
                Long = lt
            };
            filetime.dwHighDateTime = bits.Int2;
            filetime.dwLowDateTime = bits.Int1;
            return filetime;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct LongBits
        {
            [FieldOffset(0)]
            public long Long;
            [FieldOffset(0)]
            public int Int1;
            [FieldOffset(4)]
            public int Int2;
        }
    }
}

