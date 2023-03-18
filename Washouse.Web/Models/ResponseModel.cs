namespace Washouse.Web.Models
{
    public class ResponseModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
