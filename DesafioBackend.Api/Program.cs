using DesafioBackend.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Temporary database
List<Motorcycle> motos = new();
int motoIdCounter = 1;


var motosGroup = app.MapGroup("/motos").WithTags("Motos");

motosGroup.MapPost("/", (Motorcycle moto) =>
{
    if (string.IsNullOrEmpty(moto.Model) || string.IsNullOrEmpty(moto.Plate))
        return Results.BadRequest("Model and Plate are required");

    moto.Id = motoIdCounter++;
    motos.Add(moto);

    if (moto.Year == 2024)
        Console.WriteLine("Motorcycle is from 2024");

    Console.WriteLine($"Motorcycle with plate: {moto.Plate} registered successfully");

    return Results.Created($"/motos/{moto.Id}", moto);
})
.WithName("Register new motorcycle")
.WithSummary("Register a new motorcycle");

motosGroup.MapGet("/", (string? plate) =>
{
    var result = string.IsNullOrWhiteSpace(plate) ? motos
        : motos.Where(m => string.Equals(m.Plate, plate, StringComparison.OrdinalIgnoreCase)).ToList();

    return Results.Ok(result);
})
.WithName("Get motorcycles")
.WithSummary("Get all motorcycles or filter by plate");

motosGroup.MapGet("/{id:int}", (int id) =>
{
    var moto = motos.FirstOrDefault(m => m.Id == id);
    return moto is null ? Results.NotFound() : Results.Ok(moto);
})
.WithName("Get motorcycle by Id");

motosGroup.MapPut("/{id:int}/placa", (int id, Motorcycle updatedMoto) =>
{
    var moto = motos.FirstOrDefault(m => m.Id == id);
    if (moto is null)
        return Results.NotFound();

    moto.Plate = updatedMoto.Plate;
    Console.WriteLine($"Motorcycle information altered to Plate: {updatedMoto.Plate}");
    return Results.Ok(moto);
})
.WithName("Update motorcycle plate");

motosGroup.MapDelete("/{id:int}", (int id) =>
{
    var moto = motos.FirstOrDefault(m => m.Id == id);
    if (moto is null)
        return Results.NotFound();

    motos.Remove(moto);
    Console.WriteLine($"Motorcycle with id: {id} deleted");
    return Results.NoContent();
})
.WithName("Delete motorcycle");


List<Driver> drivers = new();
int driverIdCounter = 1;

var driversGroup = app.MapGroup("/entregadores").WithTags("Entregadores");

driversGroup.MapPost("/", (Driver driver) =>
{
    driver.Id = driverIdCounter++;

    if (string.IsNullOrEmpty(driver.Name))
        return Results.BadRequest("Name is required");

    if (driver.Cnpj == 0 || driver.Cnpj.ToString().Length < 14 || drivers.Any(d => d.Cnpj == driver.Cnpj))
        return Results.BadRequest("CNPJ is invalid or already registered");

    if (driver.Cnh == 0 || driver.Cnh.ToString().Length < 11 || drivers.Any(d => d.Cnh == driver.Cnh))
        return Results.BadRequest("CNH is invalid or already registered");

    if (!new[] { "A", "B", "A+B" }.Contains(driver.CnhType))
        return Results.BadRequest("CNH Type must be A, B or A+B");

    drivers.Add(driver);

    Console.WriteLine("Driver registered");
    return Results.Created($"/entregadores/{driver.Id}", driver);
})
.WithName("Register new driver")
.WithSummary("Register a new delivery driver");

var rentalGroup = app.MapGroup("/locação").WithTags("Locação");

List<Rental> rentals = new();
int rentalIdCounter = 1;

var rentalPlans = new Dictionary<int, decimal>
{
	{7, 30m},
	{15, 28m},
	{30, 22m},
	{45, 20m},
	{50, 18m}
};

rentalGroup.MapPost("/", (int driverId, int motoId, int days) =>
{
	if (!rentalPlans.ContainsKey(days))
        return Results.BadRequest("Invalid rental plan");
	
	var driver = drivers.FirstOrDefault(d => d.Id == driverId);
	if (driver is null)
    	return Results.NotFound("Driver not found");

	 if (driver.CnhType != "A" && driver.CnhType != "A+B")
        return Results.BadRequest("Driver is not allowed to rent motorcycles");

	 var moto = motos.FirstOrDefault(m => m.Id == motoId);
    if (moto is null)
        return Results.NotFound("Motorcycle not found");

	var createdAt = DateTime.Now;
    var startDate = createdAt.AddDays(1);
    var endDate = startDate.AddDays(days);

	var rental = new Rental
    {
        Id = rentalIdCounter++,
        DriverId = driverId,
        MotorcycleId = motoId,
        Days = days,
        PricePerDay = rentalPlans[days],
        TotalPrice = rentalPlans[days] * days,
        CreatedAt = createdAt,
        StartDate = startDate,
        EndDate = endDate,
        ExpectedEndDate = endDate
    };

    rentals.Add(rental);

    return Results.Created($"/locacoes/{rental.Id}", rental);
	
}).WithName("Create Rental");

rentalGroup.MapPatch("/{id}/devolucao", (int id, DateTime returnDate) =>
{
    var rental = rentals.FirstOrDefault(r => r.Id == id);
    if (rental is null)
        return Results.NotFound("Rental not found");

    decimal finalPrice = rental.TotalPrice;

    if (returnDate < rental.ExpectedEndDate)
    {
        int usedDays = (returnDate - rental.StartDate).Days;
        if (usedDays < 0) usedDays = 0; 

        decimal usedValue = usedDays * rental.PricePerDay;
        int unusedDays = rental.Days - usedDays;

        decimal penaltyRate = rental.Days switch
        {
            7 => 0.20m,
            15 => 0.40m,
            _ => 0m
        };

        decimal penalty = unusedDays * rental.PricePerDay * penaltyRate;
        finalPrice = usedValue + penalty;
    }
    else if (returnDate > rental.ExpectedEndDate)
    {
        int extraDays = (returnDate - rental.ExpectedEndDate).Days;
        finalPrice += extraDays * 50m;
    }

    rental.EndDate = returnDate;
    rental.TotalPrice = finalPrice;

    return Results.Ok(rental);
});


app.Run();

