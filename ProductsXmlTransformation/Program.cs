using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace ProductsXmlTransformation
{
    class Program
    {
        public static string ProductsXmlFilePath;
        public static string OutputRootDirectory;
        public static XmlDocument ProductsXmlDocument;
        static void Main(string[] args)
        {
            ProductsXmlFilePath = ConfigurationManager.AppSettings["ProductsXmlFilePath"];
            OutputRootDirectory = ConfigurationManager.AppSettings["OutputRootDirectory"];

            ProductsXmlDocument = new XmlDocument();
            ProductsXmlDocument.Load(ProductsXmlFilePath);

            var productXmlDocumentsGenerated = 0;

            foreach (XmlNode productProductsXml in ProductsXmlDocument.SelectNodes("/ProductFile/Products/Product"))
            {
                try
                {
                    GenerateProductXmlFile(productProductsXml);
                    productXmlDocumentsGenerated++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error encountered creating Product XML file for Product {0}.", productProductsXml.Attributes["prodId"].Value);
                    Console.WriteLine("Message: {0}", ex.Message);
                    Console.WriteLine("StackTrace: {0}", ex.StackTrace);
                    Console.WriteLine();
                }
            }

            Console.WriteLine("{0} Product XML documents generated.", productXmlDocumentsGenerated);
            Console.ReadKey();
        }

        public static void GenerateProductXmlFile(XmlNode productProductsXml)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            // root node of document: <Product>
            XmlElement productElement = doc.CreateElement("Product");
            productElement.SetAttribute("name", productProductsXml.Attributes["name"].Value);
            //TODO: how do we identify the class the product should use?
            productElement.SetAttribute("class", "");
            doc.AppendChild(productElement);

            XmlElement optionsElement = doc.CreateElement("Options");
            productElement.AppendChild(optionsElement);

            XmlElement optionElement = doc.CreateElement("Option");
            optionElement.SetAttribute("id", "Paper");
            optionElement.SetAttribute("name", "Paper Type");
            optionElement.SetAttribute("min", "1");
            optionElement.SetAttribute("max", "1");
            optionsElement.AppendChild(optionElement);

            // get paper nodes for Product in Products.xml
            var paperNodesProductsXml = productProductsXml.SelectNodes("Paper");

            foreach (XmlNode paperNodeProductsXml in paperNodesProductsXml)
            {
                XmlElement valueElement = doc.CreateElement("Value");
                valueElement.SetAttribute("id", paperNodeProductsXml.Attributes["id"].Value);
                valueElement.SetAttribute("name", paperNodeProductsXml.Attributes["name"].Value);
                valueElement.SetAttribute("dp2Product", paperNodeProductsXml.Attributes["layout"].Value);
                if (paperNodeProductsXml.Attributes["iccprofile"] != null)
                {
                    valueElement.SetAttribute("iccProfile", paperNodeProductsXml.Attributes["iccprofile"].Value);
                }
                if (paperNodeProductsXml.Attributes["papersurface"] != null)
                {
                    valueElement.SetAttribute("paperSurface", paperNodeProductsXml.Attributes["papersurface"].Value);
                }
                optionElement.AppendChild(valueElement);

                // get ProductCode node from Paper node in Products.xml
                var productCodeNodeProductsXml = paperNodeProductsXml.SelectSingleNode("ProductCode");

                // for prodId "4Wal", proQtyMultiplier is 4 for ESurface and 1 for Fuji Pearl
                // this was represented by a single node Product/ProQtyMultipierForPricing in original Product XMLs
                XmlElement productCodeElement = doc.CreateElement("ProductCode");
                productCodeElement.SetAttribute("studio", productCodeNodeProductsXml.Attributes["studio"].Value);
                productCodeElement.SetAttribute("avg", productCodeNodeProductsXml.Attributes["avg"].Value);
                productCodeElement.SetAttribute("ind", productCodeNodeProductsXml.Attributes["ind"].Value);
                productCodeElement.SetAttribute("studioPro", productCodeNodeProductsXml.Attributes["studioPro"].Value);
                productCodeElement.SetAttribute("indPro", productCodeNodeProductsXml.Attributes["indPro"].Value);
                productCodeElement.SetAttribute("proQtyMultiplier", productCodeNodeProductsXml.Attributes["proQtyMultiplier"].Value);
                valueElement.AppendChild(productCodeElement);
            }

            // get template nodes for Product in Products.xml
            var templateNodesProductsXml = productProductsXml.SelectNodes("Template");
            XmlElement optionElement2;
            if (templateNodesProductsXml.Count > 0)
            {
                optionElement2 = doc.CreateElement("Option");
                optionElement2.SetAttribute("id", "Template");
                optionElement2.SetAttribute("name", "Template");
                //TODO: shouldn't 1 template be required? FunPak.xml has min=0 and max=0
                optionElement2.SetAttribute("min", "0");
                optionElement2.SetAttribute("max", "0");
                optionsElement.AppendChild(optionElement2);

                foreach (XmlNode templateNodeProductsXml in templateNodesProductsXml)
                {
                    XmlElement valueElement = doc.CreateElement("Value");
                    valueElement.SetAttribute("id", templateNodeProductsXml.Attributes["id"].Value);
                    valueElement.SetAttribute("description", templateNodeProductsXml.Attributes["description"].Value);
                    // remove the file extension from the image attribute. some images contain '.' in their name.
                    valueElement.SetAttribute("dp2Product", templateNodeProductsXml.Attributes["image"].Value.Remove(templateNodeProductsXml.Attributes["image"].Value.LastIndexOf('.')));
                    optionElement2.AppendChild(valueElement);

                    // some text nodes are defined by nodes in Products.xml, and others are defined by attributes in the Template node
                    if (templateNodeProductsXml.SelectSingleNode("TextNodes") != null)
                    {
                        XmlElement textNodesElement = doc.CreateElement("TextNodes");
                        foreach (XmlNode textNodeProductsXml in templateNodeProductsXml.SelectNodes("TextNodes/TextNode"))
                        {

                        }
                        valueElement.AppendChild(textNodesElement);
                    }
                    else if ((templateNodeProductsXml.Attributes["textNodes"] != null) && (Convert.ToInt32(templateNodeProductsXml.Attributes["textNodes"].Value) > 0))
                    {
                        XmlElement textNodesElement = doc.CreateElement("TextNodes");                        
                        var textNodesCount = Convert.ToInt32(templateNodeProductsXml.Attributes["textNodes"].Value);
                        for (int i = 1; i <= textNodesCount; i++)
                        {
                            XmlElement textNodeElement = doc.CreateElement("TextNode");
                            textNodeElement.SetAttribute("textLine", i.ToString());
                            if (templateNodeProductsXml.Attributes[string.Format("textNodeLimit{0}", i)] != null)
                            {
                                textNodeElement.SetAttribute("limit", templateNodeProductsXml.Attributes[string.Format("textNodeLimit{0}", i)].Value);
                            }
                            textNodesElement.AppendChild(textNodeElement);
                        }
                        valueElement.AppendChild(textNodesElement);
                    }
                }
            }
        }
    }
}

