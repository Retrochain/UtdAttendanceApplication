using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UtdAttendanceApplication.Data;
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
                    // We then check if the password belongs to the course and section
                    var stdntPassCourse = _context.Courses.Where(c => c.CourseId == stdntPass.CourseId).FirstOrDefault();
                    var stdntPassSection = _context.Sections.Where(s => s.SectionId == stdntPass.SectionId).FirstOrDefault();
                    if ((stdntPassCourse != null) && (stdntPassSection != null))
                    {
                        // We then check if the password is currently live or expired
                        DateTime currentTime = DateTime.Now;
                        if (currentTime >= stdntPass.AvailableOn && currentTime <= stdntPass.AvailableUntil)
                        {
                            // We then check if the student belongs to the course or the section
                            var stdntEnroll = _context.Enrollments.Where(e => e.StudentId == stdnt.StudentId).FirstOrDefault();
                            if ((stdntEnroll?.CourseId == stdntPassCourse.CourseId) && (stdntEnroll?.SectionId == stdntPassSection.SectionId))
                            {
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                // Otherwise we display that the user doesnt exist in the course and section
                                ModelState.AddModelError("UtdId", "Student doesn't belong in the course or section");
                            }
                        }
                        else
                        {
                            // Otherwise we tell the user that the password is expired
                            ModelState.AddModelError("Password", "Password not yet live or expired");
                        }
                    }
                    else
                    {
                        // Otherwise we tell the user that the password isnt in the course OR the section
                        ModelState.AddModelError("Password", "Password entered for wrong section or course");
                    }
                }
                else
                {
                    // Otherwise we tell the user that the password doesn't exist
                    ModelState.AddModelError("Password", "Password not found, please make sure the password is correct");
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //This method will load the quiz for the student to take
    public async Task<IActionResult> Quiz(int id)
    {
        //Get the students UTD ID that was set during login
        var utdId = TempData["UtdId"]?.ToString();
        if (string.IsNullOrEmpty(utdId))
        {
            return RedirectToAction("Login");
        }

        //retrieve the quiz with course information
        var quiz = await _context.Quizes
                .Include(q => q.Course)
                .Include(q => q.Section)
                .FirstOrDefaultAsync(q => q.QuizId == id);


        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Quiz not found.";
            return RedirectToAction("Login");
        }

        // Get student ID from UTD ID
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UtdId == utdId);

        // Check if quiz is currently available
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (today < quiz.AvailabeOn || today > quiz.AvailableUntil)
        {
            ViewBag.ErrorMessage = "This quiz is not currently available.";
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("QuizClosed");
        }

        //Check if quiz was already taken?

        // Get quiz questions (0 - 3 questions)
        var questions = await _context.QuizQuestions
        .Include(q => q.QuestionOptions)
        .Where(q => q.QuestionId == quiz.QuestionId || q.QuizBanks.Any(qb => qb.QuestionId == quiz.QuestionId))
        .Take(3) // Maximum 3 questions
        .ToListAsync();

        // Create the view model with all necessary data
        var viewModel = new QuizViewModel
        {
            // viewmodel data
        };

        return View(viewModel);
    }

    /*
    [HttpPost]
    public async Task<IActionResult> SubmitQuiz(QuizViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Quiz", model);
        }

        // Get student from UTD ID
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UtdId == model.UtdId);

        // Record attendance
        var attendance = new Attendance
        {
            StudentId = student.StudentId,
            CourseId = model.CourseId,
            SectionId = model.SectionId,
            QuizId = model.QuizId,
            AttendanceStatus = "present",
            Time = DateTime.Now
        };

        _context.Attendances.Add(attendance);

        //record quiz answers 


    }*/

}
