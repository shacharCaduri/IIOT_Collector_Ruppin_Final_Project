using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Log_Parser_App.Log_Sample;
using System.Timers;

namespace Log_Parser_App
{
    class Program
    {
        private static Timer aTimer;

        public static List<Machine> machines_list = new List<Machine>();

        public static bool createDir;

        public static string dir;

        private static double interval = 20000;
        static void Main(string[] args)
        {
            SetTimer();
            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}\n", DateTime.Now);
            Console.ReadLine();
            aTimer.Stop();
            aTimer.Dispose();

            Console.WriteLine("Terminating the application...");
        }

        /*SetTimer to how much time delay the program*/
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(interval);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        /*OnTimedEvent the event that happend in the end of the timer*/
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                              e.SignalTime);
            StartParser();
        }

        /*StartParser- start the parsering process*/
        private static void StartParser()
        {
            createDir = true;
            DataTable reportTable;
            HtmlDocument log = new HtmlDocument();
            LogParser parser = new LogParser();
            if (parser.TermConfigurtion == null)
                Console.WriteLine("No Configurtion file");
            else
            {
                machines_list = Get_Machines();
                Console.WriteLine("Obtain Files Of Machines");
                Parallel.ForEach(machines_list, (current_machine) =>
                {
                    Console.WriteLine("Collection Logs For " + current_machine.Machine_Code);
                    current_machine.Files = Get_Files(current_machine);
                });
                Console.WriteLine("Begin Parse Logs");
                foreach (Machine machine in machines_list)
                {
                    foreach (Log_File log_File in machine.Files)
                    {
                        log.LoadHtml(log_File.File_Content);
                        reportTable = parser.Get_LogToDataTable(log);
                        parser.DataTableToCSV(reportTable, log_File.File_Name.Replace(".html", "") + "_" + machine.Machine_Code + "_" + DateTime.Now.ToString("yyyyMMddHHmm"));
                    }
                }
                Console.WriteLine("End Parse Logs");
                Console.WriteLine("Begin Save Logs");
                Console.WriteLine("End Save Logs");
            }
        }

        /*Get_Machines-Bring the machines from the DB*/
        private static List<Machine> Get_Machines()
        {
            List<Machine> Local_List = new List<Machine>();
            try
            {
                string connectionString = GetConnectionString();
                using (SqlConnection mycon = new SqlConnection(connectionString))
                {
                    mycon.Open();
                    using (SqlCommand mycmd = new SqlCommand())
                    {
                        mycmd.CommandTimeout = 0;
                        mycmd.Connection = mycon;
                        mycmd.CommandType = CommandType.Text;
                        mycmd.CommandText = "SELECT Company_Code, Machine_Code, IP_Adress, Log_Folder, Username, Password, Domain, Key_Parsers.Parser_Template FROM Key_Machines inner join Key_Parsers on Key_Machines.Parser_Code = Key_Parsers.Parser_Code WHERE Machine_Type=2 ORDER BY Company_Code ASC";
                        SqlDataReader reader = mycmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Machine machine = new Machine();
                                machine.Company_Code = reader["Company_Code"].ToString();
                                machine.Machine_Code = reader["Machine_Code"].ToString();
                                machine.IP_Adress = reader["IP_Adress"].ToString();
                                machine.Log_Folder = reader["Log_Folder"].ToString();
                                machine.Username = reader["Username"].ToString();
                                machine.Password = reader["Password"].ToString();
                                machine.Domain = reader["Domain"].ToString();
                                machine.Parser_Template = reader["Parser_Template"].ToString();
                                Local_List.Add(machine);
                            }
                        }
                    }
                    mycon.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.InnerException);
            }
            return Local_List;
        }

        /*Get_Files-Brings the LOGS from the folder they are stored*/
        private static List<Log_File> Get_Files(Machine machine)
        {
            List<Log_File> Files = new List<Log_File>();
            var result =  System.IO.Directory.GetFiles(@"\\" + machine.IP_Adress + machine.Log_Folder);
        
            foreach (string Log_File in result)
            {
                Log_File Current_File = new Log_File();
                Current_File.File_Name = Path.GetFileName(Log_File);
                Current_File.File_Content = File.ReadAllText(Log_File);
                Files.Add(Current_File);
            }
            return Files;
        }

        /*GetConnectionString-Connection String to the DB for security reseaon*/
        static private string GetConnectionString()
        {
            // To avoid storing the connection string in your code,
            // you can retrieve it from a configuration file.
            return "Data Source =DESKTOP-N5JUU5Q\\SHAKEDDB; Initial Catalog = IIOT_Collector; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
        }

    }

}


