using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using IRC_Client.User;

namespace IRC_Client.Channel
{
    public class IrcChannel
    {
        public string Name { get; set; }
        public IrcUserCollection Users { get; set; }

        public IrcChannel(string channel)
        {
            Name = channel;
            Users = new IrcUserCollection();
        }
    }

    public class IrcChannelCollection
    {
        public IrcChannel this[string name]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                    if (Channel[i].Name.ToLower() == name.ToLower())
                        return Channel[i];

                return null;
            }
        }
        public IrcChannel this[int index] { get { return Channel[index]; } }

        public IrcChannel[] Channel
        {
            get { return channel; }
            set { channel = value; }
        } private IrcChannel[] channel;
        public int Count { get { return Channel.Length; } }

        public IrcChannelCollection()
        {
            Channel = new IrcChannel[0];
        }

        public void Add(string channel)
        {
            if (this[channel] == null)
            {
                Array.Resize(ref this.channel, this.channel.Length + 1);
                Channel[Channel.Length - 1] = new IrcChannel(channel);
            }
        }
        public void Remove(string channel)
        {
            for (int i = 0; i < Channel.Length; i++)
                if (Channel[i].Name == channel)
                {
                    for (int j = i + 1; j < channel.Length; j++)
                        Channel[j - 1] = Channel[j];

                    Array.Resize(ref this.channel, Channel.Length - 1);
                }
        }
        public void Clear()
        {
            Channel = new IrcChannel[0];
        }
    }
}
