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
        private List<Task> _fileWorkers = new List<Task>();
        private List<Worker> _workers = new List<Worker>();
        private Dictionary<string, char> fileDelimiter = new Dictionary<string, char>();


        Application(string inputFilePath)
        {
            _inputFilePath = inputFilePath;
        }

        private void SetInputFile()
        {
            this._inputFile = File.Open(_inputFilePath, FileMode.Open);
        }

        private void GetMatchingFiles(String masks, String directory)
        {
            //Console.WriteLine("in get matching files");
            foreach (string file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                //Console.WriteLine(file);
                if (FitsOneOfMultipleMasks(file, masks))
                {
                    this._synchronizedFilesBuffer.Write(file);
                    //Console.WriteLine(file + " was written in buffer");
                }
            }
        }

        private void PickFilesToProcessAsync()
        {
            while (this._synchronizedFilesBuffer._filesBufffer.IsEmpty == false)
            {
                var fileToProcess = this._synchronizedFilesBuffer.Read();
                
                var worker = new Worker(fileToProcess);

                _fileWorkers.Add(Task.Run(() => this.fileDelimiter[fileToProcess] = worker.DetermineDelimiter() ));
                
            }
          

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

        

        static void Main(string[] args)
        {
 
            Application myApp = new Application(@"D:\lucru\Resharper_Intern_problem\ResharperIntern\input.txt");

            myApp.SetInputFile();

            myApp._input = new Input(myApp._inputFile);

            myApp._input.FormatInputData();

            var tasks = new List<Task>();

            myApp.GetMatchingFiles(myApp._input.GetMasks(), myApp._input.GetDirectory());
                        
            myApp.PickFilesToProcessAsync();

            Task.WaitAll(myApp._fileWorkers.ToArray());

            Console.WriteLine("done");
            Console.Read();
        }
    }
}
