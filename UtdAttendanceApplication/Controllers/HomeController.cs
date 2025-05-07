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

        - Quiz: This is the main method that controls the quiz functionality of the code.
                The method first validates the student, validates the quiz they are assigned, 
                validates the questions and then allowes the user access to their assigned
                quiz. It also checks if the quiz is either a) already taken or b) no longer
                available. Appropriate views are displayed for quiz, quiz taken or quiz 
                unavailable. After the student submits the quiz, the method then records their
                attendance in the database, along with their IP address and the answer choice
                they picked. The user is shown a view that displays their question/quiz result
                along with a confirmation screen telling them that their attendance has been 
                recorded. 
 */


// Written by Akshaan Singh on 4/3/2025
// Edited by Ricardo Vargas and Dylan Hua from 4/4/2025 to 4/23/2025
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly AppDBContext _context;

    public HomeController(ILogger<HomeController> logger, AppDBContext dbContext)
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
                    var stdntPassSection = _context.Sections.Where(s => s.SectionId == stdntPass.SectionId && !s.IsDeleted).FirstOrDefault();
                    if ((stdntPassCourse != null) && (stdntPassSection != null))
                    {
                        // We then check if the password is currently live or expired
                        DateTime currentTime = DateTime.Now;
                        if (currentTime >= stdntPass.AvailableOn && currentTime <= stdntPass.AvailableUntil)
                        {
                            // We then check if the student belongs to the course or the section
                            var stdntEnroll = _context.Enrollments.Where(e => e.StudentId == stdnt.StudentId).ToList();

                            bool isEnrolled = stdntEnroll.Any(e => e.CourseId == stdntPassCourse.CourseId && e.SectionId == stdntPassSection.SectionId);

                            if (isEnrolled)
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

    //This method will load and display the quiz for the student to take
    /* First the method validates that the student is authenticated and found in the database, 
     * then it validates the quiz is currently available and checks if the student has already taken the quiz.
     * After passing the validation checks the quiz is displayed to the student.
     * The method takes in the ID of the quiz to be loaded.
     * The view containing the quiz is returned for the student to take.
     */

    public async Task<IActionResult> Quiz(int id)
    {
        // Get the student's UTD ID from claims (stored during login)
        // This is used to identify which student is attempting to take the quiz
        var utdId = User.FindFirstValue("UtdId");
        if (string.IsNullOrEmpty(utdId))
        {
            // If the primary claim is not found then we use the Name claim as a backup
            // It is a secondary way to identify the student
            utdId = User.Identity?.Name;
            if (string.IsNullOrEmpty(utdId))
            {
                // If we are still unable to find the student then they will need to login again
                return RedirectToAction("Index");
            }
        }

        // Look up the student in the database using their UTD ID
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UtdId == utdId);

        if (student == null)
        {
            // If the student doesn't exist in the database then they are returned to login
            ModelState.AddModelError("", "Student not found. Please try logging in again.");
            return RedirectToAction("Index");
        }

        // Retrieve the quiz with related course and section information
        var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Section)
                .Where(q => q.QuizId == id && (q.Section == null || !q.Section.IsDeleted))
                .FirstOrDefaultAsync();

        if (quiz == null || (quiz.Section != null && quiz.Section.IsDeleted))
        {
            // If the quiz does not exist then the student is returned to login page
            ModelState.AddModelError("", "Quiz not found.");
            return RedirectToAction("Index");
        }

        // Validate that the quiz is currently available
        // This is to ensure students can only take quizzes within the period specicified by the professor
        var today = DateTime.Now;
        if (today < quiz.AvailableOn || today > quiz.AvailableUntil)
        {
            // Quiz is not currently available, display the closed quiz view
            // Quiz is not currently available, display the closed quiz view
            ViewBag.ErrorMessage = "This quiz is not currently available.";
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("QuizClosed");
        }

        // Check if student has already taken this quiz
        // This prevents duplicate attendance entries for data integrity
        var alreadyTaken = await _context.Attendances
            .AnyAsync(a => a.StudentId == student.StudentId && a.QuizId == id);

        if (alreadyTaken)
        {
            // If the student already took the quiz the already taken view is returned
            ViewBag.QuizTitle = quiz.QuizTitle;
            ViewBag.CourseName = quiz.Course.CourseName;
            ViewBag.SectionCode = quiz.Section?.SectionCode.ToString() ?? "N/A";
            return View("AlreadyTaken");
        }

        // We first get all the question IDs assigned to our bank using this table
        var questionIds = await _context.QuizQuestionBankAssignments
            .Where(q => q.QuizId == quiz.QuizId)
            .Select(q => q.QuestionId)
            .ToListAsync();

        // Get all questions directly associated with this quiz from our questionIDs (0-3 questions)
        // Questions reference the quiz they belong to through their quiz id
        var questions = await _context.QuizQuestions
            .Where(q => questionIds.Contains(q.QuestionId)) // inclused the question options
            .Include(q => q.QuestionOptions) // Get questions only for this specific quiz 
            .ToListAsync();

        // Create list of QuizQuestionViewModel objects
        // Converts database entities to view models for display
        var questionViewModels = new List<QuizQuestionViewModel>();
        foreach (var question in questions)
        {
            // Creates view model for each question with its associated options
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

        // Create the view model with all necessary data for rendering the quiz
        var viewModel = new QuizViewModel
        {
            CourseName = quiz.Course.CourseName,
            CourseSection = quiz.Section?.SectionCode ?? "",
            ProfName = quiz.Course.ProfName,
            QuizBankId = 0,
            Questions = questionViewModels
        };

        // Store data in ViewBag for display 
        ViewBag.StudentName = $"{student.FirstName} {student.LastName}";
        ViewBag.StudentId = student.StudentId;
        ViewBag.QuizTitle = quiz.QuizTitle;
        ViewBag.ExpiresAt = quiz.AvailableUntil;

        // Store IDs needed for form processing in ViewBag
        ViewBag.QuizId = quiz.QuizId;
        ViewBag.CourseId = quiz.CourseId;
        ViewBag.SectionId = quiz.SectionId;

        // Returns quiz view with populated view model
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
        var quiz = await _context.Quizzes
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

        // Prepare a list of results (correct answers) to send to the view
        var results = new List<QuizResult>();

        // Record answers if questions were answered
        if (model.Questions != null && model.Questions.Any())
        {
            foreach (var question in model.Questions)
            {
                // Only process questions that have an answer selected
                if (question.SelectedOptionId.HasValue && question.SelectedOptionId > 0)
                {
                    // Get the correct option label stored in the question
                    var correctOptionLabel = await _context.QuizQuestions
                        .Where(q => q.QuestionId == question.QuestionId)
                        .Select(q => q.CorrectOption)
                        .FirstOrDefaultAsync();

                    // Get the optionLabel for the selected optionID
                    var selectedOptionLabel = await _context.QuestionOptions
                        .Where(o => o.OptionId == question.SelectedOptionId)
                        .Select(o => o.OptionLabel)
                        .FirstOrDefaultAsync();

                    // Now compare labels, not IDs
                    bool isCorrect = correctOptionLabel.HasValue && selectedOptionLabel.HasValue && correctOptionLabel == selectedOptionLabel;

                    // Retrieve the question text from the QuizQuestions table
                    var questionText = await _context.QuizQuestions
                        .Where(q => q.QuestionId == question.QuestionId)
                        .Select(q => q.QuestionText)
                        .FirstOrDefaultAsync();

                    // Add result to the list
                    results.Add(new QuizResult
                    {
                        QuestionText = questionText,
                        IsCorrect = isCorrect
                    });

                    // Create record of student's answer
                    var studentAnswer = new StudentAnswer
                    {
                        QuizId = quizId,
                        StudentId = student.StudentId,
                        QuestionId = question.QuestionId,
                        SelectedOptionId = question.SelectedOptionId,
                        IsCorrect = (sbyte)(isCorrect ? 1 : 0),
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

        return View("QuizResult", new QuizResultViewModel { Results = results });
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


