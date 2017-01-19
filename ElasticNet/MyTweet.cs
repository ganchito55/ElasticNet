using System;
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
        [Text(Name = "tweet",Analyzer = "myAnalizer")]
        public String Msg { get; set; }

        /// <summary>
        /// Remove "RT", users (@nick), urls
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String FilterCharacters(string input)
        {   
            Regex regex = new Regex("(RT)|(@[^\\s]+)|(http[^\\s]+)");
            return regex.Replace(input, String.Empty).Trim();
        }
    }

    // ReSharper disable once InconsistentNaming
    public class MyTweetDFR : MyTweet
    {
        public MyTweetDFR(string msg) : base(msg)
        {
        }
     
        [Text(Name = "tweet", Analyzer = "myAnalizer", Similarity = "dfr")]
        public String Txt { get; set; }
    }
}