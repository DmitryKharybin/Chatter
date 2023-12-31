﻿using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationApi.Models
{
    public class Post
    {
        public int Id { get; set; }


        public required string Description { get; set; }

        //Foreign key (Group table PK)
        [ForeignKey(nameof(GroupId))]
        public virtual Group? Group { get; set; }
        [Column("Posted In")]
        public int? GroupId { get; set; }

        [Column("Publish Date")]
        public DateTime PublishDate { get; set; }

        //Foreign key (User table PK)
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        [Column("User Id")]
        public required string UserId { get; set; }

        public virtual IEnumerable<Comment>? Comments { get; set; }

    }
}
