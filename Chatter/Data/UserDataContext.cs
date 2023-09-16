namespace AuthenticationApi.Data
{
    public class UserDataContext : DbContext
    {

        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment env;


        public UserDataContext(DbContextOptions options, Microsoft.AspNetCore.Hosting.IHostingEnvironment env) : base(options)
        {
            this.env = env;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<FriendRequest> FriendRequests { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<GroupCreator> GroupCreators { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }
        public virtual DbSet<Message> Messages { get; set; }

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

            modelBuilder.Entity<FriendRequest>()
                .HasOne(u => u.Receiver)
                .WithMany(f => f.FriendRequests)
                .HasForeignKey(k => k.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //modelBuilder.Entity<FriendRequest>()
            //    .HasOne(u => u.Sender)
            //    .WithOne(f => f.SentRequest)
            //    .HasForeignKey<FriendRequest>(f => f.SenderId)
            //    .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<Message>()
                .HasOne(u => u.Receiver)
                .WithMany(s => s.ReceivedMessages)
                .HasForeignKey(k => k.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Message>()
               .HasOne(u => u.Sender)
               .WithMany(s => s.SentMessages)
               .HasForeignKey(k => k.SenderId)
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
                      Image = SeedImageToByeArr("user1.jpg")


                  },
                   new
                   {
                       Id = "c7b013f0-5201-4317-abd8-c211f91b7330",
                       Name = "Jim",
                       Gender = Gender.Male,
                       Email = "fakeUser@mail.com",
                       Image = SeedImageToByeArr("user2.jpg")

                   }
                );


        }

        //Create image byte array for image seed
        private byte[] SeedImageToByeArr(string imageName)
        {
            string defaultImagePath = Path.Combine(env.WebRootPath, "Pictures", imageName);
            byte[] imageBytes;

            using (FileStream fileStream = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);

                    imageBytes = memoryStream.ToArray();
                }
            }


            return imageBytes;
        }

    }
}
