using System;

namespace NetworkOperation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlerAttribute : Attribute
    {
        public Scope LifeTime { get; set; }
    }
}