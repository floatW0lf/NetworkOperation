using System;

namespace NetworkOperation
{
    public class TimeGeneratorId : IGeneratorId
    {
        public int Generate()
        {
            return (int) DateTime.UtcNow.Ticks;
        }
    }
}