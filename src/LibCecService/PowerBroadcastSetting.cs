using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibCecService
{
    /// <summary>
    /// Structure that provides details about the power broadcast event
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct POWERBROADCAST_SETTING
    {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }
}
