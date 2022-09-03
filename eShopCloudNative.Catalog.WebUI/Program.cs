using eShopCloudNative.Catalog.Architecture.Configuration;
using eShopCloudNative.Catalog.Services;
using Minio;
using Refit;
using Endpoint = eShopCloudNative.Catalog.Architecture.Configuration.Endpoint;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCaching();

builder.Services
    .AddRefitClient<IProductService>()    
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri($"{builder.Configuration.GetValue<string>("eshop-cloudnative:global:api-gateway")}/catalog");
        c.DefaultRequestHeaders.Add("apikey", builder.Configuration.GetValue<string>("eshop-cloudnative:global:apikey"));
        //TODO: Adicionar vers�o atual!
    });

builder.Services
    .AddRefitClient<ICategoryService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri($"{builder.Configuration.GetValue<string>("eshop-cloudnative:global:api-gateway")}/catalog");
        c.DefaultRequestHeaders.Add("apikey", builder.Configuration.GetValue<string>("eshop-cloudnative:global:apikey"));
        //TODO: Adicionar vers�o atual!
    });



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

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
