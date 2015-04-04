using System;
using System.Collections.Generic;
using System.Xml;
using PlayPassEngine;
using PlaySharp;

namespace PlayPass
{
    internal class ConfigReader
    {
        private readonly XmlDocument _config = new XmlDocument();

        public ConfigReader(string fileName)
        {
            _config.Load(fileName);
        }

        public void GetLoggers(ICollection<ILogger> loggers)
        {
            var loggersNode = _config.SelectSingleNode("playpass/settings/loggers");
            if (loggersNode == null)
                return;
            foreach (XmlNode loggerNode in loggersNode.ChildNodes)
            {
                if (Util.GetNodeAttributeValue(loggerNode, "enabled", "1") != "1")
                    continue;
                var verboseMode = (Util.GetNodeAttributeValue(loggerNode, "verbose", "0") == "1");
                var connectionString = Util.GetNodeAttributeValue(loggerNode, "settings");
                var instance = LoggerFactory.GetLogger(connectionString, verboseMode);
                loggers.Add(instance);
            }
        }

        public IQueueList GetQueueList()
        {
            var queueListNode = _config.SelectSingleNode("playpass/settings/queuelist");
            var connectionString = Util.GetNodeAttributeValue(queueListNode, "settings", "Provider=FileQueueList");
            return QueueListFactory.GetQueueList(connectionString);
        }

        public void GetPasses(PassItems passes)
        {
            var passNodes = _config.SelectNodes("playpass/passes/pass");
            if (passNodes == null)
                return;
            foreach (XmlNode passNode in passNodes)
            {
                var pass = new PassItem(
                    Util.GetNodeAttributeValue(passNode, "description"),
                    (Util.GetNodeAttributeValue(passNode, "enabled", "1") == "1")
                    );
                GetPassActions(pass.Actions, passNode);
                passes.Add(pass);
            }
        }

        private static void GetPassActions(PassActions list, XmlNode parentNode)
        {
            foreach (XmlNode actionNode in parentNode.ChildNodes)
            {
                var action = new PassAction(
                    Util.GetNodeAttributeValue(actionNode, "name"),
                    StringToPassItemType(actionNode.Name)
                    );
                if (action.Type == PassActionType.Scan)
                    GetPassActions(action.Actions, actionNode);
                list.Add(action);
            }
        }

        public PlayOn GetPlayOn()
        {
            var playonNode = _config.SelectSingleNode("playpass/settings/playon");
            var serverHost = Util.GetNodeAttributeValue(playonNode, "host",  PlayOnConstants.DefaultHost);
            var serverPort = int.Parse(Util.GetNodeAttributeValue(playonNode, "port", PlayOnConstants.DefaultPort.ToString()));            
            return new PlayOn(serverHost, serverPort);
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