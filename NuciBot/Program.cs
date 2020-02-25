using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;

using IRC_Client;
using IRC_Client.Formatting;
using IRC_Client.Channel;
using IRC_Client.User;

using eRepublik;
using eRepublik.Citizens;
using eRepublik.Parties;

namespace NuciBot
{
    class Program
    {
        private static IrcClient irc;

        private static void Main(string[] args)
        {
            irc = new IrcClient();
            irc.DataRecieved += new IrcDataEventHandler(irc_DataRecieved);
            irc.DataSent += new IrcDataEventHandler(irc_DataSent);

            irc.Connect("irc.rizon.net", 6667, "NuciBot", "112231");
            JoinAllChannels();
        }

        private static void irc_DataRecieved(object sender, IrcDataEventArgs e)
        {
            if (e.Command == "PRIVMSG")
            {
                Log(e);

                if (e.To == irc.Nick)
                    return;

                string sep = " " + IrcFormatting.Bold(IrcFormatting.Color("::", ColorCode.Cyan)) + " ";

                if (e.Message.Contains("You cannot use control codes on this channel"))
                {
                    irc.SendCommand(IrcFormatting.ClearFormatting(irc.LastSentData));
                    irc.SendMessage(e.To, "Please give me voice so I can work properly, thanks! :)");
                }
                else
                {
                    #region Get Citizen Data commands
                    string ctzName = "";
                    try
                    {
                        Citizen ctz;
                        if (e.Message.IndexOf(' ') != -1)
                            ctzName = e.Message.Split(' ')[1];
                        else
                            ctzName = e.From;

                        switch (e.Message.Split(' ')[0].ToLower())
                        {
                            case ".link":
                            case ".profile":
                            case ".wiki":
                                #region .link
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Bold("Profile Link:") + " " + ctz.ProfileLink + sep +
                                   IrcFormatting.Bold("Wiki:") + " http://wiki.erepublik.com/index.php/" + ctz.Name.Replace(" ", "_").Replace("ț", "t"));
                                break;
                                #endregion
                            case ".donate":
                            case ".send":
                                #region .donate
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Bold("Items:") + " http://www.erepublik.com/en/economy/donate-items/" + ctz.ID + sep +
                                   IrcFormatting.Bold("Items:") + " http://www.erepublik.com/en/economy/donate-money/" + ctz.ID);

                                break;
                                #endregion
                            case ".newspaper":
                            case ".paper":
                                #region .newspaper
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Bold(ctz.Newspaper.Name) + " http://www.erepublik.com/en/newspaper/" + ctz.Newspaper.ID);
                                break;
                                #endregion
                            case ".avatar":
                            case ".picture":
                            case ".pic":
                                #region .newspaper
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Bold("Avatar:") + " " + ctz.AvatarLink);
                                break;
                                #endregion
                            case ".fc":
                            case ".hit":
                                #region.hit
                                ctz = GetCitizen(ctzName);

                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Color("[Q0: " + IrcFormatting.Bold(ctz.HitQ0.ToString("#,##0") + "]"), ColorCode.Grey) + sep +
                                   IrcFormatting.Color("[Q1: " + IrcFormatting.Bold(ctz.HitQ1.ToString("#,##0") + "]"), ColorCode.Cyan) + sep +
                                   IrcFormatting.Color("[Q2: " + IrcFormatting.Bold(ctz.HitQ2.ToString("#,##0") + "]"), ColorCode.Green) + sep +
                                   IrcFormatting.Color("[Q3: " + IrcFormatting.Bold(ctz.HitQ3.ToString("#,##0") + "]"), ColorCode.Blue) + sep +
                                   IrcFormatting.Color("[Q4: " + IrcFormatting.Bold(ctz.HitQ4.ToString("#,##0") + "]"), ColorCode.Orange) + sep +
                                   IrcFormatting.Color("[Q5: " + IrcFormatting.Bold(ctz.HitQ5.ToString("#,##0") + "]"), ColorCode.Red) + sep +
                                   IrcFormatting.Color("[Q6: " + IrcFormatting.Bold(ctz.HitQ6.ToString("#,##0") + "]"), ColorCode.Brown) + sep +
                                   IrcFormatting.Color("[Q7: " + IrcFormatting.Bold(ctz.HitQ7.ToString("#,##0") + "]"), ColorCode.Purple));
                                break;
                                #endregion
                            case ".bombs":
                                #region .newspaper
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                   IrcFormatting.Bold("Bombs used:") + " " + ctz.BombsUsed.Small + "x Small, " + ctz.BombsUsed.Big + "x Big" + sep +
                                   IrcFormatting.Bold("Last used:") + " " + ctz.BombsUsed.LastBombUsed);
                                break;
                                #endregion
                            case ".lp":
                            case ".lookup":
                                #region .lp
                                ctz = GetCitizen(ctzName);

