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
        private Dictionary<string, List<string>> _fileCols = new Dictionary<string, List<string>>();

        public Worker(string fileToProcess)
        {
            _filePath = fileToProcess;
            this._fileFormat._filePath = fileToProcess;
        }
        
        public FileFormat ProcessFile()
        {
            DetermineDelimiter();
            DetermineFileStructure();
            //DetermineFloatsDelimiter();
            //DetermineThousandDelimiter();
            DetermineDateFormat();

            _fileFormat._floatsDelimiter = "undefined yet";
            _fileFormat._thousandDelimiter = "undefined yet";

            return _fileFormat;
        }

        private void DetermineDateFormat()
        {
            string format = "";
            foreach (KeyValuePair<string, string> entry in _fileFormat._fileStructure)
            {
                if (entry.Value == "Date")
                {

                    string[] result = _fileCols[entry.Key][0].Split(new char[] { '/' });

                    if (result[0].Length == 4)
                    {
                        format = "YYYY/MM/DD";
                        break;
                    }
                    else
                    {
                        format = "MM/MM/YYYY";
                        break;
                    }

                }
            }

            if (format == "MM/MM/YYYY")
            {
                foreach (KeyValuePair<string, string> entry in _fileFormat._fileStructure)
                {
                    if (entry.Value == "Date")
                    {

                        foreach (var dateTypes in _fileCols[entry.Key])
                        {

                            string[] result = dateTypes.Split(new char[] { '/' });

                            int pos0 = 0;
                            int pos1 = 0;

                            Int32.TryParse(result[0], out pos0);
                            Int32.TryParse(result[1], out pos1);

                            if (pos0 > 12)
                            {
                                format = "DD/MM/YYYY";
                                break;
                            }

                            if (pos1 > 12)
                            {
                                format = "MM/DD/YYYY";
                                break;
                            }
                        }
                    }
                }
            }

            if (format == "MM/MM/YYYY" || format == "")
            {
                _fileFormat._dateFormat = "cannot determine date format";
            }
            else
            {
                _fileFormat._dateFormat = format;
            }
        }

        private void DetermineThousandDelimiter()
        {
            char foundDelimiter = '-';
            foreach (KeyValuePair<string, string> entry in _fileFormat._fileStructure)
            {
              
                if (entry.Value == "Number")
                {
                    char[] possibleDelimiters = { ' ', ' '};

                    // i suppose that there is not the same delimiter used for multiple purposes
                    if (_fileFormat._floatsDelimiter == ".")
                    {
                        possibleDelimiters[0] = ',';
                        possibleDelimiters[1] = ' ';
                    }
                    else
                    {
                        possibleDelimiters[0] = '.';
                        possibleDelimiters[1] = ' ';
                    }

                    int index = 0;
                    bool found = false;

                    while ( !found && index < 2)
                    {
                       
                        foreach (var numberType in _fileCols[entry.Key])
                        {

                            int iterator = 0;
                            string integerPart = "";
                            while (iterator <= (numberType.Length - 1))
                            {
                                if (numberType[iterator].ToString() != _fileFormat._floatsDelimiter)
                                {
                                    integerPart += numberType[iterator];
                                    iterator++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (integerPart.Length <= 3)
                            {
                                continue; // case 100 cannot tell me anything about delimiter
                            }

                            string[] result = integerPart.Split(possibleDelimiters[index]);

                            if (result.Length == 1)
                            {
                                index++;
                                // known the fact that my number has more than 3 digits
                                // if the length of result is 1 it means that this is a wrong delimiter

                            }
                            else
                            {
                                
                                foundDelimiter = possibleDelimiters[index];
                                found = true;
                                break;
                              
                            }

                        }

                        
                    }
                }
            }

            _fileFormat._thousandDelimiter = foundDelimiter.ToString(); 
        }


        private void DetermineFloatsDelimiter()
        {
            char foundDelimiter = '-';

            foreach (KeyValuePair<string, string> entry in _fileFormat._fileStructure)
            {
                
                if (entry.Value == "Number")
                {
                    char[] possibleDelimiters = { '.', ',' };
                    int index = 0;
                    bool found = false;

                    while (!found && index < 2)
                    {

                        foreach (var numberType in _fileCols[entry.Key])
                        {
                            if (numberType.All(char.IsDigit))
                            {
                                continue; // not a float number
                            }

                            string[] result = numberType.Split(possibleDelimiters[index]);

                            if (result.Length > 2 || result.Length == 1)
                            {
                                index++;
                                continue; // thousand delimiter case or not the right delimiter
                            }
                            else
                            {

                                result[0] = CleanPunctuationAndSpace(result[0]);
                                result[1] = CleanPunctuationAndSpace(result[1]);

                                // check the case 45.765 , where we can t say if it is float or thousand number
                                if (result[1].Length % 3 != 0 || result[0].Length % 3 != 0)
                                {
                                    foundDelimiter = possibleDelimiters[index];
                                    found = true;
                                    break;

                                }
                                else
                                {
                                    if (result[1].Length % 3 == 0 && result[0].Length % 3 == 0)
                                        continue; // the case when we don't know to determine if float or thousand number
                                    else
                                        index++; // the case when the delimiter is not right
                                }

                            }
                        }
                    }

                }


            }

            _fileFormat._floatsDelimiter = foundDelimiter.ToString();
        }

        private void DetermineFileStructure()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(_filePath);

            string line;
            string[] colsName = { " " };
            if ((line = file.ReadLine()) != null)
            {
                colsName = line.Split(_fileFormat._fileDelimiter);

                for(int iterator = 0; iterator < colsName.Length; iterator++)
                {
                    Console.WriteLine(_filePath + " col " + colsName[iterator]);
                    _fileFormat._fileStructure[colsName[iterator]] = "undefined";

                    _fileCols[colsName[iterator]] = new List<string>();
                }
            }

            while ((line = file.ReadLine()) != null)
            {
                string[] result = line.Split(_fileFormat._fileDelimiter);

                for(int iterator = 0; iterator < result.Length; iterator++)
                {
                    _fileCols[colsName[iterator]].Add(result[iterator]); // add in the dictionary for file

                    if(this.IsDateType(result[iterator]))
                    {
                        if(_fileFormat._fileStructure[colsName[iterator]] == "undefined"){
                            _fileFormat._fileStructure[colsName[iterator]] = "Date";
                        }
                        else
                        {
                            if(_fileFormat._fileStructure[colsName[iterator]] != "Date")
                            {
                                Console.WriteLine("error from file structure" + _filePath);
                                Console.Read();
                            }
                        }
                    }
                    else
                    {
                        if (this.IsNumberType(result[iterator]))
                        {
                            if (_fileFormat._fileStructure[colsName[iterator]] == "undefined")
                            {
                                _fileFormat._fileStructure[colsName[iterator]] = "Number";
                            }
                            else
                            {
                                if (_fileFormat._fileStructure[colsName[iterator]] != "Number")
                                {
                                    Console.WriteLine("error from file structure" + _filePath);
                                    Console.Read();
                                }
                            }
                        }
                        else
                        {
                            if (_fileFormat._fileStructure[colsName[iterator]] == "undefined")
                            {
                                _fileFormat._fileStructure[colsName[iterator]] = "String";
                            }
                            else
                            {
                                if (_fileFormat._fileStructure[colsName[iterator]] != "String")
                                {
                                    Console.WriteLine("error from file structure" + _filePath);
                                    Console.Read();
                                }
                            }
                        }
                    }
                }
                

             }
        }

        

        private void DetermineDelimiter()
        {
            string line;
            int columnCounter = 0;
            char[] possibleDelimiters = { ',', '\t', ';' };
            int[] delimitersFrequecy = { 0, 0, 0 };
            int index = 0;
           

            while (index <= 2)
            {
                
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

                    //Console.WriteLine("for file " + _filePath + " result counter is " + result.Length + " for " + possibleDelimiters[index]);
                    //Console.Read();

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

            _fileFormat._fileDelimiter = possibleDelimiters[delimiterIndex];
        }

        private bool IsDateType(string item)
        {
            item = Regex.Replace(item, @"\s+", ""); // eliminate white spaces too
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

        private string CleanPunctuationAndSpace(string item)
        {
            item = new string(item.Where(c => !char.IsPunctuation(c)).ToArray());
            item = Regex.Replace(item, @"\s+", ""); // eliminate white spaces too

            return item;
        }

        private bool IsNumberType(string item)
        {
            var result =  new string(item.Where( c => !char.IsPunctuation(c)).ToArray());
            result = Regex.Replace(result, @"\s+", ""); // eliminate white spaces too

            return result.All(char.IsDigit);
        }
       

       





    }
}