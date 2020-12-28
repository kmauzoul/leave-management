using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Data
{
    public class LeaveAllocation
    {
        [Key]
        public int Id { get; set; }

        public int NumberOfDays { get; set; }

        public DateTime DateCreated { get; set; }

        [ForeignKey("EmployeeId")] //Foreign Key reference to EmployeeId (primary key on AspNetUsers table)
        public Employee Employee { get; set; }
        
        //This is string because in AspNetUsers class, the Id is a string
        public string EmployeeId { get; set; }

        [ForeignKey("LeaveTypeId")] //Foreign Key reference to LeaveTypeId (primary key on LeaveType table / class)
        public LeaveType LeaveType { get; set; }

        public int LeaveTypeId { get; set; }
    }
}
