using EDMX;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Testing.Models;

namespace Testing.Controllers
{
    public class CompanyController : Controller
    {
        //
        // GET: /Company/
        JobsInABAEntities db = new JobsInABAEntities();

        public JsonResult CompanyRegister(BussinessDataModel company)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Business oBusiness = new Business();
                    oBusiness.Name = company.Name;
                    oBusiness.Abbreviation = company.Abbreviation;
                    oBusiness.StartDate = Convert.ToDateTime(company.StartDate);
                    oBusiness.BusinessTypeID = company.BusinessTypeID;
                    oBusiness.IsActive = true;
                    oBusiness.IsDeleted = false;
                    oBusiness.Description = company.Description;
                    oBusiness.upddt = DateTime.Now;
                    db.Businesses.Add(oBusiness);
                    db.SaveChanges();

                    BusinessUserMap oBusinessUserMap = new BusinessUserMap();
                    oBusinessUserMap.BusinessID = oBusiness.BusinessID;
                    oBusinessUserMap.UserID = company.UserId;
                    oBusinessUserMap.IsOwner = true;
                    oBusinessUserMap.BusinessUserTypeID = company.BusinessTypeID;
                    db.BusinessUserMaps.Add(oBusinessUserMap);
                    db.SaveChanges();

                    Address oAddress = new Address();
                    if (!string.IsNullOrEmpty(company.BusinessAddressLine1) || !string.IsNullOrEmpty(company.BusinessAddressCity) || !string.IsNullOrEmpty(company.BusinessAddressState) || !string.IsNullOrEmpty(company.BusinessAddressZipCode))
                    {

                        oAddress.Line1 = company.BusinessAddressLine1;
                        oAddress.City = company.BusinessAddressCity;
                        oAddress.State = company.BusinessAddressState;
                        oAddress.ZipCode = company.BusinessAddressZipCode;
                        db.Addresses.Add(oAddress);
                        db.SaveChanges();

                        BusinessAddress oBusinessAddress = new BusinessAddress();
                        oBusinessAddress.BusinessID = oBusiness.BusinessID;
                        oBusinessAddress.AddressID = oAddress.AddressID;
                        oBusinessAddress.IsPrimary = true;
                        db.BusinessAddresses.Add(oBusinessAddress);
                        db.SaveChanges();
                    }
                    if (!string.IsNullOrEmpty(company.BusinessPhoneNumber))
                    {
                        Phone objPhone = new Phone();
                        if (oAddress != null)
                            objPhone.AddressbookID = oAddress.AddressID;
                        objPhone.Number = company.BusinessPhoneNumber;
                        db.Phones.Add(objPhone);
                        db.SaveChanges();

                        BusinessPhone objBusinessphone = new BusinessPhone();
                        objBusinessphone.BusinessID = oBusiness.BusinessID;
                        objBusinessphone.PhoneID = objPhone.PhoneID;
                        objBusinessphone.IsPrimary = true;
                        db.BusinessPhones.Add(objBusinessphone);
                        db.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(company.BusinessEmailAddress))
                    {
                        Email objEmail = new Email();
                        objEmail.Address = company.BusinessEmailAddress;
                        db.Emails.Add(objEmail);
                        db.SaveChanges();

                        BusinessEmail objBusinessEmail = new BusinessEmail();
                        objBusinessEmail.BusinessID = oBusiness.BusinessID;
                        objBusinessEmail.EmailID = objEmail.EmailID;
                        objBusinessEmail.IsPrimary = true;
                        db.BusinessEmails.Add(objBusinessEmail);
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oBusiness.BusinessID;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateCompany(BussinessDataModel company, string updateType)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Business oBusiness = db.Businesses.Where(x => x.BusinessID == company.BusinessID).FirstOrDefault();
                    if (updateType == "Name" || updateType == "Description")
                    {
                        if (oBusiness != null)
                        {
                            oBusiness.Name = company.Name;
                            oBusiness.Description = company.Description;
                            oBusiness.upddt = DateTime.Now;
                            db.SaveChanges();
                        }
                    }

                    Address oAddress = new Address();
                    int addressID = db.BusinessAddresses.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault() != null ? db.BusinessAddresses.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault().AddressID : 0;
                    oAddress = db.Addresses.Where(x => x.AddressID == addressID).FirstOrDefault();
                    if (updateType == "Address")
                    {
                        if (!string.IsNullOrEmpty(company.BusinessAddressLine1) || !string.IsNullOrEmpty(company.BusinessAddressCity) || !string.IsNullOrEmpty(company.BusinessAddressState) || !string.IsNullOrEmpty(company.BusinessAddressZipCode))
                        {

                            if (oAddress != null)
                            {
                                oAddress.Line1 = company.BusinessAddressLine1;
                                oAddress.City = company.BusinessAddressCity;
                                oAddress.State = company.BusinessAddressState;
                                oAddress.ZipCode = company.BusinessAddressZipCode;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (updateType == "Number")
                    {
                        if (!string.IsNullOrEmpty(company.BusinessPhoneNumber))
                        {
                            int businessPhoneID = db.BusinessPhones.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault() != null ? db.BusinessPhones.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault().PhoneID : 0;

                            Phone objPhone = db.Phones.Where(x => x.PhoneID == businessPhoneID).FirstOrDefault();
                            if (objPhone != null)
                            {
                                if (oAddress != null)
                                    objPhone.AddressbookID = oAddress.AddressID;
                                objPhone.Number = company.BusinessPhoneNumber;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (updateType == "BussinessEmail")
                    {
                        if (!string.IsNullOrEmpty(company.BusinessEmailAddress))
                        {
                            int businessEmailID = db.BusinessEmails.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault() != null ? db.BusinessEmails.Where(x => x.BusinessID == oBusiness.BusinessID).FirstOrDefault().EmailID : 0;

                            Email objEmail = db.Emails.Where(x => x.EmailID == businessEmailID).FirstOrDefault();
                            if (objEmail != null)
                            {
                                objEmail.Address = company.BusinessEmailAddress;
                                db.SaveChanges();
                            }
                        }
                    }

                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oBusiness.BusinessID;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserWiseBusinessList(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var record = (from a in db.BusinessUserMaps.Where(x => x.UserID == userID)
                              join b in db.Businesses
                              on a.BusinessID equals b.BusinessID
                              select new
                              {
                                  BusinessID = b.BusinessID,
                                  Name = b.Name,
                                  Description = b.Description,
                                  ImageExtension = b.BusinessImages.FirstOrDefault() != null ? b.BusinessImages.FirstOrDefault().Image != null ? b.BusinessImages.FirstOrDefault().Image.ImageExtension : "" : ""
                              }).ToList();

                res["success"] = 1;
                res["businessList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetch company list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetBusinessList(string term, string companyName, string City)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var lstBusiness = db.Businesses.Where(x => x.IsDeleted == false).ToList();
                if (!string.IsNullOrEmpty(term))
                    lstBusiness = lstBusiness.Where(x => x.Name != null && x.Name.ToLower().Contains(term.ToLower())).ToList();

                if (!string.IsNullOrEmpty(companyName))
                {
                    lstBusiness = lstBusiness.Where(x => x.Name != null && x.Name.ToLower().Contains(companyName.ToLower())).ToList();
                }

                var record = lstBusiness.AsEnumerable().Select(x => new
                {
                    Name = x.Name,
                    BusinessID = x.BusinessID,
                    Description = x.Description,
                    Email = x.BusinessEmails.FirstOrDefault() != null ? x.BusinessEmails.FirstOrDefault().Email != null ? x.BusinessEmails.FirstOrDefault().Email.Address : "" : "",
                    PhoneNumber = x.BusinessPhones.FirstOrDefault() != null ? x.BusinessPhones.FirstOrDefault().Phone != null ? x.BusinessPhones.FirstOrDefault().Phone.Number : "" : "",
                    ImageExtension = x.BusinessImages.FirstOrDefault() != null ? x.BusinessImages.FirstOrDefault().Image != null ? x.BusinessImages.FirstOrDefault().Image.ImageExtension : "" : "",
                    City = x.BusinessAddresses.FirstOrDefault() != null ? x.BusinessAddresses.FirstOrDefault().Address != null ? x.BusinessAddresses.FirstOrDefault().Address.City : "" : "",
                    StartDate = x.StartDate
                }).ToList();

                if (!string.IsNullOrEmpty(City))
                    record = record.Where(x => x.City != null && x.City.ToLower().Contains(City.ToLower())).ToList();

                res["success"] = 1;
                res["businessList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetch company list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetBussinessDetail(int BussinessId)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                var objBussiness = db.Businesses.Where(x => x.BusinessID == BussinessId).Select(z => new
                {
                    Name = z.Name,
                    Abbreviation = z.Abbreviation,
                    StartDate = z.StartDate,
                    BusinessID = z.BusinessID,
                    Description = z.Description
                }).FirstOrDefault();

                int bussinessPhoneId = db.BusinessPhones.Where(x => x.BusinessID == BussinessId).FirstOrDefault() != null ? db.BusinessPhones.Where(x => x.BusinessID == BussinessId).FirstOrDefault().PhoneID : 0;
                var objPhone = db.Phones.Where(x => x.PhoneID == bussinessPhoneId).Select(z => new
                {
                    Number = z.Number
                }).FirstOrDefault();

                int bussinessEmailId = db.BusinessEmails.Where(x => x.BusinessID == BussinessId).FirstOrDefault() != null ? db.BusinessEmails.Where(x => x.BusinessID == BussinessId).FirstOrDefault().EmailID : 0;
                var objEmail = db.Emails.Where(x => x.EmailID == bussinessEmailId).Select(z => new
                {
                    Address = z.Address
                }).FirstOrDefault();

                int bussinessAddressId = db.BusinessAddresses.Where(x => x.BusinessID == BussinessId).FirstOrDefault() != null ? db.BusinessAddresses.Where(x => x.BusinessID == BussinessId).FirstOrDefault().AddressID : 0;
                var objAddress = db.Addresses.Where(x => x.AddressID == bussinessAddressId).Select(z => new
                {
                    Line1 = z.Line1,
                    City = z.City,
                    State = z.State,
                    ZipCode = z.ZipCode
                }).FirstOrDefault();

                int businessImagesID = db.BusinessImages.Where(x => x.IsPrimary == true && x.BusinessID == BussinessId).FirstOrDefault() != null ? db.BusinessImages.Where(x => x.IsPrimary == true && x.BusinessID == BussinessId).FirstOrDefault().ImageID : 0;
                var objImages = db.Images.Where(x => x.ImageID == businessImagesID).Select(z => new
                {
                    Name = z.Name,
                    ImageExtension = z.ImageExtension,
                    ImageTypeID = z.ImageTypeID
                }).FirstOrDefault();

                var result = new { BussinessInfo = objBussiness, BussinessPhone = objPhone, BussinessEmail = objEmail, BussiessAddress = objAddress, businessImage = objImages };
                res["success"] = 1;
                res["bussinessDetail"] = result;


            }
            catch (Exception ex)
            {

            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Upload()
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];
                var fname = Path.GetFileNameWithoutExtension(file.FileName);
                var Ext = Path.GetExtension(file.FileName);
                string FileName = System.IO.Path.GetFileName(file.FileName);
                if (file.FileName != "" || fileName != null)
                {
                    var imgPath = ConfigurationManager.AppSettings["CompanyFileUploadPath"];
                    var _strBusinessID = Request["businessID"].ToString();
                    int businessID = Convert.ToInt32(_strBusinessID);

                    string physicalPath = Server.MapPath(imgPath + businessID + Ext);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                    file.SaveAs(physicalPath);


                    string Name = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

                    var imgType = Request["imageTypeId"].ToString();

                    var objBusinessImage = db.BusinessImages.Where(x => x.BusinessID == businessID && x.IsPrimary == true).FirstOrDefault();
                    if (objBusinessImage != null)
                    {
                        Image objImage = db.Images.Where(x => x.ImageID == objBusinessImage.ImageID).FirstOrDefault();
                        objImage.Name = FileName;
                        objImage.ImageTypeID = Convert.ToInt32(imgType);
                        objImage.ImageExtension = Ext;
                        db.SaveChanges();
                        res["message"] = objImage.ImageExtension;
                    }
                    else
                    {
                        Image objImage = new Image();
                        objImage.Name = FileName;
                        objImage.ImageTypeID = Convert.ToInt32(imgType);
                        objImage.ImageExtension = Ext;
                        db.Images.Add(objImage);
                        db.SaveChanges();

                        BusinessImage objBusinessImg = new BusinessImage();
                        objBusinessImg.BusinessID = businessID;
                        objBusinessImg.ImageID = objImage.ImageID;
                        objBusinessImg.IsPrimary = true;
                        db.BusinessImages.Add(objBusinessImg);
                        db.SaveChanges();
                        res["message"] = objImage.ImageExtension;
                    }


                    res["success"] = 1;

                    return Json(res, JsonRequestBehavior.AllowGet);

                    //}
                    res["error"] = 0;
                    res["message"] = "Image with same name already Exist";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
            // return RedirectToAction("AddProduct", "Product");
        }
        //Business Employee,BusinessServices,Location,Awards

        #region for service
        public JsonResult CreateServices(Service BusinessServices)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Service oService = new Service();
                    oService.Name = BusinessServices.Name;
                    oService.BusinessID = BusinessServices.BusinessID;
                    db.Services.Add(oService);
                    db.SaveChanges();
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oService.ServiceID;
                }

            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServicesListByBussinessId(int BusinessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Services.AsEnumerable().Where(c => c.BusinessID == BusinessID).Select(x => new
                {
                    BusinessID = x.BusinessID,
                    Name = x.Name,
                    ServiceID = x.ServiceID
                }).ToList();
                res["success"] = 1;
                res["ServicesList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Company Services list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteServices(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var oServies = db.Services.AsEnumerable().Where(x => x.ServiceID == id).FirstOrDefault();
                if (oServies != null)
                {
                    db.Services.Remove(oServies);
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

        #endregion

        #region employee list
        public JsonResult EmployeeRegister(UserDataModel user, int BusinessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {

                    User objUser = new User();
                    objUser.FirstName = user.FirstName;
                    objUser.MiddleName = user.MiddleName;
                    objUser.LastName = user.LastName;
                    objUser.IsActive = true;
                    objUser.IsDeleted = false;
                    objUser.insdt = DateTime.Now;
                    db.Users.Add(objUser);
                    db.SaveChanges();

                    BusinessUserMap oBusinessUserMap = new BusinessUserMap();
                    oBusinessUserMap.BusinessID = BusinessID;
                    oBusinessUserMap.UserID = objUser.UserID;
                    oBusinessUserMap.IsOwner = false;
                    db.BusinessUserMaps.Add(oBusinessUserMap);
                    db.SaveChanges();


                    if (!string.IsNullOrEmpty(user.UserAddressState))
                    {
                        Address objAdd = new Address();
                        objAdd.State = user.UserAddressState;
                        db.Addresses.Add(objAdd);
                        db.SaveChanges();

                        UserAddress objUserAdd = new UserAddress();
                        objUserAdd.UserID = objUser.UserID;
                        objUserAdd.AddressID = objAdd.AddressID;
                        objUserAdd.IsPrimary = true;
                        db.UserAddresses.Add(objUserAdd);
                        db.SaveChanges();


                        if (!string.IsNullOrEmpty(user.UserPhoneNumber))
                        {
                            Phone objPhone = new Phone();
                            objPhone.Number = user.UserPhoneNumber;
                            objPhone.AddressbookID = objUserAdd.AddressID;
                            db.Phones.Add(objPhone);
                            db.SaveChanges();

                            UserPhone objUserphone = new UserPhone();
                            objUserphone.UserID = objUser.UserID;
                            objUserphone.PhoneID = objPhone.PhoneID;
                            objUserphone.IsPrimary = true;
                            db.UserPhones.Add(objUserphone);
                            db.SaveChanges();
                        }

                    }

                    if (!string.IsNullOrEmpty(user.UserEmailAddress))
                    {
                        Email objEmail = new Email();
                        objEmail.Address = user.UserEmailAddress;
                        db.Emails.Add(objEmail);
                        db.SaveChanges();

                        UserEmail objUserEmail = new UserEmail();
                        objUserEmail.UserID = objUser.UserID;
                        objUserEmail.EmailID = objEmail.EmailID;
                        objUserEmail.IsPrimary = true;
                        db.UserEmails.Add(objUserEmail);
                        db.SaveChanges();
                    }

                    transaction.Complete();
                    res["success"] = 1;
                    res["nessage"] = "Employee Added.!";

                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateEmployeeProfile(UserDataModel user)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {

                    User objUser = db.Users.Where(x => x.UserID == user.UserID).FirstOrDefault();
                    objUser.FirstName = user.FirstName;
                    objUser.MiddleName = user.MiddleName;
                    objUser.LastName = user.LastName;
                    objUser.upddt = DateTime.Now;
                    db.SaveChanges();

                    var userAddId = db.UserAddresses.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault() != null ? db.UserAddresses.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault().AddressID : 0;
                    var objAdd = db.Addresses.Where(x => x.AddressID == userAddId).FirstOrDefault();
                    if (objAdd != null)
                    {
                        objAdd.Line1 = user.UserAddressLine1;
                        objAdd.City = user.UserAddressCity;
                        objAdd.State = user.UserAddressState;
                        objAdd.ZipCode = user.UserAddressZipCode;
                        db.SaveChanges();
                    }

                    var userPhoneId = db.UserPhones.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault() != null ? db.UserPhones.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault().PhoneID : 0;
                    var objPhone = db.Phones.Where(x => x.PhoneID == userPhoneId).FirstOrDefault();
                    if (objPhone != null)
                    {
                        objPhone.Number = user.UserPhoneNumber;
                        db.SaveChanges();
                    }

                    var userEmailId = db.UserEmails.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault() != null ? db.UserEmails.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault().EmailID : 0;
                    var objEmail = db.Emails.Where(x => x.EmailID == userEmailId).FirstOrDefault();
                    if (objEmail != null)
                    {
                        objEmail.Address = user.UserEmailAddress;
                        db.SaveChanges();
                    }

                    transaction.Complete();
                    res["success"] = 1;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEmployeeListByBussinessId(int BusinessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = (from a in db.BusinessUserMaps
                              join b in db.Users on a.UserID equals b.UserID
                              where a.BusinessID == BusinessID && a.IsOwner != true && b.IsDeleted == false
                              select new
                              {
                                  UserID = b.UserID,
                                  UserName = b.UserName,
                                  FirstName = b.FirstName,
                                  LastName = b.LastName,
                                  MiddleName = b.MiddleName,
                                  State = b.UserAddresses.FirstOrDefault() != null ? b.UserAddresses.FirstOrDefault().Address.State : "",
                                  EmailAddress = b.UserEmails.FirstOrDefault() != null ? b.UserEmails.FirstOrDefault().Email.Address : "",
                                  PhoneNumber = b.UserPhones.FirstOrDefault() != null ? b.UserPhones.FirstOrDefault().Phone.Number : ""
                              }).ToList();


                res["success"] = 1;
                res["employeeList"] = record;

            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Company Services list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteEmployee(int UserID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var objUser = db.Users.Where(x => x.UserID == UserID).FirstOrDefault();
                if (objUser != null)
                {
                    objUser.IsDeleted = true;
                    objUser.upddt = DateTime.Now;
                    db.SaveChanges();
                    res["success"] = 1;
                    res["message"] = "employee deleted successfully";
                }
                else
                {
                    res["error"] = 1;
                    res["message"] = "error in delete employee";
                }

            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region business location

        public JsonResult GetBusinessLocation(int businessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = (from a in db.BusinessAddresses
                              join b in db.Addresses on a.AddressID equals b.AddressID
                              where a.BusinessID == businessID && a.IsPrimary == false
                              select new
                              {
                                  Line1 = b.Line1,
                                  Line2 = b.Line2,
                                  State = b.State,
                                  City = b.City,
                                  ZipCode = b.ZipCode,
                                  AddressID = b.AddressID,
                                  BusinessAddressID = a.BusinessAddressID
                              }).ToList();
                res["success"] = 1;
                res["record"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddBusinessLocation(BussinessDataModel BusinessLocation)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {

                    if (!string.IsNullOrEmpty(BusinessLocation.BusinessAddressLine1) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressCity) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressState) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressZipCode))
                    {
                        Address oAddress = new Address();

                        oAddress.Line1 = BusinessLocation.BusinessAddressLine1;
                        oAddress.Line2 = BusinessLocation.BusinessAddressLine2;
                        oAddress.City = BusinessLocation.BusinessAddressCity;
                        oAddress.State = BusinessLocation.BusinessAddressState;
                        oAddress.ZipCode = BusinessLocation.BusinessAddressZipCode;
                        db.Addresses.Add(oAddress);
                        db.SaveChanges();

                        BusinessAddress oBusinessAddress = new BusinessAddress();
                        oBusinessAddress.BusinessID = BusinessLocation.BusinessID;
                        oBusinessAddress.AddressID = oAddress.AddressID;
                        oBusinessAddress.IsPrimary = false;
                        db.BusinessAddresses.Add(oBusinessAddress);
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = "Location Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateBusinessLocation(BussinessDataModel BusinessLocation)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    if (!string.IsNullOrEmpty(BusinessLocation.BusinessAddressLine1) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressCity) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressState) || !string.IsNullOrEmpty(BusinessLocation.BusinessAddressZipCode))
                    {
                        int businessAddID = db.BusinessAddresses.Where(x => x.BusinessAddressID == BusinessLocation.BusinessAddressID).FirstOrDefault() != null ? db.BusinessAddresses.Where(x => x.BusinessAddressID == BusinessLocation.BusinessAddressID).FirstOrDefault().AddressID : 0;
                        Address oAddress = db.Addresses.Where(x => x.AddressID == businessAddID).FirstOrDefault();
                        if (oAddress != null)
                        {
                            oAddress.Line1 = BusinessLocation.BusinessAddressLine1;
                            oAddress.Line2 = BusinessLocation.BusinessAddressLine2;
                            oAddress.City = BusinessLocation.BusinessAddressCity;
                            oAddress.State = BusinessLocation.BusinessAddressState;
                            oAddress.ZipCode = BusinessLocation.BusinessAddressZipCode;
                            db.SaveChanges();
                            res["success"] = 1;
                        }
                        else
                        {
                            res["error"] = 1;
                        }
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = "Location Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteBusinessLocation(int addressID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    BusinessAddress objBusinessAdd = db.BusinessAddresses.Where(x => x.AddressID == addressID).FirstOrDefault();
                    if (objBusinessAdd != null)
                    {
                        db.BusinessAddresses.Remove(objBusinessAdd);
                        db.SaveChanges();
                    }

                    Address objAdd = db.Addresses.Where(x => x.AddressID == addressID).FirstOrDefault();
                    if (objAdd != null)
                    {
                        db.Addresses.Remove(objAdd);
                        db.SaveChanges();
                    }

                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = "Location Deleted..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region business award list

        public JsonResult GetAwardlistByBussinessId(int BusinessID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Achievements.AsEnumerable().Where(c => c.BusinessID == BusinessID).Select(x => new
                {
                    BusinessID = x.BusinessID,
                    Name = x.Name,
                    AchievementID = x.AchievementID,
                }).ToList();

                res["success"] = 1;
                res["Achievelist"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Company Services list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddBusinessawards(Achievement Businessachievement)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Achievement oAchivement = new Achievement();
                    oAchivement.Name = Businessachievement.Name;
                    oAchivement.Date = Businessachievement.Date;
                    oAchivement.BusinessID = Businessachievement.BusinessID;
                    db.Achievements.Add(oAchivement);
                    db.SaveChanges();
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = " Awards Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateBusinessawards(int achievementID, string Name, DateTime? _Date)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Achievement oAchivement = db.Achievements.Where(x => x.AchievementID == achievementID).FirstOrDefault();
                    if (oAchivement != null)
                    {
                        oAchivement.Name = Name;
                        oAchivement.Date = _Date;
                        db.SaveChanges();
                        res["success"] = 1;
                    }
                    else
                    {
                        res["error"] = 1;
                    }
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteBusinessawards(int achievementID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Achievement oAchivement = db.Achievements.Where(x => x.AchievementID == achievementID).FirstOrDefault();
                    if (oAchivement != null)
                    {
                        db.Achievements.Remove(oAchivement);
                        db.SaveChanges();
                        res["success"] = 1;
                    }
                    else
                    {
                        res["error"] = 1;
                    }
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