                                irc.SendMessage(e.To, sep +
                                    IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                    IrcFormatting.Bold("Level:") + " " + ctz.Level + " (" + ctz.Experience.ToString("#,##0") + ")" + sep +
                                    IrcFormatting.Bold("Strength:") + " " + ctz.Strength.ToString("#,##0") + sep +
                                    IrcFormatting.Bold("Hit Q7:") + " " + ctz.HitQ7.ToString("#,##0") + sep +
                                    IrcFormatting.Bold("Birthday:") + " " + ctz.BirthDay.Date.ToString(GameInfo.Culture.DateTimeFormat.FullDateTimePattern, GameInfo.Culture) + " (eDay " + ctz.BirthDay.eDay + ")" + sep +
                                    IrcFormatting.Bold("Age:") + " " + ctz.BirthDay.Age + " Days" + sep +
                                    IrcFormatting.Bold("Nat Rank:") + " " + ctz.NationalRank + sep +
                                    IrcFormatting.Bold("Rank:") + " " + ctz.Rank + " (" + ctz.RankPoints.ToString("#,##0") + ")" + sep +
                                    IrcFormatting.Bold("Residence:") + " " + ctz.Residence.Region + ", " + ctz.Residence.Country);
                                irc.SendMessage(e.To, sep +
                                    IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                    IrcFormatting.Bold("Medals:") + " " + ctz.Medals.Total + sep +
                                    IrcFormatting.Bold("Decorations:") + " " + ctz.DecorationsTotal + sep +
                                    IrcFormatting.Bold("Friends:") + " " + ctz.Friends + sep +
                                    IrcFormatting.Bold("Citizenship:") + " " + ctz.Citizenship + sep +
                                    IrcFormatting.Bold("Newspaper:") + " " + ctz.Newspaper.Name + sep +
                                    IrcFormatting.Bold("Party:") + " " + ctz.PoliticalParty.Name + " (" + ctz.PoliticalParty.ID + ")" + sep +
                                    IrcFormatting.Bold("Unit:") + " " + ctz.MilitaryUnit.Name + " (" + ctz.MilitaryUnit.ID + ")");
                                break;
                                #endregion .lp
                            case ".medals":
                                #region .medals
                                ctz = GetCitizen(ctzName);
                                irc.SendMessage(e.To, sep +
                                    IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                    IrcFormatting.Bold(ctz.Medals.Total.ToString()) + "x Total Medals" + sep +
                                    IrcFormatting.Bold(ctz.Medals.FreedomFighter.ToString()) + "x FF" + sep +
                                    IrcFormatting.Bold(ctz.Medals.HardWorker.ToString()) + "x HW" + sep +
                                    IrcFormatting.Bold(ctz.Medals.CongressMember.ToString()) + "x CM" + sep +
                                    IrcFormatting.Bold(ctz.Medals.CountryPresident.ToString()) + "x CP" + sep +
                                    IrcFormatting.Bold(ctz.Medals.BattleHero.ToString()) + "x BH" + sep +
                                    IrcFormatting.Bold(ctz.Medals.CampaignHero.ToString()) + "x CH" + sep +
                                    IrcFormatting.Bold(ctz.Medals.ResistanceHero.ToString()) + "x RH" + sep +
                                    IrcFormatting.Bold(ctz.Medals.SocietyBuilder.ToString()) + "x SB" + sep +
                                    IrcFormatting.Bold(ctz.Medals.Mercenary.ToString()) + "x MR" + sep +
                                    IrcFormatting.Bold(ctz.Medals.TopFighter.ToString()) + "x TF" + sep +
                                    IrcFormatting.Bold(ctz.Medals.TruePatriot.ToString()) + "x TP");
                                break;
                                #endregion .medals
                            case ".decorations":
                                #region .decorations
                                if (ctzName.Contains("Mlendea"))
                                {
                                    ctz = GetCitizen(ctzName);

                                    irc.SendMessage(e.To, sep +
                                       IrcFormatting.Bold(ctz.Name) + " [" + ctz.ID + "]" + sep +
                                       IrcFormatting.Bold(ctz.DecorationsTotal.ToString()) + "x Total Decorations");

                                    for (int i = 0; i < ctz.Decoration.Length; i++)
                                    {
                                        irc.SendMessage(e.To,
                                            IrcFormatting.Bold(ctz.Decoration[i].Count.ToString()) + "x " +
                                            ctz.Decoration[i].Text);
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                }
                                else
                                    irc.SendNotice(e.From, "You are not authorized to use \".decorations\"!");
                                break;
                                #endregion
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        if (GetCitizenID(ctzName) == -1)
                            irc.SendMessage(e.To, "Citizen \"" + ctzName + "\" could not be found in our database, please use " + IrcFormatting.Bold("\".regid ID\""));
                    }
                    #endregion
                    #region Channel commands
                    string orders;
                    string links;

