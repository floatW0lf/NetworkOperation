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
           var models = operationTypes.Where(t => 
                        typeof(IOperation).IsAssignableFrom(t)
                        && t.IsDefined(typeof(OperationAttribute), true)
                        && !t.IsInterface 
                        && !t.IsAbstract
                        && !t.IsGenericType)
                .Select(type =>
                {
                    var metaInfo = type.GetCustomAttribute<OperationAttribute>();
                    var operationResult = type.GetResultFromOperation();

                    return new OperationDescription(metaInfo.Code, type, operationResult, metaInfo.Handle, metaInfo.ForRequest, metaInfo.ForResponse, metaInfo.WaitResponse)
                    {
                        UseAsyncSerialize = metaInfo.UseAsyncSerialize,
                    };

                }).OrderBy(d => d.Code).ToArray();

            if (models.Length == 0) throw new Exception("Not found operations");
            
            ThrowIfFindDuplicates(models);
           
            var resultModels = new OperationDescription[models.Last().Code + 1];

            foreach (var description in models)
            {
                resultModels[description.Code] = description;
            }
            return new OperationRuntimeModel(resultModels,true);
        }

        static void ThrowIfFindDuplicates(IEnumerable<OperationDescription> descriptions)
        {
            var duplicates = descriptions.GroupBy(d => d.Code).Where(group => group.Count() > 1).ToArray();
            if (duplicates.Length > 0)
            {
                var message = string.Join(",",duplicates.Select(g => $"Operation code = {g.Key}, operations:{string.Join(",", g.Select(d => d.OperationType))}\n"));
                throw new ArgumentException($"Find duplicates for:\n{message}");
            }
        }
       

        public static OperationRuntimeModel CreateFromAttribute(IEnumerable<Assembly> assemblies)
        {
            return CreateFromAttribute(assemblies
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(type => type != null);
                    }
                    catch (Exception)
                    {
                        return Array.Empty<Type>();
                    }
                }));
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

        private void ValidateOperations(OperationDescription[] descriptions)
        {
            for (int i = 0; i < descriptions.Length; i++)
            {
                var desc = descriptions[i];
                if (desc == null) continue;
                
                if (desc.Code != i) throw new ArgumentException("Description array must be ordered by code.");
                desc.OperationType.GetResultFromOperation();
            }
        }

        public OperationRuntimeModel(OperationDescription[] descriptions)
        {
            ValidateOperations(descriptions);
            ThrowIfFindDuplicates(descriptions);
            _typeToDescriptions = descriptions.Where(d => d != null).ToDictionary(d => d.OperationType);
            _descriptions = descriptions;
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
                var desc = _descriptions[i];
                if (desc != null) yield return desc;
            }
        }
    }
}