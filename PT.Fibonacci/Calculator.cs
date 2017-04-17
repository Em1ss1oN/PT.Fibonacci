using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Fibonacci
{
    public interface ICalculator
    {
        Int32 Calculate(Int32 first, Int32 second);
    }

    public class Calculator : ICalculator
    {
        private const string LessThanZeroErroText = @"Fibonnaci sequence elements can not be less than zero.";

        public Int32 Calculate(Int32 first, Int32 second)
        {
            if (first < 0)
            {
                throw new ArgumentException(LessThanZeroErroText, nameof(first));
            }

            if (second < 0)
            {
                throw new ArgumentException(LessThanZeroErroText, nameof(second));
            }

            if (first == 0 && second == 0)
            {
                return 1;
            }

            try
            {
                checked
                {
                    var result = first + second;
                    return result;
                }
            }
            catch (OverflowException e)
            {
                Logger.Instance.Error($"Error trying to calculate {first} + {second}", e);
                throw new InvalidOperationException(@"Numbers are too large.", e);
            }
        }
    }
}
