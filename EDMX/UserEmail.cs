//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDMX
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserEmail
    {
        public int UserEmailID { get; set; }
        public int UserID { get; set; }
        public int EmailID { get; set; }
        public bool IsPrimary { get; set; }
    
        public virtual Email Email { get; set; }
        public virtual User User { get; set; }
    }
}
