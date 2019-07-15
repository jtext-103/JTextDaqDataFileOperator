using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HDF5JTextBase;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.HDF5
{
    public class DataWriter : IDataWriter
    {
        private DataFileConfig myHDF5FileConfig;

        //数据队列，public Accept 方法使用
        private Queue<double[,]> dataQueue;

        //是否已经不再传入数据
        private bool shouldFinish;

        //写文件完成
        private bool isFinished;

        //每通道采集总数
        private int totalSampleCount;

        private int channelCount;
        private double sampleRate;          
        private bool dataNeedTransposeWhenSaving;
        private double startTime;
        private string fullFilePath;

        /// <summary>
        /// 这里会建立 HDF5 数据文件并打开，同时开启异步写进程
        /// </summary>
        /// <param name="dataFileDirectory">存 HDF5 文件的文件夹路径</param>
        /// <param name="name">采集文件名称不带后缀</param>
        /// <param name="channelCount"></param>
        /// <param name="sampleRate"></param>
        /// <param name="dataNeedTransposeWhenSaving"></param>
        /// <param name="startTime"></param>
        public DataWriter(string dataFileDirectory, string name, int channelCount, double sampleRate, bool dataNeedTransposeWhenSaving, double startTime)
        {
            shouldFinish = false;
            isFinished = false;

            dataQueue = new Queue<double[,]>();
            
            this.channelCount = channelCount;
            this.sampleRate = sampleRate;
            this.dataNeedTransposeWhenSaving = dataNeedTransposeWhenSaving;
            this.startTime = startTime;

            totalSampleCount = 0;

            fullFilePath = dataFileDirectory + name + ".hdf5";
            myHDF5FileConfig = new DataFileConfig(fullFilePath, sampleRate, channelCount, startTime);

            var t = OperateFile();
        }

        /// <summary>
        /// 接收新数据，将其放入队列中
        /// </summary>
        /// <param name="data"></param>
        public void AcceptNewData(double[,] data)
        {
            //只需把引用传到 queue 中，保证 queue 中引用不会被外界修改
            double[,] cloneData = data;

            //入列
            dataQueue.Enqueue(cloneData);
        }

        /// <summary>
        /// 采集停止，将不再传入新数据。该方法会卡住主进程（调用这个类的），一直等待队列中数据写完
        /// </summary>
        public void FinishWrite()
        {
            shouldFinish = true;
            WaitUntilFinish();
        }

        // 一直等待，直到队列中数据写完，之后主进程继续
        private void WaitUntilFinish()
        {
            while (isFinished == false)
            {
                Thread.Sleep(100);
            }
        }

        // 操作数据文件,（异步）不断将数据写入流
        private async Task OperateFile()
        {
            //异步将数据写入流
            await Task.Run(() => { SendDataToBinaryWriter(); });

            //写完后设置和保存 HDF5 文件
            myHDF5FileConfig.SetAndClose(totalSampleCount);

            isFinished = true;
        }

        // dataQueue 数据出队列，并写入流中
        // 只要 dataQueue 中有数据，就要一直往流中写
        private void SendDataToBinaryWriter()
        {
            //如果已有数据没有写完或者还可能有新的数据
            while ((dataQueue.Count > 0) || (shouldFinish == false))
            {
                //队列中还有数据，将数据读出并写文件
                if (dataQueue.Count > 0)
                {
                    double[,] data = dataQueue.Dequeue();

                    //写入数据到 HDF5 文件
                    //如果数据格式为“data[每通道数据个数][通道个数]”则需要转置
                    //如果数据格式为“data[通道个数][每通道数据个数]”则不需要转置；
                    AppendDataToHDF5Writer(data, dataNeedTransposeWhenSaving);
                  
                    int sampleCountPerTime;
                    if (dataNeedTransposeWhenSaving)
                    {
                        sampleCountPerTime = data.GetLength(0);
                    }
                    else
                    {
                        sampleCountPerTime = data.GetLength(1);
                    }
                    totalSampleCount += sampleCountPerTime;
                }
                //队列中没数据，等待数据到来
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        // 将数据写入 HDF5 文件中，需要将二维数组转为一维数组
        // 每个通道存到一个单独的以 "channelX" 命名的一维 Dataset 中,从 0 开始
        // 数据均为double类型
        private void AppendDataToHDF5Writer(double[,] data, bool needTranspose)
        {     
            int channelCountGet;
            int dataLengthPerChannel;

            //转置完成的数据
            double[,] formatData;

            //对于需要转置的数据格式，先转置，然后才能写入 HDF5 
            if (needTranspose)
            {  
                channelCountGet = data.GetLength(1);
                dataLengthPerChannel = data.GetLength(0);
                formatData = new double[channelCountGet, dataLengthPerChannel];
                if (channelCountGet != channelCount)
                {
                    throw new Exception("channelCount from data[,] is not equal with Config!");
                } 
                for (int i = 0; i < dataLengthPerChannel; i++)
                {
                    for (int j = 0; j < channelCountGet; j++)
                    {
                        formatData[j, i] = data[i, j];
                    }
                }
            }
            //对于不用转置的数据格式，不用处理
            else
            {
                channelCountGet = data.GetLength(0);
                dataLengthPerChannel = data.GetLength(1);
                formatData = data;
            }
            
            //将 double[n, m] 转成 n 个 double[m]
            int channelByteLength = sizeof(double) * dataLengthPerChannel;
            byte[] buffer = new byte[channelByteLength];
            double[] data1Dim = new double[dataLengthPerChannel];
            for (int i = 0; i < channelCount; i++)
            {
                Buffer.BlockCopy(formatData, i * channelByteLength, data1Dim, 0, channelByteLength);
                myHDF5FileConfig.myHDF5Datasets[i].SimpleWrite1DimChunkDataset(data1Dim);
            }
        }

        /// <summary>
        /// 返回所有数据文件完整路径
        /// </summary>
        /// <returns></returns>
        public string[] FullPathOfAllDataFiles()
        {
            return new string[] { fullFilePath };
        }
    }
}
