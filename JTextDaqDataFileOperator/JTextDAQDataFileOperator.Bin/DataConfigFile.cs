using System.IO;
using Newtonsoft.Json;

namespace JTextDAQDataFileOperator.Bin
{
    /// <summary>
    /// 读取数据时必须的配置文件
    /// （写数据结束后新建）
    /// </summary>
    public class DataConfigFile
    {
        /// <summary>
        /// 通道个数
        /// </summary>
        public int TotalChannelCount { get; set; }

        /// <summary>
        /// 每通道数据总个数
        /// </summary>
        public long TotalSampleCount { get; set; }

        /// <summary>
        /// 每次每通道写数据个数
        /// </summary>
        public int SampleCountPerTime { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// 什么都不做，仅供writer使用
        /// </summary>
        public DataConfigFile() { }

        /// <summary>
        /// 通过指定参数构造实例
        /// </summary>
        /// <param name="totalChannelCount"></param>
        /// <param name="totalSampleCount"></param>
        /// <param name="sampleCountPerTime"></param>
        /// <param name="sampleRate"></param>
        public DataConfigFile(int totalChannelCount, long totalSampleCount, int sampleCountPerTime, double sampleRate)
        {
            init(totalChannelCount, totalSampleCount, sampleCountPerTime, sampleRate);
        }

        /// <summary>
        /// 直接从配置文件构造实例
        /// </summary>
        /// <param name="configFilePath"></param>
        public DataConfigFile(string configFilePath)
        {
            DataConfigFile config = (DataConfigFile)Load(configFilePath);
            init(config.TotalChannelCount, config.TotalSampleCount, config.SampleCountPerTime, config.SampleRate);
        }

        private void init(int totalChannelCount, long totalSampleCount, int sampleCountPerTime, double sampleRate)
        {
            TotalChannelCount = totalChannelCount;
            TotalSampleCount = totalSampleCount;
            SampleCountPerTime = sampleCountPerTime;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// 从配置文件中创建对应配置文件的实例
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public object Load(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader fileStreamReader = new StreamReader(fs);
            string configJson = fileStreamReader.ReadToEnd();
            fileStreamReader.Close();
            //反序列化后返回
            var readConfig = JsonConvert.DeserializeObject(configJson, this.GetType());
            return readConfig;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter stringWriter = new StreamWriter(fs);
            //将配置序列化为Json后保存
            stringWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            stringWriter.Close();
            //对应的stream也被关闭了，所以不用再关
        }
    }
}
