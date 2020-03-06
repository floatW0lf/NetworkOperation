using System;
using NetworkOperation;
using Xunit;

namespace NetOperationTest
{
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