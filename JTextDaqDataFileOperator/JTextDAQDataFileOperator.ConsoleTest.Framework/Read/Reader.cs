using System;
using System.Collections.Generic;
using System.Text;
using JTextDAQDataFileOperator.HDF5;
using JTextDAQDataFileOperator.Interface;
using Newtonsoft.Json;

namespace JTextDAQDataFileOperator.ConsoleTest
{
    public class Reader
    {
        public int ChannelNo { get; set; }

        IDataFileFactoty dataFileFactoty = new HDF5DataFileFactory();

        IDataReader dataReader;

        public Reader(string dataFileDirectory, string name, int channelNo)
        {
            ChannelNo = channelNo;
            dataReader = dataFileFactoty.GetReader(dataFileDirectory, name);
        }

        public void Begin()
        {
            Console.WriteLine("ChannelCount:" + dataReader.ChannelCount());
            Console.WriteLine("SampleCount:" + dataReader.SampleCount());
            Console.WriteLine("SampleRate:" + dataReader.SampleRate());
            Console.WriteLine("StartTime:" + dataReader.StartTime());
            Console.WriteLine("Metadata:" + JsonConvert.SerializeObject(dataReader.Metadata(), Formatting.Indented));
            foreach(var s in dataReader.AllDataFiles())
            {
                Console.WriteLine("File:" + s);
            }
            
            long ticks;

            //LoadDataFromFile
            ticks = DateTime.Now.Ticks;
            double[] data = dataReader.LoadDataFromFile(0, 0, 0);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFile:{0}s,{1}points", (double)ticks / 10000000, data.Length);

            ticks = DateTime.Now.Ticks;
            double[] dataTime = dataReader.LoadDataFromFile_TimeAxis(0, 0, 0);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFile_TimeAxis:{0}s,{1}points", (double)ticks / 10000000, dataTime.Length);

            if (data.Length != dataTime.Length) throw new Exception();
      
            //LoadDataFromFileComplex(Ram)
            ComplexVSComplexRam(0, 0, 2, 100000, 1);
            ComplexVSComplexRam(0, 0, 10000, 100, 1);
            ComplexVSComplexRam(0, 20, 100, 1000, 50);

            //LoadDataFromFileByTime
            ticks = DateTime.Now.Ticks;
            data = dataReader.LoadDataFromFileByTime(0, -1, 0.5, 2);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileByTime:{0}s,{1}points", (double)ticks / 10000000, data.Length);

            ticks = DateTime.Now.Ticks;
            dataTime = dataReader.LoadDataFromFileByTime_TimeAxis(0, -1, 0.5, 2);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileByTime_TimeAxis:{0}s,{1}points", (double)ticks / 10000000, dataTime.Length);

            if (data.Length != dataTime.Length) throw new Exception();

            //LoadDataFromFileByTimeFuzzy
            ticks = DateTime.Now.Ticks;
            data = dataReader.LoadDataFromFileByTimeFuzzy(0, -1, 0.5, 23333);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileByTimeFuzzy:{0}s,{1}points", (double)ticks / 10000000, data.Length);

            ticks = DateTime.Now.Ticks;
            dataTime = dataReader.LoadDataFromFileByTimeFuzzy_TimeAxis(0, -1, 0.5, 23333);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileByTimeFuzzy_TimeAxis:{0}s,{1}points", (double)ticks / 10000000, dataTime.Length);
        }

        private void ComplexVSComplexRam(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            long ticks;
            Console.WriteLine("(ch ={0}, start ={1}, stride ={2}, count ={3}, block ={4})",
                channelNo, start, stride, count, block);

            ticks = DateTime.Now.Ticks;
            double[] data1 = dataReader.LoadDataFromFileComplex(channelNo, start, stride, count, block);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileComplex:{0}s,{1}points", (double)ticks / 10000000, data1.Length);

            ticks = DateTime.Now.Ticks;
            double[] dataTime1 = dataReader.LoadDataFromFileComplex_TimeAxis(channelNo, start, stride, count, block);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileComplex_TimeAxis:{0}s,{1}points", (double)ticks / 10000000, dataTime1.Length);

            if (data1.Length != dataTime1.Length) throw new Exception();

            ticks = DateTime.Now.Ticks;
            double[] data = dataReader.LoadDataFromFileComplexRam(channelNo, start, stride, count, block);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileComplexRam:{0}s,{1}points", (double)ticks / 10000000, data.Length);

            ticks = DateTime.Now.Ticks;
            double[] dataTime = dataReader.LoadDataFromFileComplexRam_TimeAxis(channelNo, start, stride, count, block);
            ticks = DateTime.Now.Ticks - ticks;
            Console.WriteLine("LoadDataFromFileComplexRam_TimeAxis:{0}s,{1}points", (double)ticks / 10000000, dataTime.Length);

            if (data.Length != dataTime.Length) throw new Exception();
        }
    }
}
