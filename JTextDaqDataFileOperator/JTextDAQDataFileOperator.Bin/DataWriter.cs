using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.Bin
{
    /// <summary>
    /// 暂时保存采集到的的数据并将其按一定格式存入文件
    /// </summary>
    public class DataWriter : IDataWriter
    {
        //数据队列
        private Queue<double[,]> dataQueue;

        //用于写二进制数据文件
        private BinaryWriter fileBinaryWriter;

        //是否已经不再传入数据flag
        private bool shouldFinish { get; set; }

        //写文件完成flag
        private bool isFinished { get; set; }

        //通道个数
        private int totalChannelCount;

        //任务完成后，保存配置文件
        private DataConfigFile configFile;

        //配置文件需要这些信息，方便读取
        //private double sampleRate;

        /// <summary>
        /// 任务完成后，写配置文件的路径
        /// </summary>
        public string ConfigFilePath { get; private set; }

        /// <summary>
        /// 每通道写入数据总数
        /// </summary>
        public long TotalSampleCount { get; private set; }

        /// <summary>
        /// 数据是否需要转置
        /// </summary>
        public bool DataNeedTransposeWhenSaving { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataFilePath">存文件的完整路径</param>
        /// <param name="configFilePath">存配置文件的完整路径</param>
        /// <param name="channelCount"></param>
        /// <param name="sampleRate"></param>
        /// <param name="dataNeedTransposeWhenSaving"></param>
        public DataWriter(string dataFilePath, string configFilePath, int channelCount, double sampleRate, bool dataNeedTransposeWhenSaving)
        {
            configFile = new DataConfigFile();
            shouldFinish = false;
            isFinished = false;
            dataQueue = new Queue<double[,]>();
            ConfigFilePath = configFilePath;
            TotalSampleCount = 0;
            totalChannelCount = channelCount;
            configFile.TotalChannelCount = channelCount;
            configFile.SampleRate = sampleRate;
            DataNeedTransposeWhenSaving = dataNeedTransposeWhenSaving;
            //建立数据文件，并打开流，准备写数据
            FileStream fs = new FileStream(dataFilePath, FileMode.Create);
            fileBinaryWriter = new BinaryWriter(fs);

            var t = OperateFile();
                        
        }

        /// <summary>
        /// 接收新数据
        /// 将新数据放入队列中
        /// </summary>
        /// <param name="data"></param>
        public void AcceptNewData(double[,] data)
        {
            //只需把引用传到queue中，保证queue中引用不会被外界修改
            double[,] cloneData = data;

            //入列
            dataQueue.Enqueue(cloneData);
        }

        /// <summary>
        /// 采集停止，将不再传入新数据
        /// 该方法会卡住主进程
        /// 一直等待队列中数据写完
        /// </summary>
        public void FinishWrite()
        {
            shouldFinish = true;
            waitUntilFinish();
        }

        /// <summary>
        /// 一直等待，直到队列中数据写完
        /// </summary>
        private void waitUntilFinish()
        {
            while (isFinished == false)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 操作数据文件
        /// 1.（异步）不断将数据写入流
        /// 2.完成后，关闭流，保存数据文件
        /// 3.保存配置文件
        /// 4.IsFinished = true
        /// </summary>
        /// <returns></returns>
        private async Task OperateFile()
        {
            //异步将数据写入流
            await Task.Run(() => { sendDataToBinaryWriter(); });
            //写完后关闭流，保存文件
            fileBinaryWriter.Close();
            //保存配置文件
            configFile.TotalSampleCount = TotalSampleCount;
            configFile.Save(ConfigFilePath);

            isFinished = true;
        }

        /// <summary>
        /// 数据出队列，并写入流中
        /// 第一次读到数据时设置configFile.SampleCountPerTime
        /// 只要queue中有数据，就要一直写
        /// </summary>
        /// <param name="fileBinaryWriter"></param>
        private void sendDataToBinaryWriter()
        {
            //第一次读到数据后，设置configFile.SampleCountPerTime
            bool hasRun = false;
            while ((dataQueue.Count > 0) || (shouldFinish == false))
            {
                //队列中还有数据，将数据读出并写文件
                if (dataQueue.Count > 0)
                {
                    double[,] data = dataQueue.Dequeue();
                    AppendDataToBinaryWriter(data, DataNeedTransposeWhenSaving);
                    //如果数据格式为“data[通道个数][每通道数据个数]”则不需要转置；
                    //如果数据格式为“data[每通道数据个数][通道个数]”则需要转置
                    int sampleCountPerTime;
                    if (DataNeedTransposeWhenSaving)
                    {
                        sampleCountPerTime = data.GetLength(0);
                    }
                    else
                    {
                        sampleCountPerTime = data.GetLength(1);
                    }
                    TotalSampleCount += sampleCountPerTime;
                    if (!hasRun)
                    {
                        configFile.SampleCountPerTime = sampleCountPerTime;
                        hasRun = true;
                    }
                }
                //队列中没数据，等待数据到来
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 将数据写入BinaryWriter中
        /// 将二维数组转为一维数组，再存入文件
        /// 存入文件的数据格式为ch0[0],ch0[1],ch0[2]...ch1[0],ch1[1],ch1[2]...ch2[0],ch1[1],ch2[2]...
        /// 如果数据格式为“data[通道个数][每通道数据个数]”则不需要转置；即与保存的格式相同
        /// 如果数据格式为“data[每通道数据个数][通道个数]”则需要转置后，再保存
        /// 数据均为double格式
        /// </summary>
        /// <param name="data"></param>
        /// <param name="needTranspose"></param>
        public void AppendDataToBinaryWriter(double[,] data, bool needTranspose)
        {
            //double类型字节长度，理论上是8
            int doubleSize = sizeof(double);
            //T[,] 转成byte[]，再将该byte[] 写文件，理论上能加快速度
            int byteBufferLength = data.GetLength(0) * data.GetLength(1) * doubleSize;
            byte[] buffer = new byte[byteBufferLength];
            //先找到末尾位置，从末尾开始写数据
            fileBinaryWriter.BaseStream.Seek(0, SeekOrigin.End);

            if (!needTranspose)
            {
                //对于不用转置的数据格式，不用处理
                //读到什么，直接将读到的数据写入buffer
                Buffer.BlockCopy(data, 0, buffer, 0, byteBufferLength);
            }
            else
            {
                //对于需要转置的数据格式，必须按转制的格式写入buffer
                int channelCount;
                int dataLengthPerChannel;
                dataLengthPerChannel = data.GetLength(0);
                channelCount = data.GetLength(1);
                for (int i = 0; i < dataLengthPerChannel; i++)
                {
                    for (int j = 0; j < channelCount; j++)
                    {
                        //将double转成byte[]，再将该byte[]复制到buffer中
                        BitConverter.GetBytes(data[i, j]).CopyTo(buffer, (i + j * dataLengthPerChannel) * doubleSize);
                    }
                }
            }
            fileBinaryWriter.Write(buffer);
        }

        public string[] FullPathOfAllDataFiles()
        {
            throw new NotImplementedException();
        }
    }
}
