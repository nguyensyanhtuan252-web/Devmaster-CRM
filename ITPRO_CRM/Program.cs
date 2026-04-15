using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ITPRO_CRM.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ITPRO_CRMContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ITPRO_CRMContext") ?? throw new InvalidOperationException("Connection string 'ITPRO_CRMContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
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
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
