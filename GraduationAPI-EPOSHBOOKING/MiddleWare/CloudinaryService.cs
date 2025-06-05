using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GraduationAPI_EPOSHBOOKING.MiddleWare
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException(nameof(image), "Image is empty");
            }
                
            using (var stream = image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = "EposhBooking",
                    PublicId = Guid.NewGuid().ToString(),

                    Transformation = new Transformation()
                            .Quality("auto:low")
                            .FetchFormat("webp")
                            .Width(1024)
                            .Crop("limit")
                               
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    throw new Exception("Failed to upload image to Cloudinary.");
                }
            }
        }
    }
}

