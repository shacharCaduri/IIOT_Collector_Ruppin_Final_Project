using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Nest;

namespace Log_Parser_App.Log_Sample
{
    /*LogParser implement the interface and parsering the HTML file to CSV */
    class LogParser :ILogParser
    {
        /*List<ConfigurtionJson> for holding the Configurtion from the Configurtion file,
         while the system running*/
        public List<ConfigurtionJson> TermConfigurtion;

        /*NO Arguments Constructor*/
        public LogParser()
        {
            this.TermConfigurtion = ConfigurtionJson.EnterConfigFileToList();
        }

        /*Parsering the HTML file to HtmlNodes and sorting with function Get_ItemToDataTable*/
        public DataTable Get_LogToDataTable(HtmlDocument log_File)
        {
            if (log_File != null) 
            {
                if (this.TermConfigurtion.Any())
                {
                    string ClassToGet = "hdr";
                    DataTable reportTable;
                    List<HtmlNode> divs = log_File.DocumentNode.SelectSingleNode("//body").DescendantsAndSelf().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == "body").ToList();
                    divs.RemoveAll(x => x.Name == "br" || x.Name == "hr");
                    if (divs[1].SelectNodes("//table[@class='" + ClassToGet + "']") != null)
                        reportTable = Get_HdrTable_Report();
                    else
                        reportTable = Get_SpanTable_Report();
                    foreach (HtmlNode item in divs)
                    {
                        Get_ItemToDataTable(reportTable, item);
                    }
                    return reportTable;
                }
                else
                {
                    Console.WriteLine("No Configurtion file");
                    return null;
                }
            }
            return null;
        }

        /*Checking the HTMLNode and determines which type is the node and calling the right 
         * function and checking the term from the Configuration file*/
        private void Get_ItemToDataTable(DataTable reportTable, HtmlNode item)
        {
            string testName="1";
            if (item.Name == "table" && IsTermConfigurtionTrue("table"))
                Get_HtmlTableToDataTable(reportTable, item);
            if (item.Name == "h3" && IsTermConfigurtionTrue("h3") || item.Name == "h5" && IsTermConfigurtionTrue("h5"))
                Get_HtagToDataTable(reportTable, item, testName);
            if (item.Name == "div")
                Get_DivToDataTable(reportTable, item);
            if (item.Name == "span" && IsTermConfigurtionTrue("span"))
                Get_SpanTagToDataTable(reportTable, item);
            if (item.Name == "a" && IsTermConfigurtionTrue("a"))
                Get_AtagToDataTable(reportTable, item);
            if (item.Name == "b" && IsTermConfigurtionTrue("b"))
                Get_BtagToDataTable(reportTable, item);
            if (item.Name == "center" && IsTermConfigurtionTrue("center"))
                Get_CenterTagToDataTable(reportTable, item);
            if (item.Name == "i" && IsTermConfigurtionTrue("i"))
                Get_ItagToDataTable(reportTable, item);
        }

