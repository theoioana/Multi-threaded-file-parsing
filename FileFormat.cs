using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResharperIntern
{
    class FileFormat
    {
        public char _fileDelimiter;
        public string _floatsDelimiter;
        public string _thousandDelimiter;
        public string _dateFormat;
        public Dictionary<string, string> _fileStructure = new Dictionary<string, string>();
        public string _filePath;
    }
}
