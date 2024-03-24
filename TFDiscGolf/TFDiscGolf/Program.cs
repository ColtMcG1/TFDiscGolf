using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TFDiscGolf.Client.Pages;
using TFDiscGolf.Components;
using TFDiscGolf.Components.Account;
using TFDiscGolf.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddGoogle(o =>
    {
#if DEBUG
        o.ClientId = builder.Configuration["Authentication:Google:ClientId"] ??= "";
        o.ClientSecret = builder.Configuratiouthentication:Google:ClientId"] ??= "";
#else
        o.ClientId = System.Environment.GetEnvironmentVariable("Authentication:Google:ClientId") ?? throw new InvalidOperationException("Client ID not found");
        o.ClientSecret = System.Environment.GetEnvironmentVariable("Authentication:Google:ClientSecret") ?? throw new InvalidOperationException("Client secret not found.");
#endif
        o.CallbackPath = "/api/oauth/google";
    })
    .AddIdentityCookies();

#if DEBUG
var connectionString = builder.Configuration.GetConnectionString("TFDiscGolfContextConnection") ?? throw new InvalidOperationException("Connection string 'TFDiscGolfContextConnection' not found.");
#else
var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings:TFDiscGolfContextConnection");
#endif

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.UseHttpsRedirection();
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseForwardedHeaders();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
