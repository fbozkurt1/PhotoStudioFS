using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Core.Repositories;
using PhotoStudioFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class ContactRepository : Repository<Contact>, IContactRepository
    {
        public ContactRepository(photostudioContext context) : base(context)
        { }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }

        public async Task<IEnumerable<Contact>> GetContactRequests(bool isRead)
        {
            return await photostudioContext.Contacts
                .Where(c => c.IsRead == isRead)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
