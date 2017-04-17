using System;

namespace PT.Fibonacci
{
    public interface ICalculation
    {
        Int32 Id { get; }
        Int32 Current { get; set; }
    }
}