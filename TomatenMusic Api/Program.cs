using TomatenMusic_Api;
using TomatenMusic_Api.Auth.Helpers;
using TomatenMusic_Api.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// configure strongly typed settings object
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<InProcessEventBus>();

builder.Services.AddSingleton<IHostedService, TomatenMusicService>();
builder.Services.AddSingleton<TomatenMusicDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

	app.UseSwagger();
	app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseWebSockets();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();


app.Run();
