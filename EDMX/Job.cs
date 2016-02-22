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
    
    public partial class Job
    {
        public Job()
        {
            this.JobApplications = new HashSet<JobApplication>();
        }
    
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
    
        public virtual Business Business { get; set; }
        public virtual ICollection<JobApplication> JobApplications { get; set; }
        public virtual TypeCode TypeCode { get; set; }
    }
}
