using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

using IRC_Client.Formatting;
using IRC_Client.Channel;
using IRC_Client.User;

namespace IRC_Client
{
    class IrcClient
    {
        #region Variables & Properties
        TcpClient irc;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        DateTime lastSentTime;

        public bool Connected
        {
            get
            {
                return connected && this != null;
            }
        } bool connected;
        public string Nick
        {
            get { return nick; }
            set
            {
                if (Connected)
                {
                    SendCommand("NICK " + value);
                    nick = value;
                    System.Diagnostics.Debug.WriteLine("Changed nick to " + value);
                }
            }
        } string nick = "?";
        public string Server { get { return server; } } string server = "SERVER";
        public IrcChannelCollection Channels { get; set; }

        public string LastSentData { get { return lastSentData; } } string lastSentData;
        public string LastRecievedData { get { return lastRecievedData; } } string lastRecievedData;
        #endregion

        public IrcClient()
        {
            DataRecieved += new IrcDataEventHandler(IrcClient_DataRecieved);
            DataSent += new IrcDataEventHandler(IrcClient_DataSent);
        }

        public void Connect(string server, int port, string nick, string password = "")
        {
            try
            {
                Channels = new IrcChannelCollection();
                irc = new TcpClient(server, port);
                ns = irc.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns) { NewLine = "\r\n", AutoFlush = true };

                SendCommand("USER " + nick + " +mode * :" + nick);
                SendCommand("NICK " + nick);

                this.nick = nick;
                this.server = server;

                if (password != "")
                    SendCommand("IDENTIFY " + password);

                connected = true;

                #region Data Reciever
                new Thread((ThreadStart)delegate
                {
                    while (Connected)
                    {
                        string s = sr.ReadLine();

                        if (s.Trim() != "" && s != null)
                            DataRecieved(this, new IrcDataEventArgs(s.Trim()));
                    }
                }).Start();
                #endregion
                #region Ping Sender
                new Thread((ThreadStart)delegate
                {
                    while (Connected)
                    {
                        sw.WriteLine("PING " + server);
                        Thread.Sleep(15000);
                    }
                }).Start();
                #endregion
            }
            catch (Exception ex)
            {
                connected = false;

                Console.WriteLine(ex.ToString().Replace(Environment.NewLine, " "));

                Thread.Sleep(10000);
                Connect(server, port, nick, password);
            }

        }
        public void Disconnect()
        {
            SendCommand("QUIT :Nuci!");
            connected = false;
        }

        #region Commands
        public void SendCommand(string command)
        {
            while ((DateTime.Now - lastSentTime).TotalMilliseconds < 500) { }

            lastSentTime = DateTime.Now;
            DataSent(this, new IrcDataEventArgs(command));
        }
        public void SendMessage(string target, string message)
        {
            SendCommand("PRIVMSG " + target.Trim() + " :" + message.Trim());
        }
        public void SendAction(string channel, string text)
        {
            SendCommand("PRIVMSG " + channel + " :" + (char)1 + "ACTION " + text);
        }
        public void SendNotice(string nick, string message)
        {
            SendCommand("NOTICE " + nick + " :" + message.Trim());
        }
        public void JoinChannel(string channel, string password = "")
        {
            channel = channel.ToLower();
            if (channel[0] != '#')
                channel = "#" + channel;

            if (Connected)
            {
                if (password == "")
                    SendCommand("JOIN " + channel);
                else
                    SendCommand("JOIN " + channel + " " + password);

                SendMessage(channel, "Hello " + channel + " !");
                SendAction(channel, "planteaza un nuc!");
            }
            else
                throw new Exception("You have to be connected to join a channel");
        }
        #endregion

