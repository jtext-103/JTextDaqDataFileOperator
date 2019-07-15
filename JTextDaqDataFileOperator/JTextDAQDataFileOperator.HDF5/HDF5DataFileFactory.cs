using System;
using System.Collections.Generic;
using System.Text;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.HDF5
{
    public class HDF5DataFileFactory : IDataFileFactoty
    {
        IDataReader IDataFileFactoty.GetReader(string dataFileDirectory, string name)
        {
            return new DataReader(dataFileDirectory, name);
        }

        IDataWriter IDataFileFactoty.GetWriter(string dataFileDirectory, string name, int channelCount, double sampleRate, bool dataNeedTransposeWhenSaving, double startTime)
        {
            return new DataWriter(dataFileDirectory, name, channelCount, sampleRate, dataNeedTransposeWhenSaving, startTime);
        }
    }
}
