using NUnit.Framework;

namespace ECG_ISHME.Tests
{
    public class Tests
    {
        public toISHME obj;

        [SetUp]
        public void Setup()
        {
          obj = new toISHME();
          
        }

        [Test]
        [TestCase((uint) 1)]
        [TestCase((uint) 0)]
        [TestCase((uint)4294967295)]
        public void SetVarLengthBlockSize_ValidSize_ReturnTrue(uint size)
        {
            var result = obj.SetVarLengthBlockSize(size);
            Assert.IsTrue(result, $"{size} is invalid");
        }

        [TestCase((string) "../rawData/1234.bin")]  
        public void ReadRawData(string filepath)
        {
            //obj.TrimRawData(filepath);
            //var result = obj.output.Length;
            //Assert.AreEqual(result, ,0, $" is invalid");
        }

        //public void SetSampleSizeECG()
        //{
        //    var result = obj.SetSampleSizeECG();
        //    Assert.IsTrue();
        //}
        public void CalculateCRC_Equal()
        {
            //Assert.Pass();
        }
    }
}