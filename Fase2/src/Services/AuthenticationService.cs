using AutoGestPro.src.Models;
using AutoGestPro.src.Services.Interfaces;

namespace AutoGestPro.src.Services
{
    /// <summary>
    /// Service responsible for user authentication within the system.
    /// </summary>
    public class AuthenticationService
    {
        // Dependency on the user service interface
        private readonly IUserService userService;

        // Hardcoded admin credentials
        private const string AdminEmail = "admin@usac.com";
        private const string AdminPassword = "admin123";

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="userService">Service to access user data.</param>
        public AuthenticationService(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Authenticates a user based on their email and password.
        /// Returns a User object if the credentials are valid; otherwise, returns null.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's password.</param>
        /// <returns>A User object if authentication is successful; otherwise, null.</returns>
        public User Authenticate(string email, string password)
        {
            // Check for empty or null input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            // Check if credentials match the hardcoded admin account
            if (email == AdminEmail && password == AdminPassword)
            {
                // Return a new admin user instance
                return new User(0, "Administrador", "", email, password);
            }

            // Attempt to find a user with the provided credentials
            User usuario = userService.FindByCredentials(email, password);
            return usuario;
        }
    }
}
