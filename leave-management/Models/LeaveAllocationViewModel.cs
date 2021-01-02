using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    //Abstracted class based on LeaveAllocation.  ViewModels should interact with ViewModels and Data should interact with Data
    public class LeaveAllocationViewModel
    {
        public int Id { get; set; }

        [Required]
        public int NumberOfDays { get; set; }

        public DateTime DateCreated { get; set; }

        public EmployeeViewModel Employee { get; set; }

        //This is string because in AspNetUsers class, the Id is a string
        public string EmployeeId { get; set; }

        public LeaveTypeViewModel LeaveType { get; set; }

        public int LeaveTypeId { get; set; }

        public int Period { get; set; }


        //this will hold a list of Employees from the database as a drop down for user
        //public IEnumerable<SelectListItem> Employees { get; set; }

        //public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }

    public class CreateLeaveAllocationViewModel
    {
        public int NumberUpdated { get; set; }

        public List<LeaveTypeViewModel> LeaveTypes { get; set; }
    }

    public class ViewAllocationViewModel
    {
        public EmployeeViewModel Employee { get; set; }
        public string EmployeeId { get; set; }

        public List<LeaveAllocationViewModel> LeaveAllocations { get; set; }
    }

    public class EditLeaveAllocationViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Number of Days")]
        public int NumberOfDays { get; set; }
        public string EmployeeId { get; set; }
        public EmployeeViewModel Employee { get; set; }
        public LeaveTypeViewModel LeaveType { get; set; }

        public int LeaveTypeId { get; set; }

        public DateTime DateCreated { get; set; }

        public int Period { get; set; }
    }
}
