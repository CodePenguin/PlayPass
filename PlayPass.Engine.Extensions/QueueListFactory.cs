using System;
using System.Collections.Generic;
using System.Data.Common;

namespace PlayPass.Engine.Extensions
{
    /// <summary>
    ///     A factory class that registers and initializes QueueList classes
    /// </summary>
    public static class QueueListFactory
    {
        private static readonly Dictionary<string, Type> Classes = new Dictionary<string, Type>();

        public static IQueueList GetQueueList(string connectionString)
        {
            var parser = new DbConnectionStringBuilder {ConnectionString = connectionString};
            if (!parser.ContainsKey("Provider"))
                throw new Exception("Queue List Provider Type is not specified");

            var providerType = parser["Provider"].ToString().ToUpper();

            if (!Classes.ContainsKey(providerType))
                throw new Exception(String.Format("Unregistered Queue List Provider Type: {0}", providerType));

            var type = Classes[providerType];
            var instance = (IQueueList) Activator.CreateInstance(type);
            instance.Initialize(connectionString);
            return instance;
        }

        public static void RegisterClass(Type type)
        {
            Classes[type.Name.ToUpper()] = type;
        }
    }
}