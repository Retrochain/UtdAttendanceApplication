using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UtdAttendanceApplication.Models;
using UtdAttendanceApplication.ViewModels;

namespace UtdAttendanceApplication.Controllers;

// Written by Akshaan Singh, Dylan Hua, Ricardo Vargas on 4/4/2025 for CS 4485.0W1
/*
    This is the main Home Controller of the application, which controls the Models, 
    Views, and ViewModels of the application. There are 2 main action methods for 
    our home controller that control the overall application:
        - Index: Also known as the Login method, this controls the login functionality
                 of the code by displaying a form to the user, where they are asked to 
                 enter their UTD ID and their password (provided by the professor) to 
                 gain access to their quiz. The UTD ID is first checked to see if the 
                 student is even present in the database, and if they aren't, they are
                 given an error message and told to contact the professor. If they are,
                 then the next thing that is checked is their password, which is a
                 temporary password provided by the professor for a particular course,
                 and for a particular section. If the password exists for that course 
                 AND section, the next thing that is checked is if the student is 
                 enrolled in that section AND that course, and then we check if the 
                 password is currently live. If these checks are passed, the student is
                 allowed to enter, otherwise, they are given approriate error messages. 
        - 
 */

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly UtdattendanceappdbContext _context;

    public HomeController(ILogger<HomeController> logger, UtdattendanceappdbContext dbContext)
    {
        _logger = logger;
        _context = dbContext;
    }

    // The GET method for our Index page, referenced when the webpage is loaded
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // The POST method for our Index page, referenced when the submit button is pressed
    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        //If the current model is valid (aka the page rendered properly)
        if (ModelState.IsValid)
        {
            // We first check if the student record exists in the database through their UTD ID
            var stdnt = _context.Students.Where(s => s.UtdId == model.UtdId).FirstOrDefault();

            // If the student exists, then we move forward
            if (stdnt != null)
            {
                // We now check if the password exists in the database
                var stdntPass = _context.Passwords.Where(p => p.Pwd == model.Password).FirstOrDefault();
                if (stdntPass != null)
                {

                }
            }
            else
            {
                // Otherwise we display the UTD ID not found error
                ModelState.AddModelError("UtdId", "UTD ID not found, please make sure the UTD ID is correct");
            }
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
