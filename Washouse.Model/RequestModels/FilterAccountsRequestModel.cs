namespace Washouse.Model.RequestModels
{
    public class FilterAccountsRequestModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchString { get; set; }
    }
}