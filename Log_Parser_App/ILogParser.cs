using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using HtmlAgilityPack;

namespace Log_Parser_App
{
    /*Interface for abstarcting the the program structure and 
    Simplifies if in the future we are asked to expand the system */
    interface ILogParser
    {
        /*Get the log to a DataTable class*/
        public DataTable Get_LogToDataTable(HtmlDocument log_File);

        /*Parser the the DataTable to CSV*/
        public void DataTableToCSV(DataTable reportTable, string file_Name);
    }
}
