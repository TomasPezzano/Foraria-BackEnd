namespace ForariaDomain.Models
{
    public class MercadoPagoPayment
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public decimal? TransactionAmount { get; set; }
        public int? Installments { get; set; }
        public TransactionDetails? TransactionDetails { get; set; }
        public object? Metadata { get; set; }
        public MercadoPagoOrder? Order { get; set; }
        public string? PaymentMethodId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateApproved { get; set; }
    }
}

