using ForariaDomain;

namespace Foraria.Domain.Model
{
    public class BlockchainProof
    {
        public int Id { get; set; }

        public int? PollId { get; set; }
        public Poll? Poll { get; set; }

        //  public int? ActaId { get; set; }
        // public Acta? Acta { get; set; }

        //  public int? PaymentId { get; set; }
        //  public Payment? Payment { get; set; }

        public int? CallTranscriptId { get; set; } 
        public CallTranscript? CallTranscript { get; set; }

        public Guid? DocumentId { get; set; }
        public string HashHex { get; set; } = null!;
        public string Uri { get; set; } = null!;
        public string TxHash { get; set; } = null!;
        public string Contract { get; set; } = null!;
        public string Network { get; set; } = "polygon";
        public int ChainId { get; set; } = 80002; // testnet Amoy
        public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
    }
}
