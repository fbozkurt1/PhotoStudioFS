using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoStudioFS.Helpers
{
    public static class Roles
    {
        public static readonly string Customer = "Customer";
        public static readonly string Admin = "Admin";
        public static readonly string Employee = "Employee";

        public static string[] GetRoles()
        {

            return new string[] { Customer, Admin, Employee };

        }
    }
}
