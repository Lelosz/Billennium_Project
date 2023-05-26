﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Przychodnia.Webapi.Websocket
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketsController> _logger;
        ConcurrentDictionary<string, WebSocket> connections { get; set; }


        public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketsController> logger)
        {
            _next = next;
            connections = new ConcurrentDictionary<string, WebSocket>();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }
            _logger.Log(LogLevel.Information, "WebSocket connection established");
            CancellationToken ct = context.RequestAborted;
            WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
            string wsId = Guid.NewGuid().ToString();

            connections.TryAdd(wsId, ws);



            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }
                string data = await ReadStringAsync(ws,ct);

                if (string.IsNullOrEmpty(data))
                {
                    if (ws.State != WebSocketState.Open)
                    {
                        _logger.Log(LogLevel.Information, "WebSocket connection closed");
                        break;
                    }

                    continue;
                }
                foreach (var item in connections)
                {
                    if (item.Value.State != WebSocketState.Open)
                    {
                        continue;
                    }

                    await SendStringAsync(item.Value, data, ct);
                }
            }
            WebSocket dummy;
            connections.TryRemove(wsId, out dummy);
            await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "UserDisconnected", ct);
            
            ws.Dispose();


            /*await _next(context);*/
        }

        async Task<string> ReadStringAsync(WebSocket ws, CancellationToken ct = default)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 8]);

            using (MemoryStream ms = new MemoryStream())
            {
                WebSocketReceiveResult receiveResult;

                do
                {
                    ct.ThrowIfCancellationRequested();

                    receiveResult = await ws.ReceiveAsync(buffer, ct);

                    ms.Write(buffer.Array, buffer.Offset, receiveResult.Count);

                } while (!receiveResult.EndOfMessage);


                ms.Seek(0, SeekOrigin.Begin); // Changing stream position to cover whole message


                if (receiveResult.MessageType != WebSocketMessageType.Text)
                    return null;

                using (StreamReader reader = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync(); // decoding message
                }

            }
        }

        Task SendStringAsync(WebSocket ws, string data, CancellationToken ct = default)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return ws.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

      /*  async Task SendEvent(string name, string data, CancellationToken ct = default)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            foreach (var item in connections)
            {
                if (item.Value.State != WebSocketState.Open)
                {
                    continue;
                }

                await item.Value.SendAsync(segment, WebSocketMessageType.Binary);
            }
        }*/
    }
}
