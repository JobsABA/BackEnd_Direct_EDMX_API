using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Testing.Models
{
    public class JobDataModel
    {
        public int JobID { get; set; }
        public int BusinessID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<int> JobTypeID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> insuser { get; set; }
        public Nullable<System.DateTime> insdt { get; set; }
        public Nullable<int> upduser { get; set; }
        public Nullable<System.DateTime> upddt { get; set; }

        public int JobApplicationID { get; set; }
        public int ApplicantUserID { get; set; }
        public System.DateTime ApplicationDate { get; set; }

        public int JobApplicationStateID { get; set; }
        public int JobApplicationStatusID { get; set; }
        public DateTime JobApplicationinsdt { get; set; }
        public int JobApplicationinsuser { get; set; }
    }
}
