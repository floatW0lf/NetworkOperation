using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace TemplateDispatcher
{
    public partial class PreGeneratedDispatcherTemplate
    {
        private OperationRuntimeModel Model { get; }
        private bool AOTSupport { get; }
        private Side Side { get; }

        public PreGeneratedDispatcherTemplate(OperationRuntimeModel model, bool aotSupport, Side side)
        {
            Model = model;
            AOTSupport = aotSupport;
            Side = side;
        }
    }
}