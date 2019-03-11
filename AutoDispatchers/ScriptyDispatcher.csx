#r "..\Contract\bin\Debug\net472\Contract.dll"
#r "..\NetworkOperation\bin\Debug\net472\NetworkOperation.dll"

using NetworkOperation;
using System;
using System.Reflection;
using System.Collections;

var operations = OperationsRuntimeModel.CreateFromAttribute();
var handleSide = Side.Server;

Output.WriteLine("using NetworkOperation;");
Output.WriteLine("using System;");
Output.WriteLine("using System.Threading.Tasks;");
Output.WriteLine("namespace NetworkOperation.Dispatching {");
Output.WriteLine("  public sealed class ScriptyDispatcher : BaseDispatcher {");
Output.WriteLine("      public ScriptyDispatcher(BaseSerializer serializer, IOperationHandlerProvider provider, OperationsRuntimeModel model) : base(serializer, provider, model){}");
Output.WriteLine("          protected override Task<byte[]> ProcessHandler(IHandler handler, uint code, byte[] rawData, bool useAsync,CancellationToken token)");
Output.WriteLine("          {");

Output.WriteLine("              switch (code)");
Output.WriteLine("              {");
foreach (OperationDescription op in operations)
{    
    if (op == null || !op.Handle.HasFlag(handleSide)) continue;
Output.WriteLine($"                 case {op.Code}: return GenericHandle<{op.OperationType.FullName}, {op.ResultType.FullName}>(handler, rawData, useAsync,token);");
}        
Output.WriteLine("              }");
Output.WriteLine("              throw new Exception(\"wrong cmd!\"); ");

Output.WriteLine("          }");
Output.WriteLine("      }");
Output.WriteLine("}");
