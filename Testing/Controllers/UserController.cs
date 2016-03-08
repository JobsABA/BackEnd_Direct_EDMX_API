using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDMX;
using System.Transactions;
using System.IO;
using System.Configuration;
using System.Text;
using Testing.Models;

namespace Testing.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }
        JobsInABAEntities db = new JobsInABAEntities();
        //
        // GET: /User/
        public JsonResult GetUserList(string username, string email, string number, string location, DateTime? datefrom, DateTime? dateTo)
        {
            var list = db.Users.Where(x => x.IsDeleted == false).ToList();

            if (!string.IsNullOrEmpty(username))
                list = list.Where(x => x.UserName != null && x.UserName.ToLower().Contains(username.ToLower())).ToList();

            if (datefrom.HasValue && dateTo.HasValue)
            {
                list = list.Where(x => x.insdt >= datefrom && x.insdt <= dateTo).ToList();
            }
            if (datefrom.HasValue && !dateTo.HasValue)
            {
                list = list.Where(x => x.insdt >= datefrom).ToList();
            }
            if (!datefrom.HasValue && dateTo.HasValue)
            {
                list = list.Where(x => x.insdt <= dateTo).ToList();
            }



            var record = list.AsEnumerable().Select(x => new
            {
                UserName = x.UserName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                UserID = x.UserID,
                UserEmailAddress = x.UserEmails.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserEmails.Where(c => c.IsPrimary == true).FirstOrDefault().Email != null ? x.UserEmails.Where(c => c.IsPrimary == true).FirstOrDefault().Email.Address : "" : "",
                UserPhoneNumber = x.UserPhones.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserPhones.Where(c => c.IsPrimary == true).FirstOrDefault().Phone != null ? x.UserPhones.Where(c => c.IsPrimary == true).FirstOrDefault().Phone.Number : "" : "",
                UserAddressLine1 = x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address.Line1 : "" : "",
                UserAddressCity = x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address.City : "" : "",
                UserAddressState = x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address.State : "" : "",
                UserAddressZipCode = x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault() != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address != null ? x.UserAddresses.Where(c => c.IsPrimary == true).FirstOrDefault().Address.ZipCode : "" : "",
                insdt = x.insdt,
                IsActive = x.IsActive,
                Description = x.Description
            }).OrderByDescending(x => x.insdt).ToList();

            if (!string.IsNullOrEmpty(email))
                record = record.Where(x => x.UserEmailAddress != null && x.UserEmailAddress.ToLower().Contains(email.ToLower())).ToList();

            if (!string.IsNullOrEmpty(number))
                record = record.Where(x => x.UserPhoneNumber != null && x.UserPhoneNumber.ToLower().Contains(number.ToLower())).ToList();

            if (!string.IsNullOrEmpty(location))
                record = record.Where(x => x.UserAddressState != null && x.UserAddressState.ToLower().Contains(location.ToLower())).ToList();

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UserRegister(UserDataModel user, string password)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {


                    User objUser = new User();
                    objUser.UserName = user.UserName;
                    objUser.FirstName = user.FirstName;
                    objUser.MiddleName = user.MiddleName;
                    objUser.LastName = user.LastName;
                    objUser.DOB = user.DOB;
                    objUser.IsActive = true;
                    objUser.IsDeleted = false;
                    objUser.insdt = DateTime.Now;
                    db.Users.Add(objUser);
                    db.SaveChanges();

                    UserAccount objUserAccount = new UserAccount();
                    objUserAccount.UserID = objUser.UserID;
                    objUserAccount.UserName = user.UserName;
                    objUserAccount.Password = System.Text.Encoding.ASCII.GetBytes(password);//user.UserAccountPassword;
                    objUserAccount.IsActive = true;
                    objUserAccount.IsDeleted = false;
                    objUserAccount.insdt = DateTime.Now;
                    db.UserAccounts.Add(objUserAccount);
                    db.SaveChanges();

                    UserRole objRole = new UserRole();
                    objRole.RoleID = 1;
                    objRole.UserID = objUser.UserID;
                    db.UserRoles.Add(objRole);
                    db.SaveChanges();

                    if (!string.IsNullOrEmpty(user.UserAddressLine1) || !string.IsNullOrEmpty(user.UserAddressCity) || !string.IsNullOrEmpty(user.UserAddressState) || !string.IsNullOrEmpty(user.UserAddressZipCode))
                    {
                        Address objAdd = new Address();
                        objAdd.Line1 = user.UserAddressLine1;
                        objAdd.City = user.UserAddressCity;
                        objAdd.State = user.UserAddressState;
                        objAdd.ZipCode = user.UserAddressZipCode;
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
                            objPhone.AddressbookID = objUserAdd.AddressID;
                            objPhone.Number = user.UserPhoneNumber;
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
                    res["userId"] = objUser.UserID;
                    res["userName"] = objUser.UserName;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SignIn(string username, string password)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (var db = new JobsInABAEntities())
                {
                    byte[] passwordInBytes = Encoding.ASCII.GetBytes(password);
                    var objUser = db.UserAccounts.Where(x => x.UserName != null && x.UserName.ToLower() == username.ToLower() && x.Password == passwordInBytes).FirstOrDefault();
                    if (objUser != null)
                    {
                        res["success"] = 1;
                        res["userId"] = objUser.UserID;
                        res["userName"] = objUser.UserName;
                        res["Name"] = objUser.User.FirstName;
                    }
                    else
                    {
                        res["error"] = 1;
                        res["message"] = "Error in signIn";
                    }
                }
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in signIn";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UserProfileDetail(int userid)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {

                int totalUser = db.Users.Where(x => x.UserID == userid).Count();
                if (totalUser > 0)
                {
                    var objUser = db.Users.Where(x => x.UserID == userid).Select(z => new
                    {
                        FirstName = z.FirstName,
                        LastName = z.LastName,
                        MiddleName = z.MiddleName,
                        Description = z.Description,
                        UserName = z.UserName,

                    }).FirstOrDefault();

                    int userPhoneID = db.UserPhones.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault() != null ? db.UserPhones.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault().PhoneID : 0;
                    var objPhone = db.Phones.Where(x => x.PhoneID == userPhoneID).FirstOrDefault() != null ? db.Phones.Where(x => x.PhoneID == userPhoneID).FirstOrDefault().Number : "";

                    int userEmailID = db.UserEmails.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault() != null ? db.UserEmails.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault().EmailID : 0;
                    var objEmail = db.Emails.Where(x => x.EmailID == userEmailID).FirstOrDefault() != null ? db.Emails.Where(x => x.EmailID == userEmailID).FirstOrDefault().Address : "";

                    int userAddressID = db.UserAddresses.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault() != null ? db.UserAddresses.Where(z => z.UserID == userid && z.IsPrimary == true).FirstOrDefault().AddressID : 0;
                    var objAddress = db.Addresses.Where(x => x.AddressID == userAddressID).Select(c => new
                    {
                        Line1 = c.Line1,
                        City = c.City,
                        State = c.State,
                        ZipCode = c.ZipCode
                    }).FirstOrDefault();

                    int userImagesID = db.UserImages.Where(x => x.IsPrimary == true && x.UserID == userid).FirstOrDefault() != null ? db.UserImages.Where(x => x.IsPrimary == true && x.UserID == userid).FirstOrDefault().ImageID : 0;
                    var objImages = db.Images.Where(x => x.ImageID == userImagesID).Select(z => new
                    {
                        Name = z.Name,
                        ImageExtension = z.ImageExtension,
                        ImageTypeID = z.ImageTypeID
                    }).FirstOrDefault();



                    var result = new { userInfo = objUser, userPhone = objPhone, userEmail = objEmail, userAddress = objAddress, userImage = objImages };
                    res["ObjUser"] = result;
                }
                res["success"] = 1;

            }
            catch (Exception ex)
            {

            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateProfile(UserDataModel user, string updateType)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {

                    if (updateType == "Description" || updateType == "All")
                    {
                        var objUser = db.Users.Where(x => x.UserID == user.UserID).FirstOrDefault();
                        objUser.Description = user.Description;
                        objUser.upddt = DateTime.Now;
                        db.SaveChanges();
                    }
                    if (updateType == "Name" || updateType == "All")
                    {
                        var objUser = db.Users.Where(x => x.UserID == user.UserID).FirstOrDefault();
                        objUser.UserName = user.UserName;
                        objUser.FirstName = user.FirstName;
                        objUser.MiddleName = user.MiddleName;
                        objUser.LastName = user.LastName;
                        objUser.upddt = DateTime.Now;
                        db.SaveChanges();
                    }

                    if (updateType == "Address" || updateType == "All")
                    {
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
                    }

                    if (updateType == "Phone" || updateType == "All")
                    {
                        var userPhoneId = db.UserPhones.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault() != null ? db.UserPhones.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault().PhoneID : 0;
                        var objPhone = db.Phones.Where(x => x.PhoneID == userPhoneId).FirstOrDefault();
                        if (objPhone != null)
                        {
                            objPhone.Number = user.UserPhoneNumber;
                            db.SaveChanges();
                        }
                    }

                    if (updateType == "Email" || updateType == "All")
                    {
                        var userEmailId = db.UserEmails.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault() != null ? db.UserEmails.Where(x => x.UserID == user.UserID && x.IsPrimary == true).FirstOrDefault().EmailID : 0;
                        var objEmail = db.Emails.Where(x => x.EmailID == userEmailId).FirstOrDefault();
                        if (objEmail != null)
                        {
                            objEmail.Address = user.UserEmailAddress;
                            db.SaveChanges();
                        }
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

        public JsonResult DeleteUser(int userID)
        {
            var objUser = db.Users.Where(x => x.UserID == userID).FirstOrDefault();
            if (objUser != null)
            {
                objUser.IsDeleted = true;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddUserawards(Achievement Userachievement)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Achievement oAchivement = new Achievement();
                    oAchivement.Name = Userachievement.Name;
                    oAchivement.UserID = Userachievement.UserID;
                    oAchivement.Date = Userachievement.Date;
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
        public JsonResult GetAwardlistByUserId(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Achievements.AsEnumerable().Where(c => c.UserID == userID).Select(x => new
                {
                    UserID = x.UserID,
                    Name = x.Name,
                    AchievementID = x.AchievementID,
                    Date = x.Date
                }).ToList();
                res["success"] = 1;
                res["Achievelist"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching UserAward list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAwards(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var oAcheivement = db.Achievements.AsEnumerable().Where(x => x.AchievementID == id).FirstOrDefault();
                if (oAcheivement != null)
                {
                    db.Achievements.Remove(oAcheivement);
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
        public JsonResult UpdateAwards(Achievement achievement)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Achievement oAchievement = db.Achievements.Where(x => x.AchievementID == achievement.AchievementID).FirstOrDefault();
                    if (oAchievement != null)
                    {
                        oAchievement.Name = achievement.Name;
                        oAchievement.Date = achievement.Date;
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = oAchievement.AchievementID;
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }



        public JsonResult AddExprienceSet(Experience experience)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Experience oExperience = new Experience();
                    oExperience.BusinessID = experience.BusinessID;
                    oExperience.UserID = experience.UserID;
                    oExperience.StartDate = experience.StartDate;
                    oExperience.EndDate = experience.EndDate;
                    oExperience.JobPossition = experience.JobPossition;
                    oExperience.IsCurrent = false;
                    db.Experiences.Add(oExperience);
                    db.SaveChanges();
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = " Experience Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetExprienceListByUserId(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Experiences.AsEnumerable().Where(c => c.UserID == userID).Select(x => new
                {
                    UserID = x.UserID,
                    BusinessID = x.BusinessID,
                    BusinessName = (x.Business != null) ? x.Business.Name : "",
                    Location = x.Business != null ? x.Business.BusinessAddresses.FirstOrDefault() != null ? x.Business.BusinessAddresses.FirstOrDefault().Address.State : "" : "",
                    JobPossition = x.JobPossition,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ExperienceID = x.ExperienceID
                }).ToList();

                res["success"] = 1;
                res["Expriencelist"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Exprience list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateExprience(Experience experience)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Experience oEducation = db.Experiences.Where(x => x.ExperienceID == experience.ExperienceID).FirstOrDefault();
                    if (oEducation != null)
                    {
                        oEducation.BusinessID = experience.BusinessID;
                        oEducation.JobPossition = experience.JobPossition;
                        oEducation.StartDate = experience.StartDate;
                        oEducation.EndDate = experience.EndDate;
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = "Updated..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteExprience(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var oEducation = db.Experiences.AsEnumerable().Where(x => x.ExperienceID == id).FirstOrDefault();
                if (oEducation != null)
                {
                    db.Experiences.Remove(oEducation);
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

        public JsonResult AddEducationSet(Education education)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Education oEducation = new Education();
                    oEducation.InstituteName = education.InstituteName;
                    oEducation.UserID = education.UserID;
                    oEducation.StartDate = education.StartDate;
                    oEducation.EndDate = education.EndDate;
                    oEducation.Degree = education.Degree;
                    oEducation.IsDelete = false;
                    oEducation.IsActive = false;
                    db.Educations.Add(oEducation);
                    db.SaveChanges();
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = " Education Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetEducationListByUserId(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Educations.AsEnumerable().Where(c => c.UserID == userID && c.IsActive == false && c.IsDelete == false).Select(x => new
                {
                    UserID = x.UserID,
                    EducationID = x.EducationID,
                    InstituteName = x.InstituteName,
                    Degree = x.Degree,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                }).ToList();

                res["success"] = 1;
                res["Educationlist"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching UserAward list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateEducation(Education education)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Education oEducation = db.Educations.Where(x => x.EducationID == education.EducationID).FirstOrDefault();
                    if (oEducation != null)
                    {
                        oEducation.InstituteName = education.InstituteName;
                        oEducation.Degree = education.Degree;
                        oEducation.StartDate = education.StartDate;
                        oEducation.EndDate = education.EndDate;
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = "Updated..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteEducation(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var oEducation = db.Educations.AsEnumerable().Where(x => x.EducationID == id).FirstOrDefault();
                if (oEducation != null)
                {
                    db.Educations.Remove(oEducation);
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



        public JsonResult AddSkill(Skill skill)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Skill oskill = new Skill();
                    oskill.Skill1 = skill.Skill1;
                    oskill.UserID = skill.UserID;
                    db.Skills.Add(oskill);
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
        public JsonResult GetSkilllistByUserId(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Skills.AsEnumerable().Where(c => c.UserID == userID).Select(x => new
                {
                    SkillID = x.SkillID,
                    UserID = x.UserID,
                    Skill1 = x.Skill1,
                }).ToList();

                res["success"] = 1;
                res["Skilllist"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching UserAward list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteSkill(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var oSkill = db.Skills.AsEnumerable().Where(x => x.SkillID == id).FirstOrDefault();
                if (oSkill != null)
                {
                    db.Skills.Remove(oSkill);
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



        public JsonResult AddLanguage(Language language)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    Language olanguage = new Language();
                    olanguage.LanguageName = language.LanguageName;
                    olanguage.UserID = language.UserID;
                    db.Languages.Add(olanguage);
                    db.SaveChanges();
                    transaction.Complete();
                    res["success"] = 1;
                    res["message"] = " Language Added..!";
                }
            }
            catch (Exception ex)
            {
                res["error"] = 2;
                res["message"] = ex.Message.ToString();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLanguageByUserId(int userID)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var record = db.Languages.AsEnumerable().Where(c => c.UserID == userID).Select(x => new
                {
                    UserID = x.UserID,
                    LanguageID = x.LanguageID,
                    LanguageName = x.LanguageName,
                }).ToList();

                res["success"] = 1;
                res["Languagelists"] = record;
            }
            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = "Error in fetching Language list";
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteLanguage(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            try
            {
                var olanguage = db.Languages.AsEnumerable().Where(x => x.LanguageID == id).FirstOrDefault();
                if (olanguage != null)
                {
                    db.Languages.Remove(olanguage);
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
                    var imgPath = ConfigurationManager.AppSettings["UserFileUploadPath"];
                    var _strUserid = Request["userID"].ToString();
                    int userID = Convert.ToInt32(_strUserid);

                    string physicalPath = Server.MapPath(imgPath + userID + Ext);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                    file.SaveAs(physicalPath);


                    string Name = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

                    var imgType = Request["imageTypeId"].ToString();

                    var objuserImage = db.UserImages.Where(x => x.UserID == userID && x.IsPrimary == true).FirstOrDefault();
                    if (objuserImage != null)
                    {
                        Image objImage = db.Images.Where(x => x.ImageID == objuserImage.ImageID).FirstOrDefault();
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

                        UserImage objUserImage = new UserImage();
                        objUserImage.UserID = userID;
                        objUserImage.ImageID = objImage.ImageID;
                        objUserImage.IsPrimary = true;
                        db.UserImages.Add(objUserImage);
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


        //dashboard call
        public JsonResult getDashboardData()
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            res["TotalUser"] = db.Users.Where(x => x.IsDeleted == false).Count();
            res["TotalCompany"] = db.Businesses.Where(x => x.IsDeleted == false).Count();
            res["TotalJob"] = db.Jobs.Where(x => x.IsDeleted == false).Count();
            res["TotalJobApplication"] = db.JobApplications.Count();
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}
