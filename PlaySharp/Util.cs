using System.Text.RegularExpressions;
using System.Xml;

namespace PlaySharp
{
    public static class Util
    {
        /// <summary>
        ///     Gets the attribute value of a node as a string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetNodeAttributeValue(XmlNode node, string attributeName, string defaultValue)
        {
            if (node == null)
                return defaultValue;
            if (node.Attributes == null)
                return defaultValue;
            var attr = node.Attributes[attributeName];
            return attr != null ? attr.Value : defaultValue;
        }

        /// <summary>
        ///     Gets the attribute value of a node as a string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetNodeAttributeValue(XmlNode node, string attributeName)
        {
            return GetNodeAttributeValue(node, attributeName, "");
        }

        /// <summary>
        ///     Gets the InnerText of a node as a string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetNodeInnerText(XmlNode node, string defaultValue)
        {
            return node != null ? node.InnerText : defaultValue;
        }

        /// <summary>
        ///     Gets the InnerText of a node as a string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetNodeInnerText(XmlNode node)
        {
            return GetNodeInnerText(node, "");
        }

        /// <summary>
        ///     Gets the attribute value of the first child node of ParentNode that matches the XPath expression ChildNodePath as a
        ///     string or returns DefaultValue if it doesn't exist.
        /// </summary>
        public static string GetChildNodeAttributeValue(XmlNode parentNode, string childNodePath, string attributeName, string defaultValue)
        {
            return parentNode != null ? GetNodeAttributeValue(parentNode.SelectSingleNode(childNodePath), attributeName, defaultValue) : defaultValue;
        }

        /// <summary>
        ///     Gets the attribute value of the first child node of ParentNode that matches the XPath expression ChildNodePath as a
        ///     string or returns a blank string if it doesn't exist.
        /// </summary>
        public static string GetChildNodeAttributeValue(XmlNode parentNode, string childName, string attributeName)
        {
            return GetChildNodeAttributeValue(parentNode, childName, attributeName, "");
        }

        /// <summary>
        ///     Returns true if the string Value matches the string Pattern using * to match multiple characters and ? to match a
        ///     single character.
        /// </summary>
        /// <returns></returns>
        public static bool MatchesPattern(string value, string pattern)
        {
            return new Regex("(?i)^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$").IsMatch(value);
        }
    }
}