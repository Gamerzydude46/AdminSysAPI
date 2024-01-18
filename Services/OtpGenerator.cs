
namespace AdminSysAPI.Services
{
    public class OtpGenerator
    {
        private Random random = new Random();

        public  int GenerateFourDigitOTP()
        {
            return random.Next(1000, 9999);
        }
    }
}
