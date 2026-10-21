using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using System.Reflection;


namespace EchoServer
{
    class MainClass
    {
        // 监听Socket
        static Socket listenfd;
        // 客户端Socket及状态信息
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        // 委托类型
        public delegate void MsgDelegate(ClientState cs, string msgArgs);
        // 全局字典
        public static Dictionary<string, MsgDelegate> msgHandlers = new Dictionary<string, MsgDelegate>();
        public static void Main(string[] args)
        {
            //注册协议
            RegisterHandlers();
            // Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            // Listen
            listenfd.Listen(0);
            Console.WriteLine("[服务器]连接成功");
            // 主循环
            while(true)
            {
                // 检查listenfd
                if (listenfd.Poll(0, SelectMode.SelectRead))
                {
                    ReadListenfd(listenfd);
                }
                // 检查clientfd
                foreach (ClientState s in clients.Values)
                {
                    Socket clientfd = s.socket;
                    if (clientfd.Poll(0, SelectMode.SelectRead))
                    {
                        if (!ReadClientfd(clientfd))
                        {
                            break;
                        }
                    }
                }
                // 防止CPU占用过高
                System.Threading.Thread.Sleep(1);
            }
        }

        //注册协议
        public static void RegisterHandlers()
        {
            msgHandlers["Enter"] = MsgHandler.MsgEnter;
            msgHandlers["List"] = MsgHandler.MsgList;
            msgHandlers["Move"] = MsgHandler.MsgMove;
            msgHandlers["Attack"] = MsgHandler.MsgAttack;
            msgHandlers["Hit"] = MsgHandler.MsgHit;

        }

        //发送
        public static void Send(ClientState cs, String sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }

        // 读取Listenfd
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Aceept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }

        // 读取clientfd
        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];

            // 接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch (SocketException ex)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);

                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException " + ex);
                return false;
            }
            // 客户端关闭
            if (count <= 0)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);

                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Close");
                return false;
            }
            // 消息处理
            string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            // 直接按\n切割协议, 去掉空行
            string[] msgArray = recvStr.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            int l = msgArray.Length;
            Console.WriteLine(l);
            // 遍历每一条完整指令
            foreach (string oneMsg in msgArray)
            {
                Console.WriteLine(oneMsg);
                foreach (ClientState cs in clients.Values)
                {
                    
                    try
                    {
                        // 解析指令
                        string[] split = oneMsg.Split('|');
                        string msgName = split[0];
                        string msgArgs = split[1];
                        // 查字典
                        if (msgHandlers.ContainsKey(msgName))
                        {
                            msgHandlers[msgName](cs, msgArgs);
                        }
                        else
                        {
                            Console.WriteLine("未注册协议:" + msgName);
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Send fail" + ex);
                    }
                }
            }
            return true;

        }
    }
    public class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
    }

}
