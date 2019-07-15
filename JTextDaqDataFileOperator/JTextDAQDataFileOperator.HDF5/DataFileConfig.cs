using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDF5JTextBase;
using System.IO;

namespace JTextDAQDataFileOperator.HDF5
{
    internal class DataFileConfig
    {
        //HDF5 文件
        private HDF5File myHDF5File;
        //HDF5 存放 Attribute 的组
        private HDF5Group myHDF5Group;

        //HDF5 数据，DataWriter 需要操作
        public HDF5Dataset[] myHDF5Datasets { get; set; }

        public DataFileConfig() { }

        /// <summary>
        /// 创建 HDF5 文件，并设置可以在采集开始的时候设置的 Attribute
        /// <param name="dataFilePath">HDF5 文件完整路径</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="channelCount">通道总数</param>
        /// <param name="startTime">采集开始时间</param>
        /// /// </summary>
        public DataFileConfig(string dataFilePath, double sampleRate, int channelCount, double startTime)
        {   
            //这个地方，之前的 BinData 不会有问题，因为只是创建了流，流写入文件是最后做的
            //而这里一开始就创建了文件，所以如果没有正常写完并调用 SetAndClose 方法，就会有文件打开而没有关闭的风险
            try
            {
                //创建文件
                myHDF5File = new HDF5File(dataFilePath);
                myHDF5Group = myHDF5File.CreateGroup("Attribute");

                //设置 Attribute
                myHDF5Group.SetAttribute("StartTime", HDF5AttributeTypes.DOUBLE, startTime);
                myHDF5Group.SetAttribute("SampleRate", HDF5AttributeTypes.DOUBLE, sampleRate);
                myHDF5Group.SetAttribute("ChannelCount", HDF5AttributeTypes.DOUBLE, channelCount);
                myHDF5Group.SetAttribute("CreateTime", HDF5AttributeTypes.DOUBLE, Convert.ToDouble(DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss")));

                //设置相应个数的 Dataset
                myHDF5Datasets = new HDF5Dataset[channelCount];
                for (int i = 0; i < channelCount; i++)
                {
                    myHDF5Datasets[i] = new HDF5Dataset();
                    myHDF5Datasets[i] = myHDF5File.CreateChunkDataset("channel" + i.ToString(), HDF5DataTypes.DOUBLE, new ulong[] { 1000 });
                }
            }
            catch(Exception e)
            {
                throw new Exception("Create HDF5 file Failed! Error message: " + e);
            }
        }

        /// <summary>
        /// 在采集结束时才能设置
        /// </summary>
        /// <param name="sampleCount">每通道采样总数</param>
        public void SetAndClose(int sampleCount)
        {
            myHDF5Group.SetAttribute("SampleCount", HDF5AttributeTypes.DOUBLE, sampleCount);

            //关闭所有打开的 HDF5 项目
            foreach(var d in myHDF5Datasets)
            {
                d.Close();
            }
            myHDF5Group.Close();
            myHDF5File.Close();
        }
    }
}
