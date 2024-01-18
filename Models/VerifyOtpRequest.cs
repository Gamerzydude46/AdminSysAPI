namespace AdminSysAPI.Models
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public int EnteredOtp { get; set; }
    }
}
