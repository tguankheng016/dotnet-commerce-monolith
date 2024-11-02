using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
	// Service provider validation
	// Used for check got any DI misconfigured
	options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
	options.ValidateOnBuild = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	// app.UseSwagger();
	// app.UseSwaggerUI();
	app.UseSwagger(options =>
	{
		options.RouteTemplate = "/openapi/{documentName}.json";
	});
	app.MapScalarApiReference();
}

app.Run();