using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PhotoStudioFS.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(1000)]
        public string Path { get; set; }
        public string ThumbnailPath { get; set; }
        public string FileName { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public string CustomerId { get; set; }
        public User Customer { get; set; }

    }

    public class PhotoViewToDownload
    {
        public int AppointmentId { get; set; }
        public string CustomerId { get; set; }
    }

    public class PhotoView
    {
        public int Id { get; set; }

        public IFormFile File { get; set; }

        public int AppointmentId { get; set; }

        public string CustomerId { get; set; }
    }

    public class ResponsePhotoUpload
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string PhotoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
