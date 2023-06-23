namespace AuthenticationApi.Data
{
    public class UserDataContext : DbContext
    {
        public UserDataContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<User>().HasData(
        //        new User
        //        {

        //            Id = Guid.NewGuid().ToString(),
        //            Name = "name",
        //            Email = "email",
        //        });

        //}
    }
}
