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
    public class TableMap
    {
        public string googleTableName { get; set; }
        public string googleTableSheetName { get; set; }
        public string dbTableName { get; set; }

        [XmlIgnore]
        public CellFeed cFeed { get; set; }

        [XmlIgnore]
        public DataTable dTable { get; private set; }        

        [XmlIgnore]
        private Logger log = new Logger();

        public TableMap(string googleTableName,
            string googleTableSheetName,
            string dbTableName)
        {
            this.googleTableName = googleTableName;
            this.googleTableSheetName = googleTableSheetName;
            this.dbTableName = dbTableName;
        }

        public TableMap()
        {
            // TODO: Complete member initialization
        }

        private class Field
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public Field(string n, string t)
            {
                Name = n;
                Type = t;
            }
        }

        public void InitDataTable()
        {
            if (cFeed != null)
            {
                System.Data.DataTable table = null;

                List<Field> Cols = new List<Field>();
                int i = 0;
                foreach (CellEntry curCell in cFeed.Entries)
                {                    
                    if (curCell.Cell.Row == 1)
                    {
                        Cols.Add(new Field(curCell.Cell.Value, null));
                    }
                    else if (Cols[(int)curCell.Column - 1].Type == null)
                    {
                        String sType = GetType(curCell.Value);
                        if (sType != null && Cols[(int)curCell.Column - 1].Type==null)
                        {
                            i++;
                            Cols[(int)curCell.Column - 1].Type = sType;
                        }
                            
                    }
                    if (i == Cols.Count() && curCell.Cell.Row > 1)
                        break;
                }

                table = GetTableStucture(googleTableName, Cols);
                dTable = table;
            }

        }

        public void LoadRowsToDataTable()
        {
            if (dTable != null)
            {
                dTable.Rows.Clear();
                System.Data.DataRow row = dTable.NewRow();
                int iCol;
                int iRow =2;

                foreach (CellEntry Cell in cFeed.Entries)
                {
                    if (Cell.Row > 1)
                    {                        
                        //if (Cell.Column == 1 && Cell.Row > 2)
                        //{
                        //    dTable.Rows.Add(row);
                        //    row = dTable.NewRow();
                        //}

                        if (iRow != (int)Cell.Row)
                        {
                            dTable.Rows.Add(row);
                            row = dTable.NewRow();
                            iRow = (int)Cell.Row;
                        }   

                        iCol = (int)Cell.Column - 1;
                        string strType = dTable.Columns[iCol].DataType.ToString();

                        if (strType == "System.DateTime")
                        {
                            DateTime dtVal;
                            DateTime.TryParse(Cell.Value, out dtVal);
                            row[iCol] = dtVal;
                        }
                        else if (strType == "System.Double")
                        {
                            try
                            {
                                double dVal = Convert.ToDouble(Cell.Value);
                                row[iCol] = dVal;
                            }
                            catch (SystemException ex)
                            {
                                log.WriteLog(ex, "Несовместимый тип в таблице " + this.googleTableName + 
                                    "-" + this.googleTableSheetName +
                                    ", в столбце " + dTable.Columns[iCol].ColumnName + 
                                    ", в строке " + iRow);
                            }                            
                            
                        }
                        else
                            row[iCol] = Cell.Value;
                    }
                }

                dTable.Rows.Add(row);
            }
        }
            
        public void RewriteRowsToDbTable(string connectionString)
        {
            if (dTable != null)
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();
                    var builder = new SqlCommandBuilder();
                    string escTableName = builder.QuoteIdentifier(this.dbTableName).Replace("[", "").Replace("]", "");
                    string tempTbl = "dbo.tblTempGoogle_" + Guid.NewGuid().ToString("N");
                    string sqlSelStr = String.Format("select * into {1} from {0};", escTableName, tempTbl);
                    SqlCommand cmd = new SqlCommand(sqlSelStr, connection);
                    cmd.ExecuteNonQuery();
                    string sqlDelStr = String.Format("delete from {0};", escTableName);
                    cmd = new SqlCommand(sqlDelStr, connection);
                    cmd.ExecuteNonQuery();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = this.dbTableName;

                        try
                        {
                            bulkCopy.WriteToServer(this.dTable);
                            string sqlDropStr = String.Format("drop table {0};", tempTbl);
                            cmd = new SqlCommand(sqlDropStr, connection);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            string sqlInsStr = String.Format("insert into {0} select * from {1}; drop table {1};", escTableName, tempTbl);
                            cmd = new SqlCommand(sqlInsStr, connection);
                            cmd.ExecuteNonQuery();
                            log.WriteLog(ex, "Текущая гугл-таблица " + this.googleTableName + "-" + this.googleTableSheetName);                              
                        }
                    }
                }
            }
        }

        private static string GetType(string val)
        {
            string res;
            DateTime d;
            if (DateTime.TryParse(val, out d))
                res = "System.DateTime";
            else
            {
                try
                {
                    double dbl = Convert.ToDouble(val);
                    res = "System.Double";
                }
                catch (SystemException)
                {                    
                    res = "System.String";                    
                }
            }
            return res;

        }

        private static DataTable GetTableStucture(String name, List<Field> cln)
        {
            System.Data.DataTable table = new DataTable(name);
            DataColumn column;

            foreach (Field item in cln)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType(item.Type);
                column.ColumnName = item.Name;
                table.Columns.Add(column);
            }

            return table;
        }

        public void ShowTable()
        {
            foreach (DataColumn col in dTable.Columns)
            {
                Console.Write("{0,-14}", col.ColumnName);
            }
            Console.WriteLine();

            foreach (DataRow row in dTable.Rows)
            {
                foreach (DataColumn col in dTable.Columns)
                {
                    if (col.DataType.Equals(typeof(DateTime)))
                        Console.Write("{0,-14:d}", row[col]);
                    else if (col.DataType.Equals(typeof(Decimal)))
                        Console.Write("{0,-14:C}", row[col]);
                    else
                        Console.Write("{0,-14}", row[col]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();            
        }

    }
}
