using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResharperIntern
{
    class SynchronizedFilesBuffer
    {
        public ConcurrentQueue<string> _filesBufffer = new ConcurrentQueue<string>();
        private ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();

        public void Write(String fileName)
        {
            rwl.EnterWriteLock();

            try  
            {
                _filesBufffer.Enqueue(fileName);
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }

        public string Read()
        {
            rwl.EnterReadLock();

            try
            {
                string result;

               _filesBufffer.TryDequeue(out result);
                return result;
            }
            finally
            {
                rwl.ExitReadLock();
            }
        }

        public void Delete()
        {
            rwl.EnterWriteLock();

            try
            {
                string result;
                _filesBufffer.TryDequeue(out result);
                
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }


        ~SynchronizedFilesBuffer()
        {
            if (rwl != null) rwl.Dispose();
        }
    }
}
