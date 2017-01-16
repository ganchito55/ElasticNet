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
        [Text(Name = "tweet",Analyzer = "juanPedrro")]
        public String Msg { get; set; }
    }
}