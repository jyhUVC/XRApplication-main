//#define _SYNC_CONNECT
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Itld_WakeUp_STT
{
    public class ItldSTTClient
    {
        private const int MAX_SOCKET_WAIT_MS = 2000;
        Socket socket;
        Stream bsSend;
        StreamReader srRecv;
        string KALDI_ENCODING = "euc-kr"; 
        const int euckrCodepage = 51949;
        StringBuilder json;
        
        public bool IsConnected()
        {
            bool ret;
            ret = (socket == null) ? false : socket.Connected;
            return ret;
        }

        string host;
        int port;
        Thread threadRecv;
        Action<string> callback;
        public ItldSTTClient(string host, int port, Action<string> cbOnResult)
        {
            this.host = host;
            this.port = port;
            json = new StringBuilder();

            callback = cbOnResult;

        }

        public bool connect()
        {
            try
            {
#if _SYNC_CONNECT
                socket.Connect(host, port);
#else
                socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                var connect = Task.Factory.FromAsync(
                                    socket.BeginConnect,
                                    socket.EndConnect,
                                    host,
                                    port,
                                    null);

                var isConnected = connect.Wait(TimeSpan.FromSeconds(1));
                if (!isConnected)
                {
                    socket.Close();
                    return false;
                }
#endif
                bsSend = new BufferedStream(new NetworkStream(socket, false));

                //provide codepage
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                srRecv = new StreamReader(new BufferedStream(new NetworkStream(socket, false)), Encoding.GetEncoding(KALDI_ENCODING));

                threadRecv = new Thread(recv);
                threadRecv.Start();
                Debug.Log("stt connected");
                return true;           
            }
            catch (Exception e)
            {
                Debug.Log("Exception " +  e.Message);
                socket.Dispose();
                return false;
            }
        }

        public void disconnect()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            if (srRecv != null) srRecv.Dispose();
            if (bsSend != null) bsSend.Dispose();                          

            json.Clear();
            socket.Close();
            socket.Dispose();
            Debug.Log("stt disconnected");
        }

        public void send(byte[] buf, int size)
        {
            if (!socket.Connected)
            {
                return;
            }
            const int LEN_SIZE = 4;
            byte[] lenArr = new byte[LEN_SIZE];

            lenArr = BitConverter.GetBytes(size);

            try
            {
                bsSend.Write(lenArr, 0, LEN_SIZE);
                bsSend.Write(buf, 0, size);
                bsSend.Flush();
            }
            catch (IOException e)
            {
                disconnect();
                return;
            }
        }

        private string getLine()
        {
            string line = null;
            if (socket.Connected)
            {
                try
                {
                    line = srRecv.ReadLine();
                }
                catch (Exception e)
                {
                    line = null;
                }
            }
            return line;
        }

        private bool isCompleteJson(StringBuilder sb)
        {
            bool ret = false;
            int cntCurlyBrackets = 0;
            int cntSquareBrackets = 0;

            if (sb.Length == 0) return ret;

            for (int i = 0; i < sb.Length; i++)
            {
                char ch = sb[i];
                if (ch.Equals("{")) cntCurlyBrackets++;
                if (ch.Equals("}")) cntCurlyBrackets--;
                if (ch.Equals("[")) cntSquareBrackets++;
                if (ch.Equals("]")) cntSquareBrackets--;
            }

            if (cntCurlyBrackets == 0 && cntSquareBrackets == 0) ret = true;

            return ret;
        }

        void recv()
        {
            while(socket != null && socket.Connected)
            {
                string line = getLine();
                if (line == null)
                {
                    callback(null);
                    disconnect();
                    continue;
                }
                if (line.Length == 0) continue;
                json.Append(line);
                if (!isCompleteJson(json)) continue;
                callback(json.ToString());
                Thread.Sleep(100);
            }
        }
    }
}
