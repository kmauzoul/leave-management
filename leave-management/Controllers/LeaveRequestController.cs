using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<Employee> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveRequestController
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Index()
        {
            var leaveRequests = await _unitOfWork.LeaveRequests.FindAll(includes: new List<string>() { "RequestingEmployee", "LeaveType", "ApprovedBy" });
            var leaveRequestsModels = _mapper.Map<List<LeaveRequestViewModel>>(leaveRequests);
            var model = new AdminLeaveRequestViewModel()
            {
                TotalRequests = leaveRequests.Count,
                ApprovedRequests = leaveRequests.Count(u => u.Approved == true),
                PendingRequests = leaveRequests.Count(u => u.Approved == null),
                RejectedRequests = leaveRequests.Count(u => u.Approved == false),
                LeaveRequests = leaveRequestsModels
            };
            return View(model);
        }

        [Authorize(Roles = "Employee")]
        public ActionResult MyLeaves()
        {
            var employeeId = _userManager.GetUserId(User);
            var leaveRequests = _unitOfWork.LeaveRequests.Find(u => u.RequestingEmployeeId == employeeId, new List<string>() { "ApprovedBy", "LeaveType" });
            var leaveAllocations = _unitOfWork.LeaveAllocations.Find(u => u.EmployeeId == employeeId);

            var model = new EmployeeLeaveRequestViewModel()
            {
                LeaveRequests = _mapper.Map<List<LeaveRequestViewModel>>(leaveRequests),
                LeaveAllocations = _mapper.Map<List<LeaveAllocationViewModel>>(leaveAllocations)
            };

            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(u => u.Id == id, new List<string>() { "RequestingEmployee","ApprovedBy","LeaveType" });
            var model = _mapper.Map<LeaveRequestViewModel>(leaveRequest);

            return View(model);
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var leaveTypeItems = leaveTypes.Select(u => new SelectListItem {  
                Text = u.Name,
                Value = u.Id.ToString()
            });
            var model = new CreateLeaveRequestViewModel()
            {
                LeaveTypes = leaveTypeItems
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestViewModel model)
        {

            try
            {
               
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
                var leaveTypeItems = leaveTypes.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                model.LeaveTypes = leaveTypeItems;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.LeaveTypeId == 0)
                {
                    ModelState.AddModelError("", "Invalid Leave Type selected");
                    return View(model);
                }

                if (DateTime.Compare(startDate, endDate) >= 0)
                {
                    ModelState.AddModelError("", "End Date must be greater than Start Date");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _unitOfWork.LeaveAllocations.Find(u => u.EmployeeId == employee.Id && u.LeaveTypeId == model.LeaveTypeId && u.Period == DateTime.Today.Year);
                int daysRequested = (int)(endDate.Date - startDate.Date).TotalDays;
                if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", $"Number of days requested ({daysRequested}) exceeds allocated days ({allocation.NumberOfDays})");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestViewModel()
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    DateActioned = DateTime.MinValue,
                    RequesterComment = model.RequesterComment,
                    Cancelled = false
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                await _unitOfWork.LeaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(MyLeaves));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        public async Task<ActionResult> ApproveRequest(int Id)
        {
            try
            {
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(u => u.Id == Id);
                leaveRequest.Approved = true;
                leaveRequest.DateActioned = DateTime.Now;
                leaveRequest.ApprovedById = _userManager.GetUserId(User);
                _unitOfWork.LeaveRequests.Update(leaveRequest);
                
                var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(u => u.EmployeeId == leaveRequest.RequestingEmployeeId && u.LeaveTypeId == leaveRequest.LeaveTypeId && u.Period == DateTime.Today.Year);
                leaveAllocation.NumberOfDays -= (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;

                _unitOfWork.LeaveAllocations.Update(leaveAllocation);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(nameof(Index));
            }
        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(u => u.Id == id);
                leaveRequest.Approved = false;
                leaveRequest.DateActioned = DateTime.Now;
                leaveRequest.ApprovedById = _userManager.GetUserId(User);
                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        public async Task<ActionResult> CancelLeaveRequest(int id)
        {
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(u => u.Id == id);
            if (leaveRequest == null)
            {
                return NotFound();
            }
            leaveRequest.Cancelled = true;
            if (leaveRequest.Approved.HasValue && leaveRequest.Approved.Value)
            {
                var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(u => u.EmployeeId == leaveRequest.RequestingEmployeeId && u.LeaveTypeId == leaveRequest.LeaveTypeId);
                leaveAllocation.NumberOfDays += (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;
                _unitOfWork.LeaveAllocations.Update(leaveAllocation); //put the days back
            }
            _unitOfWork.LeaveRequests.Update(leaveRequest);
            await _unitOfWork.Save();
            

            return RedirectToAction(nameof(MyLeaves));
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
