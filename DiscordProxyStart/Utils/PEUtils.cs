using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordProxyStart.Utils
{
    internal class PEUtils
    {
        public enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169
        }

        public static MachineType GetExecutableMachineType(string executablePath)
        {
            // dos header is 96 bytes, last element, long at 0x3c, is a pointer to the pe header
            var dosheader = new byte[96];

            using (var fs = new FileStream(executablePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(dosheader, 0, 96);

                var peHeaderStart = BitConverter.ToInt32(dosheader, 60);

                var machineBytes = new byte[2];

                fs.Seek(peHeaderStart, SeekOrigin.Begin);

                fs.Seek(4, SeekOrigin.Current);   // skip the PE magic bytes ("PE\0\0")

                fs.Read(machineBytes, 0, 2);

                var machineType = (MachineType)BitConverter.ToUInt16(machineBytes, 0);

                return machineType;
            }
        }

    }
}
