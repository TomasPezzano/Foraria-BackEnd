namespace Foraria.DTOs
{
    public class MercadoPagoPaymentDto
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public decimal? TransactionAmount { get; set; }
        public int? Installments { get; set; }
        public TransactionDetailsDto? TransactionDetails { get; set; }
        public object? Metadata { get; set; }
        public MercadoPagoOrderDto? Order { get; set; }
        public string? PaymentMethodId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateApproved { get; set; }
    }


}
