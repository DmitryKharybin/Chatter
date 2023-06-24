using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    public class GroupCreator
    {
        public int Id { get; set; }

        //Foreign key (User table PK)
        [ForeignKey(nameof(UserId))]
        public virtual required User User { get; set; }
        [Column("User Id")]
        public required string UserId { get; set; }

        public virtual ICollection<Group>? Groups { get; set; }

        [Column ("Create Date")]
        public DateTime CreateDate { get; set; }

    }
}

