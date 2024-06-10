using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using QuailtyForm.Data;
using static System.Net.Mime.MediaTypeNames;
using QuailtyForm.Helpers;


namespace QuailtyForm.Controllers
{
    public class LoginController : Controller
    {
        string text;
        public IActionResult Login()
        {
            return View();
        }

        private readonly IConfiguration _configuration;


        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public ActionResult LoginInsert(string username, string password)
        {
            bool isValidUser = false;

            // Retrieve the Oracle database connection string  private string ConnectionString { get; set; }

            text = HashHelper.HashPassword(password);

            var connectionString = _configuration.GetConnectionString("OracleDbConnection");
            //connectionString = 

            // Using Oracle.DataAccess.Client
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM users WHERE US_USERNAME=:username AND US_PASSWORD=:text";

                OracleCommand command = new OracleCommand(query, connection);
                command.Parameters.Add(new OracleParameter("username", username));
                command.Parameters.Add(new OracleParameter("password", text));

                try
                {
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 1)
                    {
                        isValidUser = true;
                    }
                    connection.Close();
                }
                catch (OracleException ex)
                {
                    // Bağlantı veya sorgu sırasında oluşan Oracle hatalarını yakalayın
                    ViewBag.Error = "Database connection error. Please try again later.";
                    //return View("Login");
                    return RedirectToAction("login", "Login");
                }
                catch (Exception ex)
                {
                    // Genel hataları yakalayın
                    ViewBag.Error = "An unexpected error occurred. Please try again later.";
                    //return View("Login");
                    return RedirectToAction("login", "Login");

                }
                finally
                {
                    connection.Close();
                }
            }
            if (isValidUser)
            {
                // Kullanıcı adını session'a kaydedin
                HttpContext.Session.SetString("User", username);
                return RedirectToAction("Index", "Main");
                //return RedirectToAction("Index", "Form");
            }
            else
            {
                ViewBag.Error = "Invalid Credentials";
                return RedirectToAction("login", "Login");
            }
        }
    }
}