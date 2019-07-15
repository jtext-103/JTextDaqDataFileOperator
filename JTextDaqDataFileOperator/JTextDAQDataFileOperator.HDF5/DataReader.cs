using System;
using System.Collections.Generic;
using System.Linq;
using HDF5JTextBase;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.HDF5
{
    public class DataReader : IDataReader
    {
        private string dataFilePath;

        private HDF5File myH5File;
        private HDF5Group myH5Group;
        private HDF5Dataset myH5Dataset;

        //初始化时提前存好，以后查询不用重新打开文件
        private int channelCount;
        private int sampleCount;
        private double sampleRate;
        private double startTime;
        private double createTime;

        //文件名，不带路径和后缀
        private string name;

        /// <summary>
        /// 依照 BinData 签名的构造函数
        /// </summary>
        /// <param name="dataFileDirectory">HDF5 文件所在文件夹</param>
        /// <param name="name">HDF5 文件名不带后缀</param>
        public DataReader(string dataFileDirectory, string name)
        {
            this.name = name;
            this.dataFilePath = dataFileDirectory + name + ".hdf5";

            myH5File = new HDF5File();
            myH5File.Open(this.dataFilePath);
            myH5Group = myH5File.GetGroup("Attribute");

            // double 转 object 后，object 不能直接转 int，需要先转 double 再转 int
            double d = (double)myH5Group.GetAttribute("ChannelCount");
            channelCount = (int)d;
            d = (double)myH5Group.GetAttribute("SampleCount");
            sampleCount = (int)d;
            sampleRate = (double)myH5Group.GetAttribute("SampleRate");
            startTime = (double)myH5Group.GetAttribute("StartTime");
            createTime = (double)myH5Group.GetAttribute("CreateTime");

            myH5Group.Close();
            myH5File.Close();
        }

        #region 根据采样点读数据

        /// <summary>
        /// 从文件中读出指定通道的数据片段
        /// </summary>
        /// <param name="channelNo">指定的通道，从 0 开始</param>
        /// <param name="start">大于等于 0 </param>
        /// <param name="length">等于 0 则代表从 start 开始，读余下的所有点；大于零则代表读 length 个数的点，最多不超过 sampleCount</param>
        /// <returns></returns>
        public double[] LoadDataFromFile(int channelNo, ulong start, ulong length)
        {
            if(start == 0 && length == 0)
            {
                length = (ulong)sampleCount;
            }
            return LoadDataFromFileComplex(channelNo, start, 1, 1, length);
        }

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public double[] LoadDataFromFile_TimeAxis(int channelNo, ulong start, ulong length)
        {
            if (start == 0 && length == 0)
            {
                length = (ulong)sampleCount;
            }
            return LoadDataFromFileComplex_TimeAxis(channelNo, start, 1, 1, length);
        }

        /// <summary>
        /// 从文件中读出指定通道的数据片段，支持复杂读取
        /// </summary>
        /// <param name="channelNo">指定的通道，从 0 开始</param>
        /// <param name="start">开始点</param>
        /// <param name="stride">每次向后多少点</param>
        /// <param name="count">读多少次</param>
        /// <param name="block">每次读多少个连续点</param>
        /// <returns></returns>
        public double[] LoadDataFromFileComplex(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            if (!IsParamsRight(channelNo, start, stride, count, block)) return null;

            try
            {
                myH5File.Open(dataFilePath);
                myH5Dataset = myH5File.GetDataset("channel" + channelNo.ToString());
                double[] data = (double[])myH5Dataset.ComplexRead1DimChunkDataset(start, stride, count, block);
                myH5Dataset.Close();
                myH5File.Close();

                return data;
            }
            catch
            {
                throw new Exception("Something wrong with Data Read, may be you can adjust your params and try again.");
            }
        }

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <returns></returns>
        public double[] LoadDataFromFileComplex_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            if (!IsParamsRight(channelNo, start, stride, count, block)) return null;

            double[] timeAxis;
            try
            {
                timeAxis = new double[count * block];
                double firstTime = startTime + start / sampleRate;
                for (ulong i = 0; i < count; i++)
                {
                    for (ulong j = 0; j < block; j++)
                    {
                        timeAxis[i * block + j] = firstTime + i * stride / sampleRate + j / sampleRate;
                    }
                }
            }
            catch
            {
                timeAxis = null;
            }
            return timeAxis;
        }

        /// <summary>
        /// 先调用 3 参数 LoadDataFromFile，之后转换成和 5 参数的一样的结果
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public double[] LoadDataFromFileComplexRam(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            if (!IsParamsRight(channelNo, start, stride, count, block)) return null;

            ulong length = stride * (count - 1) + block;
            double[] dataRam = LoadDataFromFile(channelNo, start, length);
            double[] data = new double[count * block];
            for(int i = 0, k = 0; i < (int)count; i++)
            {
                for(int j = 0; j < (int)block; j++, k++)
                {
                    data[k] = dataRam[i * (int)stride + j];
                }
            }
            return data;
        }

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <returns></returns>
        public double[] LoadDataFromFileComplexRam_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            //这两个方法不读数据，没区别
            return LoadDataFromFileComplex_TimeAxis(channelNo, start, stride, count, block);
        }

        //上面几个方法用
        private bool IsParamsRight(int channelNo, ulong start, ulong stride, ulong count, ulong block)
        {
            if (channelNo > channelCount || channelNo < 0)
            {
                throw new Exception("ERROR ChannelNo! Should be from 0 to ChannelCount-1");
            }
            if (stride == 0 || count == 0 || block == 0)
            {
                throw new Exception("stride, count or block should >= 1");
            }
            return true;
        }

        #endregion

        #region 根据时间读数据

        /// <summary>
        /// 根据时间读取数据
        /// 注意！！！如果 startTime 和 endTime 过近，或者 strdie 过大，可能导致结果没有按照理想时间分布或直接报错
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="startTime">想要的开始时间</param>
        /// <param name="endTime">想要的结束时间</param>
        /// <param name="stride">两点之间差多少点（最小是1）</param>
        /// <returns></returns>
        public double[] LoadDataFromFileByTime(int channelNo, double startTime, double endTime, ulong stride)
        {
            if (!IsParamsRight(channelNo, startTime, endTime, stride)) return null;

            //两个时间点之间的数据量
            ulong sample = (ulong)((endTime - startTime) * sampleRate);

            //开始点
            ulong start = (ulong)((startTime - this.startTime) * sampleRate);

            return LoadDataFromFileComplexRam(channelNo, start, stride, sample / stride, 1);
        }

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <returns></returns>
        public double[] LoadDataFromFileByTime_TimeAxis(int channelNo, double startTime, double endTime, ulong stride)
        {
            if (!IsParamsRight(channelNo, startTime, endTime, stride)) return null;

            ulong sample = (ulong)((endTime - startTime) * sampleRate);
            ulong start = (ulong)((startTime - this.startTime) * sampleRate);

            return LoadDataFromFileComplexRam_TimeAxis(channelNo, start, stride, sample / stride, 1);
        }

        /// <summary>
        /// 根据时间读取大约 count 个点，数据是时间轴均匀的，所以数据大概比 count 多一些
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="count">希望读取的点数</param>
        /// <returns></returns>
        public double[] LoadDataFromFileByTimeFuzzy(int channelNo, double startTime, double endTime, ulong count)
        {
            if (!IsParamsRight(channelNo, startTime, endTime, count)) return null;

            //两个时间点之间的数据量
            ulong sample = (ulong)((endTime - startTime) * sampleRate);
            ulong start = (ulong)((startTime - this.startTime) * sampleRate);
            ulong stride = sample / count;

            return LoadDataFromFileComplexRam(channelNo, start, stride, sample / stride, 1);
        }

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <returns></returns>
        public double[] LoadDataFromFileByTimeFuzzy_TimeAxis(int channelNo, double startTime, double endTime, ulong count)
        {
            if (!IsParamsRight(channelNo, startTime, endTime, count)) return null;

            ulong sample = (ulong)((endTime - startTime) * sampleRate);
            ulong start = (ulong)((startTime - this.startTime) * sampleRate);
            ulong stride = sample / count;

            return LoadDataFromFileComplexRam_TimeAxis(channelNo, start, stride, sample / stride, 1);
        }

        //上面几个方法用
        private bool IsParamsRight(int channelNo, double startTime, double endTime, ulong strideOrCount)
        {
            if(startTime < this.startTime)
            {
                throw new Exception("startTime is to early!");
            }
            if(endTime < startTime)
            {
                throw new Exception("endTime < startTime!");
            }
            if (strideOrCount == 0)
            {
                throw new Exception("strideOrCount should >= 1.");
            }
            return true;
        }

        #endregion

        #region metadata

        /// <summary>
        /// 获取通道总数
        /// </summary>
        /// <returns></returns>
        public int ChannelCount()
        {
            return channelCount;
        }

        /// <summary>
        /// 获取每通道采样数
        /// </summary>
        /// <returns></returns>
        public int SampleCount()
        {
            return sampleCount;
        }

        /// <summary>
        /// 获取采样率
        /// </summary>
        /// <returns></returns>
        public double SampleRate()
        {
            return sampleRate;
        }

        /// <summary>
        /// 获取开始时间
        /// </summary>
        /// <returns></returns>
        public double StartTime()
        {
            return startTime;
        }

        /// <summary>
        /// 获取文件创建时间
        /// </summary>
        /// <returns></returns>
        public double CreateTime()
        {
            return createTime;
        }

        /// <summary>
        /// 获取包含所有 metadata 的类
        /// </summary>
        /// <returns></returns>
        public Metadata Metadata()
        {
            return new Metadata
            {
                ChannelCount = channelCount,
                SampleCount = sampleCount,
                SampleRate = sampleRate,
                StartTime = startTime,
                CreateTime = createTime
            };
        }

        /// <summary>
        /// 获取所有采集文件名
        /// </summary>
        /// <returns></returns>
        public string[] AllDataFiles()
        {
            return new string[]
            {
                name + ".hdf5"
            };
        }

        #endregion
    }
}
