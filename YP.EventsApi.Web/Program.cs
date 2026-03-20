using YP.EventApi.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddPresentationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
} 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();