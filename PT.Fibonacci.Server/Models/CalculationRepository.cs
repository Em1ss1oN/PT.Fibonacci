using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace PT.Fibonacci.Server.Models
{
    public class CalculationRepository : ICalculationRepository
    {
        private readonly CalculationContext _context;

        public CalculationRepository(CalculationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        public async Task<ICalculation> CreateNew()
        {
            var newItem = _context.Calcultaions.Create();
            newItem.Current = 0;
            _context.Calcultaions.Add(newItem);

            await _context.SaveChangesAsync();
            return newItem;
        }

        public async Task<ICalculation> Get(int calculationId)
        {
            return await _context.Calcultaions.FindAsync(calculationId);
        }

        public async Task<bool> Remove(int calculationId)
        {
            var calcualtion = await _context.Calcultaions.FindAsync(calculationId);
            if (calcualtion == null)
            {
                return false;
            }

            _context.Calcultaions.Remove(calcualtion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task Update(ICalculation calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            var entity = calculation as Calculation;
            if (entity == null)
            {
                entity = await _context.Calcultaions.FindAsync(calculation.Id);
                if (entity == null)
                {
                    throw new InvalidOperationException(@"Can not update entity that does not exist.");
                }

                entity.Current = calculation.Current;
            }
            else
            {
                _context.Calcultaions.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}