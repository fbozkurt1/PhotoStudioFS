using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhotoStudioFS.Core.Repositories;

namespace PhotoStudioFS.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IPhotoRepository Photos { get; }
        IScheduleRepository Schedules { get; }
        IAppointmentRepository Appointments { get; }
        IFeedbackRepository Feedbacks { get; }
        IContactRepository Contacts { get; }
        Task<int> Complete();
    }
}
