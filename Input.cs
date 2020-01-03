using System;
using System.IO;

namespace ResharperIntern
{
    internal class Input
    {
        private FileStream _inputFile;
        private String _directoryPath;
        private string _masks;
        private String _outputFilePath;

        public Input(FileStream inputFile)
        {
            this._inputFile = inputFile;
        }

        internal void FormatInputData()
        {
          
            using (StreamReader sr = new StreamReader(_inputFile))
            {
                string s;
                
                if ((s = sr.ReadLine()) != null)
                {

                    String[] separators = { " " };
                    String[] inputList = s.Split(separators, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

                    _directoryPath = inputList[0];
                    
                    for(int i = 1; i <= inputList.Length - 2; i++)
                    {
                        _masks += inputList[i] + " ";
                    }

                    _outputFilePath = inputList[inputList.Length - 1];
                    
                }
               
            }
        }

        internal string GetMasks()
        {
            return this._masks;
        }

        internal string GetDirectory()
        {
            return this._directoryPath;
        }
    }
}