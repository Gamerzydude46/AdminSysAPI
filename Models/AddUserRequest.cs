namespace AdminSysAPI.Models
{
    public class AddUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
