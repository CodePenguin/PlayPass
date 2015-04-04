using System;
using System.Collections.Generic;
using System.Xml;
using PlayPassEngine;
using PlaySharp;

namespace PlayPass
{
    class ConfigReader
    {

        public IList<ILogger> Loggers;
        public PassItems Passes { get; private set; }
        public string QueueListConnectionString { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }

        public ConfigReader(string fileName)
        {
            Loggers = new List<ILogger>();
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
            QueueListConnectionString = Util.GetNodeAttributeValue(settingsNode, "queuelist", "Provider=FileQueueList");

            LoadLoggers(settingsNode);

            LoadPasses(config);
        }

        private void LoadLoggers(XmlNode settingsNode)
        {
            var loggersNode = settingsNode.SelectSingleNode("loggers");
            if (loggersNode == null)
                return;
            foreach (XmlNode loggerNode in loggersNode.ChildNodes)
            {
                if (Util.GetNodeAttributeValue(loggerNode, "enabled", "1") != "1")
                    continue;
                var verboseMode = (Util.GetNodeAttributeValue(loggerNode, "verbose", "0") == "1");
                var connectionString = Util.GetNodeAttributeValue(loggerNode, "settings", "");
                var instance = LoggerFactory.GetLogger(connectionString, verboseMode);
                Loggers.Add(instance);
            }
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
