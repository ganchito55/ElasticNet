using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Prism.Commands;
using Prism.Mvvm;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace ElasticNet
{
    public class ViewModel : BindableBase
    {  
        private ElasticNetWrapper _elasticNet = new ElasticNetWrapper();

        public ViewModel()
        {
            ConnectCommand = new DelegateCommand(Connect);
            SearchTweetsCommand = new DelegateCommand(SearchTweets);
            CreateIndecesCommand = new DelegateCommand(CreateIndices);
            ImportTweetsCommand = new DelegateCommand(ImportTweets);
            SearchInElasticCommand = new DelegateCommand(SearchInElastic);
            RefreshIndicesCommand = new DelegateCommand(RefreshIndices);
        } 
        private async void SearchTweets()
        {
            var searchParameter = new SearchTweetsParameters(SearchText)
            {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Mixed,
                MaximumNumberOfResults = 20
            };
            Auth.SetUserCredentials("Jh0bbj6EMR6AlFPh6VC7C7zSM",
                "wtlp17TnbIboqmbXn7Mn1REAxQdncNV0uynAqiYGQu3FnLH3dl",
                "229170763-jnZpiJ3XIkpy9JzwbMtyt2wOqEG0ZRKAXcGjTWga",
                "DWlRojhGL2OnxRbZMJqZLl1dVCZDDLGrutmiEtz4bbYdV");
            var tweets = await SearchAsync.SearchTweets(searchParameter);
            TweetsRecovered = new ObservableCollection<string>(tweets.Select(tweet => tweet.FullText).ToList());
        }

        private void Connect()
        {        
            var connected = _elasticNet.Connect(User, Pass, Host);
            ConnectText = connected ? "Disconnect" : " Connect";
        }

        #region Indices 
        private ObservableCollection<IndexGUI> _indexGuis; 
        public ObservableCollection<IndexGUI> IndexGUIs
        {
            get { return this._indexGuis; }
            set { SetProperty(ref _indexGuis, value); }
        }  

        private String _searchInElasticText="your search"; 
        public String SearchInElasticText
        {
            get { return this._searchInElasticText; }
            set { SetProperty(ref _searchInElasticText, value); }
        }

        public DelegateCommand SearchInElasticCommand { get; set; }
        public DelegateCommand RefreshIndicesCommand { get; set; }

        /// <summary>
        /// Get all indices which name contains the IndexName
        /// </summary>
        private async void RefreshIndices()
        {
            IndexGUIs = await _elasticNet.GetElasticSearchIndices(IndexName);
        }

        /// <summary>
        /// Search in all indices 
        /// </summary>
        private void  SearchInElastic()
        {
             _elasticNet.TweetMatchSearch(IndexGUIs, SearchInElasticText); 
        }

        /// <summary>
        /// Impor all tweets to all created indices
        /// </summary>
        private async void ImportTweets()
        {
            await _elasticNet.ImportTweetsBulk(TweetsRecovered.ToList());
            RefreshIndices();
        }



        private void CreateIndices()
        {
            _elasticNet.CreateStandardIndex(IndexName);
            _elasticNet.CreateEnglishStemmerIndex(IndexName);
            _elasticNet.CreateLightStemmerIndex(IndexName);
            //CreateEnglishStopWordsIndex(IndexName);
            //CreateMinimalStemmerIndex(IndexName);
            RefreshIndices();
        } 

    


        

       

        /// <summary>
        /// Creates an index with minimal_english stemmer
        /// </summary>
        /// <param name="name"></param>
    /*    private void CreateMinimalStemmerIndex(string name)
        {
            CustomAnalyzer analyzerDef = new CustomAnalyzer
            {
                Tokenizer = "standard",
                Filter = new List<string>() {"minimal_english"}
            };   
            var state = CreateIndexState(analyzerDef);
            _client.CreateIndex(new CreateIndexRequest(name + "-stem-min",state));
        }
      */
        

        

 

        /*private void CreateEnglishStopWordsIndex(string name)
        {
            CustomAnalyzer analyzerDef = new CustomAnalyzer
            {
                Tokenizer = "standard",
                Filter = new List<string>() { "english_stop" }
            };

            var state = CreateIndexState(analyzerDef);
            _client.CreateIndex(new CreateIndexRequest(name + "-stop-word", state));
        }
          */





    

    #endregion

        #region Properties

        #region elasticConfig

        private String _host = "188.166.147.155:4444";

        public String Host
        {
            get { return this._host; }
            set { SetProperty(ref _host, value); }
        }


        private String _user = "elastic";

        public String User
        {
            get { return this._user; }
            set { SetProperty(ref _user, value); }
        }


        private String _pass = "mineoMineo";

        public String Pass
        {
            get { return this._pass; }
            set { SetProperty(ref _pass, value); }
        }


        private String _connectText = "Connect";

        public String ConnectText
        {
            get { return this._connectText; }
            set { SetProperty(ref _connectText, value); }
        }


        public DelegateCommand ConnectCommand { get; set; }

        #endregion

        #region Tweets

        public DelegateCommand SearchTweetsCommand { get; set; }


        private string _searchText = "your search";

        public string SearchText
        {
            get { return this._searchText; }
            set { SetProperty(ref _searchText, value); }
        }


        private ObservableCollection<String> _tweetsRecovered;

        public ObservableCollection<String> TweetsRecovered
        {
            get { return this._tweetsRecovered; }
            set { SetProperty(ref _tweetsRecovered, value); }
        }

        #endregion


        private String _indexName="your index";

        public String IndexName
        {
            get { return this._indexName; }
            set { SetProperty(ref _indexName, value); }
        }

        public DelegateCommand CreateIndecesCommand { get; set; }
        public DelegateCommand ImportTweetsCommand { get; set; }

        #endregion



    }

}