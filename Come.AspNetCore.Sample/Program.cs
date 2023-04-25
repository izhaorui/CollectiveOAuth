
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc();

var app = builder.Build();
//�������ʾ�̬�ļ�/wwwrootĿ¼�ļ���Ҫ����UseRoutingǰ��
app.UseStaticFiles();

//����·�ɷ���
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();