using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public required string Description { get; set; }

        public int MyProperty { get; set; }

        //Foreign key (User table PK)
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        [Column("User Id")]
        public required string UserId { get; set; }

        //Foreign key (Post table PK)
        [ForeignKey(nameof(PostId))]
        public virtual Post? Post { get; set; }
        [Column("Post Id")]
        public required int PostId { get; set; }
    }
}
