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
}