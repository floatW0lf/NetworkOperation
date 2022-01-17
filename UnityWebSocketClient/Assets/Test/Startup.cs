using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using UnityEngine;

namespace WebGL.WebSockets.Tests
{
    public class Startup
    {
        static bool serializerRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Debug.Log("Startup");
            if (!serializerRegistered)
            {
                StaticCompositeResolver.Instance.Register(new IMessagePackFormatter[]{new StatusCodeFormatter(), new DefaultMessageFormatter()}, new IFormatterResolver[]{
                        GeneratedResolver.Instance, StandardResolver.Instance,PrimitiveObjectResolver.Instance});
                
                MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
                serializerRegistered = true;
            }
        }

#if UNITY_EDITOR


        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            Initialize();
        }

#endif
    }
}