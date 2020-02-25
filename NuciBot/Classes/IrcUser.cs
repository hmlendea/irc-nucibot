using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace IRC_Client.User
{
    public enum UserMode
    {
        Default = 0,
        Voice = 3,
        HalfOperator = 4,
        Operator = 5,
        Admin = 10,
        Owner = 69
    }

    public class IrcUser
    {
        public string Name { get; set; }
        public UserMode Mode
        {
            get
            {
                if (IsOwner)
                    return UserMode.Owner;
                else if (IsAdmin)
                    return UserMode.Admin;
                else if (IsOperator)
                    return UserMode.Operator;
                else if (IsHalfOperator)
                    return UserMode.HalfOperator;
                else if (IsVoice)
                    return UserMode.Voice;
                else
                    return UserMode.Default;
            }
        }

        public bool IsVoice { get; set; }
        public bool IsHalfOperator { get; set; }
        public bool IsOperator { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsOwner { get; set; }

        public IrcUser(string userString)
        {
            userString = userString.Trim();

            switch (userString[0])
            {
                case '+':
                    Name = userString.Substring(1);
                    IsVoice = true;
                    break;
                case '%':
                    Name = userString.Substring(1);
                    IsHalfOperator = true;
                    break;
                case '@':
                    Name = userString.Substring(1);
                    IsOperator = true;
                    break;
                case '&':
                    Name = userString.Substring(1);
                    IsAdmin = true;
                    break;
                case '~':
                    Name = userString.Substring(1);
                    IsOwner = true;
                    break;
                default:
                    Name = userString;
                    break;
            }
        }
        public IrcUser(string user, UserMode mode)
        {
            new IrcUser(((char)mode).ToString() + user);
        }
    }
    public class IrcUserCollection
    {
        public IrcUser this[string name]
        {
            get
            {
                for (int i = 0; i < Count; i++)
                    if (User[i].Name.ToLower() == name.ToLower())
                        return User[i];

                return null;
            }
        }
        public IrcUser this[int index] { get { return User[index]; } }

        public IrcUser[] User
        {
            get { return user; }
            set { user = value; }
        } private IrcUser[] user;
        public int Count { get { return User.Length; } }

        public IrcUserCollection()
        {
            User = new IrcUser[0];
        }

        public void Add(string userString)
        {
            Array.Resize(ref this.user, this.user.Length + 1);
            User[User.Length - 1] = new IrcUser(userString);
        }
        public void Add(string user, UserMode mode)
        {
            Add(((char)mode).ToString() + user);
        }
        public void Remove(string user)
        {
            int i = 0;
            while (i < User.Length)
            {
                if (User[i].Name == user)
                {
                    for (int j = i + 1; j < user.Length; j++)
                        User[j - 1] = User[j];

                    Array.Resize(ref this.user, User.Length - 1);
                }

                i += 1;
            }
        }
        public void Clear()
        {
            User = new IrcUser[0];
        }
    }
}
