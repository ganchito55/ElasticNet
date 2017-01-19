using System;
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