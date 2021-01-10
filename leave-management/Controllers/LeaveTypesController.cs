using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    //Set this over the class so you must be Authorized and you must be Administrator role to see it
    [Authorize(Roles = "Administrator")]
    public class LeaveTypesController : Controller
    {
        //private readonly ILeaveTypeRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        //public LeaveTypesController(ILeaveTypeRepository repo, IMapper mapper)
        //{
        //    _repo = repo;
        //    _mapper = mapper;
        //}

        public LeaveTypesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        // GET: LeaveTypesController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            //map to the LeaveTypeViewModel
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leaveTypes.ToList());
            return View(model);
        }

        // GET: LeaveTypesController/Details/5
        public async Task<ActionResult> Details(int id)
        {

            if (!await _unitOfWork.LeaveTypes.Exists(u => u.Id == id))
                return NotFound();

            var model = await _unitOfWork.LeaveTypes.Find(u => u.Id == id);
            var leaveType = _mapper.Map<LeaveTypeViewModel>(model);
            return View(leaveType);
        }

        // GET: LeaveTypesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;

                await _unitOfWork.LeaveTypes.Create(leaveType);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }

        // GET: LeaveTypesController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (!await _unitOfWork.LeaveTypes.Exists(u => u.Id == id))
                return NotFound();

            var model = await _unitOfWork.LeaveTypes.Find(u => u.Id == id);
            var leaveType = _mapper.Map<LeaveTypeViewModel>(model);
            return View(leaveType);
        }

        // POST: LeaveTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveType>(model);

                _unitOfWork.LeaveTypes.Update(leaveType);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveTypesController/Delete/5

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.Find(u => u.Id == id);
                if (leaveType == null)
                {
                    return NotFound();
                }
                _unitOfWork.LeaveTypes.Delete(leaveType);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest();
            }
        }

        //The below is if you want to redirect to an actual Delete page.  Check out the Delete.cshtml for calling the above method.  Javascript OnConfirm function 

        //public ActionResult Delete(int id)
        //{
        //    if (!_repo.Exists(id))
        //        return NotFound();

        //    var model = _repo.FindById(id);
        //    var leaveType = _mapper.Map<LeaveTypeViewModel>(model);
        //    return View(leaveType);
        //}

        // POST: LeaveTypesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeViewModel model)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.Find(u => u.Id == id);
                if (leaveType == null)
                {
                    return NotFound();
                }
                _unitOfWork.LeaveTypes.Delete(leaveType);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
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
