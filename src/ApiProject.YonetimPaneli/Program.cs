// Bu proje sadece wwwroot klasöründeki HTML, CSS ve JavaScript dosyalarını sunar.
// Panel, verileri tarayıcıdan doğrudan ApiProject.Api'ye istek atarak alır.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
