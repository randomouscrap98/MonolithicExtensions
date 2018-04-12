using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    /// <summary>
    /// Contains functions for retrieving information relating to the network.
    /// </summary>
    public static class NetworkServices
    {
        public static int TeaserPort = 65530;

        //Refactored from an idea given in https://stackoverflow.com/a/27376368/1066474
        public static IPEndPoint GetUsedIp(string outboundTarget)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect(outboundTarget, TeaserPort);
                return socket.LocalEndPoint as IPEndPoint;
            }
        }

        public static IPEndPoint GetUsedExternalIp()
        {
            return GetUsedIp("8.8.8.8");
        }

        /// <summary>
        /// A wrapper for GetusedExternalIp for times where you just need the address without handling exceptions
        /// </summary>
        /// <returns></returns>
        public static string GetUsedExternalIpSimple()
        {
            try
            {
                return GetUsedExternalIp().Address.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
