using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Services;
using Portfolio_Financeiro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// INJEÇÃO DE DEPENDÊNCIA (Necessário para os Logs e DataContext)
// ============================================================

// Singleton: Lê o SeedData.json do disco apenas UMA vez quando a API liga
builder.Services.AddSingleton<DataContext>();

// Scoped: Ele cria uma única instância do serviço por requisição HTTP.
builder.Services.AddScoped<IPerformanceCalculator, PerformanceCalculator>();
builder.Services.AddScoped<IRebalancingCalculator, RebalancingCalculator>();
builder.Services.AddScoped<IRiskAnalysisCalculator, RiskAnalysisCalculator>();

// ============================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();