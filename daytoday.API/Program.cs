using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using daytoday.Core.Models;
using daytoday.API.Data;
using daytoday.API.Core;
using daytoday.API.Mediator;
using daytoday.API.Middleware;
using daytoday.API.Behaviors;
using daytoday.API.Mediator.Commands.project;
using daytoday.Core.DTOs;
using daytoday.API.Controllers;
using daytoday.API.Mediator.Commands.calendarEvent;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Mediator i Pipeline Behaviors
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<IRequestHandler<CreateProjectCommand, Guid>, CreateProjectCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetAllProjectCommand, List<ProjectDto>>, GetAllProjectCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetProjectCommand, ProjectDto>, GetProjectCommandHandler>();
builder.Services.AddScoped<IRequestHandler<UpdateProjectCommand, ProjectDto>, UpdateProjectCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteProjectCommand, ProjectDto>, DeleteProjectCommandHandler>();
builder.Services.AddScoped<IRequestHandler<CreateCalendarEventCommand, CalendarEventDto>, CreateCalendarEventCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetCalendarEventCommand, CalendarEventDto>, GetCalendarEventCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetAllCalendarEventCommand, List<CalendarEventDto>>, GetAllCalendarEventCommandHandler>();
builder.Services.AddScoped<IRequestHandler<UpdateCalendarEventCommand, CalendarEventDto>, UpdateCalendarEventCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteCalendarEventCommand, CalendarEventDto>, DeleteCalendarEventCommandHandler>();

// Add FluentValidation


// Handlery
// builder.Services.AddScoped<IRequestHandler<CreateUserCommand, Guid>, CreateUserCommandHandler>();
// builder.Services.AddScoped<IRequestHandler<GetUserByIdQuery, UserDto>, GetUserByIdQueryHandler>();
// builder.Services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
// builder.Services.AddScoped<INotificationHandler<UserCreatedNotification>, UserCreatedHandler>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
