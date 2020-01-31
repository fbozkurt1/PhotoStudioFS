using PhotoStudioFS.Core;
using PhotoStudioFS.Core.Repositories;
using PhotoStudioFS.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly photostudioContext _context;
        public UnitOfWork(photostudioContext context)
        {
            _context = context;
            Photos = new PhotoRepository(_context);
            Schedules = new ScheduleRepository(_context);
            Appointments = new AppointmentRepository(_context);
            Contacts = new ContactRepository(_context);
            Feedbacks = new FeedbackRepository(_context);
            ShootTypes = new ShootType(_context);
        }
        public IPhotoRepository Photos { get; private set; }

        public IScheduleRepository Schedules { get; private set; }

        public IAppointmentRepository Appointments { get; private set; }

        public IContactRepository Contacts { get; set; }

        public IFeedbackRepository Feedbacks { get; set; }

        public IShootType ShootTypes { get; set; }

        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
