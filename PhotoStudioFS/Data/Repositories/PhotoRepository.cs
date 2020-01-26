using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Core.Repositories;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class PhotoRepository : Repository<Photo>, IPhotoRepository
    {
        public PhotoRepository(photostudioContext context) : base(context)
        { }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }

        public async Task<IEnumerable<Photo>> GetPhotos(int appointmentId, string customerId)
        {
            return await photostudioContext.Photos
                .Where(p => p.AppointmentId == appointmentId && p.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
