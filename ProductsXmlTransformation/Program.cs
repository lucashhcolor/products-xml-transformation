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
        public static int WarningCount;
        static void Main(string[] args)
        {
            ProductsXmlFilePath = ConfigurationManager.AppSettings["ProductsXmlFilePath"];
            OutputRootDirectory = ConfigurationManager.AppSettings["OutputRootDirectory"];
            ErrorText = new StringBuilder();

            ProductsXmlDocument = new XmlDocument();
            ProductsXmlDocument.Load(ProductsXmlFilePath);

            WarningCount = 0;

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
                Console.WriteLine("See file Errors.txt located in output directory for {0} warnings encountered.", WarningCount);
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

            // root node of document: <product>
            XmlElement productElement = doc.CreateElement("product");
            productElement.SetAttribute("name", productId);
            productElement.SetAttribute("display", productProductsXml.Attributes["name"].Value);

            var orderOptionXPath = string.Format("/ProductFile/OrderOptions/OrderOption[@id='{0}']", productId);
            var productOrderOptionProductsXml = ProductsXmlDocument.SelectSingleNode(orderOptionXPath);
            if ((productOrderOptionProductsXml != null) && (productOrderOptionProductsXml.Attributes["assemblyType"] != null))
            {
                productElement.SetAttribute("assemblyType", productOrderOptionProductsXml.Attributes["assemblyType"].Value);
            }
            if (productProductsXml.Attributes["category"] != null)
            {
                productElement.SetAttribute("category", productProductsXml.Attributes["category"].Value);
            }
            if (productProductsXml.Attributes["routeIndicator"] != null)
            {
                productElement.SetAttribute("routeIndicator", productProductsXml.Attributes["routeIndicator"].Value);
            }
            if (productProductsXml.Attributes["dymoProduct"] != null)
            {
                productElement.SetAttribute("dymoProduct", productProductsXml.Attributes["dymoProduct"].Value);
            }
            if (productProductsXml.Attributes["requires10x13Envelope"] != null)
            {
                productElement.SetAttribute("requires10x13Envelope", productProductsXml.Attributes["requires10x13Envelope"].Value);
            }
            if (productProductsXml.Attributes["printDp2Product"] != null)
            {
                productElement.SetAttribute("printDp2Product", productProductsXml.Attributes["printDp2Product"].Value);
            }
            if (productProductsXml.Attributes["theme"] != null)
            {
                productElement.SetAttribute("theme", productProductsXml.Attributes["theme"].Value);
            }
            if (productProductsXml.Attributes["useTemplatePricing"] != null)
            {
                productElement.SetAttribute("useTemplatePricing", productProductsXml.Attributes["useTemplatePricing"].Value);
            }

            SetProductClass(productElement, productProductsXml);

            doc.AppendChild(productElement);

            XmlElement optionsElement = doc.CreateElement("options");
            productElement.AppendChild(optionsElement);

            XmlElement optionElementPaper = doc.CreateElement("option");
            optionElementPaper.SetAttribute("name", "PaperType");
            optionElementPaper.SetAttribute("min", "1");
            optionElementPaper.SetAttribute("max", "1");
            optionsElement.AppendChild(optionElementPaper);

            // get paper nodes for Product in Products.xml
            var paperNodesProductsXml = productProductsXml.SelectNodes("Paper");

            foreach (XmlNode paperNodeProductsXml in paperNodesProductsXml)
            {
                XmlElement valueElement = doc.CreateElement("value");
                valueElement.SetAttribute("id", paperNodeProductsXml.Attributes["id"].Value);
                valueElement.SetAttribute("name", paperNodeProductsXml.Attributes["name"].Value);
                if ((paperNodeProductsXml.Attributes["layout"] != null) && (productProductsXml.SelectNodes("Template").Count == 0))
                {
                    valueElement.SetAttribute("dp2Product", paperNodeProductsXml.Attributes["layout"].Value);
                }
                if (paperNodeProductsXml.Attributes["iccprofile"] != null)
                {
                    valueElement.SetAttribute("iccprofile", paperNodeProductsXml.Attributes["iccprofile"].Value);
                }
                if (paperNodeProductsXml.Attributes["papersurface"] != null)
                {
                    valueElement.SetAttribute("papersurface", paperNodeProductsXml.Attributes["papersurface"].Value);
                }
                if (paperNodeProductsXml.Attributes["queuename"] != null)
                {
                    valueElement.SetAttribute("queuename", paperNodeProductsXml.Attributes["queuename"].Value);
                }
                if (paperNodeProductsXml.Attributes["dp2FrontierPrintCode"] != null)
                {
                    valueElement.SetAttribute("dp2FrontierPrintCode", paperNodeProductsXml.Attributes["dp2FrontierPrintCode"].Value);
                }
                if (paperNodeProductsXml.Attributes["jdfhp"] != null)
                {
                    valueElement.SetAttribute("jdfhp", paperNodeProductsXml.Attributes["jdfhp"].Value);
                }
                if (paperNodeProductsXml.Attributes["jdfxerox"] != null)
                {
                    valueElement.SetAttribute("jdfxerox", paperNodeProductsXml.Attributes["jdfxerox"].Value);
                }
                if (paperNodeProductsXml.Attributes["jdfpapercolor"] != null)
                {
                    valueElement.SetAttribute("jdfpapercolor", paperNodeProductsXml.Attributes["jdfpapercolor"].Value);
                }
                optionElementPaper.AppendChild(valueElement);

                // get ProductCode node from Paper node in Products.xml
                var productCodeNodeProductsXml = paperNodeProductsXml.SelectSingleNode("ProductCode");

                // for prodId "4Wal", proQtyMultiplier is 4 for ESurface and 1 for Fuji Pearl
                // this was represented by a single node Product/ProQtyMultipierForPricing in original Product XMLs
                XmlElement productCodeElement = doc.CreateElement("productcode");
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
                XmlElement optionElementTemplate = doc.CreateElement("option");
                optionElementTemplate.SetAttribute("name", "Template");
                //TODO: shouldn't 1 template be required? FunPak.xml has min=0 and max=0
                optionElementTemplate.SetAttribute("min", "1");
                optionElementTemplate.SetAttribute("max", "1");
                optionsElement.AppendChild(optionElementTemplate);

                foreach (XmlNode templateNodeProductsXml in templateNodesProductsXml)
                {
                    XmlElement valueElementTemplate = doc.CreateElement("value");
                    valueElementTemplate.SetAttribute("name", templateNodeProductsXml.Attributes["id"].Value);
                    valueElementTemplate.SetAttribute("description", templateNodeProductsXml.Attributes["description"].Value);
                    // remove the file extension from the image attribute. some images contain '.' in their name.
                    valueElementTemplate.SetAttribute("dp2Product", templateNodeProductsXml.Attributes["image"].Value.Remove(templateNodeProductsXml.Attributes["image"].Value.LastIndexOf('.')));
                    if (templateNodeProductsXml.Attributes["image2"] != null)
                    {
                        valueElementTemplate.SetAttribute("image2", templateNodeProductsXml.Attributes["image2"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["dp2CreatePdfGroup"] != null)
                    {
                        valueElementTemplate.SetAttribute("dp2CreatePdfGroup", templateNodeProductsXml.Attributes["dp2CreatePdfGroup"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["separateOrderItems"] != null)
                    {
                        valueElementTemplate.SetAttribute("separateOrderItems", templateNodeProductsXml.Attributes["separateOrderItems"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["class"] != null)
                    {
                        valueElementTemplate.SetAttribute("class", templateNodeProductsXml.Attributes["class"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["useFirstOrderItemImage"] != null)
                    {
                        valueElementTemplate.SetAttribute("useFirstOrderItemImage", templateNodeProductsXml.Attributes["useFirstOrderItemImage"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["idCardPrinterDuplexed"] != null)
                    {
                        valueElementTemplate.SetAttribute("idCardPrinterDuplexed", templateNodeProductsXml.Attributes["idCardPrinterDuplexed"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["routeIndicator"] != null)
                    {
                        valueElementTemplate.SetAttribute("routeIndicator", templateNodeProductsXml.Attributes["routeIndicator"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["dymoProduct"] != null)
                    {
                        valueElementTemplate.SetAttribute("dymoProduct", templateNodeProductsXml.Attributes["dymoProduct"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["isSupplyItem"] != null)
                    {
                        valueElementTemplate.SetAttribute("isSupplyItem", templateNodeProductsXml.Attributes["isSupplyItem"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["numberOfDp2ImageNodesToRepeat"] != null)
                    {
                        valueElementTemplate.SetAttribute("numberOfDp2ImageNodesToRepeat", templateNodeProductsXml.Attributes["numberOfDp2ImageNodesToRepeat"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["backLayout"] != null)
                    {
                        valueElementTemplate.SetAttribute("backLayout", templateNodeProductsXml.Attributes["backLayout"].Value);
                    }
                    if (templateNodeProductsXml.Attributes["customStartDate"] != null)
                    {
                        valueElementTemplate.SetAttribute("customStartDate", templateNodeProductsXml.Attributes["customStartDate"].Value);
                    }
                    optionElementTemplate.AppendChild(valueElementTemplate);

                    // some text nodes are defined by nodes in Products.xml, and others are defined by attributes in the Template node
                    if (templateNodeProductsXml.SelectSingleNode("TextNodes") != null)
                    {
                        XmlElement textNodesElement = doc.CreateElement("textnodes");
                        foreach (XmlNode textNodeProductsXml in templateNodeProductsXml.SelectNodes("TextNodes/TextNode"))
                        {
                            XmlElement textNodeElement = doc.CreateElement("textnode");
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
                        valueElementTemplate.AppendChild(textNodesElement);
                    }
                    else if ((templateNodeProductsXml.Attributes["textNodes"] != null) && (Convert.ToInt32(templateNodeProductsXml.Attributes["textNodes"].Value) > 0))
                    {
                        XmlElement textNodesElement = doc.CreateElement("textnodes");
                        var textNodesCount = Convert.ToInt32(templateNodeProductsXml.Attributes["textNodes"].Value);
                        for (int i = 1; i <= textNodesCount; i++)
                        {
                            XmlElement textNodeElement = doc.CreateElement("textnode");
                            textNodeElement.SetAttribute("textLine", i.ToString());
                            var textNodeLimitAttributeName = string.Format("textNodeLimit{0}", i);
                            if (templateNodeProductsXml.Attributes[textNodeLimitAttributeName] != null)
                            {
                                textNodeElement.SetAttribute("limit", templateNodeProductsXml.Attributes[textNodeLimitAttributeName].Value);
                            }
                            textNodesElement.AppendChild(textNodeElement);
                        }
                        valueElementTemplate.AppendChild(textNodesElement);
                    }
                }
            }

            // printer name and queue if defined on Products.xml product
            if ((productProductsXml.Attributes["printer"] != null) || (productProductsXml.Attributes["queue"] != null))
            {
                var additionalDetailElement = doc.CreateElement("additionaldetail");
                if (productProductsXml.Attributes["printer"] != null)
                {
                    additionalDetailElement.SetAttribute("exportprintername", productProductsXml.Attributes["printer"].Value);
                }
                if (productProductsXml.Attributes["queue"] != null)
                {
                    additionalDetailElement.SetAttribute("exportqueuename", productProductsXml.Attributes["queue"].Value);
                }
                productElement.AppendChild(additionalDetailElement);
            }

            // add any Supply Sheet information from Products.xml
            var xPathProductAssemblySupplies = string.Format("/ProductFile/ProductAssemblySupplies/ProductAssemblySupply[@name = '{0}']", productId);
            if (ProductsXmlDocument.SelectNodes(xPathProductAssemblySupplies).Count > 0)
            {
                // some products in Products.xml have multiple ProductAssemblySupply entries that depend on PaperType
                XmlElement supplySheetElement = doc.CreateElement("supplysheet");

                var productAssemblySuppliesProductsXml = ProductsXmlDocument.SelectNodes(xPathProductAssemblySupplies);
                foreach (XmlNode productAssemblySupplyProductsXml in productAssemblySuppliesProductsXml)
                {

                    // attribute values of the ProductAssemblySupply node are used in MakeTask, ex. assemblyType
                    foreach (XmlAttribute supplyAttributeProductsXml in productAssemblySupplyProductsXml.Attributes)
                    {
                        // type attribute value is always "ProdId"
                        // add paperType (if present) to the value node rather than supplysheet node since there can be multiple paper types
                        if ((supplyAttributeProductsXml.Name != "type") && (supplyAttributeProductsXml.Name != "paperType"))
                        {
                            supplySheetElement.SetAttribute(supplyAttributeProductsXml.Name, supplyAttributeProductsXml.Value);
                        }
                    }

                    // add values from ProductAssemblySupply/AssemblySupply nodes in Products.xml
                    foreach (XmlNode assemblySupplyProductsXml in productAssemblySupplyProductsXml.SelectNodes("AssemblySupply"))
                    {
                        XmlElement supplySheetValue = doc.CreateElement("value");
                        supplySheetValue.SetAttribute("name", assemblySupplyProductsXml.Attributes["name"].Value);
                        if (productAssemblySupplyProductsXml.Attributes["paperType"] != null)
                        {
                            supplySheetValue.SetAttribute("paperType", productAssemblySupplyProductsXml.Attributes["paperType"].Value);
                        }
                        supplySheetValue.SetAttribute("quantity", assemblySupplyProductsXml.Attributes["quantity"].Value);
                        supplySheetValue.SetAttribute("perImagesOrdered", assemblySupplyProductsXml.Attributes["perImagesOrdered"].Value);
                        supplySheetElement.AppendChild(supplySheetValue);
                    }
                }
                productElement.AppendChild(supplySheetElement);
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

        public static void SetProductClass(XmlElement productElement, XmlNode productProductsXml)
        {
            // logic for determining class is very strange due to inconsistencies in Products.xml
            // product classes with similar behaviors can be determined later;
            // only some analysis has been done at this point to determine products that have similar behaviors
            if ((productProductsXml.Attributes["category"].Value.Equals("Prints", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_10x13V2012", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductPrint");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Lenticular", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductLenticular");
            }
            else if ((productProductsXml.Attributes["routeIndicator"] != null) && productProductsXml.Attributes["routeIndicator"].Value.Equals("CD", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductCD");
            }
            else if (productProductsXml.Attributes["prodId"].Value.Equals("DigitalDownloadInsert", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductDigitalDownload");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Ornament", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["category"].Value.Equals("Statuette", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Statuette", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductEtcher");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Jewelry", StringComparison.OrdinalIgnoreCase) ||
                productProductsXml.Attributes["prodId"].Value.Equals("6266_BottleCap_Necklace", StringComparison.OrdinalIgnoreCase) ||
                productProductsXml.Attributes["prodId"].Value.Equals("DeluxeKeychainPhoto", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductJewelry");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Retouch", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductRetouch");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Mounts", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMount");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Labels", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductLabel");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Bookmarks", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductBookmark");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Stickers", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductSticker");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Notebook Cover", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductNotebookCover");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Notepad", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Notepad", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductNotepad");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Banners", StringComparison.OrdinalIgnoreCase) ||
                (productProductsXml.SelectSingleNode("Template[@dymoProduct = 'Dymo_Banner']") != null))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductBanner");
            }
            else if (productProductsXml.Attributes["prodId"].Value.Equals("Principal_Book_SBS", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductBook");
            }
            else if (productProductsXml.Attributes["category"].Value.Equals("Panoramas", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductPanorama");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Calendars", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ind 8x10 Styled Cal H", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductCalendar");
            }
            else if (!productProductsXml.Attributes["category"].Value.Equals("Lenticular", StringComparison.OrdinalIgnoreCase) &&
                (productProductsXml.Attributes["routeIndicator"] != null &&
                    (productProductsXml.Attributes["routeIndicator"].Value.Equals("BagTag", StringComparison.OrdinalIgnoreCase) ||
                     productProductsXml.Attributes["routeIndicator"].Value.Equals("KeyFob", StringComparison.OrdinalIgnoreCase)
                    )
                ))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductPVC");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Borders", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["category"].Value.Equals("Event Borders", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Border", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Personality_8x10H_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("GalleryMat_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SignatureMat_StudioBkgd", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductBorder");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Programs", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["name"].Value.Equals("Spring Green Screen", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductProgram");
            }
            else if (((productProductsXml.Attributes["dymoProduct"] != null) &&
                    (productProductsXml.Attributes["dymoProduct"].Value.Equals("Dymo_DyeSub", StringComparison.OrdinalIgnoreCase) ||
                    productProductsXml.Attributes["dymoProduct"].Value.Equals("Dymo_DyeSub_Plaque", StringComparison.OrdinalIgnoreCase))) ||
                (productProductsXml.SelectSingleNode("Template[@dymoProduct = 'Dymo_DyeSub' or @dymoProduct = 'Dymo_DyeSub_Mug' or @dymoProduct = 'Dymo_DyeSub_Plaque' or @dymoProduct = 'Dymo_DyeSub_DogTag']") != null) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("WalletBorderV", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Frame_", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductDyeSub");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Memory Mates", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("MemoryMate", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_MemoryMate2012", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SE_07_T039b80H", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SE_07_T039b13H_FP", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMemoryMate");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Magnets", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Magnet", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMagnet");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Buttons", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Button", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_Button2012", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductButton");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Fun Paks", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["category"].Value.Equals("Fun Pak", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_FunPak2012", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ind Styled Fun Pak", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductFunPak");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Mirrors", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Mirror", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMirror");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Trading Cards", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_TraderCard2012", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("TraderCard", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductTradingCard");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Window Decals", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductWindowDecal");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Wall Cling", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("WallCling", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_WallCling2012", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductWallCling");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Water Bottles", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("SportsBottle", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Waterbottle", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductWaterBottle");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Dry Erase", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("DryEraseBoard", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_DryErase2012", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductDryEraseBoard");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMagnet_4x10_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("LockerMag_STYLE", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMagnet_4x10_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMag_4x10_GeometrixProKO", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryErase_4x10_GeometrixSBS_DK", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryErase_4x10_GeometrixSBS_LT", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMagnet_10x4_Black_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMagnet_10x4_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("DryEraseMagnet_4x10_CP", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductDryEraseMagnet");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("1Keychain(Acrylic)", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("KeychainLarge(Acrylic)", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductKeychain");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_Trader_Mgnt_2012", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.StartsWith("Trader_Magnet", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("TraderMagnet_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("WalletMagnet_4_StudioBkgd", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductTraderMagnet");
            }
            else if ((productProductsXml.Attributes["category"].Value.Equals("Sports Passes", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SportsPass_CP", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SportsPass_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("SportsPass_ImpactPro", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductSportsPass");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("Tickets_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Tickets", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ticket_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ticket_CP", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ticket_ImpactPro", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductTicket");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("18483_Greyline_TicketMagnet2012", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("TicketMagnet_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Tickets_Magnet_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("TicketMagnet_CP", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ticket_Magnet_ImpactPro", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductTicketMagnet");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("Magazine_Cover_8x10_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_8x10_CP", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_School_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_Sports_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_8x10V_GeometrixProKO", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("MagazineMag_8x10V_GeometrixSBS_DK", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("8x10MagazineCoverNoTxt", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("8x10MagazineCover", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_8x10V_GeometrixSBS_LT", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMagazineCover");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("Magazine_Cover_Magnet_4x5_FusionPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_Magnet_SBS", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_Magnet_4x5_CP", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_SC_Magnet_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_SP_Magnet_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("MagazineMag_4x5V_GeometrixProKO", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("MagazineMag_4x5V_GeometrixSBS_DK", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_4x5V_GeometrixSBS_LT", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Magazine_4x5V_GeometrixSBS_LT", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductMagazineCoverMagnet");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("Calendar_MsgMagnet_ImpactPro", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("Ind 8x10 Styled Cal Magnet H", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductCalendarMagnet");
            }
            else if ((productProductsXml.Attributes["prodId"].Value.Equals("10x13GroupImageOnly", StringComparison.OrdinalIgnoreCase)) ||
                (productProductsXml.Attributes["prodId"].Value.Equals("16x20GroupImageOnly", StringComparison.OrdinalIgnoreCase)))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductGroupImage");
            }
            else if (productProductsXml.Attributes["prodId"].Value.Equals("HostReorderSheet", StringComparison.OrdinalIgnoreCase))
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.ProductPackageInsert");
            }
            else
            {
                productElement.SetAttribute("class", "HHColorLab.Genesis.WebService.ProductXml.SSE.Other");
                ErrorText.AppendLine(string.Format("Warning: No class identified for product id: {0}, product name: {1}, product category: {2}", 
                    productProductsXml.Attributes["prodId"].Value, productProductsXml.Attributes["name"].Value, productProductsXml.Attributes["category"].Value));
                WarningCount++;
            }
        }

        public static string RemoveInvalidCharactersFromFileName(string fileName)
        {
            return fileName.Replace("/", "");
        }
    }
}

