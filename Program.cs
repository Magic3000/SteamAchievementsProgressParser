using static System.ConsoleColor;
using System.Net;
using System.Text.RegularExpressions;

namespace Program
{
    public static class Extentions
    {
        public static void _sout(this object _text, ConsoleColor col = White, bool newStr = true)
        {
            Console.ForegroundColor = col;
            Console.Write($"{_text}{(newStr ? "\n" : "")}");
            Console.ForegroundColor = White;
        }
        public static ConsoleColor rndColor
        {
            get
            {
                return (ConsoleColor)Program.rnd.Next(9, 16);
            }
        }
    }
    public class Achievement
    {
        public string name;
        public string description;
        public string currentProgress;
        public string totalProgress;
    }
    public static class Program
    {
        public static Random rnd = new Random();
        public static string GetTextBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            return string.Empty;
        }
        static void Main(string[] args)
        {
            $"Enter you steam url name (steam api-key not required so set public profile privacy): "._sout(Green, false);
            var username = Console.ReadLine();
            $"Enter app id you want to check achievements progress: "._sout(Blue, false);
            var appid = Console.ReadLine();
            string uri = $"https://steamcommunity.com/id/{username}/stats/{appid}/?tab=achievements";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            try
            {
                using var webResponse = request.GetResponse();
                using var responseStreamReader = new StreamReader((webResponse as HttpWebResponse).GetResponseStream());
                var htmlResult = responseStreamReader.ReadToEnd();
                List<Achievement> achievements = new List<Achievement>();
                string achievementName = string.Empty;
                string achievementDescription = string.Empty;
                foreach (var segment in htmlResult.Split(" / ").ToList().Select(x => Regex.Replace(x, @"\s*(<[^>]+>)\s*", "$1", RegexOptions.Singleline)))
                {
                    var numStart = string.Empty;
                    var numEnd = string.Empty;
                    if (char.IsNumber(segment[0]))
                        for (int i = 0; i < segment.Length; i++)
                        {
                            char c = segment[i];
                            if (char.IsDigit(c) || c == ',' || c == ',')
                                numEnd += c;
                            else
                                break;
                        }

                    if (char.IsNumber(segment[segment.Length - 1]))
                        for (int i = segment.Length; i > 0; i--)
                        {
                            char c = segment[i - 1];
                            if (char.IsDigit(c) || c == ',' || c == ',')
                                numStart += c;
                            else
                                break;
                        }

                    numStart = new string(numStart.Reverse().ToArray());

                    if (segment.Contains("<div class=\"achieveTxt withProgress\"><h3 class=\"ellipsis\">"))
                        achievementName = GetTextBetween(segment, "<div class=\"achieveTxt withProgress\"><h3 class=\"ellipsis\">", "</h3>");

                    if (segment.Contains($"{achievementName}</h3><h5 class=\"ellipsis\">"))
                        achievementDescription = GetTextBetween(segment, $"{achievementName}</h3><h5 class=\"ellipsis\">", "</h5>");

                    bool numStartPresents = !string.IsNullOrWhiteSpace(numStart);
                    if (numStartPresents)
                    {
                        var achievement = new Achievement();
                        achievement.name = achievementName.Replace("&quot;", "\"");
                        achievement.description = achievementDescription.Replace("&quot;", "\"");
                        achievement.currentProgress = numStart;
                        achievements.Add(achievement);
                    }
                    if (!string.IsNullOrWhiteSpace(numEnd) && achievements.Count > 0)
                        achievements[achievements.Count - (numStartPresents ? 2 : 1)].totalProgress = numEnd;
                }

                $"Total achievements in progress: {achievements.Count}"._sout(Yellow);
                for (int i = 0; i < achievements.Count; i++)
                {
                    var achievement = achievements[i];
                    $"{i + 1}) [{achievement.currentProgress}/{achievement.totalProgress}] {achievement.name} ({achievement.description})"._sout(Cyan);
                }

            }
            catch (WebException webExc)
            {
                try
                {
                    using var exceptionStreamReader = new StreamReader(webExc.Response.GetResponseStream());
                    $"Request error: {exceptionStreamReader.ReadToEnd()}"._sout(Red);
                }
                catch (Exception exc)
                {
                    $"Request exception: {exc.Message}"._sout(Red);
                }
            }
            Console.ReadKey();
        }
    }
}