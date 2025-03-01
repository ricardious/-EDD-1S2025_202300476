using System;

namespace AutoGestPro.src.Models
{
    /// <summary>
    /// Represents a user with fixed-size character arrays for storing personal information.
    /// This struct uses unsafe code to manage fixed-size character arrays and supports linked list structure.
    /// </summary>
    public unsafe struct User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public int ID;
        
        /// <summary>
        /// Fixed-size character array for the user's first name (max 50 characters).
        /// </summary>
        public fixed char Nombres[50];
        
        /// <summary>
        /// Fixed-size character array for the user's last name (max 50 characters).
        /// </summary>
        public fixed char Apellidos[50];
        
        /// <summary>
        /// Fixed-size character array for the user's email (max 50 characters).
        /// </summary>
        public fixed char Correo[50];
        
        /// <summary>
        /// Fixed-size character array for the user's password (max 50 characters).
        /// </summary>
        public fixed char Contrasenia[50];

        /// <summary>
        /// Pointer to the next user in the linked list.
        /// </summary>
        public User* Next;

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> struct with default values.
        /// </summary>
        public User()
        {
            Next = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> struct with specified values.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        public User(int id, string firstName, string lastName, string email, string password)
        {
            ID = id;
            
            // Copy input strings into fixed-size character arrays
            fixed (char* ptr = Nombres) { CopyString(ptr, firstName); }
            fixed (char* ptr = Apellidos) { CopyString(ptr, lastName); }
            fixed (char* ptr = Correo) { CopyString(ptr, email); }
            fixed (char* ptr = Contrasenia) { CopyString(ptr, password); }
            
            Next = null;
        }

        /// <summary>
        /// Copies a string into a fixed-size character array, ensuring null termination.
        /// </summary>
        /// <param name="destination">Pointer to the destination character array.</param>
        /// <param name="source">The source string to copy.</param>
        private static void CopyString(char* destination, string source)
        {
            // Copy characters from source to destination with a limit of 49 characters
            for (int i = 0; i < source.Length && i < 50; i++)
            {
                destination[i] = source[i];
            }
            
            // Ensure null termination to avoid buffer overflow issues
            destination[Math.Min(source.Length, 49)] = '\0';
        }
    }
}
