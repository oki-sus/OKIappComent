// このファイルも自動生成されたもので、一部変更して使っています

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

//最初の画面を自動生成の初期画面ではなく自作の画面にする
app.MapGet("/", context =>
{
	// 標準のホームページ（Home/Index）へ行く前に割り込み、
    // 強制的にタスク一覧画面（/TaskItems）へブラウザを一瞬で自動転送（リダイレクト）する
	context.Response.Redirect("/TaskItems");
	// 非同期の処理が何の問題もなく無事にすべて完了した、という合図をシステムに返す
	return Task.CompletedTask;
});

// ここまで設定したすべての内容の通りにWEBサーバーを起動し、
// ユーザーからのアクセスを待ち受ける（アプリの本当のスタート位置）
app.Run();
