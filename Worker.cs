using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace ResharperIntern
{
    internal class Worker
    {
        private string _filePath;
        private FileFormat _fileFormat = new FileFormat();

        public Worker(string fileToProcess)
        {
            _filePath = fileToProcess;
        }

        public FileFormat ProcessFile()
        {
            _fileFormat._fileDelimiter = DetermineDelimiter();
            _fileFormat._fileStructure = DetermineFileStructure();
            _fileFormat._floatsDelimter = DetermineFloatsDelimiter();
            _fileFormat._thousandDelimiter = DetermineThousandDelimiter();
            _fileFormat._dateFormat = DetermineDateFormat();

            return _fileFormat;
        }

        private void DetermineFileStructure()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(_filePath);

            string line;
            if ((line = file.ReadLine()) != null)
            {
                string[] result = line.Split(_fileFormat._fileDelimiter);

                for(int iterator = 0; iterator < result.Length; iterator++)
                {
                    _fileFormat._fileStructure[result[iterator]] = "undefined";
                }
            }

            while ((line = file.ReadLine()) != null)
            {
                string[] result = line.Split(_fileFormat._fileDelimiter);

                for(int iterator = 0; iterator < result.Length; iterator++)
                {
                    if(this.IsDateType(result[iterator]))
                    {
                        _fileFormat._fileStructure[result[iterator]] = "Date";
                    }
                    else
                    {
                        if (this.IsNumberType(result[iterator]))
                        {
                            _fileFormat._fileStructure[result[iterator]] = "Number";
                        }
                        else
                        {
                            _fileFormat._fileStructure[result[iterator]] = "String";
                        }
                    }
                }
                

             }
        }

        private char DetermineFloatsDelimiter()
        {
            throw new NotImplementedException();
        }

        public char DetermineDelimiter()
        {
            string line;
            int columnCounter = 0;
            char[] possibleDelimiters = { ',', '\t', ';' };
            int[] delimitersFrequecy = { 0, 0, 0 };
            int index = 0;
           

            while (index <= 2)
            {
                Console.WriteLine("in whilee for " + _filePath);
                Console.WriteLine(index);

                System.IO.StreamReader file = new System.IO.StreamReader(_filePath);

                if((line = file.ReadLine()) != null)
                {
                    string[] result = line.Split(possibleDelimiters[index]);

                    columnCounter = result.Length;

                    Console.WriteLine("for file " + _filePath + " counter is " + columnCounter + " for " + possibleDelimiters[index]);
                    Console.Read();
                }

                bool ok = true;
                while ((line = file.ReadLine()) != null)
                {
                    string[] result = line.Split(possibleDelimiters[index]);

                    Console.WriteLine("for file " + _filePath + " result counter is " + result.Length + " for " + possibleDelimiters[index]);
                    Console.Read();

                    if (result.Length != columnCounter)
                    {
                        delimitersFrequecy[index] = 0;
                        ok = false; 
                        break;
                    }
                }

                if (ok == true)
                {
                    delimitersFrequecy[index] = columnCounter;
                }

                index++;
                file.Close();
            }

            int maxVal = 0;
            int delimiterIndex = 0;

            for(int i = 0; i <= 2; i++)
            {
                if(delimitersFrequecy[i] > maxVal)
                {
                    maxVal = delimitersFrequecy[i];
                    delimiterIndex = i;
                }
            }

            return possibleDelimiters[delimiterIndex];
        }

        private bool IsDateType(string item)
        {
            char[] separator = { '/' };
            string[] result = item.Split(separator);

            if(result.Length != 3)
            {
                return false;
            }
            else
            {
                bool isDateType = true;
                for(int iterator = 0; iterator < 3; iterator++)
                {
                    if (result[iterator].All(char.IsDigit) == false)
                    {
                        isDateType = false;
                    }
                }

                return isDateType;
            }
        }

        private bool IsNumberType(string item)
        {
            var result =  new string(item.Where( c => !char.IsPunctuation(c)).ToArray());
            result = Regex.Replace(result, @"\s+", ""); // eliminate white spaces too

            return result.All(char.IsDigit);
        }
       

       





    }
}