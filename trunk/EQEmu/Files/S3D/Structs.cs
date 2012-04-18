using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EQEmu.Files.S3D
{
    public class ArchiveFile
    {
        public string Name { get; set; }
        public PFSMeta Meta { get; set; }
        public byte[] Bytes { get; set; }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    unsafe struct PFSHeader
    {
        public UInt32 Offset;                 // This is a pointer to the file meta block.
        public fixed byte MagicCookie[4];     // As a character byte string, it is PFS which is an identifier as a PFS archive.
        public UInt32 Unknown;                // Unknown - Seems to always be 131072
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct PFSMeta
    {
        public UInt32 CRC;          // IEEE 802.3 Ethernet CRC-32 checksum for the end file
        public UInt32 Offset;                 // Pointer to the first compressed data block in the archive
        public UInt32 Size;                   // The real size of the decompressed file
    };



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct PFSData
    {
        public UInt32 DeflatedLength;
        public UInt32 InflatedLength;
    };
}
