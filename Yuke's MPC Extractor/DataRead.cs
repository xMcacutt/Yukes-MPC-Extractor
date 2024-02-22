using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yukes_MPC_Extractor;

namespace Yukes_MPC_Extractor
{
    internal class DataRead
    {
        public unsafe static int ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                if (Program.LittleEndian)
                {
                    return *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
                }

                return (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
            }
        }

        public unsafe static short ToInt16(byte[] value, int startIndex)
        {
            fixed (byte* ptr = &value[startIndex])
            {
                if (Program.LittleEndian)
                {
                    return (short)(value[startIndex] | (value[startIndex + 1] << 8));
                }
                return (short)(value[startIndex + 1] | (value[startIndex] << 8));
            }
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint)ToInt32(value, startIndex);
        }

        public static unsafe ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)ToInt16(value, startIndex);
        }
    }
}
