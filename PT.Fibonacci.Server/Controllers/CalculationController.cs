using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PT.Fibonacci.Server.Models;

namespace PT.Fibonacci.Server.Controllers
{
    public class CalculationController : ApiController
    {
        private readonly ICalculationRepository _repository;
        private readonly ICalculator _calculator;
        private readonly IResponseSender _responseSender;

        public CalculationController(ICalculationRepository repository, ICalculator calculator, IResponseSender responseSender)
        {
            _repository = repository;
            _calculator = calculator;
            _responseSender = responseSender;
        }

        // POST <controller>/new
        [HttpGet]
        [Route(@"calculation/new")]
        public async Task<IHttpActionResult> CreateNewCalculation()
        {
            Logger.Instance.Info($"Processing CreateNewCalculation().");
            try
            {
                var calculation = await _repository.CreateNew();
                return Ok(calculation.Id);
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Unknown error on CreateNewCalculation().", e);
                return InternalServerError(e);
            }
        }

        // POST <controller>/<id>/5
        [HttpPost]
        public async Task<IHttpActionResult> CalculateNext(int id, int current)
        {
            Logger.Instance.Info($"Processing CalculateNext({id}, {current}).");
            try
            {
                var calculation = await _repository.Get(id);

                if (calculation == null)
                {
                    return NotFound();
                }

                int next;
                var previous = calculation.Current;
                try
                {
                    next = _calculator.Calculate(previous, current);
                }
                catch (InvalidOperationException)
                {
                    return StatusCode(HttpStatusCode.ResetContent);
                }

                calculation.Current = next;
                await _repository.Update(calculation);
                await _responseSender.SendResponseAsync(calculation);
                return Ok();
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Unknown error on CalculateNext({id}, {current}).", e);
                return InternalServerError(e);
            }
        }

        // DELETE <controller>/<id>
        [HttpDelete]
        public async Task<IHttpActionResult> CompleteCalculation(int id)
        {
            Logger.Instance.Info($"Processing CompleteCalculation({id}).");
            try
            {
                if (await _repository.Remove(id))
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Unknown error on CompleteCalculation({id}).", e);
                return InternalServerError(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}