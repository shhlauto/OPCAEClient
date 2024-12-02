using System;
using System.Runtime.InteropServices;
namespace OPCAE.OPCAE
{
    [StructLayout(LayoutKind.Sequential, Pack=2), ComVisible(true)]
    public class DUMMY_VARIANT
    {
        public static short VT_TYPEMASK = 0xfff;
        public static short VT_VECTOR = 0x1000;
        public static short VT_ARRAY = 0x2000;
        public static short VT_BYREF = 0x4000;
        public static short VT_ILLEGAL = -1;
        public short vt;
        public short r1;
        public short r2;
        public short r3;
        public int v1;
        public int v2;
        [DllImport("oleaut32.dll")]
        public static extern void VariantInit(IntPtr addrofvariant);
        [DllImport("oleaut32.dll")]
        public static extern int VariantClear(IntPtr addrofvariant);
        public static string VarEnumToString(VarEnum vevt)
        {
            string str = "";
            short num = (short) vevt;
            if (num == VT_ILLEGAL)
            {
                return "VT_ILLEGAL";
            }
            if ((num & VT_ARRAY) != 0)
            {
                str = str + "VT_ARRAY | ";
            }
            if ((num & VT_BYREF) != 0)
            {
                str = str + "VT_BYREF | ";
            }
            if ((num & VT_VECTOR) != 0)
            {
                str = str + "VT_VECTOR | ";
            }
            VarEnum enum2 = ((VarEnum) num) & ((VarEnum) VT_TYPEMASK);
            return (str + enum2.ToString());
        }

        public static int ConstSize
        {
            get
            {
                if (IntPtr.Size == 8)
                {
                    return 0x18;
                }
                return 0x10;
            }
        }
    }
}

