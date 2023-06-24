using AuthenticationApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    [PrimaryKey(nameof(UserId), nameof(GroupId))]
    public class GroupUser
    {
        public required virtual User User { get; set; }
        public required virtual Group Group { get; set; }


        [Column("User Id", Order = 0)]
        public required string UserId { get; set; }

        [Column("Group Id", Order = 1)]
        public required int GroupId { get; set; }

    }
}


