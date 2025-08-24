namespace Gymmetry.Domain.Options
{
    public class PaymentsOptions
    {
        public string? BaseSuccessUrl { get; set; }
        public string? BaseFailureUrl { get; set; }
        public string? BasePendingUrl { get; set; }
        public int GatewayProvider { get; set; } = 3; // 1=PayU,2=Wompi,3=MercadoPago,4=Stripe
        public int PendingTTLMinutes { get; set; } = 60; // tiempo por defecto para expirar intents pendientes
    }
}
