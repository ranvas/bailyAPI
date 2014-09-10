using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using API;
using System.Xml;


namespace APITests
{
  [TestFixture]
    public class getFunctionsTest
    {
      [Test]
      public void loadXmlDocument()
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(@"D:\Programming\Projects\API\API\API.xml");
        Assert.AreEqual(doc.OuterXml, API.API.getFunctions());
      }
    }
}
