using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Schedule>> GetSchedulesByPhotoType(DateTime start, DateTime end, int photoType)
        {

            return await photostudioContext.Schedules
                .Where(s => s.isEmpty == true && s.start >= start && s.end <= end && s.ShootTypeId == photoType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedules(DateTime start, DateTime end)
        {
            return await photostudioContext.Schedules.Where(s => s.start >= start && s.end <= end)
                .ToListAsync();
        }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }
    }
}
