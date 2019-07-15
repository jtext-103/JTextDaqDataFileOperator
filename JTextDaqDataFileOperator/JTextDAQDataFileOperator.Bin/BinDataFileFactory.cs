using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.Bin
{
    public class BinDataFileFactory : IDataFileFactoty
    {
        //todo:有BUG要完善
        IDataReader IDataFileFactoty.GetReader(string dataFileDirectory, string name)
        {
            //todo:没有完成
            return new DataReader(new DataConfigFile(null), null);
        }

        //todo:有BUG要完善
        IDataWriter IDataFileFactoty.GetWriter(string dataFileDirectory, string name, int channelCount, double sampleRate, bool dataNeedTransposeWhenSaving, double startTime)
        {
            //todo:没有完成
            return new DataWriter(dataFileDirectory, null, channelCount, sampleRate, dataNeedTransposeWhenSaving);
        }
    }
}
