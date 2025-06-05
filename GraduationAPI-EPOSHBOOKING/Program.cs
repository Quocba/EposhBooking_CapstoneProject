
using GraduationAPI_EPOSHBOOKING.DataAccess;
using Microsoft.Extensions.FileProviders;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GraduationAPI_EPOSHBOOKING.IRepository;
using GraduationAPI_EPOSHBOOKING.Model;
using GraduationAPI_EPOSHBOOKING.Repository;
using GraduationAPI_EPOSHBOOKING.Ultils;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using GraduationAPI_EPOSHBOOKING.BackgroundService;
using GraduationAPI_EPOSHBOOKING.MiddleWare;
using System.Text;
using GraduationAPI_EPOSHBOOKING.DesignPattern;
using Serilog;
using Serilog.Sinks.Discord;
using Serilog.Events;

#pragma warning disable // tắt cảnh báo để code sạch hơn

var builder = WebApplication.CreateBuilder(args);
//var configuration = builder.Configuration;
//string expectedKey = configuration["ApplicationKey:Key"];
//Console.WriteLine("Please Enter Application Key: ");
//string input = ReadPassword();
//string hasInput = Utils.HashPassword(input);
//if (!hasInput.Equals(expectedKey))
//{
//    Console.WriteLine("Application key incorrect");
//    return;
//}
//Console.Clear();
//static string ReadPassword()
//{
//    StringBuilder password = new StringBuilder();
//    ConsoleKeyInfo keyInfo;

//    do
//    {
//        keyInfo = Console.ReadKey(intercept: true);

//        if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
//        {
//            password.Append(keyInfo.KeyChar);
//            Console.Write("*");
//        }
//        else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
//        {
//            password.Remove(password.Length - 1, 1);
//            Console.Write("\b \b");
//        }
//    } while (keyInfo.Key != ConsoleKey.Enter);

//    Console.WriteLine();
//    return password.ToString();
//}
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IFeedbackRepository,FeedBackRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IReportFeedbackRepository, ReportFeedbackRepository>();
builder.Services.AddScoped<Utils>();
builder.Services.AddScoped<GoogleDriveService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddSingleton<AccountSigleton>();
builder.Services.AddSingleton<DBContext>();

// Telegram bot log
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();


// Discord Log

var discordHook = builder.Configuration["Discord:Hook"];
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug().
    WriteTo.Console()
   .WriteTo.Discord(webhookId: 1380141354943774780, webhookToken: "5Ush-Fu7ew3_DCNdHWDiPe_Sn1hgwbKmYxo8iEIafnJr-zWBeB3aEzsxbz9ZV-EuxhhI",
   restrictedToMinimumLevel: LogEventLevel.Error).CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var account = new CloudinaryDotNet.Account(
        configuration["Cloudinary:CloudName"],
        configuration["Cloudinary:ApiKey"],
        configuration["Cloudinary:Secret"]
    );
    return new CloudinaryDotNet.Cloudinary(account);
});
builder.Services.AddDbContext<DBContext>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024 * 1024 * 50; // 50 MB
});


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue; // No limit
});
builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });
builder.Services.AddLogging();

var app = builder.Build();


app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "images")),
        RequestPath = "/images"
    });

app.MapGet("/test-telegram-error", () =>
{
    Log.Error("Đây là lỗi thử nghiệm gửi về Telegram");
    throw new Exception("Đây là Exception test gửi về Telegram");
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
