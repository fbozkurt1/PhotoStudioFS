using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Core.Repositories;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(photostudioContext context) : base(context)
        { }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByIsApproved(short isApproved)
        {
            return await photostudioContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.ShootType)
                .Where(a => a.IsApproved == isApproved)
                .OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByState(short state)
        {
            return await photostudioContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.ShootType)
                .Where(a => a.State == state && a.CustomerId != null)
                .OrderBy(a => a.CreatedAt).ToListAsync();
        }

        public async Task<int> GetCountAppointmentsByState(short state)
        {
            return await photostudioContext.Appointments
                .Where(a => a.State == state)
                .CountAsync();
        }

        public async Task<int> GetCountAppointmentsByIsApproved(short isApproved)
        {
            return await photostudioContext.Appointments
                .Where(a => a.IsApproved == isApproved)
                .CountAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomer(string customerId)
        {
            return await photostudioContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.ShootType)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDateStart).ToListAsync();
        }
    }
}
