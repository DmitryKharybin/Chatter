using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddAuthentication(auth => {
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  }
).AddJwtBearer(jwt => {
    //var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection());
});



var app = builder.Build();
app.UseRouting();
app.MapControllers();


app.Run();
