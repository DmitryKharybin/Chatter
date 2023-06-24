using Castle.Components.DictionaryAdapter;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    public class Group
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public string? Image { get; set; }

        //public virtual IEnumerable<User>? Users { get; set; }
        public virtual ICollection<GroupUser>? Users { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }


        [ForeignKey(nameof(CreatorOfGroup))]
        public virtual required GroupCreator Creator { get; set; }
        [Column("Creator of group")]
        public int CreatorOfGroup { get; set; }
        //[Column("Creator Of Group")]
        //public int CreatorOfGroup { get; set; }

    }
}
