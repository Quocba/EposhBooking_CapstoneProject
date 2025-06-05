using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using Microsoft.EntityFrameworkCore;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class ReportFeedbackRepository : IReportFeedbackRepository
    {
        private readonly DBContext db;
        public ReportFeedbackRepository(DBContext db)
        {
            this.db = db;
        }

        public ResponseMessage CreateReportFeedback(int feedbackId,String ReporterEmail,String ReasonReport)
        {
            var getFeedback = db.feedback.FirstOrDefault(feedback => feedback.FeedBackID == feedbackId);
            if (getFeedback == null)
            {
                return new ResponseMessage { Success = true, Data = getFeedback, Message = "Data not found", StatusCode = (int)HttpStatusCode.OK };
            }

            ReportFeedBack addReport = new ReportFeedBack
            {
                FeedBack = getFeedback,
                ReporterEmail = ReporterEmail,
                ReasonReport = ReasonReport,
                Status = "Awaiting Approval"
            };
            getFeedback.Status = "Reported";
            db.feedback.Update(getFeedback);
            db.reportFeedBack.Add(addReport);
            db.SaveChanges();
            return new ResponseMessage { Success = true, Data = addReport, Message = "Report Successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage GetAllReportFeedBack()
        {
            var report = db.reportFeedBack
                     .Include(feedback => feedback.FeedBack)
                     .Include(account => account.FeedBack.Account)
                     .ThenInclude(profile => profile.Profile)
                     .ToList();
            if (report.Any())
            {   
                return new ResponseMessage { Success = true, Data = report, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = report, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage ConfirmReport(int reportID)
        {
            var getReport = db.reportFeedBack
                              .Include(feedback => feedback.FeedBack)
                              .FirstOrDefault(report => report.ReportID == reportID);
            if (getReport == null)
            {
                return new ResponseMessage { Success = false, Data = getReport, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
            var feedback = db.feedback.FirstOrDefault(feedback => feedback.FeedBackID == getReport.FeedBack.FeedBackID);
            if (feedback == null)
            {
                return new ResponseMessage { Success = false, Data = feedback, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }

            String emailContent = "Your request for a feedback report has been processed.";
            getReport.Status = "Approved";
            feedback.Status = "Hidden";
            db.reportFeedBack.Update(getReport);  
            db.SaveChanges();
            Ultils.Utils.SendMailRegistration(getReport.ReporterEmail, emailContent);
            return new ResponseMessage { Success = true, Data = getReport, Message = emailContent, StatusCode = (int)HttpStatusCode.OK };

        }

        public ResponseMessage RejectReport(int reportID,String ReasonReject)
        {
            var getReport = db.reportFeedBack.FirstOrDefault(report => report.ReportID == reportID);
            if (getReport == null)
            {
                return new ResponseMessage { Success = false, Data = getReport, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
            }

            
            getReport.Status = "Rejected";
            db.reportFeedBack.Update(getReport);
            db.SaveChanges();
            Ultils.Utils.SendMailRegistration(getReport.ReporterEmail, ReasonReject);
            return new ResponseMessage { Success = true, Data = getReport, Message = "Sucessfully", StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
