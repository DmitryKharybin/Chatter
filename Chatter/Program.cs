var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

//User Data repository
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();

builder.Services.AddControllers().AddJsonOptions(j =>
j.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);



//builder.Services.AddEndpointsApiExplorer(); //?? Need to research

builder.Services.AddIdentity<IdentityUser, IdentityRole>(identity =>
{
    identity.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<AuthenticationContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IJwtService, JwtService>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Add Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        //TODO: after settting the client (React), Add Audience & Issuer to it
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))

        
    };
});

builder.Services.AddEndpointsApiExplorer();



//Add Identity DbContext
builder.Services.AddDbContext<AuthenticationContext>(option =>
option.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

//Add SocialWeb DbContext
builder.Services.AddDbContext<UserDataContext>(option =>
option.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SocialWebsiteDb")));




var app = builder.Build();



//During development , both data bases (user data & authentication) be deleted and recreated 
//Cors are open to all during development

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var authenticationContext = scope.ServiceProvider.GetRequiredService<AuthenticationContext>();


        authenticationContext.Database.EnsureDeleted();
        authenticationContext.Database.EnsureCreated();

        var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();

        userDataContext.Database.EnsureDeleted();
        userDataContext.Database.EnsureCreated();
    }



    app.UseCors(x => x
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .SetIsOriginAllowed(origin => true) // allow any origin
                   .AllowCredentials()); // allow credentials
}

//In production , check that both data bases , exist
else
{
    using (var scope = app.Services.CreateScope())
    {
        var authenticationContext = scope.ServiceProvider.GetRequiredService<AuthenticationContext>();
        var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();

        authenticationContext.Database.EnsureCreated();
        userDataContext.Database.EnsureCreated();


    }

    //TODO: add cors policy for production
}


app.UseHttpsRedirection();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
