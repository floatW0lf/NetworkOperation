using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetworkOperation.StatusCodes
{
    public static class StatusEncoding
    {
        private static readonly Dictionary<Type, EnumRangeValue> EnumRegistry;

        static StatusEncoding()
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

        public static bool IsValidValue<TOperation, TEnum>(TOperation operation) where TOperation : IOperationMessage
        {
            return IsValidValue<TEnum>(operation.StateCode);
        }

        public static bool IsValidValue<TEnum>(uint code)
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

        public static void Encode<TEnum, TMessage>(ref TMessage message, TEnum status) where TEnum : IConvertible
            where TMessage : IOperationMessage
        {
            var code = status.ToUInt32(CultureInfo.InvariantCulture);
            if (!IsValidValue<TEnum>(code))
                throw new InvalidOperationException(
                    $"Status code {AsString(message.StateCode)} cannot encode as {typeof(TEnum)}");
            message.StateCode = code;
        }

        public static TEnum Decode<TEnum, TMessage>(TMessage message, TEnum likeThis) where TMessage : IOperationMessage
            where TEnum : IConvertible
        {
            if (!IsValidValue<TMessage, TEnum>(message))
                throw new InvalidOperationException(
                    $"Status code {AsString(message.StateCode)} cannot decode as {typeof(TEnum)}");

            return (TEnum) Enum.ToObject(typeof(TEnum), message.StateCode);
        }

        public static bool IsStatus<TEnum, TMessage>(TMessage message, TEnum status)
            where TMessage : IOperationMessage where TEnum : IConvertible
        {
            return message.StateCode == status.ToUInt32(CultureInfo.InvariantCulture);
        }

        public static string AsString(uint code)
        {
            foreach (var enumRange in EnumRegistry)
                if (enumRange.Value.Contain(code))
                    return $"{enumRange.Key.Name}.{Enum.ToObject(enumRange.Key, code)}";
            return $"unknown code: {code}";
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
    }
}