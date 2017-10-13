namespace CHC.Consent.WebApi.Abstractions
{
    public interface IPagingProperties
    {
        int Page { get; set; }
        int PageSize { get; set; }
    }
}