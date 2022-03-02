using System;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;
using Xunit;

namespace NetOperationTest
{
    public enum SomeCode : ushort { }
    
    [Operation(0)]
    public class Op : IOperation<float> { }
    [Operation(2)]
    public class Op2 : IOperation<float> { }

    [Operation(4)]
    public class Op4 : IOperation<float> { }

    [Operation(4)]
    public class Op5 : IOperation<float>{}
    
    [Operation(2)]
    public class Op6 : IOperation<float>{}
    
    [Operation(0)]
    public class Op7 : IOperation<float> { }

    public enum WrongStatus { A,C }
    public class RuntimeModelTest
    {
        [Fact]
        public void CreateTest()
        {
            var model = OperationRuntimeModel.CreateFromAttribute(new[] {typeof(Op), typeof(Op2), typeof(Op4)});
            var desc = model.GetDescriptionBy(0);
            Assert.Equal(typeof(Op),desc.OperationType);
            Assert.Equal(typeof(float), desc.ResultType);

            foreach (var d in model)
            {
            }
        }

        [Fact]
        public void DuplicateTest()
        {
            Assert.Throws<ArgumentException>(() => { OperationRuntimeModel.CreateFromAttribute(new[] {typeof(Op), typeof(Op2), typeof(Op4),typeof(Op5),typeof(Op6)});});
        }
    }
}