namespace AuthenticationApi.Data
{
    public class AuthenticationContext : IdentityDbContext
    {
        public AuthenticationContext(DbContextOptions<AuthenticationContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedUsers(builder);
            SeedRoles(builder);
        }

        private void SeedUsers(ModelBuilder builder)
        {

            builder.Entity<IdentityUser>().HasData(

                  new IdentityUser()
                  {
                      Id = "0f8fad5b-d9cb-469f-a165-70867728950e",
                      UserName = "Admin",
                      NormalizedUserName = "Admin",
                      PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(null, "Aa1!234567"),
                  },
                  
                  new IdentityUser()
                  {
                      Id = "c7b013f0-5201-4317-abd8-c211f91b7330",
                      UserName = "user",
                      NormalizedUserName = "User",
                      PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(null, "Aa1!234567"),
                  }
                );
        }


        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "0f8fad5b-d9cb-469f-a165-70867728950e", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Id = "c7b013f0-5201-4317-abd8-c211f91b7330", Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
                );
        }
    }
}
