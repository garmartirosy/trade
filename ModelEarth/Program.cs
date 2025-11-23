// Load .env file for environment variables
dotenv.net.DotEnv.Load(options: new dotenv.net.DotEnvOptions(
    envFilePaths: new[] { "../.env", ".env" },
    ignoreExceptions: true
));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register our services for dependency injection
builder.Services.AddScoped<ModelEarth.Services.ICsvImportService, ModelEarth.Services.CsvImportService>();
builder.Services.AddScoped<ModelEarth.Services.ITradeDataRepository, ModelEarth.Services.TradeDataRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
