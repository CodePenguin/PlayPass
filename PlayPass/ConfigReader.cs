using System;
using System.Xml;
using PlaySharp;

namespace PlayPass
{
    class ConfigReader
    {

        public string QueueListConnectionString { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }
        public PassItems Passes { get; private set; }

        public ConfigReader(string fileName)
        {
            Passes = new PassItems();

            // Default values
            ServerHost = PlayOnConstants.DefaultHost;
            ServerPort = PlayOnConstants.DefaultPort;
            QueueListConnectionString = "";

            // Load config values
            var config = new XmlDocument();
            config.Load(fileName);
            var settingsNode = config.SelectSingleNode("playpass/settings");
            if (settingsNode == null) 
                return;
            ServerHost = Util.GetNodeAttributeValue(settingsNode, "server", ServerHost);
            ServerPort = int.Parse(Util.GetNodeAttributeValue(settingsNode, "port", ServerPort.ToString()));
            QueueListConnectionString = Util.GetNodeAttributeValue(settingsNode, "queuelist", "Provider=FILE");

            LoadPasses(config);
        }

        private void LoadPasses(XmlNode config)
        {
            var passNodes = config.SelectNodes("playpass/passes/pass");
            if (passNodes == null)
                return;
            foreach (XmlNode passNode in passNodes)
            {
                var pass = new PassItem(
                    Util.GetNodeAttributeValue(passNode, "description"),
                    (Util.GetNodeAttributeValue(passNode, "enabled", "1") == "1")
                    );
                LoadPassActions(pass.Actions, passNode);
                Passes.Add(pass);
            }
        }

        private static void LoadPassActions(PassActions list, XmlNode parentNode)
        {
            foreach (XmlNode actionNode in parentNode.ChildNodes)
            {
                var action = new PassAction(
                    Util.GetNodeAttributeValue(actionNode, "name"),
                    StringToPassItemType(actionNode.Name)
                    );
                if (action.Type == PassActionType.Scan)
                    LoadPassActions(action.Actions, actionNode);
                list.Add(action);
            }
        }

        private static PassActionType StringToPassItemType(string type)
        {
            switch (type.ToLower())
            {
                case "queue":
                    return PassActionType.Queue;
                case "scan":
                    return PassActionType.Scan;
                default:
                    throw new Exception(String.Format("Invalid PassItemType string: {0}", type));
            }
        }

        

    }
}
