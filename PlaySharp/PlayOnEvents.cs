using System;
using System.Xml;

namespace PlaySharp
{
    public class XmlRequestEventArgs : EventArgs
    {
        public string RequestUrl;
        public XmlDocument Xml;
    }

    public delegate void XmlRequestEventHandler(object sender, XmlRequestEventArgs e);
}
