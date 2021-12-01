// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Protos/game.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace ServerAdmin.Protos {
  public static partial class GameService
  {
    static readonly string __ServiceName = "greet.GameService";

    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    static readonly grpc::Marshaller<global::ServerAdmin.Protos.AddGameRequest> __Marshaller_greet_AddGameRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ServerAdmin.Protos.AddGameRequest.Parser));
    static readonly grpc::Marshaller<global::ServerAdmin.Protos.TextResponseGame> __Marshaller_greet_TextResponseGame = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ServerAdmin.Protos.TextResponseGame.Parser));
    static readonly grpc::Marshaller<global::ServerAdmin.Protos.UpdateGameRequest> __Marshaller_greet_UpdateGameRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ServerAdmin.Protos.UpdateGameRequest.Parser));
    static readonly grpc::Marshaller<global::ServerAdmin.Protos.DeleteGameRequest> __Marshaller_greet_DeleteGameRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ServerAdmin.Protos.DeleteGameRequest.Parser));

    static readonly grpc::Method<global::ServerAdmin.Protos.AddGameRequest, global::ServerAdmin.Protos.TextResponseGame> __Method_AddGame = new grpc::Method<global::ServerAdmin.Protos.AddGameRequest, global::ServerAdmin.Protos.TextResponseGame>(
        grpc::MethodType.Unary,
        __ServiceName,
        "AddGame",
        __Marshaller_greet_AddGameRequest,
        __Marshaller_greet_TextResponseGame);

    static readonly grpc::Method<global::ServerAdmin.Protos.UpdateGameRequest, global::ServerAdmin.Protos.TextResponseGame> __Method_Update = new grpc::Method<global::ServerAdmin.Protos.UpdateGameRequest, global::ServerAdmin.Protos.TextResponseGame>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Update",
        __Marshaller_greet_UpdateGameRequest,
        __Marshaller_greet_TextResponseGame);

    static readonly grpc::Method<global::ServerAdmin.Protos.DeleteGameRequest, global::ServerAdmin.Protos.TextResponseGame> __Method_Delete = new grpc::Method<global::ServerAdmin.Protos.DeleteGameRequest, global::ServerAdmin.Protos.TextResponseGame>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Delete",
        __Marshaller_greet_DeleteGameRequest,
        __Marshaller_greet_TextResponseGame);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::ServerAdmin.Protos.GameReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of GameService</summary>
    [grpc::BindServiceMethod(typeof(GameService), "BindService")]
    public abstract partial class GameServiceBase
    {
      /// <summary>
      /// Sends a greeting
      /// </summary>
      /// <param name="request">The request received from the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>The response to send back to the client (wrapped by a task).</returns>
      public virtual global::System.Threading.Tasks.Task<global::ServerAdmin.Protos.TextResponseGame> AddGame(global::ServerAdmin.Protos.AddGameRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::ServerAdmin.Protos.TextResponseGame> Update(global::ServerAdmin.Protos.UpdateGameRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::ServerAdmin.Protos.TextResponseGame> Delete(global::ServerAdmin.Protos.DeleteGameRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(GameServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_AddGame, serviceImpl.AddGame)
          .AddMethod(__Method_Update, serviceImpl.Update)
          .AddMethod(__Method_Delete, serviceImpl.Delete).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, GameServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_AddGame, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ServerAdmin.Protos.AddGameRequest, global::ServerAdmin.Protos.TextResponseGame>(serviceImpl.AddGame));
      serviceBinder.AddMethod(__Method_Update, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ServerAdmin.Protos.UpdateGameRequest, global::ServerAdmin.Protos.TextResponseGame>(serviceImpl.Update));
      serviceBinder.AddMethod(__Method_Delete, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ServerAdmin.Protos.DeleteGameRequest, global::ServerAdmin.Protos.TextResponseGame>(serviceImpl.Delete));
    }

  }
}
#endregion
