using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize (Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;
        public LeaveAllocationController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Employee> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        // GET: LeaveAllocationController1
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var mappedLeaveTypes = _mapper.Map<List<LeaveTypeViewModel>> (leaveTypes.ToList());
            var model = new CreateLeaveAllocationViewModel()
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = 0
            };
            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _unitOfWork.LeaveTypes.Find(u => u.Id == id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            
            foreach (var e in employees)
            {
                if (await _unitOfWork.LeaveAllocations.Exists(u => u.LeaveTypeId == id && u.EmployeeId == e.Id))
                    continue;
                var allocation = new LeaveAllocationViewModel()
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = e.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                    
                };
                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _unitOfWork.LeaveAllocations.Create(leaveAllocation);
            }
            await _unitOfWork.Save();
            //Challenge - keep a tally of how many times this loop was ran and redirect to Index, passing that number in

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeViewModel>>(employees);

            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _mapper.Map<EmployeeViewModel>(await _userManager.FindByIdAsync(id));
            var allocations = _mapper.Map<List<LeaveAllocationViewModel>>(await _unitOfWork.LeaveAllocations.FindAll(u => u.EmployeeId == id));
                        
            var model = new ViewAllocationViewModel()
            {
                Employee = employee,
                LeaveAllocations = allocations
            };
            return View(model);
        }

        // GET: LeaveAllocationController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController1/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(u => u.Id == id);
            var model = _mapper.Map<EditLeaveAllocationViewModel>(leaveAllocation);
            return View(model);
        }

        // POST: LeaveAllocationController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var allocation = _mapper.Map<LeaveAllocation>(model);
                //an alternate would be to load the LeaveAllocation by the ID that's in the model, and changing just the "NumberOfDays" property and updating
                //less code than having a bunch of "hidden" fields in the razor page
                _unitOfWork.LeaveAllocations.Update(allocation);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());
                return View(model);
            }
        }

        // GET: LeaveAllocationController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        //Ensures that the unit of work is disposed when the controller is no longer in use
        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }


    }
}
