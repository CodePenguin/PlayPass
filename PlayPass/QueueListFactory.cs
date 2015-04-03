using System;
using System.Collections.Generic;
using System.Data.Common;
using PlayPassEngine;

namespace PlayPass
{
    static class QueueListFactory
    {
        private static Dictionary<string, Type> _classes = new Dictionary<string, Type>();

        public static IQueueList GetQueueList(string connectionString)
        {
            var parser = new DbConnectionStringBuilder() { ConnectionString = connectionString };
            if (!parser.ContainsKey("Provider"))
                throw new Exception("Queue List Provider Type is not specified");

            var providerType = parser["Provider"].ToString().ToUpper();

            if (!_classes.ContainsKey(providerType))
                throw new Exception(String.Format("Unregistered Queue List Provider Type: {0}", providerType));

            var type = _classes[providerType];
            var instance = (IQueueList) Activator.CreateInstance(type);
            instance.Initialize(connectionString);
            return instance;
        }

        public static void RegisterClass(string providerType, Type type)
        {
            _classes[providerType.ToUpper()] = type;
        }
    }
}
