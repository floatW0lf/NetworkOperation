using System;
using System.Collections.Generic;

namespace NetworkOperation
{
    public interface IInterfaceMapAccessor
    {
        Dictionary<Type, Type> InterfaceToClassMap { get; set; }
    }
}