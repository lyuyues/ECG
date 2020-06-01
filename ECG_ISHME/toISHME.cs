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

        // 0(non-exist) by defualt, set only when more space needed for additional info
        public bool SetVarLengthBlockSize(uint size)
        {
            fixLengthBlock.varLengthBlockSize = size;
            return true;
        }

        private void SetSampleSizeECG()
        {
            fixLengthBlock.sampleSizeECG = (uint)output.Length / 2 * SAMPLE_RATE;
        }

        private void SetOffsetVarLengthBlock()
        {
            if (header.VarLengthBlock == null) {
                fixLengthBlock.offsetVarLengthBlock = 0; // no VarLengthBlock
            } else
            {
                fixLengthBlock.offsetECGBlock = MAGICNUMBER_CRC_LEN + FIX_BLOCK_LEN;
            }
        }

        public void SetOffsetECGBlock()
        {
            fixLengthBlock.offsetECGBlock = MAGICNUMBER_CRC_LEN + FIX_BLOCK_LEN + fixLengthBlock.varLengthBlockSize;
        }

        public bool SetFileVersion(short version)
        {
            fixLengthBlock.fileVersion = version;
            return true;
        }

        public bool SetFirstName(String firstName)
        { int len = fixLengthBlock.firstName.Length;
            if (firstName == null || firstName.Length > len)
                return false;

            fixLengthBlock.firstName = toCharArray(firstName, len);
            return true;
        }

        public bool SetlastName(String lastName)
        { int len = fixLengthBlock.lastName.Length;
            if (lastName == null || lastName.Length > len)
                return false;

            fixLengthBlock.firstName = toCharArray(lastName, len);
            return true;
        }

        public bool SetID(String id)
        {
            int len = fixLengthBlock.id.Length;
            if (id == null || id.Length > len)
                return false;

            fixLengthBlock.id = toCharArray(id, len);
            return true;
        }

        public bool SetSex(String sex)
        {
            if (sex.Equals("Man"))
            {
                fixLengthBlock.sex = 1;
                return true;
            }
            else if (sex.Equals("Woman"))
            {
                fixLengthBlock.sex = 2;
                return true;
            }
                return false;

       
        }

        public bool SetRace(String race)
        {
            return false;
        }

        public bool SetBirthDate(String birthDate)
        {
            DateTime date = Convert.ToDateTime(birthDate);
            if (date == null)
                return false;
            fixLengthBlock.birthDate = new ushort[] { (ushort)date.Day, (ushort)date.Month, (ushort)date.Year };
            return true;
        }

        public bool SetRecordDate(String recordDate)
        {
            DateTime date = Convert.ToDateTime(recordDate);
            if (date == null)
                return false;
            fixLengthBlock.recordDate = new ushort[]{(ushort)date.Day, (ushort)date.Month, (ushort)date.Year};
            return true;
        }

        public bool SetFileDate(String fileDate)
        {
            DateTime date = Convert.ToDateTime(fileDate);
            if (date == null)
                return false;
            fixLengthBlock.fileDate = new ushort[] { (ushort)date.Day, (ushort)date.Month, (ushort)date.Year };
            return true;

        }

        public bool SetStartTime(DateTime startTime)
        {
            if (startTime == null)
                return false;
            fixLengthBlock.startTime = new ushort[] { (ushort)startTime.Hour, (ushort)startTime.Minute, (ushort)startTime.Second};
            return true;
        }

        private bool SetnLeads(ushort num)
        { if (num > 12) //a maximum of 12 leads will be stored
                return false;
            fixLengthBlock.nLeads = num;
            return true;
        }

        public bool SetLeadSpec()
        {
            return false;
        }

        public bool SetLeadQual()
        {
            return false;
        }

        public bool SetResolution()
        {
            return false;
        }

        public bool SetPacemaker()
        {
            return false;
        }

        public bool SetRecorder()
        {
            return false;
        }

        public void SetSamplingRate()
        {
            fixLengthBlock.samplingRate = SAMPLE_RATE;

        }

        public bool SetProprietary()
        {
            return false;
        }

        public bool SetCopyright()
        {
            return false;
        }

        public bool SetReserved()
        {
            return false;
        }



        static void Main(string[] args)
        {
            Console.WriteLine(short.MaxValue);
        }
    }
}
