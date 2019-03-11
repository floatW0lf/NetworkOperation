using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetworkOperation.Extensions;

namespace NetworkOperation
{
    public class OperationRuntimeModel : IEnumerable<OperationDescription>
    {
        public static OperationRuntimeModel CreateFromAttribute(IEnumerable<Type> operationTypes)
        {
           var models = operationTypes.Where(t => typeof(IOperation).IsAssignableFrom(t)
                        && t.IsDefined(typeof(OperationAttribute), true)
                        && !t.IsInterface && !t.IsAbstract
                        && !t.IsGenericType)
                .Select(type =>
                {
                    var metaInfo = type.GetCustomAttribute<OperationAttribute>();
                    var arguments =type.GetGenericArgsFromOperation();

                    return new OperationDescription(metaInfo.Code, arguments[0], arguments[1], metaInfo.Handle)
                    { UseAsyncSerialize = metaInfo.UseAsyncSerialize };

                }).OrderBy(d => d.Code).ToArray();

            var resultModels = new OperationDescription[models.Last().Code + 1];

            foreach (var description in models)
            {
                resultModels[description.Code] = description;
            }
            return new OperationRuntimeModel(resultModels,true);
        }

        

        public static OperationRuntimeModel CreateFromAttribute(IEnumerable<Assembly> assemblies)
        {
            return CreateFromAttribute(assemblies.Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes()));
        }

        public static OperationRuntimeModel CreateFromAttribute()
        {
            return CreateFromAttribute(AppDomain.CurrentDomain.GetAssemblies());
        }

        private OperationDescription[] _descriptions;
        private Dictionary<Type, OperationDescription> _typeToDescriptions;

        private OperationRuntimeModel(OperationDescription[] descriptions, bool skipCheck)
        {
            _descriptions = descriptions;
            _typeToDescriptions = descriptions.Where(d => d != null).ToDictionary(d => d.OperationType);
        }

        private void ValidateOperations()
        {
            for (int i = 0; i < _descriptions.Length; i++)
            {
                var desc = _descriptions[i];
                if (desc.Code != i) throw new Exception("Description array must be ordered by code.");
                desc.OperationType.GetGenericArgsFromOperation();
            }
        }

        public OperationRuntimeModel(OperationDescription[] descriptions)
        {
            _descriptions = descriptions;
            ValidateOperations();
            _typeToDescriptions = descriptions.Where(d => d != null).ToDictionary(d => d.OperationType);
        }

        public OperationDescription GetDescriptionBy(uint code)
        {
            return _descriptions[code];
        }

        public OperationDescription GetDescriptionBy(Type operationType)
        {
            return _typeToDescriptions[operationType];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _descriptions.GetEnumerator();
        }

        public int Counts => _descriptions.Length;

        public IEnumerator<OperationDescription> GetEnumerator()
        {
            for (int i = 0; i < _descriptions.Length; i++)
            {
                yield return _descriptions[i];
            }
        }
    }
}