using EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Testing.Models;

namespace Testing.Controllers
{
    public class JobapplicationController : Controller
    {
        //
        // GET: /Jobapplication/
        JobsInABAEntities db = new JobsInABAEntities();
        public JsonResult CreateJob(JobDataModel Jobapplication)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {


                    Job oJob = new Job();
                    oJob.BusinessID = Jobapplication.BusinessID;
                    oJob.Title = Jobapplication.Title;
                    oJob.Description = Jobapplication.Description;
                    oJob.JobTypeID = Jobapplication.JobTypeID; ;
                    oJob.IsActive = true;
                    oJob.IsDeleted = false;
                    oJob.StartDate = Jobapplication.StartDate;
                    oJob.EndDate = Jobapplication.EndDate;
                    oJob.insdt = DateTime.Now;
                    db.Jobs.Add(oJob);
                    db.SaveChanges();

                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oJob.JobID;

                }

            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateJob(JobDataModel Job)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {


                    Job oJob = db.Jobs.Where(x => x.JobID == Job.JobID).FirstOrDefault();
                    if (oJob != null)
                    {
                        oJob.Title = Job.Title;
                        oJob.Description = Job.Description;
                        oJob.IsActive = true;
                        oJob.IsDeleted = false;
                        oJob.StartDate = Job.StartDate;
                        oJob.EndDate = Job.EndDate;
                        oJob.insdt = DateTime.Now;
                        db.SaveChanges();
                    }

                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oJob.JobID;

                }

            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJobsinABAList(string city, string company, string jobKeyword, int? userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                if (!userID.HasValue) userID = 0;
                var record = db.Jobs.AsEnumerable().Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new
                {
                    Title = x.Title,
                    BusinessID = x.BusinessID,
                    BusinessName = x.Business != null ? x.Business.Name : "",
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Name = db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault() != null ? db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault().Name : "",
                    JobID = x.JobID,
                    ImageExtension = x.Business.BusinessImages.FirstOrDefault() != null ? x.Business.BusinessImages.FirstOrDefault().Image != null ? x.Business.BusinessImages.FirstOrDefault().Image.ImageExtension : "" : "",
                    City = x.Business != null ? x.Business.BusinessAddresses != null ? x.Business.BusinessAddresses.FirstOrDefault() != null ? x.Business.BusinessAddresses.FirstOrDefault().Address != null ? x.Business.BusinessAddresses.FirstOrDefault().Address.City : "" : "" : "" : "",
                    IsBusinessOwner = db.BusinessUserMaps.Where(c => c.UserID == userID && c.BusinessID == x.BusinessID && c.IsOwner == true).Count(),

                    //Location = x.Business.BusinessAddresses.FirstOrDefault().Address.City

                }).ToList();

                if (!string.IsNullOrEmpty(jobKeyword))
                    record = record.Where(x => x.Title != null && x.Title.ToLower().Contains(jobKeyword.ToLower())).ToList();

                if (!string.IsNullOrEmpty(city))
                    record = record.Where(x => x.City != null && x.City.ToLower().Contains(city.ToLower())).ToList();

                if (!string.IsNullOrEmpty(company))
                    record = record.Where(x => x.BusinessName != null && x.BusinessName.ToLower().Contains(company.ToLower())).ToList();

