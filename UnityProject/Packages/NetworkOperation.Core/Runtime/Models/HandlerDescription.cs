using System;
using System.Collections.Generic;

namespace NetworkOperation.Core.Models
{
    public enum Scope : byte
    {
        Single,
        Session,
        Request
    }
    public class HandlerDescription
    {
        public HandlerDescription(Scope lifeTime)
        {
            LifeTime = lifeTime;
        }
        public Scope LifeTime { get; } 
    }

    public class DescriptionRuntimeModel
    {
        private Dictionary<Type,HandlerDescription> _descriptions = new Dictionary<Type, HandlerDescription>();
        private static HandlerDescription Default = new HandlerDescription(Scope.Single);
        private bool _freezee;
        public void Register(Type operation, HandlerDescription handler)
        {
            if (_freezee) throw new InvalidOperationException($"{typeof(DescriptionRuntimeModel)} is freeze. Use Register before first user GetByOperation");
            _descriptions.Add(operation, handler);
        }
        
        public HandlerDescription GetByOperation(Type operation)
        {
            _freezee = true;
            if (_descriptions.TryGetValue(operation, out var description))
            {
                return description;
            }
            return Default;
        }
    }
}