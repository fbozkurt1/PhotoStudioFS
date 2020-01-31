using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PhotoStudioFS.Models;
using PhotoStudioFS.Models.Setting;

namespace PhotoStudioFS.Data
{
    public partial class photostudioContext : IdentityDbContext<User>
    {
        public photostudioContext(DbContextOptions<photostudioContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<ShootType> ShootTypes { get; set; }

    }
}
