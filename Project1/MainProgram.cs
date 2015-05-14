using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Spreadsheets;
using GoogleSync;

    class MainProgram
    {
        static void Main(string[] args)
        {
            DbAdapter dbadp = new DbAdapter();
            dbadp.ReadXML();

            if(dbadp.gAuth!=null)
            {                
                Console.WriteLine("Load from xml");
            }
            else
            {
                //authObj.googleLogin = "report.sales.gl@gmail.com";
                //authObj.googlePassword = "214365870912";
                GoogleAuth authObj;
                authObj = new GoogleAuth();
                authObj.client_id = "200462351526-o7u1bsuavnu6piv4nkjlptoabdpvf006.apps.googleusercontent.com";
                authObj.client_secret = "QnIVE0HJ2IRvJml_tNm3mfwv";
                authObj.redirect_uri = "urn:ietf:wg:oauth:2.0:oob";
                authObj.GetAccess();
                dbadp.gAuth = authObj;
                dbadp.WriteXML();
            }

            TableMap tbl = new TableMap();
            tbl.googleTableName = "Продажи дистрибуторов в ценах фабрики (Ответы 29 янв 2015)";
            tbl.googleTableSheetName = "Ответы на форму";
            tbl.dbTableName = "dbo.tblGoogleSSD";
            //tbl.dbTableName = "dbo.testGoogle";            
            
            dbadp.TableMaps.Add(tbl);            
            dbadp.connectionString = @"Persist Security Info=False;Integrated Security=True; database=adb;server=INFORMSQL";
            //dbadp.connectionString = @"Persist Security Info=False;Integrated Security=True; database=lab;server=STAS-PC\SQLEXPRESS";
            dbadp.Init();
            dbadp.Sync();
        }
    }

