using Microsoft.AspNetCore.Identity;//
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using SakilaApp.Services;
using SakilaApp.Settings;
using SakilaApp.Services.Payments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<SakilaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>//
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));//

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
                                                 //Cambiar a true  
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailSender, GmailEmailSender>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        });
}
else
{
    builder.Services.AddAuthentication();
}

//Pasarela de Pagos con PayPhone
builder.Services.Configure<PayPhoneSettings>(
    builder.Configuration.GetSection("PayPhone"));

builder.Services.AddHttpClient<PayPhoneApiLinkService>();

//Pasarelas de Pagos con PayPal
builder.Services.Configure<PayPalSettings>(
    builder.Configuration.GetSection("PayPal"));

builder.Services.AddHttpClient<PayPalService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) //Error enviar a la pagina de error 
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    // Aplica automáticamente las migraciones pendientes de Identity
    // al iniciar el contenedor (crea AspNetUsers, AspNetRoles, etc. si no existen)
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await appDb.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    await FilmStockSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();