using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using UtdAttendanceApplication.Data;
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
    public async Task<IActionResult> Index(LoginViewModel model)
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
                                // Create authentication claims
                                var claims = new List<Claim>
                                {
                                  new Claim("UtdId", stdnt.UtdId),
                                  new Claim(ClaimTypes.Name, $"{stdnt.FirstName} {stdnt.LastName}")
                                 };

                                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                                // Sign in the user
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                                // Redirect to Quiz with the quiz ID
                                var quizId = stdntPass.QuizId;
                                return RedirectToAction("Quiz", new { id = quizId });
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
    /* First the method validates that the student is authenticated and found in the database, 
     * then loads the appropriate quiz. The method takes in the ID of the quiz to be loaded.
     * The view containing the quiz is returned for the student to take.
     */
    public async Task<IActionResult> Quiz(int id)
    {
        // Get the student's UTD ID from claims (stored during login)
        // Used to identify which student is taking the quiz
        var utdId = User.FindFirstValue("UtdId");
        if (string.IsNullOrEmpty(utdId))
        {
            // If UTD ID is not found in primary claim then we try getting it from the Name claim as a fallback
            utdId = User.Identity?.Name;

            if (string.IsNullOrEmpty(utdId))
            {
                // If we cannot identify the student, they will need to login in again
                return RedirectToAction("Index");
            }
        }
        // Look up the student in the database using their UTD ID
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UtdId == utdId);

        if (student == null)
        {
            // Student was not found in the database and is returned to login
            ModelState.AddModelError("", "Student not found. Please try logging in again.");
            return RedirectToAction("Index");
        }

        // Retrieve the quiz with related course and section information
        var quiz = await _context.Quizes
                .Include(q => q.Course)
                .Include(q => q.Section)
                .FirstOrDefaultAsync(q => q.QuizId == id);

        if (quiz == null)
        {
            // Quiz not found, return to login page
            ModelState.AddModelError("", "Quiz not found.");
            return RedirectToAction("Index");
        }

        // Validate that the quiz is currently available based on its scheduled date
        // Ensures quiz can only be taken during the specified time window
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (today < quiz.AvailabeOn || today > quiz.AvailableUntil)
        {
            // If the quiz is not within the avaiability window we show an error message
            ViewBag.ErrorMessage = "This quiz is not currently available.";
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("QuizClosed");
        }

        // Check if student has already taken this quiz
        var alreadyTaken = await _context.Attendances
            .AnyAsync(a => a.StudentId == student.StudentId && a.QuizId == id);

        if (alreadyTaken)
        {
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("AlreadyTaken");
        }

        // Get quiz questions (0 - 3 questions)
        var questions = new List<QuizQuestion>();

        // First add the main question linked directly to the quiz
        var mainQuestion = await _context.QuizQuestions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == quiz.QuestionId);

        if (mainQuestion != null)
        {
            questions.Add(mainQuestion);
        }

        // Then try to find additional questions from quiz banks for the selected quiz
        var additionalQuestions = await _context.QuizQuestions
            .Include(q => q.QuestionOptions)
            .Where(q => _context.QuizBanks.Any(
                qb => qb.CourseId == quiz.CourseId &&
                      qb.SectionId == quiz.SectionId &&
                      qb.QuestionId == q.QuestionId &&
                      q.QuestionId != quiz.QuestionId)) // Exclude the main question
            .Take(3 - questions.Count) // Take enough to have a total of 3
            .ToListAsync();


        questions.AddRange(additionalQuestions);


        // Create list of QuizQuestionViewModel objects
        var questionViewModels = new List<QuizQuestionViewModel>();
        foreach (var question in questions)
        {
            // Create view model for each question with its options
            var questionViewModel = new QuizQuestionViewModel
            {
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                Options = question.QuestionOptions.Select(o => new QuestionOptionViewModel
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                }).ToList()
            };

            questionViewModels.Add(questionViewModel);
        }

        // Create the view model with all necessary data for the quiz view
        var viewModel = new QuizViewModel
        {
            CourseName = quiz.Course.CourseName,
            CourseSection = quiz.Section?.SectionCode ?? 0,
            ProfName = quiz.Course.ProfName,
            QuizBankId = quiz.QuestionId,
            Questions = questionViewModels
        };

        // Store data in ViewBag for display 
        ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
        ViewBag.StudentId = student.StudentId;
        ViewBag.QuizTitle = quiz.QuizTitle;
        ViewBag.ExpiresAt = quiz.AvailableUntil;

        // Store IDs needed for form processing in ViewBag
        // These will be submitted back when the form is posted
        ViewBag.QuizId = quiz.QuizId;
        ViewBag.CourseId = quiz.CourseId;
        ViewBag.SectionId = quiz.SectionId;

        return View(viewModel);
    }

    /* This method processes the quiz submission from a student.
     * The students attentence and their answers to the quiz questions are 
     * recorded in the database. Quiz answers are also checked if they were correct or incorrect.
     * The method takes the QuizViewModel, quizId, courseId, and the sectionId as inputs.
     * The QuizResult view is returned showing a vonirmation of submission.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(QuizViewModel model, int quizId, int courseId, int? sectionId)
    {
        // Retrieve the student's identity from authentication claims
        // This ensures we're recording attendance for the correct student
        var utdId = User.FindFirstValue("UtdId");
        if (string.IsNullOrEmpty(utdId))
        {
            // Try getting it from the Name claim as a fallback iif primary claim is not found
            utdId = User.Identity?.Name;

            if (string.IsNullOrEmpty(utdId))
            {
                // Student is redirected to login if they cannot be identified
                return RedirectToAction("Index");
            }
        }

        // Look up the student in the database using their UTD ID
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UtdId == utdId);

        if (student == null)
        {
            // Student not found in database, redirect to login
            ModelState.AddModelError("", "Student not found. Please try logging in again.");
            return RedirectToAction("Index");
        }

        // Retrieve quiz information for recording and displaying results
        var quiz = await _context.Quizes
            .Include(q => q.Course)
            .Include(q => q.Section)
            .FirstOrDefaultAsync(q => q.QuizId == quizId);

        if (quiz == null)
        {
            // Quiz not found, redirect to login
            ModelState.AddModelError("", "Quiz not found.");
            return RedirectToAction("Index");
        }

        // Check if student has already taken this quiz
        var alreadyTaken = await _context.Attendances
            .AnyAsync(a => a.StudentId == student.StudentId && a.QuizId == quizId);

        if (alreadyTaken)
        {
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("AlreadyTaken");
        }

        // Create and record the attendance entry
        var attendance = new Attendance
        {
            StudentId = student.StudentId,
            CourseId = courseId,
            SectionId = sectionId,
            QuizId = quizId,
            AttendanceStatus = "present",
            Time = DateTime.Now
        };

        _context.Attendances.Add(attendance);

        // Record answers if questions were answered
        if (model.Questions != null && model.Questions.Any())
        {
            foreach (var question in model.Questions)
            {
                // Only process questions that have an answer selected
                if (question.SelectedOptionId.HasValue && question.SelectedOptionId > 0)
                {
                    // Get the correct option for scoring
                    var correctOption = await _context.QuizQuestions
                        .Where(q => q.QuestionId == question.QuestionId)
                        .Select(q => q.CorrectOption)
                        .FirstOrDefaultAsync();

                    // Create record of student's answer
                    var studentAnswer = new StudentAnswer
                    {
                        QuizId = quizId,
                        StudentId = student.StudentId,
                        QuestionId = question.QuestionId,
                        SelectedOptionId = question.SelectedOptionId,
                        IsCorrect = (sbyte)(question.SelectedOptionId == correctOption ? 1 : 0),
                        SubmittedOn = DateTime.Now,
                        IpAddress = GetClientIpAddress()
                    };

                    _context.StudentAnswers.Add(studentAnswer);
                }
            }
        }

        await _context.SaveChangesAsync();

        // Prepare result view data
        ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
        ViewBag.CourseName = quiz.Course.CourseName;
        ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
        ViewBag.QuizTitle = quiz.QuizTitle;
        ViewBag.SubmissionTime = DateTime.Now;

        return View("QuizResult");
    }
    // Helper method to retrieve the client's IP address
    private string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = Request.Headers["X-Forwarded-For"];
        }

        return ipAddress ?? "Unknown";
    }

}

