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

        public required string Email { get; set; }

        public string? Image { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<GroupUser>? Groups { get; set; }
        //public virtual IEnumerable<Group>? Group { get; set; }


    }
}
