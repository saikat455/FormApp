// CloudinaryHelper.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Npgsql.BackendMessages;
using System.Security.Principal;

namespace FormApp.Helpers
{
    public static class CloudinaryHelper
    {
        private static readonly string CloudName = "dujr6a1so";
        private static readonly string ApiKey = "392117722713314";
        private static readonly string ApiSecret = "xLY4JdgN68WfPBjXEaSq0LIZDSE";

        private static readonly Cloudinary _cloudinary;

        static CloudinaryHelper()
        {
            var account = new Account(CloudName, ApiKey, ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public static async Task<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "form-app"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }
}