        #region Events
        public event IrcDataEventHandler DataRecieved;
        public event IrcDataEventHandler DataSent;
        private void IrcClient_DataRecieved(object sender, IrcDataEventArgs e)
        {
            IrcClient_UpdateChannelList(sender, e);
            if (e.Command != "PONG")
            {
                lastRecievedData = e.Data;

                //AppendText(DateTime.Now.ToString("<HH:mm:ss>") + e.Data + Environment.NewLine);
                Console.WriteLine(DateTime.Now.ToString("<HH:mm:ss>") +
                    "<" + e.From + " " + e.Command + " " + e.To + "> " +
                    IrcFormatting.ClearFormatting(e.Message));
            }
        }
        private void IrcClient_DataSent(object sender, IrcDataEventArgs e)
        {
            lastSentData = e.Data;
            IrcDataEventArgs e2 = new IrcDataEventArgs(":" + Nick + " " + e.Data);
            IrcClient_UpdateChannelList(sender, e2);

            sw.WriteLine(e.Data);

            Console.WriteLine(DateTime.Now.ToString("<HH:mm:ss>") +
                "<" + e2.From + " " + e2.Command + " " + e2.To + "> " +
                IrcFormatting.ClearFormatting(e2.Message));

            if (e2.Command == "NICK")
                nick = e2.To;
        }
        private void IrcClient_UpdateChannelList(object sender, IrcDataEventArgs e)
        {
            string[] msg = e.Message.Split(' ');
            switch (e.Command)
            {
                case "JOIN":
                    if (e.From == Nick)
                        Channels.Add(e.To);
                    else
                        Channels[e.To].Users.Add(e.From);
                    break;
                case "PART":
                    if (e.From == Nick)
                        Channels.Remove(e.To);
                    else
                        Channels[e.To].Users.Remove(e.From);
                    break;
                case "NICK":
                    for (int i = 0; i < Channels.Count; i++)
                        if (Channels[i].Users[e.From] != null)
                            Channels[i].Users[e.From].Name = e.To;
                    break;
                case "MODE":
                    switch (msg[0])
                    {
                        case "+a":
                            Channels[e.To].Users[msg[1]].IsAdmin = true;
                            break;
                        case "-a":
                            Channels[e.To].Users[msg[1]].IsAdmin = false;
                            break;
                        case "+o":
                            Channels[e.To].Users[msg[1]].IsOperator = true;
                            break;
                        case "-o":
                            Channels[e.To].Users[msg[1]].IsOperator = false;
                            break;
                        case "+h":
                            Channels[e.To].Users[msg[1]].IsHalfOperator = true;
                            break;
                        case "-h":
                            Channels[e.To].Users[msg[1]].IsHalfOperator = false;
                            break;
                        case "+v":
                            Channels[e.To].Users[msg[1]].IsVoice = true;
                            break;
                        case "-v":
                            Channels[e.To].Users[msg[1]].IsVoice = false;
                            break;
                    }
                    break;
                case "353":
                    string[] nick = e.Message.Substring(e.Message.IndexOf(':') + 1).Split(' ');
                    string chan = e.Message.Substring(e.Message.IndexOf('#'));
                    chan = chan.Substring(0, chan.IndexOf(' ')).Trim().ToLower();

                    Channels[chan].Users.Clear();
                    for (int i = 0; i < nick.Length; i++)
                        Channels[chan].Users.Add(nick[i]);

                    break;
            }
        }
        #endregion
    }

    #region Event Class
    public delegate void IrcDataEventHandler(object sender, IrcDataEventArgs e);
    public class IrcDataEventArgs : EventArgs
    {
        public string Data { get; set; }
        public string Message
        {
            get
            {
                if (Data.Split(' ').Length > 3)
                {
                    string str = Data;

                    for (int i = 0; i < 3; i++)
                        str = str.Substring(str.IndexOf(' ') + 1);

                    if (str[0] == ':' ||
                        str[0] == '=' ||
                        str[0] == ' ')

                        str = str.Substring(1);

                    return str.Trim();
                }
                else
                    return "";
            }
        }
        public string From
        {
            get
            {
                string str = Data.Substring(0, Data.IndexOfAny(new char[] { '!', ' ' }));

                if (str[0] == ':')
                    str = str.Substring(1);

                return str;
            }
        }
        public string To { get { return Data.Substring(1).Split(' ')[2].Replace(":", ""); } }
        public string Command { get { return Data.Substring(1).Split(' ')[1].ToUpper(); } }

        public IrcDataEventArgs(string data)
        {
            Data = data;
        }
        public string GetInfo()
        {
            return Data;
        }
    }
    #endregion
}
