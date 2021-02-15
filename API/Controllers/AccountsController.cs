using API.Context;
using API.Models;
using API.Services;
using API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [EnableCors("mypolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly MyContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        SmtpClient client = new SmtpClient();
        Verification verification = new Verification();
        RandomGenerator randomGenerator = new RandomGenerator();
        ServiceEmail serviceEmail = new ServiceEmail();


        public AccountsController(MyContext myContext,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _context = myContext;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterViewModel registerVM)
        {
            var theCode = randomGenerator.GenerateRandom().ToString();
            serviceEmail.SendEmail(registerVM.Email, theCode);
            var pwHashed = BCrypt.Net.BCrypt.HashPassword(registerVM.Password, 12);
            var user = new User
            {
                Email = registerVM.Email,
                PasswordHash = pwHashed,
                UserName = registerVM.Username,
                EmailConfirmed = false,
                PhoneNumber = registerVM.Phone,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                SecurityStamp = theCode,
                AccessFailedCount = 0
            };
            _context.Users.AddAsync(user);
            var role = new UserRole
            {
                UserId = user.Id,
                RoleId = "2"
            };
            _context.UserRoles.AddAsync(role);
            _context.SaveChanges();
            return Ok("Registered successfully");

        }

        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> VerifyCode(User userViewModel)
        {
            if (ModelState.IsValid)
            {
                var getCode = _context.Users.Where(U => U.SecurityStamp == userViewModel.SecurityStamp).Any();
                if (!getCode)
                {
                    return BadRequest(new { msg = "Verification proccess is failed. Please enter the invalid code" });
                }
                var userEmail = _context.UserRoles.Include("Role").Include("User").Where(U => U.User.Email == userViewModel.Email).FirstOrDefault();
                var getUser = new UsersViewModel();
                userEmail.User.SecurityStamp = null;
                userEmail.User.EmailConfirmed = true;
                getUser.RoleName = userEmail.Role.Name;
                getUser.Username = userEmail.User.UserName;
                getUser.Id = userEmail.User.Id;
                getUser.Email = userEmail.User.Email;
                getUser.Phone = userEmail.User.PhoneNumber;
                await _context.SaveChangesAsync();
                return StatusCode(200, getUser);
            }
            return BadRequest(500);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginViewModel userVM)
        {
            if (ModelState.IsValid)
            {
                var pwd = userVM.Password;
                var masuk = _context.UserRoles.Include("Role").Include("User").FirstOrDefault(m => m.User.Email == userVM.Email);
                if (masuk == null)
                {
                    return BadRequest("Please use the existing email or register first");
                }
                else if (!BCrypt.Net.BCrypt.Verify(userVM.Password, masuk.User.PasswordHash))
                {
                    return BadRequest("Incorret password");
                }
                else if (pwd == null || pwd.Equals(""))
                {
                    return BadRequest("Please enter the password");
                }
                else
                {
                    var user = new UsersViewModel();
                    user.Id = masuk.User.Id;
                    user.Username = masuk.User.UserName;
                    user.Email = masuk.User.Email;
                    user.Phone = masuk.User.PhoneNumber;
                    user.RoleName = masuk.Role.Name;
                    if (user.Email != null)
                    {
                        var Claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                            new Claim("id", user.Id),
                            new Claim("uname", user.Username),
                            new Claim("email", user.Email),
                            new Claim("phone", user.Phone),
                            new Claim("lvl", user.RoleName)
                        };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], Claims, notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                        return Ok(new JwtSecurityTokenHandler().WriteToken(token));

                    }
                    return StatusCode(200, user);

                }
            }
            return BadRequest(500);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        // GET api/values
        [HttpGet]
        public async Task<List<UsersViewModel>> GetAll()
        {
            List<UsersViewModel> list = new List<UsersViewModel>();

            var getUserRole = await _context.UserRoles.Include("User").Include("Role").ToListAsync();
            if (getUserRole.Count == 0)
            {
                return null;
            }
            foreach (var item in getUserRole)
            {
                var user = new UsersViewModel()
                {
                    Id = item.User.Id,
                    Username = item.User.UserName,
                    Email = item.User.Email,
                    Password = item.User.PasswordHash,
                    Phone = item.User.PhoneNumber,
                    RoleName = item.Role.Name,
                };
                list.Add(user);
            }
            return list;
        }
        [HttpGet("{id}")]
        public UsersViewModel GetID(string id)
        {

            var getData = _context.UserRoles.Include("User").Include("Role").SingleOrDefault(x => x.UserId == id);
            if (getData == null || getData.Role == null || getData.User == null)
            {
                return null;
            }
            var user = new UsersViewModel()
            {
                Id = getData.User.Id,
                Username = getData.User.UserName,
                Email = getData.User.Email,
                Password = getData.User.PasswordHash,
                Phone = getData.User.PhoneNumber,
                RoleID = getData.Role.Id,
                RoleName = getData.Role.Name
            };
            return user;
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var getId = _context.Users.Find(id);
            _context.Users.Remove(getId);
            _context.SaveChanges();
            return Ok("Deleted succesfully");
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, UsersViewModel userVM)
        {
            if (ModelState.IsValid)
            {
                var getData = _context.UserRoles.Include("Role").Include("User").SingleOrDefault(x => x.UserId == id);
                getData.User.UserName = userVM.Username;
                getData.User.Email = userVM.Email;
                getData.User.PhoneNumber = userVM.Phone;
                if (!BCrypt.Net.BCrypt.Verify(userVM.Password, getData.User.PasswordHash))
                {
                    getData.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userVM.Password);
                }
                getData.RoleId = userVM.RoleID;

                _context.UserRoles.Update(getData);
                _context.SaveChanges();
                return Ok("Update success");
            }
            return BadRequest("Something wrong");
        }
    }
}
