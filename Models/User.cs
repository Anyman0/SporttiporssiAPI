namespace SporttiporssiAPI.Models
{
    public class User
    {
        public int UserId { get; set; } // Primary key
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
    }
}
