using System;
using System.IO;
using System.Text;

/**
 * Convert the raw data form sensor to ISHME data format due to mathch the input of data analysis tools.
 * Also, add information into ISHME format data.
 * The sequential structure of the Standard ISHME Output File :
 * three blocks of data preceded by a magic number and a checksum calculated over the two blocks of the header.
 * Magic number + CRC + Header( fixed length block + var length block) + ECG data
 */

// author: Yue Lyu
// Date: May, 2020

namespace ECG_ISHME
{
    class ISHMEPackage
    { 
        public string MagicNumber { get; set; } // 8 bytes
        public short CheckSum { get; set; } // 2 bytes
        public Header Header { get; set; } // 512 + var bytes
    }

    class Header
    { 
        public FixLengthBlock FixLengthBlock { get; set; } // 512 bytes
        public VarLengthBlock VarLengthBlock { get; set; } // var bytes
    }

    class FixLengthBlock
    {  // the fixed-length (512 bytes) header block.
        public uint varLengthBlockSize = 0;    // size(in bytes) of variable length block: 4 bytes
        public uint sampleSizeECG; // size (in samples) of ECG: 4 bytes
        public uint offsetVarLengthBlock;  // offset of variable length block(in bytes from beginning of file): 4 byte
        public uint offsetECGBlock;    // offset of ECG block(in bytes from beginning of life): 4 bytes
        public short fileVersion; // version of the file: 2 bytes
        public char[] firstName = new char[40]; // subject first name: 40 bytes
        public char[] lastName = new char[40];  //subject last name: 40 bytes
        public char[] id = new char[20];    // subject ID: 20 bytes
        public ushort sex = 0;  // subject sex: (0: unknown, 1: male, 2:female) 2 bytes
        public ushort race = 0; // subject race: (0: unknown, 1: Caucasian, 2:Black, 3: Oriental, 4-9: Reserved) 2 bytes
        public ushort[] birthDate = new ushort[3]; // Date of birth(European: day, month, year): 6 bytes
        public ushort[] recordDate = new ushort[3];    // Date of recording (European: day, month, year): 6 bytes
        public ushort[] fileDate = new ushort[3];  // Date of creation of Output file (European): 6 bytes
        public ushort[] startTime = new ushort[3];    // Start time (European: hour[0-23], min, sec): 6 bytes
        public ushort nLeads = 2;// number of stored leads: 2 bytes  
        public short[] leadSpec = new short[12]; // lead specification:  2 * 12 bytes
        public short[] leadQual = new short[12]; // lead quality: 2* 12 bytes
        public short[] resolution = new short[12];   // Amplitude resolution in integer no.of nV: 2* 12 bytes
        public short pacemaker = 0; // Pacemaker code: 2 bytes
        public char[] recorder = new char[40]; //Type of recorder (either analog or digital): 40 bytes
        public ushort samplingRate; // Sampling rate (in hertz): 2 bytes
        public char[] proprietary = new char[80];   // Proprietary of ECG (if any): 80 bytes
        public char[] copyright = new char[80]; //Copyright and restriction of diffusion(if any): 80 bytes
        public char[] reserved = new char[88];  //88 bytes
    }

    /**
     * The variable-length block will consist simply of a stream of ASCII 
     * (extended set of 256 characters) characters that any user or manufacturer 
     * What will use according to his needs.
     */
    class VarLengthBlock
    {
        public char[] reserved; 
    }

    class toISHME
    {
        ISHMEPackage package;
        Header header;
        FixLengthBlock fixLengthBlock;
        private static readonly ushort SAMPLE_RATE = 250;
        private static readonly uint MAGICNUMBER_CRC_LEN = 10;
        private static readonly uint FIX_BLOCK_LEN = 512;

        byte[] headerBlcok;
        byte[] input;
        byte[] output;


        public toISHME()
        {
            package = new ISHMEPackage();
            header = package.Header;
            fixLengthBlock = header.FixLengthBlock;
        }

    


        static void Main(string[] args)
        {
            Console.WriteLine(short.MaxValue);
        }
    }
}
