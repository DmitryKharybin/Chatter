namespace AuthenticationApi.Models
{
    public class Message
    {
        public required string Id { get; set; }


        [ForeignKey(nameof(ReceiverId))]
        public virtual User? Receiver { get; set; }

        public bool IsRead { get; set; }

        [Column("Receiver Id")]
        public required string ReceiverId { get; set; }


        [ForeignKey(nameof(SenderId))]
        public virtual User? Sender { get; set; }

        [Column("Sender Id")]
        public required string SenderId { get; set; }


        public  DateTime? Date { get; set; }

        public required string Body { get; set; }
    }
}
