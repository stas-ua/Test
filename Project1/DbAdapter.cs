

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Spreadsheets;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml.Serialization;

namespace GoogleSync
{

    public class DbAdapter
    {      

        public List<TableMap> TableMaps = new List<TableMap>();

        public string connectionString { get; set; }
                
        public GoogleAuth gAuth { get; set; }
        
        [XmlIgnore]
        public string FilePath { get; private set; }

        [XmlIgnore]
        private SpreadsheetsService myService { get; set; }

        [XmlIgnore]
        private Logger log = new Logger();

        public DbAdapter()
        {
            FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleDbAdapter.xml");
        }

        public void Init(int IsOAuth = 1 )
        {                         
            try
            {                
                this.myService = new SpreadsheetsService("Default");
                if (IsOAuth == 1)
                    myService.RequestFactory = gAuth.GetRequestFactory();
                else if (IsOAuth == 0)
                    gAuth.SetUserCred(myService);                  
                LoadGoogleSheets();
            }
            catch(Exception ex)
            {
                throw;               
            }
        }

        private  void LoadGoogleSheets()
        {
            SpreadsheetQuery query = new SpreadsheetQuery();            
            SpreadsheetFeed feed = myService.Query(query);    

            foreach (TableMap tblMap in TableMaps)
            {
                foreach (SpreadsheetEntry entry in feed.Entries)
                {
                    if (entry.Title.Text == tblMap.googleTableName)
                    {
                        int iTry = 0;
                        bool isSuccessfulTry = false;

                        WorksheetFeed wShFeed = null;

                        while (iTry < 5 && !isSuccessfulTry)
                        {
                            try
                            {
                                wShFeed = entry.Worksheets;
                                isSuccessfulTry = true;
                            }
                            catch (Exception ex)
                            {
                                log.WriteLog(ex, "Текущая гугл-таблица " + tblMap.googleTableName + "-" +
                                    tblMap.googleTableSheetName + ", попытка " + (iTry + 1));
                                iTry++;
                            }
                        }                        

                        foreach (WorksheetEntry worksheet in wShFeed.Entries)
                        {
                            if (worksheet.Title.Text == tblMap.googleTableSheetName)
                            {                                
                                CellQuery cQuery = new CellQuery(worksheet.CellFeedLink);
                                //System.Threading.Thread.Sleep(1000); //bjbjbsfs
                                iTry = 0;
                                isSuccessfulTry = false;
                                while (iTry < 5 && !isSuccessfulTry)
                                {
                                    try 
                                    { 
                                        tblMap.cFeed = myService.Query(cQuery);
                                        isSuccessfulTry = true;
                                    }
                                    catch(Exception ex)
                                    {
                                        log.WriteLog(ex, "Текущая гугл-таблица " + tblMap.googleTableName + "-" +
                                            tblMap.googleTableSheetName + ", попытка " + (iTry + 1));
                                        iTry++;
                                    }  
                                }
                                                             
                            }
                        }
                    }
                }
            }
            
        }

        public void Sync()
        {
            foreach (TableMap tblMap in TableMaps)
            {
                if(tblMap.dTable==null)
                {
                    tblMap.InitDataTable();
                }

                tblMap.LoadRowsToDataTable();
                tblMap.RewriteRowsToDbTable(connectionString);
            }
        }
        
        public bool WriteXML()
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(DbAdapter));
            bool res = false;
            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(FilePath)))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(FilePath);
                try
                {
                    writer.Serialize(file, this);
                    res = true;
                }
                catch
                {
                    throw;
                }           
     
                file.Close();   
            }
            return res;
        }

        public bool ReadXML()
        {
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(DbAdapter));
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(FilePath);
                try
                {
                    DbAdapter dba = new DbAdapter();
                    dba = (DbAdapter)reader.Deserialize(file);
                    file.Close();
                    this.connectionString = dba.connectionString;
                    this.gAuth = dba.gAuth;
                    this.TableMaps = dba.TableMaps;                   
                    return true;
                }
                catch (Exception ex)
                {
                    log.WriteLog(ex);
                }
            }
            return false;
        }




    }
}
