
namespace AuthenticationApi.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }

        
        [ForeignKey(nameof(ReceiverId))]
        public virtual  User? Receiver { get; set; }

        [Column("Receiver Id")]
        public required string ReceiverId { get; set; }

        
        [ForeignKey(nameof(SenderId))]
        public virtual  User? Sender { get; set; }

        [Column("Sender Id")]
        public required string SenderId { get; set; }


        public required DateTime Date { get; set; }
    }
}
