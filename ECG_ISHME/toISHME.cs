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

        public uint GetOffsetVarLengthBlock()
        {
            return fixLengthBlock.offsetVarLengthBlock;
        }

        public void SetOffsetECGBlock()
        {
            fixLengthBlock.offsetECGBlock = MAGICNUMBER_CRC_LEN + FIX_BLOCK_LEN + fixLengthBlock.varLengthBlockSize;
        }

        public uint GetOffsetECGBlock()
        {
            return fixLengthBlock.offsetECGBlock;
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

        public bool SetnLeads(ushort num)
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

        // copy string (from) to char array (des) 
        private char[] toCharArray(String from, int len)
        {
            char[] des = new char[len];
            char[] arr = from.ToCharArray();
            int i = 0;
            for (i = 0; i < arr.Length; i++)
            {
                des[i] = arr[i];
            }
            des[i] = '\0';
            return des;
        }


        /**
         * The storage size of one ECG sample has been fixed to two bytes. 
         * Data will be stored in the signed format with digital 0 matching 0 mV; 
         * most significant bit is "dedicated" to the sign and the range of 
         * stored values covers the interval from -32,768 to +32767. 
         * Negative values will be stored in a twocomplement way. 
         * All two-byte samples will be stored in little-endian form (LSB first).
         * 
         * the raw data from sensor: 
         *  byte 0  |   byte 1  |   byte 2  |   byte 3  | byte 4
         *  Reserved| high bits | low bits  | high bits | low bits
         *           of channel1  of chal 1   of chal 2   of chal 2
         *           
         * ISHME data form: (each sample sizes 2 bytes)        
         *    byte 0  |   byte 2  |   byte 3  | byte 4     
         *   low bits | high bits | low bits  | high bits 
         *   of chal 1  of chal 1   of chal 2   of chal 2 
         *   
         *   ch1,1st sample | ch2, 1st sample ...ch1,2nt sample | ch2, 2nd sample...
         */
        public void ReadRawData(String filepath)
        {
            // read file into byte[]
            try
            {
                input = File.ReadAllBytes(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Cannot open file.");
                return;
            }

            output = new byte[input.Length / 5 * 4];


            // write file
            short sample = 0;
            byte lowByte = 0;
            byte highByte = 0;
            ulong outputIdx = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (i % 5 == 0)
                    continue;
                else if (i % 5 == 1 || i % 5 == 3)
                {
                    output[outputIdx + 1] = input[i];
                }
                else
                {
                    output[outputIdx++] = input[i];
                }
            }
        }

        public void WarpHeader()
        {
            headerBlcok = new byte[FIX_BLOCK_LEN + fixLengthBlock.varLengthBlockSize];
            uint desIdx = 0;

            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.varLengthBlockSize), headerBlcok, desIdx, 4);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.sampleSizeECG), headerBlcok, desIdx, 4);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.offsetVarLengthBlock), headerBlcok, desIdx, 4);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.offsetECGBlock), headerBlcok, desIdx, 4);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.fileVersion), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.firstName), headerBlcok, desIdx, 40);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.lastName), headerBlcok, desIdx, 40);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.id), headerBlcok, desIdx, 20);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.sex), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.race), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.birthDate), headerBlcok, desIdx, 6);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.recordDate), headerBlcok, desIdx, 6);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.fileDate), headerBlcok, desIdx, 6);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.startTime), headerBlcok, desIdx, 6);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.nLeads), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.leadSpec), headerBlcok, desIdx, 24);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.leadQual), headerBlcok, desIdx, 24);
            desIdx = CopyBytes(ConvertToByteArray(fixLengthBlock.resolution), headerBlcok, desIdx, 24);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.pacemaker), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.recorder), headerBlcok, desIdx, 40);
            desIdx = CopyBytes(BitConverter.GetBytes(fixLengthBlock.samplingRate), headerBlcok, desIdx, 2);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.proprietary), headerBlcok, desIdx, 80);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.copyright), headerBlcok, desIdx, 80);
            desIdx = CopyBytes(Encoding.ASCII.GetBytes(fixLengthBlock.reserved), headerBlcok, desIdx, 88);
            if (fixLengthBlock.varLengthBlockSize != 0)
            {
                CopyBytes(Encoding.ASCII.GetBytes(header.VarLengthBlock.reserved), headerBlcok, desIdx, fixLengthBlock.varLengthBlockSize);
            }
        }

        private uint CopyBytes(byte[] source, byte[] headerBlcok, uint destinationIndex, uint len)
        {
            Array.Copy(source, 0, headerBlcok, destinationIndex, len);
            return destinationIndex + len;

        }

        private byte[] ConvertToByteArray(ushort[] source)
        {
            byte[] desArray = new byte[source.Length * 2];
            int idx = 0;
            for (int i = 0; i < source.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(source[i]), 0, desArray, idx, 2);
                idx += 2;
            }
            return desArray;
        }

        private byte[] ConvertToByteArray(short[] source)
        {
            byte[] desArray = new byte[source.Length];
            int idx = 0;
            for (int i = 0; i < source.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(source[i]), 0, desArray, idx, 2);
            }
                return desArray;
        }

        /**
         * Calculate a CRC-CCITT checksum ((X^16 + X^12 + X^5 + 1).  1 0001 0000 0010 0001
         * The CRC is a 16-bit quantity and should be preset to all 1s ($FFFF) at the start of the calculation
         * for each block of data. Note: all operations are on bytes.
         * The final check on CRC is accomplished by adding or concatenating CRCHI and CRCLO at the end of data stream.
         */
        public void CalculateCRC()
        {
            byte A; // new byte
            byte B; // temp byte
            byte CRCHI = 0xff; // High Byte(most significant) of the 16 - bit CRC
            byte CRCLO = 0xff; // Low Byte (least significant) of the 16-bit CRC

            for (int i = 0; i < headerBlcok.Length; i++) {
                A = headerBlcok[i];
                A = (byte) (A ^ CRCHI);                CRCHI = A;                A = (byte) (A >> 4);   //SHIFT A RIGHT FOUR TIMES { ZERO FILL}
                A = (byte) (A ^ CRCHI); //{ I J K L M N O P}
                CRCHI = CRCLO;  //swap CRCHI, CRCLO
                CRCLO = A;                A = (byte)(A << 4); //A LEFT 4 TIMES { M N O P I J K L}
                B = A;  //temp save 
                A = (byte)((A >> 7) | (A << 1));    //ROTATE A LEFT ONCE { N O P I J K L M}
                A = (byte) (A & 0x1f); // {0 0 0 I J K L M}
                CRCHI = (byte) (A ^ CRCHI);                A = (byte) (B & 0xf0);  //{ M N O P 0 0 0 0)                CRCHI = (byte) (A ^ CRCHI);  // CRCHI complete
                B = (byte)((B >> 7) | (B << 1));    //ROTATE B LEFT ONCE { N O P 0 0 0 0 M}
                B = (byte) (B & 0xe0); // (NOP 0 0 0 0 0 )                CRCLO = (byte) (B ^ CRCLO); // CRCLO complete
            }


        }


        public void WarpPackage()
        {
            // creat output file
            String outputFile = @""; // output file path formatted in ISHME
            FileStream fs;

            // create file
            try
            {
                fs = File.Create(outputFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Cannot create file.");
                return;
            }
            try
            {
                fs.Write(Encoding.ASCII.GetBytes(package.MagicNumber));
                fs.Write(BitConverter.GetBytes(package.CheckSum));
                fs.Write(headerBlcok);
                fs.Write(output);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Cannot write file.");
                return;
            }
        }

        /**
         * convert raw data value into format with digital 0 matching 0 mV
         * stored values covers the interval from -32,768 to +32767 (2 bytes).
        */
        private static short Reformat(byte input)
        {
            //    if (input > DIV)
            //        input = -(((~input) & MAX_INPUT) + 1);
            //    return input * 1.0d / DIV * MAX_VOLTAGE;
            return 0;
        }
        static void Main(string[] args)
        {
            byte A;
            byte B; 
            byte CRCLO = 0xff;
            byte CRCHI = 0xff;


            A = 0xa5; // 1010 0101
            Console.WriteLine(Convert.ToString(A, 2));
            A = (byte)(A ^ CRCHI); // 0101 1010
            Console.WriteLine(Convert.ToString(A, 2));
            CRCHI = A;  
            A = (byte)(A >> 4);   //0000 0101 SHIFT A RIGHT FOUR TIMES { ZERO FILL}
            Console.WriteLine(Convert.ToString(A, 2));
            A = (byte)(A ^ CRCHI); //0000 0101 ^  0101 1010 = 0101 1111 { I J K L M N 0 P}
            Console.WriteLine(Convert.ToString(A, 2));
            CRCHI = CRCLO;  //swap CRCHI, CRCLO,  CRCHI = 1111 1111
            CRCLO = A;   // CRCLO  = 0101 1111
            A = (byte)(A << 4); //A LEFT 4 TIMES { M N 0 P I J K L} 1111 0000
            Console.WriteLine(Convert.ToString(A, 2));
            B = A;  //temp save  1111 0000
            A = (byte)((A >> 7)|(A << 1)); //ROTATE A LEFT ONCE { N 0 P I J K L M} 1110 0001
            Console.WriteLine(Convert.ToString(A, 2));
            A = (byte)(A & 0x1f); // {0 0 0 I J K L M}  1110 0001 & 0001 1111 = 0000 0001
            Console.WriteLine(Convert.ToString(A, 2));
            CRCHI = (byte)(A ^ CRCHI);  // 0000 0001 ^ 1111 1111 = 1111 1110
            A = (byte)(B & 0xf0);  //{ M N 0 P 0 0 0 0)  1111 0000 & 1111 0000 = 1111 0000
            Console.WriteLine(Convert.ToString(A, 2));
            Console.WriteLine();
            CRCHI = (byte)(A ^ CRCHI);  // CRCHI complete 1111 0000 ^ 1111 1111 = 0000 1111
            Console.WriteLine(Convert.ToString(B, 2));
            B = (byte)((B >> 7) | (B << 1));   //ROTATE B LEFT ONCE {N 0 P 0 0 0 0 M}
            Console.WriteLine(Convert.ToString(B, 2));
            B = (byte)(B & 0xe0); // (NOP 0 0 0 0 0 ) & 1110 0000 = 
            CRCLO = (byte)(B ^ CRCLO); // CRCLO complete

           
        }
    }
}
