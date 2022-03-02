using System;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public readonly struct StatusCode : IEquatable<StatusCode>
    {
        public readonly byte TypeTag;
        public readonly ushort EnumValue;
        public bool Equals(StatusCode other)
        {
            return TypeTag == other.TypeTag && EnumValue == other.EnumValue;
        }

        public override bool Equals(object obj)
        {
            return obj is StatusCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TypeTag.GetHashCode() * 397) ^ EnumValue.GetHashCode();
            }
        }

        public StatusCode(byte type, ushort enumValue)
        {
            TypeTag = type;
            EnumValue = enumValue;
        }
        public static implicit operator StatusCode(BuiltInOperationState status)
        {
            return new StatusCode(1, (ushort)status);
        }

        public static bool operator ==(StatusCode a, BuiltInOperationState b)
        {
            return a.TypeTag == 1 && a.EnumValue == (ushort)b;
        }

        public static bool operator !=(StatusCode a, BuiltInOperationState b)
        {
            return !(a == b);
        }
    }
}