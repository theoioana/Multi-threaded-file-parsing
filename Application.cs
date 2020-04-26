using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ResharperIntern
{
    class Application
    {
        private FileStream _inputFile;
        private string _inputFilePath;
        private Input _input;
        private SynchronizedFilesBuffer _synchronizedFilesBuffer = new SynchronizedFilesBuffer();
        private List<Task> _fileWorkers;
        private Dictionary<string, FileFormat> _filesProperties = new Dictionary<string, FileFormat>();
        private Dictionary<FileFormat, int> output = new Dictionary<FileFormat, int>();

        Application(string inputFilePath)
        {
            _inputFilePath = inputFilePath;
            _fileWorkers = new List<Task>();
        }

        private void SetInputFile()
        {
            this._inputFile = File.Open(_inputFilePath, FileMode.Open);
        }

        private void GetMatchingFiles(String masks, String directory)
        {
            Console.WriteLine("in get matching files");
            foreach (string file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(file);
                if (FitsOneOfMultipleMasks(file, masks))
                {
                    this._synchronizedFilesBuffer.Write(file);
                    Console.WriteLine(file + " was written in buffer");
                }
            }
        }

        private void  PickFilesToProcess()
        {
            while (this._synchronizedFilesBuffer._filesBufffer.IsEmpty == false)
            {
                var fileToProcess = this._synchronizedFilesBuffer.Read();
                
                var worker = new Worker(fileToProcess);

                this._fileWorkers.Add(Task.Run(() => {
                    this._filesProperties[fileToProcess] = worker.ProcessFile();
                }));
                
            }
            Task.WaitAll(this._fileWorkers.ToArray());
        }

        private bool FitsMask(string fileName, string fileMask)
        {
            Regex mask = new Regex(
                '^' +
                fileMask
                    .Replace(".", "[.]")
                    .Replace("*", ".*")
                + '$',
                RegexOptions.IgnoreCase);
            return mask.IsMatch(fileName);
        }

        private bool FitsOneOfMultipleMasks(string fileName, string fileMasks)
        {
            return fileMasks
                .Split(new string[] { "\r\n", "\n", ",", "|", " " },
                    StringSplitOptions.RemoveEmptyEntries)
                .Any(fileMask => FitsMask(fileName, fileMask));
        }

        private void WriteOutputToFile(string filePath)
        {
            foreach(var fileformat in this._filesProperties)
            {
                if (output.ContainsKey(fileformat.Value)) 
                {
                    output[fileformat.Value]++;
                }
                else
                {
                    output.Add(fileformat.Value, 0);
                }
            }

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filePath))
            {
                foreach (var fileFormatStructure in this.output)
                {
                    file.WriteLine("The structure of file " + fileFormatStructure.Key._filePath + " is:");
                    foreach (var cols in fileFormatStructure.Key._fileStructure)
                    {
                        file.Write(cols.Key + ": " + cols.Value + " ");
                    }

                    file.WriteLine();
                    file.WriteLine("The file delimiter: " +  fileFormatStructure.Key._fileDelimiter.Equals('\t') ? "tab" : fileFormatStructure.Key._fileDelimiter.ToString());
                    file.WriteLine("The date format: " + fileFormatStructure.Key._dateFormat);

                    file.WriteLine();
                    file.WriteLine();

                }
            }
        }


        static void Main(string[] args)
        {
 
            Application myApp = new Application(@"input.txt");

            myApp.SetInputFile();

            myApp._input = new Input(myApp._inputFile);

            myApp._input.FormatInputData();
            
            myApp.GetMatchingFiles(myApp._input.GetMasks(), myApp._input.GetDirectory());
                        
            myApp.PickFilesToProcess();

            string outputFilePath = @"output.txt";

            myApp.WriteOutputToFile(outputFilePath);
            
        }
    }
}
