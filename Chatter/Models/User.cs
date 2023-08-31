using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    public class User
    {
        [Column(TypeName = "varchar")]
        [MaxLength(36)]
        public required string Id { get; set; }

        public required string Name { get; set; }

        public Gender Gender { get; set; }

        public required string Email { get; set; }

        public byte[]? Image { get; set; }


        // A user can either send a request or have a collection of requests sent to him 
       // public virtual FriendRequest? SentRequest { get; set; }
        public virtual ICollection<FriendRequest>? FriendRequests { get; set; }
        public virtual ICollection<Message>? ReceivedMessages { get; set; }
        public virtual ICollection<Message>? SentMessages { get; set; }
        public virtual ICollection<User>? Friends { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<GroupUser>? Groups { get; set; }

        public virtual IEnumerable<Comment>? Comments { get; set; }

    }
}
