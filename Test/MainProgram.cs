﻿using System;
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
            Logger log = new Logger();
            bool argStop = true;
            if (args.Length != 0)
            {
                if (args[0] == "-nostop")
                    argStop = false;
            }

            try
            {                               
                //hghghgghhgfhfвпаваппотрп454
                DbAdapter dbadp = new DbAdapter();
                
                if (dbadp.ReadXML())
                {
                    log.WriteLog(System.DateTime.Now + ". Файл загружен: " + dbadp.FilePath);

                    if (!dbadp.gAuth.RefreshAccess() && argStop)
                    {
                        log.WriteLog(System.DateTime.Now + ". Необходимо зарегистрировать приложение.");

                        Console.WriteLine("Посетите ссылку и вставьте полученый код в строку консоли.\n" +
                            "Нажмите клавишу для продолжения...");                        
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

                    log.WriteLog(System.DateTime.Now + ". Таблицы обновлены.");                   

                    if(dbadp.WriteXML())
                        log.WriteLog(System.DateTime.Now + ". Файл сохранен: " + dbadp.FilePath);                                                
                    else
                        log.WriteLog(System.DateTime.Now + ". Файл не сохранен!");  
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
                        log.WriteLog(System.DateTime.Now + ". В папке с программой создан xml файл: \n" + dbadp.FilePath);                        
                        Console.WriteLine("Сохраните его со своими параметрами и запустите приложение заново.");
                    }
                    else
                    {
                        log.WriteLog(System.DateTime.Now + ". Не найден xml файл конфигурации. \n"
                                        + "Для его создания, запустите приложение без параметров.");                          
                    }              
                    
                }                
            }
            catch (Exception ex)
            {
                log.WriteLog(System.DateTime.Now + ". Объект " + ex.Source +
                            ", Метод " + ex.TargetSite +
                            ", Сообщение " + ex.Message +
                            ", Тип исключения" + ex.ToString());
                Console.WriteLine();                   

            }

            if (argStop)
            {
                Console.WriteLine("Нажмите клафишу для завершения...");                    
                Console.ReadKey();
            }
            

        }
    }

