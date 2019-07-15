using System;
using System.Collections.Generic;
using System.Text;

namespace JTextDAQDataFileOperator.Interface
{
    /// <summary>
    /// 所有元数据
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// 通道总数
        /// </summary>
        public int ChannelCount { get; set; }

        /// <summary>
        /// 每通道采样数
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// 文件创建时间
        /// </summary>
        public double CreateTime { get; set; }
    }
}
