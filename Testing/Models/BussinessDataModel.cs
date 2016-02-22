using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Testing.Models
{
    public class BussinessDataModel
    {
        public int BusinessID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public int BusinessTypeID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<int> insuser { get; set; }
        public Nullable<System.DateTime> insdt { get; set; }
        public Nullable<int> upduser { get; set; }
        public Nullable<System.DateTime> upddt { get; set; }

        public int BusinessAddressID { get; set; }
        public string BusinessAddressTitle { get; set; }
        public string BusinessAddressLine1 { get; set; }
        public string BusinessAddressLine2 { get; set; }
        public string BusinessAddressLine3 { get; set; }
        public string BusinessAddressCity { get; set; }
        public string BusinessAddressState { get; set; }
        public string BusinessAddressZipCode { get; set; }
        public Nullable<int> BusinessAddressCountryID { get; set; }
        public Nullable<int> BusinessAddressAddressTypeID { get; set; }

        public int BusinessEmailID { get; set; }
        public string BusinessEmailAddress { get; set; }
        public Nullable<int> BusinessEmailTypeID { get; set; }

        public int BusinessPhoneID { get; set; }
        public Nullable<int> BusinessPhoneCountryID { get; set; }
        public string BusinessPhoneNumber { get; set; }
        public string BusinessPhoneExt { get; set; }
        public Nullable<int> BusinessPhoneTypeID { get; set; }

        public string Description { get; set; }

        public int UserId { get; set; }
    }
}
