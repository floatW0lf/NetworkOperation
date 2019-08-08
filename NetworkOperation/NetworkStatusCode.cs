using System;
using System.Collections.Generic;
using System.Linq;
using NetworkOperation.StatusCodes;

namespace NetworkOperation
{
    public struct NetworkStatusCode : IEquatable<NetworkStatusCode>, IComparable<NetworkStatusCode>, IEquatable<Enum>
    {
        private static readonly Dictionary<Type, EnumRangeValue> EnumRegistry;

        static NetworkStatusCode()
        {
            EnumRegistry = new Dictionary<Type, EnumRangeValue>();
            Register(typeof(BuiltInOperationState));
        }

        public static void UnregisterAll()
        {
            lock (EnumRegistry)
            {
                EnumRegistry.Clear();
            }
            Register(typeof(BuiltInOperationState));
        }

        public static void Register(params Type[] enums)
        {
            lock (EnumRegistry)
            {
                var registrations = enums.Select(CreateEnumRegistration).Concat(EnumRegistry).ToArray();
                CheckIntersections(registrations);
                AddRegistrations(registrations);
            }
        }

        private static void AddRegistrations(KeyValuePair<Type, EnumRangeValue>[] registrations)
        {
            foreach (var reg in registrations)
            {
                if (EnumRegistry.ContainsKey(reg.Key)) continue;
                EnumRegistry.Add(reg.Key, reg.Value);
            }
        }

        private static void CheckIntersections(KeyValuePair<Type, EnumRangeValue>[] registrations)
        {
            foreach (var first in registrations)
            foreach (var other in registrations)
            {
                if (first.Key == other.Key) continue;
                if (first.Value.Intersect(other.Value))
                    throw new ArgumentException(
                        $"{first.Key}{first.Value} and {other.Key}{other.Value} values conflict.");
            }
        }

        private static KeyValuePair<Type, EnumRangeValue> CreateEnumRegistration(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException($"{enumType} must be enum");
            var values = Enum.GetValues(enumType);
            return new KeyValuePair<Type, EnumRangeValue>(enumType,
                new EnumRangeValue((uint) values.GetValue(0), (uint) values.GetValue(values.Length - 1)));
        }

        private static bool IsValidValue<TEnum>(uint code)
        {
            try
            {
                return EnumRegistry[typeof(TEnum)].Contain(code);
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(
                    $"{typeof(TEnum)} must be registered. Use {nameof(StatusEncoding)}.{nameof(Register)}");
            }
        }

        public static bool IsEnumRegistered<TEnum>()
        {
            return EnumRegistry.ContainsKey(typeof(TEnum));
        }

        private static void CannotDecodeThrow<TEnum>(uint statusCode)
            where TEnum : IConvertible 
        {
            throw new InvalidOperationException(
                $"Status code {statusCode} cannot encode as {typeof(TEnum)}");
        }

        public bool Equals(Enum other)
        {
            return _code == Convert.ToUInt32(other);
        }

        public override string ToString()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var enumRange in EnumRegistry)
                if (enumRange.Value.Contain(_code))
                    return $"{enumRange.Key.Name}.{Enum.ToObject(enumRange.Key, _code)}";
            return $"unknown code: {_code}";
        }

        private struct EnumRangeValue
        {
            public readonly uint Start;
            public readonly uint End;

            public EnumRangeValue(uint start, uint end)
            {
                Start = start;
                End = end;
            }

            public bool Intersect(EnumRangeValue other)
            {
                return Contain(other.Start) || Contain(other.End);
            }

            public bool Contain(uint value)
            {
                return value >= Start && value <= End;
            }

            public override string ToString()
            {
                return $"[{Start};{End}]";
            }
        }
        private uint _code;

        private NetworkStatusCode(uint raw)
        {
            _code = raw;
        }

        public uint Code
        {
            get { return _code; }
        }

        public bool Equals(NetworkStatusCode other)
        {
            return _code == other._code;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkStatusCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) _code;
        }

        public int CompareTo(NetworkStatusCode other)
        {
            return _code.CompareTo(other._code);
        }

        public static implicit operator NetworkStatusCode(uint rawCode)
        {
            return new NetworkStatusCode(rawCode);
        }
        
        public static implicit operator NetworkStatusCode(BuiltInOperationState @enum)
        {
            return new NetworkStatusCode((uint)@enum);
        }
        
        public static implicit operator NetworkStatusCode(Enum @enum)
        {
            return new NetworkStatusCode(Convert.ToUInt32(@enum));
        }
        
        public static bool operator == (NetworkStatusCode a, NetworkStatusCode b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(NetworkStatusCode a, NetworkStatusCode b)
        {
            return !(a == b);
        }

        public bool Equals<TEnum>(TEnum @enum) where TEnum : Enum
        {
            return true;
        }
    }
}