using System.Text.RegularExpressions;
using Nest;

namespace ElasticNet
{
    public class MyTweet
    {
        public MyTweet(string msg)
        {
            Msg = msg;
        }

        [Text(Name = "tweet", Analyzer = "myAnalizer")]
        public string Msg { get; set; }

        /// <summary>
        ///     Remove "RT", users (@nick), urls
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FilterCharacters(string input)
        {
            var regex = new Regex("(RT)|(@[^\\s]+)|(http[^\\s]+)");
            return regex.Replace(input, string.Empty).Trim();
        }
    }

    // ReSharper disable once InconsistentNaming
    public class MyTweetDFR : MyTweet
    {
        public MyTweetDFR(string msg) : base(msg)
        {
        }

        [Text(Name = "tweet", Analyzer = "myAnalizer", Similarity = "dfr")]
        public string Txt { get; set; }
    }
}