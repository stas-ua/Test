using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Google.GData.Spreadsheets;
using GoogleSync;

    class MainProgram
    {
        static void Main(string[] args)
        {
            bool argStop = true;
            if (args.Length != 0)
            {
                if (args[0] == "-nostop")
                    argStop = false;
            }

            try
            {                               
                //hghghgghhgfhfвпаваппотрп
                DbAdapter dbadp = new DbAdapter();
                
                if (dbadp.ReadXML())
                {
                    Console.WriteLine("Файл загружен: "  + dbadp.FilePath);
                    Console.WriteLine();

                    if (!dbadp.gAuth.RefreshAccess() && argStop)
                    {
                        Console.WriteLine("Необходимо зарегистрировать приложение. Посетите ссылку и вставьте \n"
                                        + "полученый код в строку консоли. Нажмите клавишу для продолжения...");                        
                        Console.ReadKey();
                        Console.WriteLine();

                        string url = dbadp.gAuth.GetAuthorizationUrl();
                        dbadp.gAuth.StartAuthInBrowser(url);

                        Console.WriteLine("Ваш код: ");
                        string accCode = Console.ReadLine();
                        dbadp.gAuth.GetAccess(accCode);

                        Console.WriteLine("\nЖдите...");
                        Console.WriteLine();
                    }

                    dbadp.Init();
                    dbadp.Sync();

                    Console.WriteLine("Таблицы обновлены");
                    Console.WriteLine();

                    if(dbadp.WriteXML())
                        Console.WriteLine("Файл сохранен: " + dbadp.FilePath);                        
                    else
                        Console.WriteLine("Файл не сохранен!" );

                    Console.WriteLine();

                }
                else 
                {                    
                    if (argStop)
                    {
                        GoogleAuth authObj;
                        authObj = new GoogleAuth();

                        authObj.client_id = "your client_id";
                        authObj.client_secret = "your client_secret";
                        authObj.redirect_uri = "your redirect_uri";
                        dbadp.gAuth = authObj;

                        TableMap tbl;
                        tbl = new TableMap();
                        tbl.googleTableName = "your googleTableName";
                        tbl.googleTableSheetName = "your googleTableSheetName";
                        tbl.dbTableName = "your dbTableName";

                        dbadp.TableMaps.Add(tbl);

                        dbadp.connectionString = @"your connectionString";

                        dbadp.WriteXML();
                        Console.WriteLine("В папке с программой создан xml файл: \n{0}", dbadp.FilePath);
                        Console.WriteLine("Сохраните его со своими параметрами и запустите приложение заново.");
                    }
                    else
                    {
                        Console.WriteLine("Не найден xml файл конфигурации. \n"
                                        + "Для его создания, запустите приложение без параметров.");
                        Console.WriteLine();
                    }              
                    
                }                
            }
            catch(Exception e)
            {
                Console.WriteLine("Произошла ошибка: \n" + e.Message);
                Console.WriteLine();
               
            }

            if (argStop)
            {
                Console.WriteLine("Нажмите клафишу для завершения...");                    
                Console.ReadKey();
            }
            

        }
    }

