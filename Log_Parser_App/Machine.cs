using System;
using System.Collections.Generic;
using System.Text;

namespace Log_Parser_App
{
    class Machine
    {
        public string Company_Code { get; set; }
        public string Machine_Code { get; set; }
        public string IP_Adress { get; set; }
        public string Log_Folder { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string Parser_Template { get; set; }
        public List<Log_File> Files { get; set; }
    }

    class Log_File
    {
        public string File_Name { get; set; }
        public string File_Content { get; set; }
    }
}
