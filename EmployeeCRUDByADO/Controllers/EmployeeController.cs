using EmployeeCRUDByADO.DAL;
using EmployeeCRUDByADO.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCRUDByADO.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeDAL _dal;

        public EmployeeController(EmployeeDAL dal)
        {
            _dal = dal;
        }

        // GET
        public IActionResult Index()
        {
            var employees = _dal.GetEmployees();

            return View(employees);
        }

        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            _dal.AddEmployee(emp);

            return RedirectToAction("Index");
        }

        // EDIT
        public IActionResult Edit(int id)
        {
            var emp = _dal.GetEmployeeById(id);

            return View(emp);
        }

        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            _dal.UpdateEmployee(emp);

            return RedirectToAction("Index");
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            var emp = _dal.GetEmployeeById(id);

            return View(emp);
        }

        [HttpPost]
        public IActionResult Delete(Employee emp)
        {
            _dal.DeleteEmployee(emp.Id);

            return RedirectToAction("Index");
        }
    }
}
