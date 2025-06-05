using GraduationAPI_EPOSHBOOKING.Controllers.Guest;
using GraduationAPI_EPOSHBOOKING.DataAccess;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
#pragma warning disable // tắt cảnh báo để code sạch hơn

namespace GraduationAPI_EPOSHBOOKING.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly DBContext db;
        private readonly IWebHostEnvironment environment;
        private readonly GenericCacheInvalidator<Blog> _blogCacheInvalidator;
        public BlogRepository(DBContext dbContext, IWebHostEnvironment environment, GenericCacheInvalidator<Blog> blogCacheInvalidator)
        {
            db = dbContext;
            this.environment = environment;
            _blogCacheInvalidator = blogCacheInvalidator;
        }


        public ResponseMessage GetAllBlogs()
        {

            try
            {
                var blogs = db.blog
                    .Include(account => account.Account)
                    .ThenInclude(profile => profile.Profile)
                    .Include(b => b.Comment)
                    .Include(b => b.BlogImage)
                    .ToList();
                var result = blogs.Select(blog => new
                {
                    BlogID = blog.BlogID,
                    Title = blog.Title,
                    Description = blog.Description,
                    Location = Ultils.Utils.RemoveVietnameseDiacritics(blog.Location),
                    PublishDate = blog.PublishDate,
                    ReasonReject = blog.ReasonReject,
                    Status = blog.Status,
                    BlogImages = blog.BlogImage.Select(bi => new
                    {
                        bi.ImageID,
                        bi.Image
                    }),
                    Account = new
                    {
                        blog.Account.Profile.fullName,
                        blog.Account.Email
                    },
                    Comments = blog.Comment.Select(comment => new
                    {
                        comment.CommentID,
                        comment.Description,
                        comment.DateComment
                    })

                });
             return new ResponseMessage { Success = true, Message = "Successfully", Data = result, StatusCode = (int)HttpStatusCode.OK };


            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        public ResponseMessage GetBlogDetailById(int blogId)
        {
            try
            {
                var blog = db.blog
                    .Include(b => b.Comment)
                    .ThenInclude(accountComment => accountComment.account)
                    .ThenInclude(profileAccount  => profileAccount.Profile)
                    .Include(account => account.Account)
                    .ThenInclude(profile => profile.Profile)
                    .Include(b => b.BlogImage)
                    .FirstOrDefault(b => b.BlogID == blogId);
                

                if (blog != null)
                {
                    var responseData = new
                    {
                        blog.BlogID,
                        blog.Title,
                        blog.Description,
                        blog.Location,
                        blog.PublishDate,
                        blog.Status,
                        blog.Account,
                        Comments = blog.Comment?.Select(c => new
                        {
                            c.CommentID,
                            c.Description,
                            c.DateComment,
                            Account = new
                            {
                                AccountID = c.account.AccountID,
                                Email = c.account.Email,
                                Phone = c.account.Phone,
                                Profile = new
                                {
                                   FullName = c.account.Profile.fullName,
                                   Avatar = c.account.Profile.Avatar
                                }
                            }
                        }).OrderByDescending(x => x.DateComment).ToList(),
                        BlogImages = blog.BlogImage.Select(img => new
                        {
                            img.ImageID,
                            img.Image
                        }).ToList()
                    };
                    return new ResponseMessage { Success = true, Message = "Successfully retrieved blog details", Data = responseData, StatusCode = (int)HttpStatusCode.OK };
                }
                else
                {
                    return new ResponseMessage { Success = false, Message = "Blog not found",Data = new int[0], StatusCode = (int)HttpStatusCode.NotFound };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = ex.Message, StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        public ResponseMessage GetBlogsByAccountId(int accountId)
        {
            var getBlog = db.blog.Include(img => img.BlogImage)
                .Include(account => account.Account)
                .ThenInclude(profile =>  profile.Profile)
                .Where(blog => blog.Account.AccountID == accountId)
                .ToList();
            var responseData = getBlog.Select(blog => new
            {
                
                    BlogID = blog.BlogID,
                    Title = blog.Title,
                    Description = blog.Description,
                    Location = blog.Location,
                    PublishDate = blog.PublishDate,
                    Status = blog.Status,
                    ReasonReject = blog.ReasonReject,
                    BlogImage = blog.BlogImage.Select(img => new
                    {
                        ImageID = img.ImageID,
                        Image = img.Image
                    }).ToList(),
                    Account = new
                    {
                        Fullname = blog.Account.Profile.fullName,
                        Avatar = blog.Account.Profile.Avatar
                        
                    }
                    
            });

            if (getBlog.Any())
            {
               
                return new ResponseMessage { Success = true, Data = responseData, Message = "Successfully", StatusCode = (int)HttpStatusCode.OK };
            }
            return new ResponseMessage { Success = false, Data = responseData, Message = "Data not found", StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ResponseMessage CreateBlog(Blog blog, int accountId, List<IFormFile> images)
        {
            var account = db.accounts
                            .Include(profile => profile.Profile)
                            .FirstOrDefault(a => a.AccountID == accountId);

            if (account == null)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Data = accountId,
                    Message = "Account not found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            if (images == null || images.Count == 0)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "No images provided",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            Blog addBlog = new Blog
            {
                Title = blog.Title,
                Description = blog.Description,
                Location = blog.Location,
                Status = "Awaiting Approval",
                PublishDate = DateTime.Now,
                Account = account
            };

            db.blog.Add(addBlog);

            foreach (var image in images)
            {
                try
                {
                    if (image != null && environment != null)
                    {
                        BlogImage addImage = new BlogImage
                        {
                            Blog = addBlog,
                            Image = Ultils.Utils.SaveImage(image, environment)
                        };
                        db.blogImage.Add(addImage);
                    }
                    else
                    {
                        return new ResponseMessage
                        {
                            Success = false,
                            Message = "Invalid image or environment settings",
                            StatusCode = (int)HttpStatusCode.InternalServerError
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = $"Exception: {ex.Message}",
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            }

            db.SaveChanges();

            var result = new
            {
                addBlog.BlogID,
                addBlog.Title,
                addBlog.Description,
                addBlog.Location,
                addBlog.Status,
                BlogImages = addBlog.BlogImage.Select(bi => new
                {
                    bi.ImageID,
                    bi.Image
                }),
                Account = new
                {
                    addBlog.Account.AccountID,
                    addBlog.Account.Email,
                    addBlog.Account.Phone,
                    addBlog.Account.Profile.fullName
                }
            };

            return new ResponseMessage
            {
                Success = true,
                Data = result,
                Message = "Blog created successfully",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

            public ResponseMessage CommentBlog(int blogId, int accountId, string description)
        {
            var blog = db.blog
                .FirstOrDefault(b => b.BlogID == blogId);
            var account = db.accounts
                .Include(profile => profile.Profile)
                .FirstOrDefault(a => a.AccountID == accountId);
            if (blog == null)
            {
                return new ResponseMessage { Success = false, Data = blog, Message = "Blog not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
            if (account == null)
            {
                return new ResponseMessage { Success = false, Data = account, Message = "Account not found", StatusCode = (int)HttpStatusCode.NotFound };
            }
            CommentBlog comment = new CommentBlog
            {
                blog = blog,
                account = account,
                Description = description,
                DateComment = DateTime.Now.AddHours(14)
            };
            db.blogComment.Add(comment);
            db.SaveChanges();

            // Remove sensitive data from the response
            comment.account.Password = null;
            comment.account.Phone = null;

            var result = new
            {
                comment.CommentID,
                comment.Description,
                comment.DateComment,
                Blog = new
                {
                    blog.BlogID,
                    blog.BlogImage,
                    blog.Title,
                    blog.Description,
                    blog.Location,
                },
                AccountComment = new
                {
                    comment.account.Email,
                    comment.account.Phone,
                    comment.account.Profile.fullName
                }
            };
            return new ResponseMessage { Success = true, Data = result, Message = "Commented successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage DeleteBlog(int blogId)
        {
            var blog = db.blog.Include(b => b.BlogImage).Include(b => b.Comment).FirstOrDefault(b => b.BlogID == blogId);
            if (blog == null)
            {
                return new ResponseMessage { Success = false, Data = blogId, Message = "Blog not found", StatusCode = (int)HttpStatusCode.NotFound };
            }

            foreach (var comment in blog.Comment)
            {
                db.blogComment.Remove(comment);
            }
            foreach (var image in blog.BlogImage)
            {
                db.blogImage.Remove(image);
            }
            db.blog.Remove(blog);
            db.SaveChanges();

            return new ResponseMessage { Success = true, Data = blogId, Message = "Blog deleted successfully", StatusCode = (int)HttpStatusCode.OK };
        }

        public ResponseMessage FilterBlogwithStatus(string status)
        {
            // Filter status với các giá trị: "Wait for confirm", "Confirmed", "Rejected"
            var blogs = db.blog
                  .Include(b => b.Comment)
                  .Include(b => b.BlogImage)
                  .Where(b => b.Status == status)
                  .ToList();
            if (blogs.Any())
            {
                return new ResponseMessage { Success = true, Message = "Successfully", Data = blogs, StatusCode = (int)HttpStatusCode.OK };
            }
            else
            {
                return new ResponseMessage { Success = false, Message = "No blogs found", StatusCode = (int)HttpStatusCode.NotFound };
            }
        }

        public ResponseMessage ConfirmBlog(int blogId)
        {
            {
                var existingBlog = db.blog
                                     .Include(account => account.Account)
                                     .ThenInclude(profile => profile.Profile)
                                     .Include(image => image.BlogImage)
                                     .FirstOrDefault(b => b.BlogID == blogId);
                if (existingBlog == null)
                {
                    return new ResponseMessage { Success = false, Message = "Blog not found", StatusCode = (int)HttpStatusCode.NotFound };
                }
                String status = "Approved";
                existingBlog.Status = status;
                existingBlog.ReasonReject = null; // Hoặc existingBlog.ReasonReject = "";

                db.blog.Update(existingBlog);
                db.SaveChanges();

                // Lấy email của chủ blog
                var blogOwnerEmail = db.blog
                    .Where(b => b.BlogID == blogId)
                    .Select(b => b.Account.Email)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(blogOwnerEmail))
                {
                    string content = "Congratulations! Your blog has been approved.";
                    Utils.SendMailRegistration(blogOwnerEmail, content);
                }

                return new ResponseMessage { Success = true, Message = "Blog status updated successfully", StatusCode = (int)HttpStatusCode.OK };
            }
        }

        public ResponseMessage RejectBlog(int blogId,string reasonReject)   
        {
            var existingBlog = db.blog
                                 .Include(account => account.Account)
                                 .ThenInclude(profile => profile.Profile)
                                 .Include(image => image.BlogImage)
                                 .FirstOrDefault(b => b.BlogID == blogId);
            if (existingBlog == null)
            {
                return new ResponseMessage { Success = false, Message = "Blog not found", StatusCode = (int)HttpStatusCode.NotFound };
            }

            existingBlog.Status = "Rejected";
            existingBlog.ReasonReject = reasonReject;

            db.blog.Update(existingBlog);
            db.SaveChanges();

            // Lấy email của chủ blog
            var blogOwnerEmail = db.blog
                .Where(b => b.BlogID == blogId)
                .Select(b => b.Account.Email)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(blogOwnerEmail))
            {
                string content = $"Your blog has been rejected. Reason: {reasonReject}.";
                Utils.SendMailRegistration(blogOwnerEmail, content);
            }

            return new ResponseMessage { Success = true, Message = "Blog rejected successfully", StatusCode = (int)HttpStatusCode.OK };
        }
    }
}

