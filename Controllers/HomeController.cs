using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        
        [HttpGet("")]
        public IActionResult LoginUser()
        {
            return View("Login");
        }
        [HttpPost("userlogin")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u=>u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                else
                {
                    HttpContext.Session.SetString("CurrentUser", userInDb.Email);
                    HttpContext.Session.SetInt32("CurrentUserID", userInDb.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Login");
            }
        }        
/////////////////////////////////////////////////////////
        [HttpGet("register")]
        public IActionResult RegisterUser()
        {
            return View("Register");
        }
        [HttpPost("userregister")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u=>u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", user.Email);
                    return View("Register");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    dbContext.Users.Add(user);
                    HttpContext.Session.SetString("CurrentUser", user.Email);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("CurrentUserID", user.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Register");
            }
        }
////////////////////////////////////////////////////////////
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetString("CurrentUser") != null)
            {
                int? ID =  HttpContext.Session.GetInt32("CurrentUserID");
                int realId = (int) ID;
                User currentuser = dbContext.Users.Include(u=>u.AllTransactions).FirstOrDefault(u=>u.UserId == realId);
                List<Transaction> trans = dbContext.Transactions.Where(t=>t.UserId == realId).OrderByDescending(t=>t.CreatedAt).ToList();
                ViewBag.Sum = trans.Sum(t=>t.Amount); 
                ViewBag.User = currentuser;
                return View("Dashboard");
            }
            else
            {
                return Redirect("/");
            }
        }
/////////////////////////////////////////////////////////////
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }
/////////////////////////////////////////////////////////////
        [HttpPost("withdrawdeposit")]
        public IActionResult AddAmount(Transaction newtrans)
        {
            if(HttpContext.Session.GetString("CurrentUser") != null)
            {
                dbContext.Add(newtrans);
                dbContext.SaveChanges();
                return Redirect("Dashboard");
            }
            else
            {
                return Redirect("Dashboard");
            }
            
        }

       



























        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
