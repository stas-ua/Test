using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GoogleSync
{
    public class Logger
    {

        private string filepath;
        public Logger()
        {
            filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt"); 
        }

        public void WriteLog(String lines) {

            Console.WriteLine(lines);
            Console.WriteLine();
            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filepath)))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true);
                file.WriteLine(lines);
                file.WriteLine();
                file.Close();
            }            

        }
    }
}
