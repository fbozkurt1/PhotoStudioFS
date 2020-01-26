using PhotoStudioFS.Core.Repositories;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(photostudioContext context) : base(context)
        { }

        public IEnumerable<Schedule> GetSchedulesByPhotoType(DateTime start, DateTime end, string photoType)
        {

            return photostudioContext.Schedules.Where(s => s.isEmpty == true && s.start >= start && s.end <= end && s.photoShootType.Equals(photoType)).ToList();
        }

        public IEnumerable<Schedule> GetSchedules(DateTime start, DateTime end)
        {
            return photostudioContext.Schedules.Where(s => s.start >= start && s.end <= end).ToList();
        }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }
    }
}
