using GraduationAPI_EPOSHBOOKING.Model;

namespace GraduationAPI_EPOSHBOOKING.IRepository
{
    public interface IBlogRepository
    {
        public ResponseMessage GetAllBlogs();
        public ResponseMessage GetBlogDetailById(int blogId);
        public ResponseMessage GetBlogsByAccountId(int accountId);
        public ResponseMessage CreateBlog(Blog blog, int accountId, List<IFormFile> image);
        public ResponseMessage DeleteBlog(int blogId);
        public ResponseMessage CommentBlog(int blogId, int accountId, string description);
        public ResponseMessage FilterBlogwithStatus(String status);
        public ResponseMessage ConfirmBlog(int blogId);
        public ResponseMessage RejectBlog(int blogId,string reasonReject);

    }
}
