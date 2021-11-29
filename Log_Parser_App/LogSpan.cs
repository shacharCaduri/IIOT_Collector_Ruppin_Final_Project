
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Log_Parser_App
{
    class LogSpan
    {
        public static DataTable Get_LogSpanTableToDataTable(HtmlDocument log_File)
        {
            DataTable reportTable = Get_SpanTable_Report();
            HtmlNode body = log_File.DocumentNode.SelectSingleNode("//html//body");
            List<HtmlNode> bodyChilds = body.ChildNodes.ToList();
            foreach (HtmlNode item in bodyChilds)
            {
                if (item.Name == "table")
                    Get_HtmlColspanLogTableToDataTable(reportTable, item);
                if (item.Name == "center")
                    Get_HtmlCenterToDataTable(reportTable, item);
                if (item.Name == "i")
                    Get_HtmlTagI_ToDataTable(reportTable, item);
                if (item.Name == "span")
                    Get_SpanToDataTable(reportTable, item);
                if (item.Name == "u")
                    Get_HtmlTagU_ToDataTable(reportTable, item);
            }
            return reportTable;
        }
        private static void Get_HtmlColspanLogTableToDataTable(System.Data.DataTable reportTable, HtmlNode table)
        {
            List<HtmlNode> tableRows = table.ChildNodes.Where(x => x.Name == "tbody").SingleOrDefault().ChildNodes.Where(y => y.Name == "tr").ToList();
            if (tableRows.Any(x => x.ChildNodes.Where(x => x.Name == "th").Count() > 1))
                Get_HtmlColspan3Columns(reportTable, table);
            else
                Get_HtmlColspan2Columns(reportTable, table);
        }
        private static void Get_HtmlColspan3Columns(DataTable reportTable, HtmlNode table)
        {
            List<HtmlNode> tableRows = table.ChildNodes.Where(x => x.Name == "tbody").SingleOrDefault().ChildNodes.Where(y => y.Name == "tr").ToList();
            string[] value = new string[3];
            int counter = 0;
            foreach (HtmlNode row in tableRows)
            {
                List<HtmlNode> tds = row.ChildNodes.Where(x => x.Name == "td").ToList();
                List<HtmlNode> ths = row.ChildNodes.Where(x => x.Name == "th").ToList();
                if (ths.Count() > 1 || tds.Count() == 0)
                {
                    foreach (HtmlNode item in ths)
                    {
                        value[counter] = item.InnerText.Replace("&nbsp;", "").Trim();
                        counter++;
                    }
                    counter = 0;
                }
                if (ths.Count() == 1 || tds.Count() > 0)
                {
                    foreach (HtmlNode item in ths)
                    {
                        value[counter] = item.InnerText.Replace("&nbsp;", "").Trim();
                        counter++;
                    }
                    foreach (HtmlNode item in tds)
                    {
                        value[counter] = item.InnerText.Replace("&nbsp;", "").Trim();
                        counter++;
                    }
                    counter = 0;
                }
                reportTable.Rows.Add(value[0], value[1], value[2]);
            }
        }
        private static void Get_HtmlColspan2Columns(DataTable reportTable, HtmlNode table)
        {
            List<HtmlNode> tableRows = table.ChildNodes.Where(x => x.Name == "tbody").SingleOrDefault().ChildNodes.Where(y => y.Name == "tr").ToList();
            string key = "";
            string value = "";
            foreach (HtmlNode row in tableRows)
            {
                HtmlNode td = row.ChildNodes.Where(x => x.Name == "td").SingleOrDefault();
                HtmlNode th = row.ChildNodes.Where(x => x.Name == "th").SingleOrDefault();
                if (td == null && th != null)
                {
                    key = th.InnerText.Trim();
                    value = "";
                }
                if (td != null && th != null)
                {
                    key = th.InnerText.Trim();
                    value = td.InnerText.Replace("&nbsp;", "").Trim();
                }
                reportTable.Rows.Add(key, value);
            }
        }
        private static void Get_HtmlCenterToDataTable(DataTable reportTable, HtmlNode center)
        {
            HtmlNode text = center.FirstChild.FirstChild.FirstChild;
            string key = text.InnerText.Replace("&nbsp;", " ").Trim();
            reportTable.Rows.Add(key);
        }
        private static void Get_HtmlTagI_ToDataTable(DataTable reportTable, HtmlNode i)
        {
            if (i.ChildNodes.Any(x => x.Name == "u"))
            {
                HtmlNode span = i.ChildNodes.Where(x => x.Name == "u").SingleOrDefault().ChildNodes.Where(y => y.Name == "span").SingleOrDefault();
                Get_SpanToDataTable(reportTable, span);
            }
        }
        private static void Get_HtmlTagU_ToDataTable(DataTable reportTable, HtmlNode u)
        {
            HtmlNode text = u.FirstChild;
            if (text.InnerText.Length > 0)
            {
                string key = text.InnerText.Replace("&nbsp;", " ").Trim();
                reportTable.Rows.Add(key);
            }
        }
        private static void Get_SpanToDataTable(DataTable reportTable, HtmlNode span)
        {
            if (span.InnerText.Length > 0)
            {
                int counter = 0;
                double num;
                string[] rowToDataTable = new string[6];
                string[] headline = span.InnerText.Replace("&nbsp;", " ").Split("  ");
                for (int i = 0; i < headline.Length; i++)
                {
                    if (headline[i].Length > 0 && counter < 6)
                    {
                        rowToDataTable[counter] = headline[i].TrimStart();
                        if (headline[i].Contains("&amp;&amp;"))
                            if (i > 0)
                            {
                                rowToDataTable[i] = rowToDataTable[i].Replace("&amp;&amp;", " && ");
                                rowToDataTable[i - 1] = "" + rowToDataTable[i - 1] + rowToDataTable[i];
                                rowToDataTable[i] = null;
                            }
                        counter++;
                    }
                }
                if (rowToDataTable[5] == null)
                {
                    if (!double.TryParse(rowToDataTable[4], out num) && rowToDataTable[2] != null)
                    {
                        if (!double.TryParse(rowToDataTable[2], out num))
                            reportTable.Rows.Add("", rowToDataTable[0], rowToDataTable[1], rowToDataTable[2], rowToDataTable[3], rowToDataTable[4]);
                        else
                            reportTable.Rows.Add(rowToDataTable[0], "NaN", rowToDataTable[1], rowToDataTable[2], rowToDataTable[3], rowToDataTable[4]);
                    }
                    else
                        reportTable.Rows.Add(rowToDataTable[0], rowToDataTable[1], rowToDataTable[2], rowToDataTable[3], rowToDataTable[4], rowToDataTable[5]);
                }
                else
                    reportTable.Rows.Add(rowToDataTable[0], rowToDataTable[1], rowToDataTable[2], rowToDataTable[3], rowToDataTable[4], rowToDataTable[5]);
            }
        }
        private static DataTable Get_SpanTable_Report()
        {
            DataTable table = new DataTable("Table_Report");
            table.Columns.Add("Test Name", typeof(string));
            table.Columns.Add("Received", typeof(string));
            table.Columns.Add("Lower Limit", typeof(string));
            table.Columns.Add("Upper Limit", typeof(string));
            table.Columns.Add("Units", typeof(string));
            table.Columns.Add("Status", typeof(string));
            return table;
        }

    }
}
