using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IReportFeedbackRepository
    {
        ResponseMessage GetAllReportFeedBack();
        ResponseMessage CreateReportFeedback(int feedbackId,String ReporterEmail,String ReasonReport);
        ResponseMessage ConfirmReport(int reportID);
        ResponseMessage RejectReport(int reportID, String ReasonReject);

    }
}
