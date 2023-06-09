﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Przychodnia.Webapi.Data;
using Przychodnia.Webapi.Models;
using Przychodnia.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NuGet.Protocol;
using System;

namespace Przychodnia.Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Employee> _userManager;
        private readonly IWebHostEnvironment _environment;
        
        public EmployeeController(ApplicationDbContext db, UserManager<Employee> userManager, IWebHostEnvironment environment)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Employee>>> Get()
        {
            var employees = await _db.Employees.Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                e.Specialization,
            }).ToListAsync();
            return Ok(employees);
        }

        [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        [HttpGet("account")]
        public async Task<IActionResult> GetById([FromRoute] string? id)
        {
            string? userId = "";
            if (id == null) userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            else userId = id;
            var Worker = await _db.Employees.Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                e.Specialization,
                e.Email,
                e.PhoneNumber,
                e.DateOfBirth,
                e.Pesel
            }).FirstOrDefaultAsync(e => e.Id == userId);
            return Worker == null ? NotFound("Employee not found") : Ok(Worker);
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> Update([FromRoute]string id, [FromBody]UpdateEmployeeDto dto)
        {
            if (dto == null) return BadRequest("Model is null");
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return BadRequest("User not found");
                foreach (var prop in typeof(Employee).GetProperties())
                {
                    var fromProp = typeof(UpdateEmployeeDto).GetProperty(prop.Name);
                    var toValue = fromProp != null ? fromProp.GetValue(dto, null) : null;
                    if (toValue != null)
                    {
                        prop.SetValue(user, toValue, null);
                        _db.Entry(user).Property(prop.Name).IsModified = true;
                    }
                }
                foreach (var prop in typeof(IdentityUser).GetProperties())
                {
                    var fromProp = typeof(UpdateEmployeeDto).GetProperty(prop.Name);
                    var toValue = fromProp != null ? fromProp.GetValue(dto, null) : null;
                    if (toValue != null)
                    {
                        prop.SetValue(user, toValue, null);
                        _db.Entry(user).Property(prop.Name).IsModified = true;
                    }
                }
                await _db.SaveChangesAsync();

                return Ok("Updated");
            }
            return BadRequest("Model is not valid");
        }

        // TODO: sprawdzanie dostępności lekarza o konkretnej godzinie (przypisywanie wizyt)
        [Authorize(Roles = "Staff")]
        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateTime date, [FromQuery] Specialization specialization)
        {
            if (date.CompareTo(DateTime.Now) < 0 || date.Equals(null)) return BadRequest("Model error");

            var doctors = await _db.Employees.Where(e => e.Specialization == specialization 
                && !e.Appointments.Select(a => a.Date).Contains(date))
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                }).ToListAsync();

            /*var test = _db.Employees.Select(e => e.Appointments.Select(a => a.Date));*/
            return Ok(doctors);
        }

        [Authorize(Roles = "Staff, Employee")]
        [HttpPost("upload-image/{doctorId}")]
        public async Task<IActionResult> UploadImage([FromRoute] string? doctorId, [FromForm] IFormFile postedFile)
        {
            if (postedFile == null) return BadRequest("File is null");
            string fileName;
            if (doctorId != null) fileName = doctorId + ".png";
            else fileName = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)! + ".png";
            string path = Path.Combine(_environment.ContentRootPath, "StaticFiles");
            if (postedFile.Length > 0)
            {
                string upload = Path.Combine(path, fileName);
                using (Stream fileStream = new FileStream(upload, FileMode.Create))
                {
                    await postedFile.CopyToAsync(fileStream);
                }
                return Ok("Image updated");
            }
            return BadRequest("File is empty");
        }
    }
}
