using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Models;
using System.Security.Cryptography;
using System.Text;

namespace SeguimientoCriptomonedas.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext dbContext,  ILogger<AccountController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        public IActionResult Register() => View();
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "El email ya está registrado.");
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return View(model);
            }

            try
            {
                var user = new User
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    PasswordHash = HashPassword(model.Password),
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registro exitoso. ¡Ahora puedes iniciar sesión!";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                ModelState.AddModelError("", "Ocurrió un error al registrar el usuario. Inténtalo más tarde.");
                return View(model);
            }
        }
        
        public IActionResult Login() => View();
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLower();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.PasswordHash != HashPassword(model.Password))
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
                return View(model);
            }

            // Guardar sesión segura
            HttpContext.Session.SetString("LoggedUser", user.Username);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["LoggedUser"] = user.Username;

            _logger.LogInformation($"Usuario {user.Email} inició sesión correctamente.");

            return RedirectToAction("Index", "Home");
        }
        
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("LoggedUser");
            return RedirectToAction(nameof(Login));
        }
    }
}
