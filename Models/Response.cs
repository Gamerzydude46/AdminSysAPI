namespace AdminSysAPI.Models
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Users>? Data { get; set; }
    }
}

