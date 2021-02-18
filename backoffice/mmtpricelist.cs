using System.Xml.Serialization;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace backoffice
{
    [XmlRoot(ElementName = "PriceListData", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLPriceListData
    {
        [XmlElement(ElementName = "RetrievalDate", Namespace = "https://www.mmt.com.au")]
        public string RetrievalDate { get; set; }
    }

    [XmlRoot(ElementName = "Manufacturer", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLManufacturer
    {
        [XmlElement(ElementName = "ManufacturerCode", Namespace = "https://www.mmt.com.au")]
        public string ManufacturerCode { get; set; }
        [XmlElement(ElementName = "ManufacturerName", Namespace = "https://www.mmt.com.au")]
        public string ManufacturerName { get; set; }
    }

    [XmlRoot(ElementName = "Category", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLCategory
    {
        [XmlElement(ElementName = "CategoryNumber", Namespace = "https://www.mmt.com.au")]
        public string CategoryNumber { get; set; }
        [XmlElement(ElementName = "ParentCategoryName", Namespace = "https://www.mmt.com.au")]
        public string ParentCategoryName { get; set; }
        [XmlElement(ElementName = "CategoryName", Namespace = "https://www.mmt.com.au")]
        public string CategoryName { get; set; }
    }

    [XmlRoot(ElementName = "DotPoints", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLDotPoints
    {
        [XmlElement(ElementName = "Point", Namespace = "https://www.mmt.com.au")]
        public List<string> Point { get; set; }
    }

    [XmlRoot(ElementName = "Description", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLDescription
    {
        [XmlElement(ElementName = "ShortDescription", Namespace = "https://www.mmt.com.au")]
        public string ShortDescription { get; set; }
        [XmlElement(ElementName = "LongDescription", Namespace = "https://www.mmt.com.au")]
        public string LongDescription { get; set; }
        [XmlElement(ElementName = "DotPoints", Namespace = "https://www.mmt.com.au")]
        public MMTXMLDotPoints DotPoints { get; set; }
    }

    [XmlRoot(ElementName = "Pricing", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLPricing
    {
        [XmlElement(ElementName = "YourPrice", Namespace = "https://www.mmt.com.au")]
        public string YourPrice { get; set; }
        [XmlElement(ElementName = "RRPInc", Namespace = "https://www.mmt.com.au")]
        public string RRPInc { get; set; }
    }

    [XmlRoot(ElementName = "Files")]
    public class MMTXMLFiles
    {
        [XmlElement(ElementName = "LargeImageURL")]
        public List<string> LargeImageURL { get; set; }
        [XmlElement(ElementName = "ThumbnailImageURL")]
        public List<string> ThumbnailImageURL { get; set; }
    }

    [XmlRoot(ElementName = "Status", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLStatus
    {
        [XmlElement(ElementName = "StatusName", Namespace = "https://www.mmt.com.au")]
        public string StatusName { get; set; }
    }

    [XmlRoot(ElementName = "Product", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLProduct
    {
        [XmlElement(ElementName = "MMTCode", Namespace = "https://www.mmt.com.au")]
        public string MMTCode { get; set; }
        [XmlElement(ElementName = "Manufacturer", Namespace = "https://www.mmt.com.au")]
        public MMTXMLManufacturer Manufacturer { get; set; }
        [XmlElement(ElementName = "Category", Namespace = "https://www.mmt.com.au")]
        public MMTXMLCategory Category { get; set; }
        [XmlElement(ElementName = "Description", Namespace = "https://www.mmt.com.au")]
        public MMTXMLDescription Description { get; set; }
        [XmlElement(ElementName = "Weight", Namespace = "https://www.mmt.com.au")]
        public string Weight { get; set; }
        [XmlElement(ElementName = "UnitMeasurements", Namespace = "https://www.mmt.com.au")]
        public string UnitMeasurements { get; set; }
        [XmlElement(ElementName = "Availability", Namespace = "https://www.mmt.com.au")]
        public string Availability { get; set; }
        [XmlElement(ElementName = "ETA", Namespace = "https://www.mmt.com.au")]
        public string ETA { get; set; }
        [XmlElement(ElementName = "Pricing", Namespace = "https://www.mmt.com.au")]
        public MMTXMLPricing Pricing { get; set; }
        [XmlElement(ElementName = "Files", Namespace = "https://www.mmt.com.au")]
        public MMTXMLFiles Files { get; set; }
        [XmlElement(ElementName = "Status", Namespace = "https://www.mmt.com.au")]
        public MMTXMLStatus Status { get; set; }
        [XmlElement(ElementName = "Barcode", Namespace = "https://www.mmt.com.au")]
        public string Barcode { get; set; }
    }

    [XmlRoot(ElementName = "Products", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLProducts
    {
        [XmlElement(ElementName = "Product", Namespace = "https://www.mmt.com.au")]
        public List<MMTXMLProduct> Product { get; set; }
    }

    [XmlRoot(ElementName = "MMTPriceList", Namespace = "https://www.mmt.com.au")]
    public class MMTXMLPriceList
    {
        [XmlElement(ElementName = "PriceListData", Namespace = "https://www.mmt.com.au")]
        public MMTXMLPriceListData PriceListData { get; set; }
        [XmlElement(ElementName = "Products", Namespace = "https://www.mmt.com.au")]
        public MMTXMLProducts Products { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }

        public async static Task<MMTXMLPriceList> loadFromURLAsync(string datafeedurl)
        {

            try
            {
                WebClient client = new WebClient();
                client.Headers.Add(HttpRequestHeader.ContentType, "apoplication/xml");
                string response = await client.DownloadStringTaskAsync(datafeedurl);

                XmlSerializer serial = new XmlSerializer(typeof(MMTXMLPriceList));
                return (MMTXMLPriceList)serial.Deserialize(new StringReader(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }

}


/*

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "https://www.mmt.com.au", IsNullable = false)]
public partial class MMTPriceList
{

    private object[] itemsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PriceListData", typeof(MMTPriceListPriceListData))]
    [System.Xml.Serialization.XmlElementAttribute("Products", typeof(MMTPriceListProducts))]
    public object[] Items
    {
        get
        {
            return this.itemsField;
        }
        set
        {
            this.itemsField = value;
        }
    }

    public static MMTPriceList loadFromURL(string datafeedurl)
    {

        try
        {
            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "apoplication/xml");
            string response = client.DownloadString(datafeedurl);

            XmlSerializer serial = new XmlSerializer(typeof(MMTPriceList));
            return (MMTPriceList)serial.Deserialize(new StringReader(response));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async static Task<MMTPriceList> loadFromURLAsync(string datafeedurl)
    {

        try
        {
            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "apoplication/xml");
            string response = await client.DownloadStringTaskAsync(datafeedurl);

            XmlSerializer serial = new XmlSerializer(typeof(MMTPriceList));
            return (MMTPriceList)serial.Deserialize(new StringReader(response));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }


}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListPriceListData
{

    private string retrievalDateField;

    /// <remarks/>
    public string RetrievalDate
    {
        get
        {
            return this.retrievalDateField;
        }
        set
        {
            this.retrievalDateField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProducts
{

    private MMTPriceListProductsProduct[] productField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Product")]
    public MMTPriceListProductsProduct[] Product
    {
        get
        {
            return this.productField;
        }
        set
        {
            this.productField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProduct
{

    private string mMTCodeField;
    private string availabilityField;
    private string ETAField;
    private MMTPriceListProductsProductManufacturer[] manufacturerField;
    private MMTPriceListProductsProductCategory[] categoryField;
    private MMTPriceListProductsProductDescription[] descriptionField;
    private MMTPriceListProductsProductPricing[] pricingField;

    /// <remarks/>
    public string MMTCode
    {
        get
        {
            return this.mMTCodeField;
        }
        set
        {
            this.mMTCodeField = value;
        }
    }

    /// <remarks/>
    public string Availability
    {
        get
        {
            return this.availabilityField;
        }
        set
        {
            this.availabilityField = value;
        }
    }

    /// <remarks/>
    public string ETA
    {
        get
        {
            return this.ETAField;
        }
        set
        {
            this.ETAField = value;
        }
    }

    [System.Xml.Serialization.XmlElementAttribute("Status")]
    public MMTPriceListProductsProductStatus[] Status { get; set; }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Manufacturer")]
    public MMTPriceListProductsProductManufacturer[] Manufacturer
    {
        get
        {
            return this.manufacturerField;
        }
        set
        {
            this.manufacturerField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Category")]
    public MMTPriceListProductsProductCategory[] Category
    {
        get
        {
            return this.categoryField;
        }
        set
        {
            this.categoryField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Description")]
    public MMTPriceListProductsProductDescription[] Description
    {
        get
        {
            return this.descriptionField;
        }
        set
        {
            this.descriptionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Pricing")]
    public MMTPriceListProductsProductPricing[] Pricing
    {
        get
        {
            return this.pricingField;
        }
        set
        {
            this.pricingField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProductManufacturer
{

    private string manufacturerCodeField;

    private string manufacturerNameField;

    /// <remarks/>
    public string ManufacturerCode
    {
        get
        {
            return this.manufacturerCodeField;
        }
        set
        {
            this.manufacturerCodeField = value;
        }
    }

    /// <remarks/>
    public string ManufacturerName
    {
        get
        {
            return this.manufacturerNameField;
        }
        set
        {
            this.manufacturerNameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProductCategory
{

    private string categoryNumberField;

    private string parentCategoryNameField;

    private string categoryNameField;

    /// <remarks/>
    public string CategoryNumber
    {
        get
        {
            return this.categoryNumberField;
        }
        set
        {
            this.categoryNumberField = value;
        }
    }

    /// <remarks/>
    public string ParentCategoryName
    {
        get
        {
            return this.parentCategoryNameField;
        }
        set
        {
            this.parentCategoryNameField = value;
        }
    }

    /// <remarks/>
    public string CategoryName
    {
        get
        {
            return this.categoryNameField;
        }
        set
        {
            this.categoryNameField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProductStatus
{
    private string statusNameField;

    public string StatusName
    {
        get
        {
            return this.statusNameField;
        }
        set
        {
            this.statusNameField = value;
        }
    }
    
}


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProductDescription
{

    private string shortDescriptionField;

    private string longDescriptionField;

    /// <remarks/>
    public string ShortDescription
    {
        get
        {
            return this.shortDescriptionField;
        }
        set
        {
            this.shortDescriptionField = value;
        }
    }

    /// <remarks/>
    public string LongDescription
    {
        get
        {
            return this.longDescriptionField;
        }
        set
        {
            this.longDescriptionField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "https://www.mmt.com.au")]
public partial class MMTPriceListProductsProductPricing
{

    private string yourPriceField;

    private string rRPIncField;

    /// <remarks/>
    public string YourPrice
    {
        get
        {
            return this.yourPriceField;
        }
        set
        {
            this.yourPriceField = value;
        }
    }

    /// <remarks/>
    public string RRPInc
    {
        get
        {
            return this.rRPIncField;
        }
        set
        {
            this.rRPIncField = value;
        }


}
}

*/
