//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LULU_WCF_Service
{
    using System;
    using System.Collections.Generic;
    
    public partial class GPS_Login : Login
    {
        public int GPS_LoginID { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    
        public virtual AtttendedClass AtttendedClass { get; set; }
    }
}
