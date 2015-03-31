using System.Xml;
using System.Text.RegularExpressions;

namespace PlaySharp
{

    public static class Util
    {
        /// <summary>
        /// Gets the attribute value of a node as a string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetNodeAttributeValue(XmlNode Node, string AttributeName, string DefaultValue)
        {
            if (Node != null) {
                XmlAttribute attr = Node.Attributes[AttributeName];
                if (attr != null)
                    return attr.Value;
            }
            return DefaultValue;
        }

        /// <summary>
        /// Gets the attribute value of a node as a string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetNodeAttributeValue(XmlNode Node, string AttributeName)
        {
            return GetNodeAttributeValue(Node, AttributeName, "");
        }

        /// <summary>
        /// Gets the InnerText of a node as a string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetNodeInnerText(XmlNode Node, string DefaultValue)
        {
            if (Node != null)
                return Node.InnerText;
            return DefaultValue;
        }

        /// <summary>
        /// Gets the InnerText of a node as a string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetNodeInnerText(XmlNode Node)
        {
            return GetNodeInnerText(Node, "");
        }

        /// <summary>
        /// Gets the attribute value of the first child node of ParentNode that matches the XPath expression ChildNodePath as a string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetChildNodeAttributeValue(XmlNode ParentNode, string ChildNodePath, string AttributeName, string DefaultValue)
        {
            if (ParentNode != null)
                return GetNodeAttributeValue(ParentNode.SelectSingleNode(ChildNodePath), AttributeName, DefaultValue);
            return DefaultValue;
        }

        /// <summary>
        /// Gets the attribute value of the first child node of ParentNode that matches the XPath expression ChildNodePath as a string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetChildNodeAttributeValue(XmlNode ParentNode, string ChildName, string AttributeName)
        {
            return GetChildNodeAttributeValue(ParentNode, ChildName, AttributeName, "");
        }

        /// <summary>
        /// Returns true if the string Value matches the string Pattern using * to match multiple characters and ? to match a single character.
        /// </summary>
        /// <returns></returns>
        public static bool MatchesPattern(string Value, string Pattern)
        {
            return new Regex("(?i)^" + Regex.Escape(Pattern).Replace(@"\*", ".*").Replace(@"\?",".") + "$").IsMatch(Value);
        }
    }

}
