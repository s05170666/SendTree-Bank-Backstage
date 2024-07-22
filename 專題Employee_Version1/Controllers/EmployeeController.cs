using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using 專題Employee_Version1.Models;
using System.Data.Entity;


namespace 專題Employee_Version1.Controllers
{
    public class EmployeeController : Controller
    {
        private Version2_EmployeeEntities _db = new Version2_EmployeeEntities();
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EmployeeList()
        {
            var employees = _db.Employees.ToList();
            return View(employees);
        }

        public ActionResult Edit(int id)
        {
            var employee = _db.Employees.Find(id);
            return View(employee);
        }

        [HttpPost]
        public ActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("EmployeeList");
            }
            return View(employee);
        }


    }
}