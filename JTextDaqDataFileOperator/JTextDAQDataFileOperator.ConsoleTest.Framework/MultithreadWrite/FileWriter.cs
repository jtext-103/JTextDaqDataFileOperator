using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JTextDAQDataFileOperator.HDF5;
using JTextDAQDataFileOperator.Interface;

namespace JTextDAQDataFileOperator.ConsoleTest
{
    public class FileWriter
    {
        public int Index { get; set; }
        public string RootPath { get; set; }

        //HDF5文件格式
        IDataFileFactoty dataFileFactoty = new HDF5DataFileFactory();

        //Bin文件格式
        //IDataFileFactoty dataFileFactoty = new BinDataFileFactory();

        IDataWriter dataWriter;

        public FileWriter(int index, string rootPath)
        {
            Index = index;
            RootPath = rootPath;

            //HDF5文件格式
            dataWriter = dataFileFactoty.GetWriter(rootPath, index.ToString(), 16, 2000000, false, -1.2);

            //Bin文件格式
            //dataWriter = dataFileFactoty.GetWriter(rootPath + index.ToString() + ".bin", rootPath + index.ToString() + ".config", 16, 2000000, false, -1.2);
        }

        public void Begin()
        {
            for(int i = 0; i < 400; i++)
            {
                double[,] data = new double[16, 10000];
                for (int j = 0; j < 16; j++)
                {
                    for (int k = 0; k < 10000; k++)
                    {
                        data[j, k] = i * 10000 + k;
                    }
                }
                dataWriter.AcceptNewData(data);
                Thread.Sleep(5);
            }
            dataWriter.FinishWrite();
        }
    }
}
