using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetworkOperation
{
    [StructLayout(LayoutKind.Explicit)]
    public struct StatusCode : IEquatable<StatusCode>, IComparable<StatusCode>
    {
        [FieldOffset(0)]
        private readonly uint _code;
        [FieldOffset(0)]
        private readonly ushort _typeCode;
        [FieldOffset(2)]
        private readonly ushort _value;
        
        private static Type[] _enumRegistry = { typeof(BuiltInOperationState) };
        private static bool _freeze;
        
        public static void UnregisterAll()
        {
            _enumRegistry = new[] {typeof(BuiltInOperationState)};
            _freeze = false;
        }

        public static void Register(params Type[] enums)
        {
            if (_freeze) throw new InvalidOperationException($"{nameof(StatusCode)} registry is freeze.");
            if (enums.Length + _enumRegistry.Length >= ushort.MaxValue) throw new InvalidOperationException("Max registered enum");
            if (enums.Any(type => !type.IsEnum)) throw new InvalidOperationException("Register type must be enum");
            
            var newRegistry = new Type[enums.Length + _enumRegistry.Length];
            Array.Copy(_enumRegistry,newRegistry, _enumRegistry.Length);
            Array.Copy(enums,0,newRegistry,_enumRegistry.Length, enums.Length);
            _enumRegistry = newRegistry;
            _freeze = true;
        }

        public Type GetEnumType()
        {
            return _typeCode < _enumRegistry.Length ? _enumRegistry[_typeCode] : null;
        }
        private static void ThrowIfNotRegistered(Type enumType)
        {
            if (IsEnumRegistered(enumType)) return;
            throw new InvalidOperationException(
                $"{enumType} must be registered. Use {nameof(StatusCode)}.{nameof(Register)}");
        }
        
        public static bool IsEnumRegistered(Type enumType)
        {
            return GetEnumTypeCode(enumType) != ushort.MaxValue;
        }
        
        public static bool IsEnumRegistered<TEnum>() where TEnum : Enum
        {
            return IsEnumRegistered(typeof(TEnum));
        }
        
        public override string ToString()
        {
            if (_typeCode < _enumRegistry.Length)
            {
                return Enum.GetName(_enumRegistry[_typeCode], _value) ?? "null";
            }
            return $"Unknown.{_code}";
        }

        private StatusCode(ushort enumTypeCode, ushort value)
        {
            _freeze = true;
            _code = 0;
            _typeCode = enumTypeCode;
            _value = value;
        }
        private StatusCode(uint raw)
        {
            _freeze = true;
            _typeCode = 0;
            _value = 0;
            _code = raw;
        }
        
        public uint Code => _code;

        public bool Equals(StatusCode other)
        {
            return _code == other._code;
        }
        
        public override bool Equals(object obj)
        {
            return obj is StatusCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) _code;
        }

        public int CompareTo(StatusCode other)
        {
            return _code.CompareTo(other._code);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ConvertToValue(Enum @enum)
        {
            return Convert.ToUInt16(@enum);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort GetEnumTypeCode(Type enumType)
        {
            for (ushort i = 0; i < _enumRegistry.Length; i++)
            {
                if (_enumRegistry[i] == enumType) return i;
            }
            return ushort.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static StatusCode ConvertToStatusCode(Enum @enum) 
        {
            return new StatusCode(GetEnumTypeCode(@enum.GetType()), ConvertToValue(@enum));
        }
        
        public bool Equals<TEnum>(TEnum @enum) where TEnum : Enum, IConvertible
        {
            return GetEnumTypeCode(typeof(TEnum)) == _typeCode && _value == @enum.ToUInt16(CultureInfo.InvariantCulture);
        }

        #region Operators
        
        public static explicit operator StatusCode(uint rawCode)
        {
            return new StatusCode(rawCode);
        }
        
        public static implicit operator StatusCode(BuiltInOperationState @enum)
        {
            return new StatusCode(0,(ushort) @enum);
        }
        
        public static implicit operator StatusCode(Enum @enum)
        {
            ThrowIfNotRegistered(@enum.GetType());
            return ConvertToStatusCode(@enum);
        }
        
        public static bool operator == (StatusCode a, StatusCode b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(StatusCode a, StatusCode b)
        {
            return !(a == b);
        }
       
        public static bool operator ==(StatusCode a, Enum b)
        {
            return a == ConvertToStatusCode(b);
        }

        public static bool operator !=(StatusCode a, Enum b)
        {
            return !(a == b);
        }

        public static bool operator >(StatusCode a, StatusCode b)
        {
            return a._code > b._code;
        }

        public static bool operator >=(StatusCode a, StatusCode b)
        {
            return a._code >= b._code;
        }

        public static bool operator <=(StatusCode a, StatusCode b)
        {
            return a._code <= b._code;
        }

        public static bool operator <(StatusCode a, StatusCode b)
        {
            return a._code < b._code;
        }
        
        public static bool operator >(StatusCode a, Enum b)
        {
            return a > ConvertToStatusCode(b);
        }

        public static bool operator >=(StatusCode a, Enum b)
        {
            return a >= ConvertToStatusCode(b);
        }

        public static bool operator <=(StatusCode a, Enum b)
        {
            return a <= ConvertToStatusCode(b);
        }

        public static bool operator <(StatusCode a, Enum b)
        {
            return a < ConvertToStatusCode(b);
        }
        
        #endregion
        
    }
}