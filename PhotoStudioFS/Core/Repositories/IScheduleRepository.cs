using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Core.Repositories
{
    public interface IScheduleRepository : IRepository<Schedule>
    {

        IEnumerable<Schedule> GetSchedulesByPhotoType(DateTime start, DateTime end, string photoType);
        IEnumerable<Schedule> GetSchedules(DateTime start, DateTime end);
    }
}
