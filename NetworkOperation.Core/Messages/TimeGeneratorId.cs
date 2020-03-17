using System;

namespace NetworkOperation.Core.Messages
{
    public class TimeGeneratorId : IGeneratorId
    {
        public int Generate()
        {
            return (int) DateTime.UtcNow.Ticks;
        }
    }
}