                    switch (e.Message.Split(' ')[0].ToLower())
                    {
                        case ".orders":
                            #region .orders
                            orders = GetOrders(e.To);
                            if (orders != null)
                                irc.SendMessage(e.To,
                                    IrcFormatting.Bold(IrcFormatting.Color("Orders for " + e.To + " : ", ColorCode.Red)) +
                                    IrcFormatting.Bold(orders + " ") +
                                    IrcFormatting.Bold(IrcFormatting.Color("!!!", ColorCode.Red)));
                            else
                                irc.SendMessage(e.To, IrcFormatting.Bold("Orders for \"" + e.To + "\" not set!"));
                            break;
                            #endregion
                        case "!orders":
                            #region !orders
                            if (irc.Channels[e.To].Users[e.From].Mode >= UserMode.Operator)
                            {
                                orders = e.Message.Substring(e.Message.IndexOf(' ') + 1).Trim();
                                if (orders != "!orders")
                                {
                                    SaveOrders(e.To, orders);
                                    irc.SendMessage(e.To,
                                         IrcFormatting.Bold(IrcFormatting.Color("New Orders for " + e.To + " : ", ColorCode.Red)) +
                                         IrcFormatting.Bold(orders + " !!!"));
                                }
                                else
                                {
                                    ClearOrders(e.To);
                                    irc.SendMessage(e.To, IrcFormatting.Bold("Orders for \"" + e.To + "\" cleared!"));
                                }
                            }
                            else
                                irc.SendNotice(e.From, "You are not authorized to use \"!orders\" on this channel!");
                            break;
                            #endregion
                        case ".links":
                            #region .links
                            links = GetLinks(e.To);
                            if (links != null)
                                irc.SendMessage(e.To,
                                    IrcFormatting.Bold("Links for " + e.To + " : ") +
                                    IrcFormatting.Bold(links + " ") +
                                    IrcFormatting.Bold("!!!"));
                            else
                                irc.SendMessage(e.To, IrcFormatting.Bold("Links for \"" + e.To + "\" not set!"));
                            break;
                            #endregion
                        case "!links":
                            #region !links
                            if (irc.Channels[e.To].Users[e.From].Mode >= UserMode.Operator)
                            {
                                links = e.Message.Substring(e.Message.IndexOf(' ') + 1).Trim();
                                if (links != "!links")
                                {
                                    SaveLinks(e.To, links);
                                    irc.SendMessage(e.To,
                                         IrcFormatting.Bold("New Links for " + e.To + " : ") +
                                         IrcFormatting.Bold(links + " !!!"));
                                }
                                else
                                {
                                    ClearLinks(e.To);
                                    irc.SendMessage(e.To, IrcFormatting.Bold("Links for \"" + e.To + "\" cleared!"));
                                }
                            }
                            else
                                irc.SendNotice(e.From, "You are not authorized to use \"!links\" on this channel!");
                            break;
                            #endregion
                        case ".tutoriale":
                            #region .tutoriale
                            irc.SendMessage(e.To, sep +
                               IrcFormatting.Bold("Tutoriale utile din Ministerul Educatiei") + sep +
                               "http://eromania.wordpress.com/tutoriale");
                            break;
                            #endregion
                        default:
                            break;
                    }
                    #endregion
                    #region Party commands
                    Party prt;
                    try
                    {
                        switch (e.Message.Split(' ')[0].ToLower())
                        {
                            case ".party":
                                prt = new Party(Convert.ToInt32(e.Message.Split(' ')[1]));

                                irc.SendMessage(e.To, sep +
                                   IrcFormatting.Bold(prt.Name) + " [" + prt.ID + "]" + sep +
                                   IrcFormatting.Bold("Country:") + " " + prt.Country + sep +
                                   IrcFormatting.Bold("Members:") + " " + prt.Members + sep +
                                   IrcFormatting.Bold("President:") + " " + prt.President + " (" + prt.PresidentID + ")" + sep +
                                   IrcFormatting.Bold("Congressmans:") + " " + prt.CongressMembers + " (" + prt.CongressOccupancy + "%)" + sep +
                                   IrcFormatting.Bold("Orientation:") + " " + prt.Orientation + sep +
                                   IrcFormatting.Bold("Link:") + " " + prt.Link + sep +
                                   IrcFormatting.Bold("Wiki:") + " " + prt.Wiki);
                                break;
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        irc.SendMessage(e.To, "Error, fuuu!!! >:(");
                    }
                    #endregion
                    #region Other commands
                    try
                    {
                        switch (e.Message.Split(' ')[0].ToLower())
                        {
                            case ".nuci":
                                irc.SendMessage(e.To, (char)1 + "ACTION planteaza un nuc!");
                                break;
                            case ".regid":
                            case ".regnick":
                                SaveCitizenID(e.From, e.Message.Split(' ')[1]);
                                irc.SendMessage(e.To, "Registered nick \"" + IrcFormatting.Bold(e.From).ToLower() + "\" with id " + IrcFormatting.Bold(e.Message.Split(' ')[1]));
                                break;
                            case ".intrape":
                                if (e.From.Contains("Mlendea"))
                                    irc.JoinChannel(e.Message.Split(' ')[1].ToLower());
                                else
                                    irc.SendNotice(e.From, "You are not authorized to invite this bot on other channels!"); break;
                            case ".boobs":
                                irc.SendMessage(e.To, GetRandomBoobsEntry());
                                break;
                            case ".helpbot":
                            case ".bothelp":
                            case ".botinfo":
                            case ".help":
                                irc.SendMessage(e.To,
                                   IrcFormatting.Bold("NuciBot") + " [v" + Assembly.GetExecutingAssembly().GetName().Version + "]" + sep +
                                   IrcFormatting.Bold("Help doc:") + " http://bit.ly/1e0R1YB" + sep +
                                   IrcFormatting.Bold("By:") + " Mlendea Horatiu");
                                break;
                            case ".call":
                                #region .call
                                if (irc.Channels[e.To].Users[e.From].Mode >= UserMode.Operator || e.From == "Mlendea_Horatiu")
                                {
                                    string callMessage;
                                    if (e.Message.IndexOf(' ') != 0)
                                        callMessage = e.Message.Substring(e.Message.IndexOf(' ') + 1);
                                    else
                                        callMessage = "";
                                    callMessage = "»» " + callMessage + " ««";

                                    int j = 0;
                                    while (j < irc.Channels[e.To].Users.Count)
                                    {
                                        string st2 = "";

                                        int i;
                                        for (i = 0; i < 21; i++)
                                            if (i + j < irc.Channels[e.To].Users.Count)
                                                st2 += irc.Channels[e.To].Users.User[j + i].Name + " ";
                                        j += i;

                                        irc.SendMessage(e.To, st2);
                                        st2 = "";
                                    }

                                    irc.SendMessage(e.To, IrcFormatting.Bold(IrcFormatting.Color(callMessage, ColorCode.Red)));
                                }
                                else
                                    irc.SendNotice(e.From, "You are not authorized to use \".call\" on this channel!");
                                break;
                                #endregion
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        irc.SendMessage(e.To, "Error, fuuu!!! >:(");
                        irc.SendMessage(e.To, ex.Message);
                    }
                    #endregion
                }
            }
        }
        private static void irc_DataSent(object sender, IrcDataEventArgs e)
        {
            e = new IrcDataEventArgs(":" + irc.Nick + " " + e.Data);

            if (e.Command == "PRIVMSG" && e.To != irc.Nick)
                Log(e);
        }

        private static Citizen GetCitizen(string nick)
        {
            int id = GetCitizenID(nick);

            if (id != -1)
            {
                Citizen ctz = new Citizen();
                ctz.ID = id;
                ctz.Scan();
                return ctz;
            }

            return null;
        }
        private static int GetCitizenID(string nick)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("NickIDs.XML");

            if (xml.SelectSingleNode("/NickIDs/" + nick.ToLower()) != null)
                return Convert.ToInt32(xml.SelectSingleNode("/NickIDs/" + nick.ToLower()).InnerText);
            else
                return -1;
        }
        private static void SaveCitizenID(string nick, string id)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("NickIDs.XML");

            if (xml.SelectSingleNode("/NickIDs/" + nick.ToLower()) != null)
                xml.SelectSingleNode("/NickIDs/" + nick.ToLower()).InnerText = id;
            else
            {
                XmlElement xe = xml.CreateElement(nick.ToLower());
                xe.InnerText = id;
                xml.DocumentElement.AppendChild(xe);
            }

            xml.Save("NickIDs.XML");
        }

        #region Orders & Links
        private static string GetOrders(string channel)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Orders.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Orders/" + channel.ToLower()) != null)
                return xml.SelectSingleNode("/Orders/" + channel.ToLower()).InnerText;
            else
                return null;
        }
        private static void SaveOrders(string channel, string orders)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Orders.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Orders/" + channel.ToLower()) != null)
                xml.SelectSingleNode("/Orders/" + channel.ToLower()).InnerText = orders;
            else
            {
                XmlElement xe = xml.CreateElement(channel.ToLower());
                xe.InnerText = orders;
                xml.DocumentElement.AppendChild(xe);
            }

            xml.Save("Orders.XML");
        }
        private static void ClearOrders(string channel)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Orders.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Orders/" + channel.ToLower()) != null)
            {
                XmlNode xn = xml.SelectSingleNode("/Orders/" + channel.ToLower());
                xn.ParentNode.RemoveChild(xn);
            }


            xml.Save("Orders.XML");
        }

        private static string GetLinks(string channel)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Links.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Links/" + channel.ToLower()) != null)
                return xml.SelectSingleNode("/Links/" + channel.ToLower()).InnerText;
            else
                return null;
        }
        private static void SaveLinks(string channel, string links)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Links.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Links/" + channel.ToLower()) != null)
                xml.SelectSingleNode("/Links/" + channel.ToLower()).InnerText = links;
            else
            {
                XmlElement xe = xml.CreateElement(channel.ToLower());
                xe.InnerText = links;
                xml.DocumentElement.AppendChild(xe);
            }

            xml.Save("Links.XML");
        }
        private static void ClearLinks(string channel)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Links.XML");

            channel = channel.Replace("#", "");

            if (xml.SelectSingleNode("/Links/" + channel.ToLower()) != null)
            {
                XmlNode xn = xml.SelectSingleNode("/Links/" + channel.ToLower());
                xn.ParentNode.RemoveChild(xn);
            }


            xml.Save("Links.XML");
        }
        #endregion

        private static void JoinAllChannels()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Channels.XML");
            XmlNodeList chans = xml.SelectNodes("/Channels/Channel");

            foreach(XmlNode chan in chans)
            {
                irc.JoinChannel(chan.InnerText);
            }
        }
        private static string GetRandomBoobsEntry()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Boobs.XML");

            Random rnd = new Random();
            int count = xml.SelectNodes("/Boobs/Entry").Count;
            int no = rnd.Next(0, count);

            return "Boobs #" + no.ToString().PadLeft(count.ToString().Length, '0') + ": " + xml.SelectNodes("/Boobs/Entry")[no].InnerText;
        }

        private static void Log(IrcDataEventArgs e)
        {
            string path = "Logs\\" + DateTime.Now.ToString("yyyy.MM.dd") + "\\" + irc.Server + "\\" + e.To + ".LOG";

            if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (StreamWriter sw = File.AppendText(path))
                sw.WriteLine(DateTime.Now.ToString("<HH:mm>") + "<" + e.From + "> " + IrcFormatting.ClearFormatting(e.Message));
        }
    }
}
