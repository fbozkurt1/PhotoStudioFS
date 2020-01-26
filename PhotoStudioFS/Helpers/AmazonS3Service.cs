using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PhotoStudioFS.Helpers
{
    public class AmazonS3Service
    {
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        private static String accessKey = "";
        private static String accessSecret = "";
        private static string bucketName = "";
        private static string key = "fuatDeneme";
        public AmazonS3Service(string _accessKey, string _accessSecret, string _bucketName)
        {
            accessKey = _accessKey;
            accessSecret = _accessSecret;
            bucketName = _bucketName;
            s3Client = new AmazonS3Client(accessKey, accessSecret, bucketRegion);
        }

        public async Task<ResponsePhotoUpload> UploadFileAsync(IFormFile file, string subFolderName)
        {
            try
            {
                if (!await CreateSubfolderIfNotExistAsync(subFolderName))
                {
                    return new ResponsePhotoUpload()
                    {
                        Message = "Yükleme Başarısız. Subfolder oluşturulamadı.",
                        Success = false,
                        PhotoUrl = "",
                        ThumbnailUrl = ""
                    };
                }

                // get the file and convert it to the byte[]
                byte[] fileBytes = new Byte[file.Length];
                file.OpenReadStream().Read(fileBytes, 0, Int32.Parse(file.Length.ToString()));

                string fileName = DateTime.Now.ToString("H.mm.ss") + Path.GetExtension(file.FileName);

                // create unique file name for prevent the mess
                var keyName = key + "/" + subFolderName + "/" + Guid.NewGuid() + "-" + fileName;

                PutObjectResponse response = null;

                using (var stream = new MemoryStream(fileBytes))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName,
                        InputStream = stream,
                        ContentType = file.ContentType,
                        CannedACL = S3CannedACL.PublicRead
                    };


                    response = await s3Client.PutObjectAsync(request);

                };

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new ResponsePhotoUpload()
                    {
                        Message = "Yükleme Başarılı",
                        Success = true,
                        PhotoUrl = "https://" + bucketName + ".s3." + bucketRegion.SystemName + "." + bucketRegion.PartitionDnsSuffix + "/" + keyName,
                        ThumbnailUrl = "https://" + bucketName + ".s3." + bucketRegion.SystemName + "." + bucketRegion.PartitionDnsSuffix + "/" + keyName

                    };

                }
                else
                {
                    // this model is up to you, in my case I have to use it following;
                    return new ResponsePhotoUpload()
                    {
                        Message = "Yükleme Başarısız",
                        Success = false,
                        PhotoUrl = "",
                        ThumbnailUrl = ""
                    };
                }
            }
            catch (Exception)
            {

                return new ResponsePhotoUpload()
                {
                    Message = "Yükleme Başarısız. Hata oluştu!",
                    Success = false,
                    PhotoUrl = "",
                    ThumbnailUrl = ""
                };
            }
        }

        private async Task<bool> CreateSubfolderIfNotExistAsync(string folderName)
        {
            var keyName = key + "/" + folderName;

            PutObjectResponse response = null;

            using (var stream = new MemoryStream())
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = stream,
                    CannedACL = S3CannedACL.PublicRead
                };
                response = await s3Client.PutObjectAsync(request);
            };

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? true : false;
        }


        public async Task<Stream> Download(string keyName)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    using (var responseStream = response.ResponseStream)
                    {
                        var stream = new MemoryStream();
                        await responseStream.CopyToAsync(stream);
                        stream.Position = 0;
                        return stream;
                    }
                }


            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }

        }

        /*public async Task<string> UploadFileAsync(IFormFile file)
        {

            // Create list to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

            // Setup information required to initiate the multipart upload.

            var keyName = "fuatDeneme/" + Guid.NewGuid() + file.FileName;

            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            // Initiate the upload.
            InitiateMultipartUploadResponse initResponse =
                await s3Client.InitiateMultipartUploadAsync(initiateRequest);



            // Upload parts.
            long contentLength = file.Length;
            byte[] fileBytes = new Byte[contentLength];
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB
            file.OpenReadStream().Read(fileBytes, 0, Int32.Parse(file.Length.ToString()));
            try
            {
                using (var stream = new MemoryStream(fileBytes))
                {
                    long filePosition = 0;
                    for (int i = 1; filePosition < contentLength; i++)
                    {
                        UploadPartRequest uploadRequest = new UploadPartRequest
                        {
                            BucketName = bucketName,
                            Key = keyName,
                            UploadId = initResponse.UploadId,
                            PartNumber = i,
                            PartSize = partSize,
                            FilePosition = filePosition,
                            InputStream = stream
                        };

                        // Track upload progress.
                        //uploadRequest.StreamTransferProgress +=
                        //    new EventHandler<StreamTransferProgressArgs>(UploadPartProgressEventCallback);

                        // Upload a part and add the response to our list.
                        uploadResponses.Add(await s3Client.UploadPartAsync(uploadRequest));

                        filePosition += partSize;
                    }
                }


                // Setup to complete the upload.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId
                };
                completeRequest.AddPartETags(uploadResponses);

                string a = completeRequest.Key;

                // Complete the upload.
                CompleteMultipartUploadResponse completeUploadResponse =
                    await s3Client.CompleteMultipartUploadAsync(completeRequest);



                if (completeUploadResponse.HttpStatusCode == HttpStatusCode.OK)
                    return completeUploadResponse.Location;
                //    MailSender.SendEmail(fileName, "Bulut 1", 1);
                else
                    return "";
                //    MailSender.SendEmail(fileName, "Bulut 1", 0);

            }
            catch (Exception)
            {

                //MailSender.SendEmail(fileName, "Bulut 1", 0);
                // Abort the upload.
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId
                };
                await s3Client.AbortMultipartUploadAsync(abortMPURequest);

                return "";
            }
        }*/
    }
}
