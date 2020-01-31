using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class ShootType : Repository<PhotoStudioFS.Models.Setting.ShootType>, IShootType
    {
        public ShootType(photostudioContext context) : base(context)
        { }

        public photostudioContext photostudioContext
        {
            get { return context as photostudioContext; }
        }

    }
}
