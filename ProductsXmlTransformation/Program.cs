using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.IO;

namespace ProductsXmlTransformation
{
    class Program
    {
        public static string ProductsXmlFilePath;
        public static string OutputRootDirectory;
        public static XmlDocument ProductsXmlDocument;
        public static StringBuilder ErrorText;
        static void Main(string[] args)
        {
            ProductsXmlFilePath = ConfigurationManager.AppSettings["ProductsXmlFilePath"];
            OutputRootDirectory = ConfigurationManager.AppSettings["OutputRootDirectory"];
            ErrorText = new StringBuilder();

            ProductsXmlDocument = new XmlDocument();
            ProductsXmlDocument.Load(ProductsXmlFilePath);

            var productCount = 0;
            var productXmlDocumentsGenerated = 0;

            foreach (XmlNode productProductsXml in ProductsXmlDocument.SelectNodes("/ProductFile/Products/Product"))
            {
                try
                {
                    productCount++;
                    GenerateProductXmlFile(productProductsXml);
                    productXmlDocumentsGenerated++;
                }
                catch (Exception ex)
                {
                    ErrorText.AppendLine(string.Format("Error encountered creating Product XML file for Product {0}.", productProductsXml.Attributes["prodId"].Value));
                    ErrorText.AppendLine(string.Format("Message: {0}", ex.Message));
                    ErrorText.AppendLine(string.Format("StackTrace: {0}", ex.StackTrace));
                    ErrorText.AppendLine();
                }
            }

            Console.WriteLine("{0} Product XML documents generated.", productXmlDocumentsGenerated);
            if (ErrorText.Length > 0)
            {
                using (StreamWriter writer = new StreamWriter(OutputRootDirectory + "Errors.txt"))
                {
                    writer.WriteLine(ErrorText.ToString());
                }
                Console.WriteLine("Failed to generate {0} product XML files.", productCount - productXmlDocumentsGenerated);
                Console.WriteLine("See file Errors.txt located in output directory for details.");
            }
            Console.ReadKey();
        }

        public static void GenerateProductXmlFile(XmlNode productProductsXml)
        {
            var productId = productProductsXml.Attributes["prodId"].Value;

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
                if (paperNodeProductsXml.Attributes["layout"] != null)
                {
                    valueElement.SetAttribute("dp2Product", paperNodeProductsXml.Attributes["layout"].Value);
                }
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
            if (templateNodesProductsXml.Count > 0)
            {
                XmlElement optionElement2 = doc.CreateElement("Option");
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
                            XmlElement textNodeElement = doc.CreateElement("TextNode");
                            if (textNodeProductsXml.Attributes["textLine"] != null)
                            {
                                textNodeElement.SetAttribute("textLine", textNodeProductsXml.Attributes["textLine"].Value);
                            }
                            if (textNodeProductsXml.Attributes["limit"] != null)
                            {
                                textNodeElement.SetAttribute("limit", textNodeProductsXml.Attributes["limit"].Value);
                            }
                            textNodesElement.AppendChild(textNodeElement);
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
                            var textNodeLimitAttributeName = string.Format("textNodeLimit{0}", i);
                            if (templateNodeProductsXml.Attributes[textNodeLimitAttributeName] != null)
                            {
                                textNodeElement.SetAttribute("limit", templateNodeProductsXml.Attributes[textNodeLimitAttributeName].Value);
                            }
                            textNodesElement.AppendChild(textNodeElement);
                        }
                        valueElement.AppendChild(textNodesElement);
                    }
                }
            }

            // add any Supply Sheet information from Products.xml
            var xPathProductAssemblySupply = string.Format("/ProductFile/ProductAssemblySupplies/ProductAssemblySupply[@name = '{0}']", productId);
            if (ProductsXmlDocument.SelectSingleNode(xPathProductAssemblySupply) != null)
            {
                var productAssemblySupplyProductsXml = ProductsXmlDocument.SelectSingleNode(xPathProductAssemblySupply);
                XmlElement supplySheetElement = doc.CreateElement("SupplySheet");

                // attribute values of the ProductAssemblySupply node are used in MakeTask, ex. assemblyType
                foreach (XmlAttribute supplyAttributeProductsXml in productAssemblySupplyProductsXml.Attributes)
                {
                    // type attribute value is always "ProdId"
                    if (supplyAttributeProductsXml.Name != "type")
                    {
                        supplySheetElement.SetAttribute(supplyAttributeProductsXml.Name, supplyAttributeProductsXml.Value);
                    }
                }
                productElement.AppendChild(supplySheetElement);

                // add values from ProductAssemblySupply/AssemblySupply nodes in Products.xml
                foreach (XmlNode assemblySupplyProductsXml in productAssemblySupplyProductsXml.SelectNodes("AssemblySupply"))
                {
                    XmlElement supplySheetValue = doc.CreateElement("Value");
                    supplySheetValue.SetAttribute("name", assemblySupplyProductsXml.Attributes["name"].Value);
                    supplySheetValue.SetAttribute("quantity", assemblySupplyProductsXml.Attributes["quantity"].Value);
                    supplySheetValue.SetAttribute("perImagesOrdered", assemblySupplyProductsXml.Attributes["perImagesOrdered"].Value);
                    supplySheetElement.AppendChild(supplySheetValue);
                }
            }

            // write individual product xml file
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            var filePath = OutputRootDirectory + RemoveInvalidCharactersFromFileName(productId) + ".xml";
            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                doc.Save(writer);
            }
        }

        public static string RemoveInvalidCharactersFromFileName(string fileName)
        {
            return fileName.Replace("/", "");
        }
    }
}

