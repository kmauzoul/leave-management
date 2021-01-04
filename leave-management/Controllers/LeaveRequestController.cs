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
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(ILeaveRequestRepository leaveRequestRepo, ILeaveTypeRepository leaveTypeRepo, IMapper mapper, ILeaveAllocationRepository leaveAllocationRepo, UserManager<Employee> userManager)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveRequestController
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var leaveRequests = _leaveRequestRepo.FindAll();
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

        // GET: LeaveRequestController/Details/5
        public ActionResult Details(int id)
        {
            var leaveRequest = _leaveRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestViewModel>(leaveRequest);

            return View(model);
        }

        // GET: LeaveRequestController/Create
        public ActionResult Create()
        {
            var leaveTypes = _leaveTypeRepo.FindAll();
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
        public ActionResult Create(CreateLeaveRequestViewModel model)
        {

            try
            {
               
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = _leaveTypeRepo.FindAll();
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

                var employee = _userManager.GetUserAsync(User).Result;
                var allocation = _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
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
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                if (!_leaveRequestRepo.Create(leaveRequest))
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(model);
                }

                

                return RedirectToAction(nameof(Index), "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        public ActionResult ApproveRequest(int Id)
        {
            try
            {
                var leaveRequest = _leaveRequestRepo.FindById(Id);
                leaveRequest.Approved = true;
                leaveRequest.DateActioned = DateTime.Now;
                leaveRequest.ApprovedById = _userManager.GetUserId(User);
                if (!_leaveRequestRepo.Update(leaveRequest))
                {
                    ModelState.AddModelError("", "Something went wrong.");
                    return View(nameof(Index));
                }

                var leaveAllocation = _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                leaveAllocation.NumberOfDays -= (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;
                if (!_leaveAllocationRepo.Update(leaveAllocation))
                {
                    ModelState.AddModelError("", "Something went wrong.");
                    return View(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(nameof(Index));
            }
        }

        public ActionResult RejectRequest(int Id)
        {
            try
            {
                var leaveRequest = _leaveRequestRepo.FindById(Id);
                leaveRequest.Approved = false;
                leaveRequest.DateActioned = DateTime.Now;
                leaveRequest.ApprovedById = _userManager.GetUserId(User);
                if (!_leaveRequestRepo.Update(leaveRequest))
                {
                    ModelState.AddModelError("", "Something went wrong.");
                    return View(nameof(Index));
                }

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
    }
}