        /*Extracting the informtion from the Div tag to the DataTable*/
        private void Get_DivToDataTable(DataTable reportTable, HtmlNode item)
        {
            List<HtmlNode> divChilds = item.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).ToList();
            foreach (HtmlNode divChild in divChilds)
            {
                Get_ItemToDataTable(reportTable, divChild);
            }
                    
        }

        /*Extracting the informtion from the Table tag to the DataTable*/
        private void Get_HtmlTableToDataTable(DataTable reportTable, HtmlNode item)
        {
            string timeStemp = DateTime.Now.ToString();
            string testName="";
            string[] check2;
            string check3 = "";
            List<HtmlNode> tables = item.ChildNodes.Descendants().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == "tbody").ToList();
            HtmlNode lastTd;
            HtmlNode elementInTd;
            foreach (HtmlNode tr in tables)
            {
                List<HtmlNode> tds = tr.Descendants().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == tr.Name).ToList();
                lastTd = tds.LastOrDefault();
                foreach (HtmlNode td in tds)
                {
                    if (td.ParentNode.Name != "font")
                    {
                        elementInTd = td.DescendantsAndSelf().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == td.Name).SingleOrDefault();
                        if (elementInTd != null)
                        {
                            if (elementInTd.ChildNodes.Any(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == elementInTd.Name))
                                elementInTd = elementInTd.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).SingleOrDefault();
                        }
                        else
                        {
                            elementInTd = td;
                        }
                        check3 += elementInTd.InnerHtml.Trim().Replace(":", "").Replace("&nbsp;", "");
                        check3 = Regex.Replace(check3, @"(\<(\/)?(\w)*(\d)?\>)", string.Empty);
                        if (td != lastTd)
                            check3 += "&&&";
                        if (elementInTd.Attributes.Count()>0)
                            if (elementInTd.InnerHtml.Contains("Test"))
                                testName = check3.Split("&&&")[0];
                        else if(reportTable.AsEnumerable().Last().ItemArray[3].ToString() != "")
                            testName = reportTable.AsEnumerable().Last().ItemArray[3].ToString();
                        
                    }
                    else
                        Get_FontTagToDataTable(reportTable, td);
                }
                if(check3.Split("&&&").Length== 2)
                    check3 = check3 + "&&&" + timeStemp+ "&&&" + testName;
                else
                    check3 = check3 + "&&&" +" "+ "&&&" + timeStemp+ "&&&" + testName;
                check2 = check3.TrimEnd().Split("&&&");
                reportTable.Rows.Add(check2);
                check3 = "";
            }
        }

        /*Extracting the informtion from the H tag to the DataTable*/
        private void Get_HtagToDataTable(DataTable reportTable, HtmlNode h,string testName)
        {
            ConfigurtionJson checkTerm;
            if (h.Name == "h3")
            {
                checkTerm = this.TermConfigurtion.Find(tc => tc.TagName == "h3");
                if(checkTerm.TagValue!=null && h.InnerText.Trim().Contains(checkTerm.TagValue))
                     reportTable.Rows.Add(h.InnerText.Trim());
                else if(checkTerm.TagValue == null)
                    reportTable.Rows.Add(h.InnerText.Trim());
                testName = null;
            }
            if (h.Name == "h5")
            {
                checkTerm = this.TermConfigurtion.Find(tc => tc.TagName == "h5");
                if (checkTerm.TagValue != null && h.InnerText.Trim().Contains(checkTerm.TagValue))
                    reportTable.Rows.Add(h.InnerText.Trim(),"",DateTime.Now, h.InnerText.Trim());
                else if (checkTerm.TagValue == null)
                    reportTable.Rows.Add(h.InnerText.Trim(), "", DateTime.Now, h.InnerText.Trim());
                testName = h.InnerText.Trim();
            }
        }

        /*Extracting the informtion from the Span tag to the DataTable*/
        private void Get_SpanTagToDataTable(DataTable reportTable, HtmlNode span)
        {
            if (span.Descendants().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == span.Name).Count() == 0)
            {
                string check = span.InnerText.Replace("&nbsp;", "   ").Replace("**","").Trim();
                check = Regex.Replace(check, @"\s+", "   ").Trim();
                string[] withoutSpace = check.Split("   ");
                if (withoutSpace.Length >6)
                {
                    withoutSpace[0] = FixString(withoutSpace);
                    reportTable.Rows.Add(withoutSpace[0]);
                    check = " ";
                }
                if (check.Length > 0 && check!=" ")
                     reportTable.Rows.Add(withoutSpace);
            }
            else
            {
                List<HtmlNode> spanChildren = span.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).ToList();
                foreach (HtmlNode spanChild in spanChildren)
                {
                    if (spanChild.Name != "br")
                        Get_ItemToDataTable(reportTable, spanChild);
                }
            }
        }

        /*Extracting the informtion from the a tag to the DataTable*/
        private void Get_AtagToDataTable(DataTable reportTable, HtmlNode a)
        {
            List<HtmlNode> aChildren = a.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element).ToList();
            foreach (HtmlNode item in aChildren)
            {
                Get_ItemToDataTable(reportTable, item);
            }
        }

        /*Extracting the informtion from the b tag to the DataTable*/
        private void Get_BtagToDataTable(DataTable reportTable, HtmlNode b)
        {
            reportTable.Rows.Add(b.InnerText.Replace("&nbsp;", " ").Trim());
        }

        /*Extracting the informtion from the i tag to the DataTable*/
        private void Get_ItagToDataTable(DataTable reportTable, HtmlNode i)
        {
            List<HtmlNode> iChildren = i.Descendants().Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == i.Name).ToList();
            foreach (HtmlNode iChild in iChildren)
            {
                Get_ItemToDataTable(reportTable, iChild);
            }
        }

        /*Extracting the informtion from the Center tag to the DataTable*/
        private void Get_CenterTagToDataTable(DataTable reportTable, HtmlNode center)
        {
            List<HtmlNode> centerChildren = center.Descendants().Where(x => x.NodeType == HtmlNodeType.Element).ToList();
            string check = "";
            int count = 0;
            foreach (HtmlNode centerChild in centerChildren)
            {
                if(centerChild.InnerText.Replace("&nbsp;", " ").Trim()!=check ||count==0)
                   {
                    check= centerChild.InnerText.Replace("&nbsp;", " ").Trim();
                    count++;
                    }
            }
            if (count == 1)
            {
                reportTable.Rows.Add(check);
            }
        }

        /*Extracting the informtion from the Font tag to the DataTable*/
        private void Get_FontTagToDataTable(DataTable reportTable, HtmlNode font)
        {
            List<HtmlNode> fonts = font.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element && x.ParentNode.Name == font.Name).ToList();
                        foreach (HtmlNode item in fonts)
                        {
                            if(item.Name!="br")
                                 Get_ItemToDataTable(reportTable, item);
                        }
        }

        /*Puts the DataTable in to a CSV file*/
        public void DataTableToCSV(DataTable reportTable, string file_Name)
        {
            if (reportTable != null && reportTable.Rows.Count > 0)
            {
                // create object for the StringBuilder class
                StringBuilder sb = new StringBuilder();
                string strLocation = CreateDir(file_Name);

                // Get name of columns from datatable and assigned to the string array
                string[] columnNames = reportTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();

                // Create comma sprated column name based on the items contains string array columnNames
                sb.AppendLine(string.Join(",", columnNames));

                // Fatch rows from datatable and append values as comma saprated to the object of StringBuilder class 
                foreach (DataRow row in reportTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                    sb.AppendLine(string.Join(",", fields));
                }
                // save the file
                if (!File.Exists(strLocation+"\\"+ file_Name))
                { // Create a file to write to 
                    File.WriteAllText(strLocation, sb.ToString());
                }
            }
        }

        /*Creating the folder for the CSV file*/
        public string CreateDir(string file_Name)
        {
            StringBuilder sb = new StringBuilder();
            string subDir = DateTime.Now.ToString("yyyyMMddHHmmss");
            ConfigurtionJson location = this.TermConfigurtion.FirstOrDefault(tc => tc.NodeType == "file");

            if (!Directory.Exists(location.TagValue))
            {
                Directory.CreateDirectory(location.TagValue);
            }

            // Create a sub directory
            if (!Directory.Exists(location.TagValue + "\\" + subDir) && Program.createDir)
            {
                Directory.CreateDirectory(location.TagValue + "\\" + subDir);
                Program.dir = location.TagValue + "\\" + subDir;
                Program.createDir = false;
            }
            return $@"{Program.dir}\{file_Name}.csv";
        }

        /*Checking if the term from the Configuration file exist*/
        public bool IsTermConfigurtionTrue(string tagName,string tagVal=null)
        {
            if (tagVal == null)
            {
                if (this.TermConfigurtion.Find(tc => tc.TagName == tagName) != null)
                    return true;
                return false;
            }
            else
            {
                if (this.TermConfigurtion.Find(tc => tc.TagName == tagName && tc.TagValue == tagVal) != null)
                    return true;
                return false;
            }
        }

        /*NO Arguments Constructor for DataTable*/
        private DataTable Get_HdrTable_Report()
        {
            DataTable table = new DataTable("Table_Report");
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.Columns.Add("timeStemp", typeof(DateTime));
            table.Columns.Add("Test", typeof(string));
            return table;
        }

        /*NO Arguments Constructor for DataTable*/
        private DataTable Get_SpanTable_Report()
        {
            DataTable table = new DataTable("Table_Report");            
            table.Columns.Add("Test Name", typeof(string));
            table.Columns.Add("Received", typeof(string));
            table.Columns.Add("Lower Limit", typeof(string));
            table.Columns.Add("Upper Limit", typeof(string));
            table.Columns.Add("Units", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("timeStemp", typeof(DateTime));
            table.Columns.Add("Test", typeof(string));
            return table;
        }

        /*Fix wrong string in one of the logs*/
        private string FixString(string[] withoutSpace)
        {
            string withoutSpace2 = "";
            for (int i = 0; i < withoutSpace.Length; i++)
            {
                withoutSpace2 = withoutSpace2 + withoutSpace[i] + " ";
            }
            return withoutSpace2;
        }
    }
}
