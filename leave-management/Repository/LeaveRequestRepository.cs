using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(LeaveRequest entity)
        {
            await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<bool> Exists(int id)
        {
            return await _db.LeaveRequests.AnyAsync(u => u.Id == id);
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests
                .Include(u => u.RequestingEmployee)
                .Include(u => u.ApprovedBy)
                .Include(u => u.LeaveType)
                .ToListAsync();
        }

        public async Task<ICollection<LeaveRequest>> FindByEmployee(string employeeId)
        {
            return await _db.LeaveRequests
                .Where(u => u.RequestingEmployeeId == employeeId)
                .Include(u => u.ApprovedBy)
                .Include(u => u.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            return await _db.LeaveRequests
                .Include(u => u.RequestingEmployee)
                .Include(u => u.ApprovedBy)
                .Include(u => u.LeaveType)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
