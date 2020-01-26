using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Core.Repositories
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        // if we have special query methods for Photo, they should be placed here.

        Task<IEnumerable<Photo>> GetPhotos(int appointmentId, string customerId);

    }
}
