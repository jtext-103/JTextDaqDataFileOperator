namespace JTextDAQDataFileOperator.Interface
{
    /// <summary>
    /// 获取数据处理类的实例
    /// </summary>
    public interface IDataFileFactoty
    {
        /// <summary>
        /// 通过已经存在的数据文件和配置文件获取数据的类
        /// </summary>
        /// <param name="dataFileDirectory">数据文件所在文件夹，需要分隔符结尾</param>
        /// <param name="name">采集文件名称不带后缀</param>
        /// <returns></returns>
        IDataReader GetReader(string dataFileDirectory, string name);


        /// <summary>
        /// <param name="dataFileDirectory">存储文件路径，是个文件夹，需要分隔符结尾</param>
        /// <param name="name">采集文件名称不带后缀</param>
        /// <param name="channelCount">通道数量，之后通道号默认从 0 开始递增</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="dataNeedTransposeWhenSaving">来的数据类型是double[,]，看第一维是不是通道号判断是否需要转置</param>
        /// <param name="startTime">采集开始时间</param>
        /// <param name="createTime">采集文件创建时间</param>
        /// <returns></returns>
        IDataWriter GetWriter(string dataFileDirectory, string name, int channelCount, double sampleRate, bool dataNeedTransposeWhenSaving, double startTime);
    }
}
