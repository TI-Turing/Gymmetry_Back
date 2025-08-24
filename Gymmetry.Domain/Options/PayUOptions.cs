namespace Gymmetry.Domain.Options
{
    public class PayUOptions
    {
        public string? ApiKey { get; set; }
        public string? ApiLogin { get; set; }
        public string? MerchantId { get; set; }
        public string? AccountId { get; set; }
        public string BaseUrl { get; set; } = "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi";
        public string CheckoutUrl { get; set; } = "https://sandbox.checkout.payulatam.com/ppp-web-gateway/";
        public string Currency { get; set; } = "COP";
    }
}
