using AdminSysAPI.Models;
using AdminSysAPI.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;


namespace AdminSysAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly dBContext _dBContext;

        public UsersController()
        {
            _dBContext = new dBContext();
        }

        [HttpPost]
        [Route("registration")]
        public IActionResult Registration(Users user)
        {
            try
            {
                // Check if the user already exists
                if (_dBContext.Users.Any(u => u.UserName == user.UserName || u.Email == user.Email))
                {
                    var res = new Response
                    {
                        Success = false,
                        Message = "User already exists"
                    };

                    return Conflict(res); 
                }

                user.Password = Services.PasswordHasher.HashPassword(user.Password);

                _dBContext.Users.Add(user);
                _dBContext.SaveChanges();

                var response = new Response
                {
                    Success = true,
                    Message = "User account created successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
                return StatusCode(StatusCodes.Status500InternalServerError,response);
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(Login login) {

            try
            {
                // Find the user by username
                var user = _dBContext.Users.SingleOrDefault(u => u.UserName == login.UserName);

                // Check if the user exists and the password is correct
                if (user != null && Services.PasswordHasher.VerifyPassword(login.Password, user.Password))
                {
                    // Instantiate OtpGenerator
                    OtpGenerator otpGenerator = new OtpGenerator();

                    // Call GenerateFourDigitOTP method
                    int otp = otpGenerator.GenerateFourDigitOTP();

                    // Store the OTP temporarily 
                    var otpLog = new OTPLog
                    {
                        UID = user.Id,
                        OTP = otp
                    };

                    _dBContext.OTPLog.Add(otpLog);
                    _dBContext.SaveChanges();

                    // Send the OTP to the user's email
                    EmailService emailService = new EmailService();

                    Response reply = emailService.SendOtpEmail(user.Email, otp, null);


                    var response = new Response
                    {
                        Success = true,
                        Message = $"Login successful: {reply.Message} "
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new Response
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                var response = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            
        }

        [HttpPost]
        [Route("verifyotp")]
        public IActionResult VerifyOtp(VerifyOtpRequest verifyOtpRequest)
        {
            try
            {
                // Find the user by username
                var user = _dBContext.Users.SingleOrDefault(u => u.Email == verifyOtpRequest.Email);      

                // Check if the user exists
                if (user == null)
                {
                    // Return a structured error response
                    var response = new Response
                    {
                        Success = false,
                        Message = "User not found"
                    };

                    return NotFound(response);
                }

                var otpLog = _dBContext.OTPLog.SingleOrDefault(o => o.UID == user.Id);

                // Check if the temporary identifier exists
                if (verifyOtpRequest.EnteredOtp == otpLog.OTP)
                {
                    // Remove the OTP from temporary storage after successful verification
                    _dBContext.OTPLog.Remove(otpLog);
                    _dBContext.SaveChanges();

                    // Return a structured response
                    var response = new Response
                    {
                        Success = true,
                        Message = "OTP verification successful"
                    };

                    return Ok(response);
                }

                // Return a structured error response
                var errorResponse = new Response
                {
                    Success = false,
                    Message = "Invalid OTP or temporary identifier"
                };

                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // Return a structured error response
                var response = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("getallusers")]
        public IActionResult GetAllUsers()
        {
            try
            {
                // Retrieve all users from the database
                var allUsers = _dBContext.Users.ToList();

                // Return a structured response
                var response = new Response
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = allUsers  
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Return a structured error response
                var errorResponse = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPut]
        [Route("updateuser")]
        public IActionResult UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
        {
            try
            {
                // Find the user by ID
                var user = _dBContext.Users.SingleOrDefault(u => u.Id == updateUserRequest.Id);

                // Check if the user exists
                if (user == null)
                {
                    // Return a structured error response
                    var res = new Response
                    {
                        Success = false,
                        Message = "User not found"
                    };

                    return NotFound(res);
                }

                // Update only the allowed properties
                user.UserName = updateUserRequest.UserName;
                user.Email = updateUserRequest.Email;
                user.IsActive = updateUserRequest.IsActive;

                // Save changes to the database
                _dBContext.SaveChanges();

                // Return a structured response
                var response = new Response
                {
                    Success = true,
                    Message = "User Details updated successfully",
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Return a structured error response
                var errorResponse = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpDelete]
        [Route("deleteuser")]
        public IActionResult DeleteUser([FromBody] DeleteUserRequest deleteUserRequest)
        {
            try
            {
                // Find the user by ID
                var user = _dBContext.Users.SingleOrDefault(u => u.Id == deleteUserRequest.Id);

                // Check if the user exists
                if (user == null)
                {
                    // Return a structured error response
                    var res = new Response
                    {
                        Success = false,
                        Message = "User not found"
                    };

                    return NotFound(res);
                }

                // Remove the user from the database
                _dBContext.Users.Remove(user);
                _dBContext.SaveChanges();

                // Return a structured response
                var response = new Response
                {
                    Success = true,
                    Message = "User deleted successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Return a structured error response
                var errorResponse = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPost]
        [Route("adduser")]
        public IActionResult AddUser([FromBody] AddUserRequest addUserRequest)
        {
            try
            {
                var newPassword = PasswordGenerator.GeneratePassword();

                // Send the Password to the user's email
                EmailService emailService = new EmailService();

                Response reply = emailService.SendOtpEmail(addUserRequest.Email, null, newPassword);

                // Create a new user
                var newUser = new Users
                {
                    UserName = addUserRequest.UserName,
                    Email = addUserRequest.Email,
                    IsActive = addUserRequest.IsActive,
                    Password = Services.PasswordHasher.HashPassword(newPassword)
            };

                // Add the new user to the database
                _dBContext.Users.Add(newUser);
                _dBContext.SaveChanges();

                // Return a structured response
                var response = new Response
                {
                    Success = true,
                    Message = "New Admin added successfully",
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Return a structured error response
                var errorResponse = new Response
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

    }
}
