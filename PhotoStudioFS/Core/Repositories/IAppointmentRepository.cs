using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Core.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByIsApproved(short isApproved);
        Task<IEnumerable<Appointment>> GetAppointmentsByState(short isApproved);
        Task<int> GetCountAppointmentsByState(short state);
        Task<int> GetCountAppointmentsByIsApproved(short isApproved);
        Task<IEnumerable<Appointment>> GetAppointmentsByCustomer(string customerId);
        Task<Appointment> GetAppointment(int id);
    }
}
