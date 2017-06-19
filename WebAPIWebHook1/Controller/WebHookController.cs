using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace WebAPIWebHook1.Controller
{
    public class WebHookController : ApiController
    {
        public string Get()
        {
            return "Hello World";
        }
        public void Post(HttpRequestMessage request)
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(request.Content.ReadAsStreamAsync().Result);

            var mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("a", "http://www.docusign.net/API/3.0");

            var envelopeStatus = xmldoc.SelectSingleNode("//a:EnvelopeStatus", mgr);
            var envelopeId = envelopeStatus.SelectSingleNode("//a:EnvelopeID", mgr);
            var status = envelopeStatus.SelectSingleNode("//a:Status", mgr);
            if (envelopeId != null)
            {
                System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/Documents/" +
                    envelopeId.InnerText + "_" + status.InnerText + "_" + Guid.NewGuid() + ".xml"), xmldoc.OuterXml);
            }

            if (status.InnerText == "Completed")
            {
                // Loop through the DocumentPDFs element, storing each document.

                var docs = xmldoc.SelectSingleNode("//a:DocumentPDFs", mgr);
                foreach (XmlNode doc in docs.ChildNodes)
                {
                    var documentName = doc.ChildNodes[0].InnerText; // pdf.SelectSingleNode("//a:Name", mgr).InnerText;
                    var documentId = doc.ChildNodes[2].InnerText; // pdf.SelectSingleNode("//a:DocumentID", mgr).InnerText;
                    var byteStr = doc.ChildNodes[1].InnerText; // pdf.SelectSingleNode("//a:PDFBytes", mgr).InnerText;

                    System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/Documents/" + envelopeId.InnerText + "_" + documentId + "_" + documentName), byteStr);
                }
            }
        }
    }
}
