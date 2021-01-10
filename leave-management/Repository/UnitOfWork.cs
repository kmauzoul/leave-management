using leave_management.Contracts;
using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IGenericRepository<LeaveType> _leaveTypes;
        private IGenericRepository<LeaveAllocation> _leaveAllocations;
        private IGenericRepository<LeaveRequest> _leaveRequests;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public IGenericRepository<LeaveType> LeaveTypes
        {
            get => _leaveTypes ?? new GenericRepository<LeaveType>(_context);
        }
        public IGenericRepository<LeaveRequest> LeaveRequests
        {
            get => _leaveRequests ?? new GenericRepository<LeaveRequest>(_context);
        }
    
        public IGenericRepository<LeaveAllocation> LeaveAllocations
        {
            get => _leaveAllocations ?? new GenericRepository<LeaveAllocation>(_context);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
            {
                _context.Dispose();

            }
        }

        public async Task Save()
        {
           await _context.SaveChangesAsync();
        }
    }
}
