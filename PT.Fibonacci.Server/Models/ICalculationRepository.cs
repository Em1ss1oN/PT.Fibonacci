using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PT.Fibonacci.Server.Models
{
    public interface ICalculationRepository : IDisposable
    {
        Task<ICalculation> CreateNew();
        Task<ICalculation> Get(int calculationId);
        Task<bool> Remove(int calculationId);
        Task Update(ICalculation calculation);
    }
}