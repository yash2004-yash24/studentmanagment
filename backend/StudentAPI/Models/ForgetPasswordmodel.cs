namespace StudentAPI.Models
{
    public class ForgetPasswordmodel
    {
        public string Email { get; set; }
    }

    public class VerifyOtpModel
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
