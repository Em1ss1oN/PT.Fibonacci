using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace PT.Fibonacci.Server.Models
{
    public class Calculation : ICalculation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        public Int32 Current { get; set; }
    }

    public class CalculationContext : DbContext
    {
        public DbSet<Calculation> Calcultaions { get; set; }
    }

    public class CalculationContextInitializer : CreateDatabaseIfNotExists<CalculationContext>
    {
        
    }
}