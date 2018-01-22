﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public static class HttpListenerExtensions
    {
        /// <summary>
        /// A blocking function used to simplify the entire process of responding with a JSON object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="responseObject"></param>
        /// <param name="statusCode"></param>
        /// <param name="statusDescription"></param>
        public static void WriteJsonResponse<T>(this HttpListenerResponse response, T responseObject, 
            int statusCode = 200, string statusDescription = "OK")
        {
            response.StatusCode = statusCode;
            response.StatusDescription = statusDescription;
            response.ContentType = "application/json";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(MySerialize.JsonStringify<T>(responseObject));
            response.ContentLength64 = buffer.Length;
            response.Close(buffer, true);
        }

        /// <summary>
        /// A blocking function used to simplify the entire process of responding with binary data 
        /// (something that browsers probably don't care to see)
        /// </summary>
        /// <param name="response"></param>
        /// <param name="bytes"></param>
        /// <param name="statusCode"></param>
        /// <param name="statusDescription"></param>
        public static void WriteBinaryResponse(this HttpListenerResponse response, byte[] bytes,
            int statusCode = 200, string statusDescription = "OK")
        {
            response.StatusCode = statusCode;
            response.StatusDescription = statusDescription;
            response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            response.ContentLength64 = bytes.Length;
            response.Close(bytes, true);
        }
    }
}