                res["success"] = 1;
                res["JobsList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Job list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        //get job list for login user
        //public JsonResult GetJobsinABAListWithOwnerList(int userID)
        //{
        //    Dictionary<string, object> res = new Dictionary<string, object>();
        //    try
        //    {
        //        var record = db.Jobs.AsEnumerable().Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new
        //        {
        //            Title = x.Title,
        //            BusinessID = x.BusinessID,
        //            Description = x.Description,
        //            StartDate = x.StartDate,
        //            EndDate = x.EndDate,
        //            Name = db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault() != null ? db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault().Name : "",
        //            JobID = x.JobID,
        //            IsBusinessOwner = db.BusinessUserMaps.Where(c => c.UserID == userID && c.BusinessID == x.BusinessID && c.IsOwner == true).Count(),
        //            ImageExtension = x.Business.BusinessImages.FirstOrDefault() != null ? x.Business.BusinessImages.FirstOrDefault().Image != null ? x.Business.BusinessImages.FirstOrDefault().Image.ImageExtension : "" : ""
        //            //Location = x.Business.BusinessAddresses.FirstOrDefault().Address.City

        //        }).ToList();

        //        res["success"] = 1;
        //        res["JobsList"] = record;

        //    }
        //    catch (Exception ex)
        //    {
        //        res["error"] = 1;
        //        res["message"] = "Error in fetching Job list";
        //    }
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetJobsinABAListByBussinessId(int bussinessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var record = db.Jobs.AsEnumerable().Where(c => c.BusinessID == bussinessID && c.IsDeleted == false).Select(x => new
                {
                    Title = x.Title,
                    BusinessID = x.BusinessID,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive,
                    Name = db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault() != null ? db.Businesses.Where(z => z.BusinessID == x.BusinessID).FirstOrDefault().Name : "",
                    JobID = x.JobID,
                    insdt = x.insdt
                    //Location = x.Business.BusinessAddresses.FirstOrDefault().Address.City

                }).ToList();

                res["success"] = 1;
                res["JobsList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Job list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        //change job activation
        public JsonResult ChangeJobDisplay(int jobID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var objJob = db.Jobs.AsEnumerable().Where(c => c.JobID == jobID).FirstOrDefault();
                if (objJob != null)
                {
                    if (objJob.IsActive)
                        objJob.IsActive = false;
                    else
                        objJob.IsActive = true;
                    db.SaveChanges();
                    res["success"] = 1;
                }
                else
                {

                    res["error"] = 1;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Job list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getJobDetailById(int jobID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var objJobDetail = db.Jobs.AsEnumerable().Where(x => x.JobID == jobID).OrderByDescending(x => x.insdt).Select(z => new
                {
                    Title = z.Title,
                    StartDate = z.StartDate,
                    EndDate = z.EndDate,
                    insdt = z.insdt,
                    Description = z.Description,
                    JobID = z.JobID
                }).FirstOrDefault();
                res["success"] = 1;
                res["message"] = objJobDetail;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteJob(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var objJobDetail = db.Jobs.AsEnumerable().Where(x => x.JobID == id).FirstOrDefault();
                if (objJobDetail != null)
                {
                    objJobDetail.IsDeleted = true;
                    db.SaveChanges();
                }
                res["success"] = 1;
                res["message"] = "Delete Successfully";

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckUserBusinessOwner(int bussinessID, int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var objUseBusiness = db.BusinessUserMaps.Where(x => x.UserID == userID && x.BusinessID == bussinessID && x.IsOwner == true).FirstOrDefault();
                if (objUseBusiness != null)
                {
                    res["message"] = true;
                }
                else
                {
                    res["message"] = false;
                }
                res["success"] = 1;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApplyJob(int JobID, int UserID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                JobApplication objJobAplicat1 = db.JobApplications.Where(x => x.JobID == JobID && x.ApplicantUserID == UserID).FirstOrDefault();
                if (objJobAplicat1 != null)
                {
                    res["duplicate"] = 1;
                }
                else
                {
                    JobApplication objJobAplicat = new JobApplication();
                    objJobAplicat.JobID = JobID;
                    objJobAplicat.ApplicantUserID = UserID;
                    objJobAplicat.ApplicationDate = DateTime.Now;
                    db.JobApplications.Add(objJobAplicat);
                    db.SaveChanges();
                    res["success"] = 1;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJobApplicationList(int JobID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var lstJobAplicat = db.JobApplications.AsEnumerable().Where(x => x.JobID == JobID).Select(z => new
                {
                    Title = db.Jobs.Where(c => c.JobID == z.JobID).FirstOrDefault() != null ? db.Jobs.Where(c => c.JobID == z.JobID).FirstOrDefault().Title : "",
                    UserName = db.Users.Where(c => c.UserID == z.ApplicantUserID).FirstOrDefault() != null ? db.Users.Where(c => c.UserID == z.ApplicantUserID).FirstOrDefault().UserName : "",
                    ApplicationDate = z.ApplicationDate
                }).ToList();
                res["success"] = 1;
                res["applicationList"] = lstJobAplicat;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

    }
}
