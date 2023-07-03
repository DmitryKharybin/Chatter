namespace AuthenticationApi.Data
{
    public class UserDataContext : DbContext
    {
        public UserDataContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<GroupCreator> GroupCreators { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<GroupUser>()
                  .HasOne(g => g.Group)
                  .WithMany(u => u.Users)
                  .HasForeignKey(u => u.GroupId)
                  .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<GroupUser>()
                   .HasOne(u => u.User)
                   .WithMany(g => g.Groups)
                   .HasForeignKey(g => g.UserId)
                   .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<Post>()
                       .HasOne(p => p.User)
                       .WithMany(u => u.Posts)
                       .HasForeignKey(p => p.UserId)
                       .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Post>().HasData(
                new
                {
                    Id = 1,
                    Description = "Hello EveryOne, How are you all doing ?",
                    PublishDate = DateTime.Now,
                    UserId = "c7b013f0-5201-4317-abd8-c211f91b7330"
                });
                


            modelBuilder.Entity<User>().HasData(
                  new
                  {
                      Id = "0f8fad5b-d9cb-469f-a165-70867728950e",
                      Name = "John",
                      Gender = Gender.Male,
                      Email = "fakeAdmin@mail.com",
                     
                     
                  },
                   new
                   {
                       Id = "c7b013f0-5201-4317-abd8-c211f91b7330",
                       Name = "Jim",
                       Gender = Gender.Male,
                       Email = "fakeUser@mail.com",
                       
                   }
                );







        }

    }
}
