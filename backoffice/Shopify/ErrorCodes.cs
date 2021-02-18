using System;
using System.Collections.Generic;
using System.Text;

namespace backoffice
{
    /// <summary>
    /// Bitwise return codes that Verify_Product raises to determine which verification a product fails
    /// return code can be used to determine which fix(es) need to be applied.
    /// 
    /// Need this so that we can flag problems and then allow human intervention before fixing
    /// Although some codes we may feel safe just doing automatically
    /// </summary>
    [Flags]
    public enum Product_Fault_Codes
    {
        None = 0b_0000_0000,
        Invalid_Price = 0b_0000_0001,
        Poor_Description = 0b_0000_0010,
        Mismatched_Supplier = 0b_0000_0100,
        No_ETA_Tags = 0b_0000_1000,
        Poor_Title = 0b_0001_0000,
        Product_Taxable = 0b_0010_0000,
        Mismatched_Vendor = 0b_0100_0000,
        No_mbot_Tags = 0b_1000_0000
    }

    /// <summary>
    /// Bitware return codes for Cost issues
    /// First 3 options are more warnings and ordered so that
    /// you can search for anthing higher than No_Cost to find faults
    /// as you probably want to ignore other flags
    /// </summary>
    [Flags]
    public enum Cost_Fault_Codes
    {
        None = 0b_0000_0000,
        Price_More_Than_RRP = 0b_0000_0001,
        RRP_Not_Set = 0b_0000_0010,
        Cost_Not_Set = 0b_0000_0100,
        Price_Less_Than_Cost = 0b_0000_1000,
        Price_Is_Zero = 0b_0001_0000,
        Price_Not_Set = 0b_0010_0000
    }

    /// <summary>
    /// Bitware return codes for Description problems
    /// </summary>
    [Flags]
    public enum Title_Fault_Codes
    {
        None = 0b_0000_0000,
        Too_Long = 0b_0000_0001,
        Invalid_Chars = 0b_0000_0010,
    }

    public class Product_Fault_Code_Details
    {
        public Title_Fault_Codes Title_Fault_Code { get; set; }
        public List<Cost_Fault_Codes> Cost_Fault_Code { get; set; }
    }
}
