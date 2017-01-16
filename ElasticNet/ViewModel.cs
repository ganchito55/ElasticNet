using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Elasticsearch.Net;
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
        private ElasticClient _client;

        public ViewModel()
        {
            ConnectCommand = new DelegateCommand(Connect);
            SearchTweetsCommand = new DelegateCommand(SearchTweets);
            CreateIndecesCommand = new DelegateCommand(CreateIndeces);
            ImportTweetsCommand = new DelegateCommand(ImportTweets);
            SearchInElasticCommand = new DelegateCommand(SearchInElastic);
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
            
            Uri uri;
            if (String.IsNullOrEmpty(User) || String.IsNullOrEmpty(Pass))
            {
                uri = new Uri("http://" + _host);
            }
            else
            {
                uri = new Uri("http://" + _user + ":" + _pass + "@" + _host);
            }


            _client = new ElasticClient(uri);
            ConnectText = _client.CatHealth().IsValid ? "Disconnect" : "Connect";  
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

        private void SearchInElastic()
        {
            var request = new SearchRequest
            {
               Query = new MatchQuery {Field = "msg", Query = "university" }
            };

            var response = _client.Search<MyTweet>(request);
        }

        /// <summary>
        /// Retrievals all created indices
        /// </summary>
        private async void GetElasticSearchIndices()
        {
            var response = (await _client.CatIndicesAsync())?.Records.Where(t => t.Index.Contains(IndexName)).ToList(); 
            IndexGUIs = new ObservableCollection<IndexGUI>();
            if (response != null)
                foreach (CatIndicesRecord indicesRecord in response)
                {
                    IndexGUIs.Add(new IndexGUI() { Name = indicesRecord.Index, DocumentsNumber = indicesRecord.DocsCount });
                }
        }

        private void CreateIndeces()
        {
            CreateAllIndices(IndexName);
            GetElasticSearchIndices();
        }

        private void CreateAllIndices(string name)
        {
            //CreateStandardIndex(name);
            CreateEnglishStemmerIndex(name);
            //CreateEnglishStopWordsIndex(name);
            CreateLightStemmerIndex(name);
            CreateStandardIndex(name);
            //CreateMinimalStemmerIndex(name);
        }

        private async void ImportTweets()
        {
            var operations = new List<IBulkOperation>();
           


            foreach (var tweet in TweetsRecovered)
            {
                operations.Add(new BulkIndexOperation<MyTweet>(new MyTweet(tweet))); 
            }

            List<Task> imports= new List<Task>();

            foreach (IndexGUI indexGui in IndexGUIs)
            {
                imports.Add(_client.BulkAsync(new BulkRequest(indexGui.Name)
                {
                    Operations = operations
                }));  
            }

            await Task.WhenAll(imports);
            GetElasticSearchIndices(); 
            //MyTweet tweet = new MyTweet("hello world");
            //var response = _client.Index(tweet, idx => idx.Index("aa11-std"));
        }


        /// <summary>
        /// Creates an standard index
        /// </summary>
        /// <param name="name"></param>
        private void CreateStandardIndex(string name)
        {
            _client.CreateIndex(name + "-std");
        }

        /// <summary>
        /// Creates an index state
        /// </summary>
        /// <param name="analyzerDef"></param>
        /// <returns></returns>
        private IndexState CreateIndexState()
        {
            CustomAnalyzer analyzerDef = new CustomAnalyzer
            {
                Tokenizer = "standard",
                Filter = new List<string>() { "myFilter" }
            };

            IndexState state = new IndexState
            {
                Settings = new IndexSettings
                {
                    Analysis = new Analysis { Analyzers = new Analyzers { { "default", analyzerDef } } }
                }
            };
            return state;
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
        /// <summary>
        /// Creates an index with light english stemmer
        /// </summary>
        /// <param name="name"></param>
        private void CreateLightStemmerIndex(string name)
        {

            var state = CreateIndexState();
            state.Settings.Analysis.TokenFilters = new TokenFilters
            {
                {"myFilter", new StemmerTokenFilter() {Language = "light_english"}}
            };  
            _client.CreateIndex(new CreateIndexRequest(name + "-stem-lig", state));
        }   

        /// <summary>
        /// Creates an index with english stemmer
        /// </summary>
        /// <param name="name"></param>
        private void CreateEnglishStemmerIndex(string name)
        {
            var state = CreateIndexState(); 
            state.Settings.Analysis.TokenFilters = new TokenFilters
            {
                {"myFilter", new StemmerTokenFilter() {Language = "english"}}
            }; 
            
             
                _client.CreateIndex(name + "-stem-eng", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.Stemmer("myFilter", j => j.Language("english")));
                    a.Analyzers(an => an.Custom("juanPedrro", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters(new string[] {"lowercase", "myFilter" }
                        );
                        return def;
                    }));
                    return a;
                }));

                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));


                return c;
            });
           // _client.CreateIndex(new CreateIndexRequest(name + "-stem-eng", state));
        }

 

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