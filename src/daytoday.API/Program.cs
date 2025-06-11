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
using daytoday.API.Mediator.Commands.calendarEvent;
using daytoday.API.Mediator.Commands.task;

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
// Commands and Handlers
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
builder.Services.AddScoped<IRequestHandler<CreateTaskCommand, TaskDto>, CreateTaskCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetAllTaskCommand, List<TaskDto>>, GetAllTaskCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetTaskCommand, TaskDto>, GetTaskCommandHandler>();
builder.Services.AddScoped<IRequestHandler<UpdateTaskCommand, TaskDto>, UpdateTaskCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteTaskCommand, TaskDto>, DeleteTaskCommandHandler>();

// Handlery
// builder.Services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
// builder.Services.AddScoped<INotificationHandler<UserCreatedNotification>, UserCreatedHandler>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
