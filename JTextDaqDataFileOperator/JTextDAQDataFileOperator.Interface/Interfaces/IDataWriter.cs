namespace JTextDAQDataFileOperator.Interface
{
    /// <summary>
    /// 写新的数据文件
    /// </summary>
    public interface IDataWriter
    {
        /// <summary>
        /// 获取采集数据
        /// </summary>
        /// <param name="data">每次来的数据，执行多次直到数据流完毕</param>
        void AcceptNewData(double[,] data);

        /// <summary>
        /// 数据流完毕，结束传输，等待数据写入硬盘后结束
        /// </summary>
        void FinishWrite();

        /// <summary>
        /// 获取所有采集文件完整路径
        /// </summary>
        /// <returns></returns>
        string[] FullPathOfAllDataFiles();
    }
}