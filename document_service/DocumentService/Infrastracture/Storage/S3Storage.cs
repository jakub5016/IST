using Amazon.S3;
using Amazon.S3.Model;
using DocumentService.Domain;
using MassTransit.SqlTransport.Topology;
using Microsoft.Extensions.Options;

namespace DocumentService.Infrastracture.Storage
{
    public class S3Storage(IAmazonS3 s3Client, IOptions<S3Options> awsOptions) : IFileStorage
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly S3Options _awsOptions = awsOptions.Value;

        public async Task<string> GetFile(string path)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = path,
                Expires = DateTime.Now.AddDays(30),
                Verb = HttpVerb.GET,
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentDisposition = "attachment"
                }
            };
            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }

        public async Task UploadFile(Stream file, string path)
        {
            await _s3Client.UploadObjectFromStreamAsync(_awsOptions.BucketName, path, file, new Dictionary<string, object>());
        }
    }
}
