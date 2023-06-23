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

// !! Need more researching on the topic !!
builder.Services.AddEndpointsApiExplorer();






//Add Identity DbContext
builder.Services.AddDbContext<AuthenticationContext>(option =>
option.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

//Add SocialWeb DbContext
builder.Services.AddDbContext<UserDataContext>(option =>
option.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SocialWebsiteDb")));




var app = builder.Build();


app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials()); // allow credentials


//Todo: Add SocialWebsiteDb//

if (app.Environment.IsDevelopment())
{

}




using (var scope = app.Services.CreateScope())
{
    var authenticationContext = scope.ServiceProvider.GetRequiredService<AuthenticationContext>();


    authenticationContext.Database.EnsureDeleted();
    authenticationContext.Database.EnsureCreated();

    var userDataContext = scope.ServiceProvider.GetRequiredService<UserDataContext>();

    userDataContext.Database.EnsureDeleted();
    userDataContext.Database.EnsureCreated();


}

app.UseHttpsRedirection();

//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<UserDataContext>();
//    context.Database.EnsureDeleted();
//    context.Database.EnsureCreated();
//}



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization(); //<< This needs to be between app.UseRouting(); and app.UseEndpoints();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

app.MapControllers();

app.Run();
