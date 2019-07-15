namespace JTextDAQDataFileOperator.Interface
{
    /// <summary>
    /// 读已经存在的数据文件
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// 依据采集点获取数据
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="start">开始点</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        double[] LoadDataFromFile(int channelNo, ulong start, ulong length);

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        double[] LoadDataFromFile_TimeAxis(int channelNo, ulong start, ulong length);

        /// <summary>
        /// 依据采集点获取数据，支持复杂查找，大数据可能比较慢
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="start">开始点</param>
        /// <param name="stride">每次跳过多少个点</param>
        /// <param name="count">读多少次</param>
        /// <param name="block">每次读取多少点</param>
        /// <returns></returns>
        double[] LoadDataFromFileComplex(int channelNo, ulong start, ulong stride, ulong count, ulong block);

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        double[] LoadDataFromFileComplex_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block);

        /// <summary>
        /// 从 start 到最后一个点整个从硬盘中读出来，再从内存中计算 stride 等参数，和 5 参数 LoadDataFromFile 一样的功能但速度更快，不过更耗内存 
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        double[] LoadDataFromFileComplexRam(int channelNo, ulong start, ulong stride, ulong count, ulong block);

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="start"></param>
        /// <param name="stride"></param>
        /// <param name="count"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        double[] LoadDataFromFileComplexRam_TimeAxis(int channelNo, ulong start, ulong stride, ulong count, ulong block);

        /// <summary>
        /// 依据采集时间获取数据
        /// </summary>
        /// /// <param name="channelNo">通道号</param>
        /// <param name="startTime">开始时间，依据 IDataWriter 中的开始时间</param>
        /// <param name="endTime">结束时间，如果和开始时间相同则根据点数</param>
        /// <param name="stride">读取点之间的跨越，和另外几个方法的 stride 参数含义相同</param>
        /// <returns></returns>
        double[] LoadDataFromFileByTime(int channelNo, double startTime, double endTime, ulong stride);

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        double[] LoadDataFromFileByTime_TimeAxis(int channelNo, double startTime, double endTime, ulong stride);

        /// <summary>
        /// 依据采集时间获取数据，数据是时间轴均匀的，所以数据大概比 count 多一些
        /// </summary>
        /// <param name="channelNo">通道号</param>
        /// <param name="startTime">开始时间，依据 IDataWriter 中的开始时间</param>
        /// <param name="endTime">结束时间，如果和开始时间相同则根据点数</param>
        /// <param name="count">两个时间点中间要多少个点，由于需要保持时间轴均匀所以返回结果会 >= count 但最接近</param>
        /// <returns></returns>
        double[] LoadDataFromFileByTimeFuzzy(int channelNo, double startTime, double endTime, ulong count);

        /// <summary>
        /// 对应方法的时间轴
        /// </summary>
        /// <param name="channelNo"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        double[] LoadDataFromFileByTimeFuzzy_TimeAxis(int channelNo, double startTime, double endTime, ulong count);

        /// <summary>
        /// 获取通道总数
        /// </summary>
        /// <returns></returns>
        int ChannelCount();

        /// <summary>
        /// 获取每通道采样数
        /// </summary>
        /// <returns></returns>
        int SampleCount();

        /// <summary>
        /// 获取采样率
        /// </summary>
        /// <returns></returns>
        double SampleRate();

        /// <summary>
        /// 获取开始时间
        /// </summary>
        /// <returns></returns>
        double StartTime();

        /// <summary>
        /// 获取文件创建时间
        /// </summary>
        /// <returns></returns>
        double CreateTime();

        /// <summary>
        /// 获取Metadata类
        /// </summary>
        /// <returns></returns>
        Metadata Metadata();

        /// <summary>
        /// 获取所有文件名，带后缀不带路径
        /// </summary>
        /// <returns></returns>
        string[] AllDataFiles();  
    }
}