using Discord;
using Discord.Commands;

using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace TheHerald {

    public class MyBot {
        // Recognizes the existence of Discord:
        DiscordClient discord;
        
        public MyBot() {
            discord = new DiscordClient(x => {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x => {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            var commands = discord.GetService<CommandService>();

            commands.CreateCommand("newshelp").Do(async (e) =>
            {
                await e.Channel.SendMessage("**__The Herald | Commands:__**");
                await e.Channel.SendMessage(":newspaper2: **!news** | Returns the first news article pertaining to the topic.");
                await e.Channel.SendMessage(">>> If you want to add whitespace, please use '_' instead.");
                await e.Channel.SendMessage(":microscope: **!data** | Returns the first data article pertaining to the topic.");
            });

            commands.CreateCommand("news").Parameter("topic", ParameterType.Required).Do(async (e) =>
                {
                    await FindNews(e);
                });

            commands.CreateCommand("data").Parameter("topic", ParameterType.Required).Do(async (e) =>
                {
                    await FindData(e);
                });

            discord.ExecuteAndWait(async () => {
                await discord.Connect("MzQ3MDY3OTEzNzIwNjkyNzQ2.DHTJIA.dTSr-rUUxHbfGDg_TNF9P_DQ8AU", TokenType.Bot);
            });
        }

        private void Log (object sender, LogMessageEventArgs e) {
            Console.WriteLine(e.Message);
        }

        private async Task FindNews(CommandEventArgs e) {
            string search = e.Args[0].Replace('_', '+');
            string aesthetic = e.Args[0].Replace('_', ' ');
            String url = "http://www.bbc.co.uk/search?q=" + search;
            HttpWebRequest access = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse result = (HttpWebResponse)access.GetResponse();
            string html;

            if (result.StatusCode == HttpStatusCode.OK) {
                Stream responseStream = result.GetResponseStream();
                StreamReader readStream;

                if (result.CharacterSet == null) {
                    readStream = new StreamReader(responseStream);
                } else {
                    readStream = new StreamReader(responseStream, Encoding.GetEncoding(result.CharacterSet));
                }

                html = readStream.ReadToEnd();
                result.Close();
                readStream.Close();

                Regex stuff = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase);
                Match match;
                List<string> urls = new List<string>();
                List<string> news = new List<string>();

                int even = 0;
                for (match = stuff.Match(html); match.Success; match = match.NextMatch()) {
                    foreach (Group group in match.Groups) {
                        if (even == 0) {
                            even++;
                        } else {
                            urls.Add("" + group);
                            even = 0;
                        }
                        
                    }
                }

                for (int i = 0; i < urls.Count; i++) {
                    if ((urls[i].Contains("/news/") || urls[i].Contains("newsbeat")) && urls[i].Contains("http") && !urls[i].Contains("data")) {
                        news.Add(urls[i]);
                        Console.WriteLine(urls[i]);
                    }
                }

                if (news.Count == 0) {
                    await e.Channel.SendMessage("**Sorry... I couldn't find news on " + aesthetic + "...** :frowning:");
                } else {
                    await e.Channel.SendMessage("**Here is the most recent news article on " + aesthetic + " I could find:**");
                    await e.Channel.SendMessage(news[0]);
                }
            } else {
                await e.Channel.SendMessage("Unable to find news at this time. Sorry! :sob:");
            }
            
        }

        // LOCATES DATA INSTEAD OF NEWS
        private async Task FindData(CommandEventArgs e)
        {
            string search = e.Args[0].Replace('_', '+');
            string aesthetic = e.Args[0].Replace('_', ' ');
            String url = "http://www.bbc.co.uk/search?q=" + search;
            HttpWebRequest access = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse result = (HttpWebResponse)access.GetResponse();
            string html;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = result.GetResponseStream();
                StreamReader readStream;

                if (result.CharacterSet == null)
                {
                    readStream = new StreamReader(responseStream);
                }
                else
                {
                    readStream = new StreamReader(responseStream, Encoding.GetEncoding(result.CharacterSet));
                }

                html = readStream.ReadToEnd();
                result.Close();
                readStream.Close();

                Regex stuff = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase);
                Match match;
                List<string> urls = new List<string>();
                List<string> news = new List<string>();

                int even = 0;
                for (match = stuff.Match(html); match.Success; match = match.NextMatch())
                {
                    foreach (Group group in match.Groups)
                    {
                        if (even == 0)
                        {
                            even++;
                        }
                        else
                        {
                            urls.Add("" + group);
                            even = 0;
                        }

                    }
                }

                for (int i = 0; i < urls.Count; i++)
                {
                    if (urls[i].Contains("data") && urls[i].Contains("http"))
                    {
                        news.Add(urls[i]);
                        Console.WriteLine(urls[i]);
                    }
                }

                if (news.Count == 0)
                {
                    await e.Channel.SendMessage("**Sorry... I couldn't find data on " + aesthetic + "...** :frowning:");
                }
                else
                {
                    await e.Channel.SendMessage("**Here is the most recent data article on " + aesthetic + " I could find:**");
                    await e.Channel.SendMessage(news[0]);
                }
            }
            else
            {
                await e.Channel.SendMessage("Unable to find data at this time. Sorry! :sob:");
            }

        }

    }

}
