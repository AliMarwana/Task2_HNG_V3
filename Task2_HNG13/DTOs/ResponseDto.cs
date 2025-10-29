namespace Task2_HNG13.DTOs
{
    public class ResponseDto
    {
        public string Error { get; set; }
        public string Details { get; set; }
        public Dictionary<string, string> DetailsValidation{ get; set; }
    }
}
