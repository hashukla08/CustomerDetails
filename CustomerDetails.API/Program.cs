using CustomerDetails.API.DataAccess.Data;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.API.DataAccess.Repository;
using CustomerDetails.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDBContext>(
	options => {
		options.UseSqlite(
			builder.Configuration.GetConnectionString("SQLiteConnection"));
	});

builder.Services.AddControllers().AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddScoped<ICustomerRepository,CustomerRespository>();
builder.Services.AddHttpClient<IProfilePictureService, ProfilePictureService>();
builder.Services.AddScoped<IProfilePictureService, ProfilePictureService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
