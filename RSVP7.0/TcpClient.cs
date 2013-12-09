﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace RSVP7._0
{
    public class TcpClient
    {
        public TcpClient(String ip, int portnum)
        {
            HostIP = IPAddress.Parse(ip);
            point = new IPEndPoint(HostIP, portnum);
        }

        public void connectTohost()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(point);
                thread = new Thread(new ThreadStart(Process));
                thread.Start();
            }
            catch (Exception ey)
            {
                MessageBox.Show("服务器没有开启\r\n" + ey.Message);
            }
        }

        public void disconnect()
        {
            try
            {
                if (null != socket)
                {
                    socket.Disconnect(false);
                }
                if (null != thread)
                {
                    thread.Abort();
                }
            }
            catch
            {
                MessageBox.Show("wrong with disconnect client\n");
            }
        }

        private void Process()
        {
            if (socket.Connected)
            {              
                while (true)
                {
                    byte[] receiveByte = new byte[8000];
                    socket.Receive(receiveByte, receiveByte.Length, 0);
                    string Info = Encoding.ASCII.GetString(receiveByte);
                    string[] tmp = Info.Split(';');
                    if ('A' == Info[0])
                    {
                        // 结果的图像相似度差，重新获取结果图像                        
                        if (tmp.Length - 1 != Config.m_trialnum)
                            MessageBox.Show("number of images come back from Server doesnot equal to Trial Number!");
                        else
                        {
                            for (int i = 1; i < tmp.Length; ++i)
                            {
                                Config.feedback[i].imagepath = tmp[i];
                            }

                            CommandEventArgs e = new CommandEventArgs();
                            e.command = 'A';
                            CommandHandler(this, e);
                        }

                    }
                    else if ('B' == Info[0])
                    {
                        for (int i = 1; i < tmp.Length; ++i)
                        {
                            Config.feedback[i].imagepath = tmp[i];
                        }

                        CommandEventArgs e = new CommandEventArgs();
                        e.command = 'B';
                        e.number = tmp.Length - 1;
                        CommandHandler(this, e);
                    }
                    else
                    {
                        //do nothing
                    }
                                      
                }
            }

        }

        public void SendData(String[] imagename, int quantity)
        {
            try
            {
                String content = null;
                for (int i = 0; i < quantity; ++i)
                {
                    content += imagename[i];
                    content += ";";
                }
                byte[] sendBuffer = Encoding.ASCII.GetBytes(content.ToCharArray());
                socket.Send(sendBuffer, sendBuffer.Length, 0);
            }
            catch { }

        }

        #region variable

        public void get_Handler(PicShow psw)
        {
            if (null != psw)
            {
                psw.add_Handler(null, this);
            }
        }

        public event TcpEventHandler CommandHandler; 
        Thread thread = null;
        IPAddress HostIP;
        IPEndPoint point;
        Socket socket;

        #endregion
    }
}
