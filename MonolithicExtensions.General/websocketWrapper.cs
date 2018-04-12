using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using MonolithicExtensions.Portable;
using MonolithicExtensions.Portable.Logging;

/// <summary>
/// Extension functions for .NET's builtin websocket objects.
/// </summary>
/// <remarks>
/// I don't believe anything actually uses these websocket extensions. You can probably safely remove these... probably.
/// </remarks>
namespace MonolithicExtensions.General 
{
   public static class ClientWebSocketExtensions 
   {
      //Maybe these will be settable in the future
      public const int ReceiveChunkSize = 1024;
      public const int SendChunkSize = 1024;

      private static ILogger Logger = LogServices.CreateLoggerFromDefault(typeof(ClientWebSocketExtensions));

      // .NET's websocket client is kind of lacking. SendMessageAsync, combined with ReadMessageAsync,
      // enable easy message-based communication over a websocket client.

      /// <summary>
      /// Send a block of data (given as a string) over the given ClientWebSocket.
      /// </summary>
      /// <param name="client"></param>
      /// <param name="message"></param>
      /// <param name="token"></param>
      /// <returns></returns>
      /// <remarks>
      /// That cancellation token is gross because... optional parameters and whatever
      /// </remarks>
      public static async Task SendMessageAsync(this ClientWebSocket client, string message, CancellationToken? token = null)
      {
         Logger.Trace($"SendMessageAsync called with message of size {message.Length}");

         CancellationToken realToken = CancellationToken.None;
         if(token != null) realToken = (CancellationToken)token;

         byte[] messageBytes = Encoding.UTF8.GetBytes(message);
         int chunkCount = (int)Math.Ceiling((double)messageBytes.Length / SendChunkSize);
         int currentChunkSize = SendChunkSize;
         bool lastMessage = false;

         for(int i = 0; i < chunkCount; i++)
         {
            int offset = SendChunkSize * i;

            if(i == (chunkCount - 1))
            {
               currentChunkSize = messageBytes.Length % SendChunkSize;
               lastMessage = true;
            }

            await client.SendAsync(new ArraySegment<byte>(messageBytes, offset, currentChunkSize),
                  WebSocketMessageType.Text, lastMessage, realToken);
         }
      }

      /// <summary>
      /// Read a block of data from the ClientWebSocket and return it as a string.
      /// </summary>
      /// <param name="client"></param>
      /// <param name="token"></param>
      /// <returns></returns>
      public static async Task<string> ReadMessageAsync(this ClientWebSocket client, CancellationToken? token = null)
      {
         Logger.Trace($"ReadMessageAsync called");

         CancellationToken realToken = CancellationToken.None;
         if(token != null) realToken = (CancellationToken)token;

         byte[] buffer = new byte[ReceiveChunkSize];
         WebSocketReceiveResult result;

         //This probably has TERRIBLE performance but at least it'll... work?
         MemoryStream messageBytes = new MemoryStream();

         do
         {
            result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), realToken);

            //Do I really need to handle this?
            if (result.MessageType == WebSocketMessageType.Close)
            {
               await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, realToken);
               return null; //wahahaha hacks
            }
            else
            {
               messageBytes.Write(buffer, 0, result.Count);
            }
         }
         while (!result.EndOfMessage);

         return Encoding.UTF8.GetString(messageBytes.ToArray());
      }
   }
}
