﻿using System;
using System.Collections.Generic;
using System.Xml;
using PlaySharp;

namespace PlayPass.Engine.Extensions
{
    /// <summary>
    ///     A class that reads the config file
    /// </summary>
    public class ConfigReader
    {
        private readonly XmlDocument _config = new XmlDocument();

        public ConfigReader(string fileName)
        {
            _config.Load(fileName);
        }

        private static PassAction CreatePassActionInstance(PassActionType actionType)
        {
            switch (actionType)
            {
                case PassActionType.Queue: return new PassQueueAction(); ;
                case PassActionType.Scan: return new PassScanAction();
                case PassActionType.Search: return new PassSearchAction();
                default: throw new Exception($"Unhandled PassActionType {actionType}");
            }
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
                var debugMode = (Util.GetNodeAttributeValue(loggerNode, "debug", "0") == "1");
                var verboseMode = (Util.GetNodeAttributeValue(loggerNode, "verbose", "0") == "1");
                var connectionString = Util.GetNodeAttributeValue(loggerNode, "settings");
                var instance = LoggerFactory.GetLogger(connectionString, debugMode, verboseMode);
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
                var actionType = StringToPassActionType(actionNode.Name);
                var action = CreatePassActionInstance(actionType);
                LoadPassActionFromXmlNode(action, actionNode);
                if (PassItemTypeHasActions(action.Type))
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

        public IQueueValidator GetQueueValidator(IQueueList queueList)
        {
            var limitsNode = _config.SelectSingleNode("playpass/settings/limits");
            var queueDurationLimit = TimeSpan.Parse(Util.GetNodeAttributeValue(limitsNode, "queue_duration", "00:00:00"));
            var queueCountLimit = int.Parse(Util.GetNodeAttributeValue(limitsNode, "queue_count", "0"));
            return new QueueValidator(queueList) { QueueDurationLimit = queueDurationLimit, QueueCountLimit = queueCountLimit };
        }

        private static void LoadPassActionFromXmlNode(PassAction action, XmlNode actionNode)
        {
            action.Name = Util.GetNodeAttributeValue(actionNode, "name");
            action.Exclude = Util.GetNodeAttributeValue(actionNode, "exclude");
            action.Reverse = Util.GetNodeAttributeBooleanValue(actionNode, "reverse");
            if (action is PassQueueAction queueAction)
            {
                queueAction.CountLimit = int.Parse(Util.GetNodeAttributeValue(actionNode, "limit_count", "0"));
                queueAction.DurationLimit = TimeSpan.Parse(Util.GetNodeAttributeValue(actionNode, "limit_duration", "00:00:00"));
            }
        }

        private static bool PassItemTypeHasActions(PassActionType actionType)
        {
            return (actionType == PassActionType.Scan || actionType == PassActionType.Search);
        }

        private static PassActionType StringToPassActionType(string type)
        {
            switch (type.ToLower())
            {
                case "queue":
                    return PassActionType.Queue;
                case "scan":
                    return PassActionType.Scan;
                case "search":
                    return PassActionType.Search;
                default:
                    throw new Exception($"Invalid PassActionType string: {type}");
            }
        }
    }
}