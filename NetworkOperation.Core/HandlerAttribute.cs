using System;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlerAttribute : Attribute
    {
        public Scope LifeTime { get; set; }
    }
}