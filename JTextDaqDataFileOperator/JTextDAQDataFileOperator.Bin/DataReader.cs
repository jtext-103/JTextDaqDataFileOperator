using System;
using System.IO;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.Bin
{
    public class DataReader : IDataReader
    {
        //配置文件
        private DataConfigFile config;

        //数据文件路径
        private string dataFilePath;

        public DataReader(DataConfigFile configFile, string dataFilePath)
        {
            config = configFile;
            this.dataFilePath = dataFilePath;
        }

        /// <summary>
        /// 从文件中读出指定通道的数据片段
        /// </summary>
        /// <param name="channelNo">指定的通道，从0开始</param>
        /// <param name="start">大于等于0</param>
        /// <param name="length">如果小于0则代表从start开始，读余下的所有点；大于等于零则代表读length个数的点,目前最多只支持int32，即大约8g数据</param>
        /// <returns></returns>
        public double[] LoadDataFromFile(int channelNo, ulong startU, ulong lengthU)
        {
            long start = (long)startU;
            int length = (int)lengthU;
            int totalChannelCount = config.TotalChannelCount;
            long totalSampleCount = config.TotalSampleCount;
            int sampleCountPerTime = config.SampleCountPerTime;
            //真正读数据的长度
            int readDataLength;
            if (start < 0)
            {
                throw new Exception("读取请求数据起始位置应大于等于0！");
            }
            else
            {
                if (channelNo > totalChannelCount)
                {
                    throw new Exception("通道号错误！");
                }
                if (length < 0)
                {
                    if (totalSampleCount - start > Int32.MaxValue)
                    {
                        throw new Exception("所读数据过大！");
                    }
                    readDataLength = (int)(totalSampleCount - start);
                }
                else
                {
                    readDataLength = length;
                    if (start + readDataLength > totalSampleCount)
                    {
                        throw new Exception("读取请求位置和长度不匹配！");
                    }
                }
            }
            double[] result = new double[readDataLength];
            int doubleSize = sizeof(double);
            //暂存每次读到的数据
            byte[] tempBytesPerRead = new byte[sampleCountPerTime * doubleSize];
            FileStream dataFileStream = new FileStream(dataFilePath, FileMode.Open);
            BinaryReader fileBinaryReader = new BinaryReader(dataFileStream);
            //已读数据个数（单位是double）
            int hasReadLength = 0;
            //每次读取的字节数
            int bytesCount = 0;

            //先找到开始的位置
            //第一个加号前：跳过的数据块
            //第二个加号前：当前数据块跳过的其它通道
            //第二个加号后：当前通道跳过的数据
            long initPosition = (start / sampleCountPerTime) * sampleCountPerTime * totalChannelCount + channelNo * sampleCountPerTime + start % sampleCountPerTime;
            fileBinaryReader.BaseStream.Seek(initPosition * doubleSize, SeekOrigin.Begin);
            //当前块该通道数据个数（包括第一个）
            int firstBlockThisChannelRemain = sampleCountPerTime - (int)(start % sampleCountPerTime);
            //只读取当前块即可满足要求
            if (firstBlockThisChannelRemain >= readDataLength)
            {
                bytesCount = readDataLength * doubleSize;
                tempBytesPerRead = fileBinaryReader.ReadBytes(bytesCount);
                Buffer.BlockCopy(tempBytesPerRead, 0, result, hasReadLength * doubleSize, bytesCount);
                return result;
            }
            //否则就要继续读
            else
            {
                //把当前块读完
                bytesCount = firstBlockThisChannelRemain * doubleSize;
                tempBytesPerRead = fileBinaryReader.ReadBytes(bytesCount);
                Buffer.BlockCopy(tempBytesPerRead, 0, result, hasReadLength * doubleSize, bytesCount);
                hasReadLength += firstBlockThisChannelRemain;
                //之后每次读一块数据，一直读到所需数据不足一整块
                //每次读sampleCountPerTime个数据
                bytesCount = sampleCountPerTime * doubleSize;
                //每次读完后移动流的位置
                int streamOffset = (totalChannelCount - 1) * sampleCountPerTime * doubleSize;
                while (readDataLength - hasReadLength >= sampleCountPerTime)
                {
                    //移动到下一数据块中该通道第一个数据前的位置
                    fileBinaryReader.BaseStream.Seek(streamOffset, SeekOrigin.Current);
                    tempBytesPerRead = fileBinaryReader.ReadBytes(bytesCount);
                    Buffer.BlockCopy(tempBytesPerRead, 0, result, hasReadLength * doubleSize, bytesCount);
                    hasReadLength += sampleCountPerTime;
                }
                //只要还有数据，把最后剩余不足一整块的数据读完
                int remainSampleCount = readDataLength - hasReadLength;
                if (remainSampleCount > 0)
                {
                    bytesCount = remainSampleCount * doubleSize;
                    fileBinaryReader.BaseStream.Seek(streamOffset, SeekOrigin.Current);
                    tempBytesPerRead = fileBinaryReader.ReadBytes(bytesCount);
                    Buffer.BlockCopy(tempBytesPerRead, 0, result, hasReadLength * doubleSize, bytesCount);
                    hasReadLength += remainSampleCount;
                }
                fileBinaryReader.Close();

                return result;
            }
        }

        public double[] LoadDataFromFileComplex(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            //unfinished
            return null;
        }

        public double[] LoadDataFromFileComplexRam(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            //unfinished
            return null;
        }

        public double[] LoadDataFromFileByTime(int channelNo, double startTime, double endTime, ulong stride)
        {
            //unfinished
            return null;
        }

        public double[] LoadDataFromFileByTimeFuzzy(int channelNo, double startTime, double endTime, ulong count)
        {
            //unfinished
            return null;
        }

        public int ChannelCount()
        {
            //unfinished
            return 0;
        }

        public int SampleCount()
        {
            //unfinished
            return 0;
        }

        public double SampleRate()
        {
            //unfinished
            return 0;
        }

        public double StartTime()
        {
            //unfinished
            return 0;
        }

        public double CreateTime()
        {
            throw new NotImplementedException();
        }

        public double[] LoadDataFromFile_TimeAxis(int channelNo, ulong start, ulong length)
        {
            throw new NotImplementedException();
        }

        public double[] LoadDataFromFileComplex_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            throw new NotImplementedException();
        }

        public double[] LoadDataFromFileComplexRam_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            throw new NotImplementedException();
        }

        public double[] LoadDataFromFileByTime_TimeAxis(int channelNo, double startTime, double endTime, ulong stride)
        {
            throw new NotImplementedException();
        }

        public double[] LoadDataFromFileByTimeFuzzy_TimeAxis(int channelNo, double startTime, double endTime, ulong count)
        {
            throw new NotImplementedException();
        }

        public Metadata Metadata()
        {
            throw new NotImplementedException();
        }

        public string[] AllDataFiles()
        {
            throw new NotImplementedException();
        }
    }
}
