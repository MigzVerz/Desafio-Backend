namespace DesafioBackend.Api.Models;

public class Rental
{
         public int Id { get; set; }
         public int DriverId { get; set; }
         public int MotorcycleId { get; set; }
         public int Days { get; set; }
         public decimal PricePerDay { get; set; }
         public decimal TotalPrice { get; set; }
         public DateTime CreatedAt { get; set; }
         public DateTime StartDate { get; set; }
         public DateTime EndDate { get; set; }
         public DateTime ExpectedEndDate { get; set; }
         
         
}