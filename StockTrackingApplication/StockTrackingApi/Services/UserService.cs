using StockTrackingAuthAPI.Config;
using StockTrackingAuthAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace StockTrackingAuthAPI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        //private readonly IMongoCollection<RoleAccess> _roleAccessCollection;

        public UserService(IOptions<MongoDbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

            _userCollection = mongoDatabase.GetCollection<User>("Users");
            //_roleAccessCollection = mongoDatabase.GetCollection<RoleAccess>("RoleAccess");
        }

        // public async Task<List<string>> GetAllRoleNamesAsync()
        // {
        //     var roles = await _roleAccessCollection.Find(_ => true).ToListAsync();
        //     return roles.Select(r => r.RoleName).ToList();
        // }

        public async Task<List<User>> GetAllAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(CreateUserRequest request)
        {
            var existing = await _userCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (existing != null)
                throw new ArgumentException("Email already exists.");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            if (!IsStrongPassword(request.Password))
                throw new ArgumentException("Password must be at least 6 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.");

            //var validRoles = await GetAllRoleNamesAsync();
            

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = hashedPassword,
                
            };

            await _userCollection.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(string id, CreateUserRequest request)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found.");

            var existing = await _userCollection.Find(u => u.Email == request.Email && u.Id != id).FirstOrDefaultAsync();
            if (existing != null)
                throw new ArgumentException("Email already exists.");

            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            if (!IsStrongPassword(request.Password))
                throw new ArgumentException("Password must be at least 6 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.");

            

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var updatedUser = new User
            {
                Id = id,
                UserName = request.UserName,
                Email = request.Email,
                Password = hashedPassword,
               
            };

            await _userCollection.ReplaceOneAsync(u => u.Id == id, updatedUser);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found.");

            await _userCollection.DeleteOneAsync(u => u.Id == id);
        }

        private bool IsStrongPassword(string password)
        {
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
