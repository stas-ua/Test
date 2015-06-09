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

        public Logger(bool CheckFileAge = false)
        {
            filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt");
            if (CheckFileAge)
            {                
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filepath)))
                {
                    string[] lns = System.IO.File.ReadAllLines(filepath);
                    System.IO.StreamWriter file = new System.IO.StreamWriter(filepath);

                    foreach (string ln in lns)
                    {
                        if (ln.Length >= 10)
                        {
                            DateTime d;
                            DateTime dPoint = System.DateTime.Now.AddDays(-1);
                            DateTime.TryParse(ln.Substring(0, 10), out d);
                            if (d >= dPoint)
                            {
                                file.WriteLine(ln);
                                file.WriteLine();
                            }
                        }
                    }

                    file.Close();
                } 
            }            
        }

        public void WriteLog(Exception ex = null, String lines = "") {
            string exMsg = null;
            if (ex != null)
                exMsg = String.Format(". Объект {0}, Метод {1}, Сообщение {2}, Тип исключения {3}"
                                    , ex.Source, ex.TargetSite, ex.Message, ex.ToString());

            Console.WriteLine(System.DateTime.Now  + exMsg + ". " + lines);
            Console.WriteLine();

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filepath)))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true);
                file.WriteLine(System.DateTime.Now + exMsg + ". " + lines);
                file.WriteLine();
                file.Close();
            }            

        }
    }
}
