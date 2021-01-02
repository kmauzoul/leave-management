using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using leave_management.Models;

namespace leave_management.Data
{
    //Since we are extending the IdentityDbContext base class, when the Update-Database is ran,
    //It will add the Employee data to AspNetUsers table
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //When database is being created or updated, this will tell it to include Employee class (that extends IdentityUser)
        //Package Manager Console command: add-migration SomeNotesOnMigration
        //add-migration AddedEmployeeDataPoints
        public DbSet<Employee> Employees { get; set; }

        public DbSet<LeaveHistory> LeaveHistories { get; set; }

        public DbSet<LeaveType> LeaveTypes { get; set; }

        public DbSet<LeaveAllocation> LeaveAllocations { get; set; }

        public DbSet<leave_management.Models.LeaveTypeViewModel> LeaveTypeViewModel { get; set; }

        public DbSet<leave_management.Models.EmployeeViewModel> EmployeeViewModel { get; set; }

        public DbSet<leave_management.Models.LeaveAllocationViewModel> LeaveAllocationViewModel { get; set; }

        public DbSet<leave_management.Models.EditLeaveAllocationViewModel> EditLeaveAllocationViewModel { get; set; }
    }
}
