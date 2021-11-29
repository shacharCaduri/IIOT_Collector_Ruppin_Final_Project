using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Log_Parser_App
{
    /*Class for using the Configurtion file while the program is running*/
    class ConfigurtionJson
    {
        public string NodeType { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }

        /*NO Arguments Constructor*/
        public ConfigurtionJson()
        {
            this.NodeType = null;
            this.TagName = null;
            this.TagValue = null;
        }
        /*2 Arguments Constructor*/
        public ConfigurtionJson(string nodeType, string TagName)
        {
            this.NodeType = nodeType;
            this.TagName = TagName;
            this.TagValue = null;
        }
        /*3 Arguments Constructor*/
        public ConfigurtionJson(string nodeType, string TagName, string TagValue)
        {
            this.NodeType = nodeType;
            this.TagName = TagName;
            this.TagValue = TagValue;
        }
        /*Function that read the Configurtion file and creating the object*/
        public static List<ConfigurtionJson> EnterConfigFileToList()
        {
            using (StreamReader jsonFile = File.OpenText(ConnectionStringConfigFile()))
            {
                if (jsonFile != null)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    List<ConfigurtionJson> listOfConfigurtionJson = (List<ConfigurtionJson>)serializer.Deserialize(jsonFile, typeof(List<ConfigurtionJson>));
                    return listOfConfigurtionJson;
                }
                else
                    return null;
            } 
        }

        /*Function that return the Configurtion file path for security*/
        private static string ConnectionStringConfigFile()
        {
            return @"D:\Final Project\IIOT_Collector\Log_Parser_App\JSONconfig.json";
        }
    }
}