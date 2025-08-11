using Fuddo.Models;
using Fuddo.Repository.Interface;
using Fuddo.Repository.SQLImpl;
using Fuddo.Service.Email;
using Fuddo.Service.ImplV1;
using Fuddo.Service.Interface;
using Fuddo.Services.Email;
using Microsoft.EntityFrameworkCore;

 var builder = WebApplication.CreateBuilder(args);

// 🔹 Thêm DbContext
builder.Services.AddDbContext<FuddoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));


// 🔹 Thêm Service và Repository
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IPasswordResetTokenRepo, PasswordResetTokenRepoSQL>();
builder.Services.AddTransient<IEmailVerificationTokenRepo, EmailVerificationTokenRepoSQL>();
builder.Services.AddScoped<IProductRepo, ProductRepoSQL>();
builder.Services.AddScoped<IProductService, ProductServiceV1>();
builder.Services.AddScoped<IUserRepo, UserRepoSQL>();
builder.Services.AddScoped<IUserService, UserServiceV1>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepoSQL>();
builder.Services.AddScoped<ICategoryService, CategoryServiceV1>();
builder.Services.AddScoped<IOrderRepo, OrderRepoSQL>();
builder.Services.AddScoped<IOrderService, OrderServiceV1>();
builder.Services.AddScoped<ICartRepo, CartRepoSQL>();
builder.Services.AddScoped<ICartService, CartServiceV1>();

// 🔹 Thêm session và Memory Cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "account";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });
builder.Services.AddAuthorization();

// 🔹 Thêm MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// 🔹 Middleware pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Nếu có auth
app.UseAuthorization();



// 🔹 Mặc định route controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

