using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace ElasticNet
{
    public class ViewModel : BindableBase
    {
        private readonly ElasticNetWrapper _elasticNet = new ElasticNetWrapper();

        public ViewModel()
        {
            ConnectCommand = new DelegateCommand(Connect);
            SearchTweetsCommand = new DelegateCommand(SearchTweets);
            CreateIndecesCommand = new DelegateCommand(CreateIndices);
            ImportTweetsCommand = new DelegateCommand(ImportTweets);
            SearchInElasticCommand = new DelegateCommand(SearchInElastic);
            RefreshIndicesCommand = new DelegateCommand(RefreshIndices);
            ShowResultsWindowCommand = new DelegateCommand<string>(ShowResultsWindow);
        }

        #region Indices

        private ObservableCollection<IndexGUI> _indexGuis;
        // ReSharper disable once InconsistentNaming
        public ObservableCollection<IndexGUI> IndexGUIs
        {
            get { return _indexGuis; }
            set { SetProperty(ref _indexGuis, value); }
        }

        private string _searchInElasticText = "your search";

        public string SearchInElasticText
        {
            get { return _searchInElasticText; }
            set { SetProperty(ref _searchInElasticText, value); }
        }

        public DelegateCommand SearchInElasticCommand { get; set; }
        public DelegateCommand RefreshIndicesCommand { get; set; }

        /// <summary>
        ///     Get all indices which name contains the IndexName
        /// </summary>
        private async void RefreshIndices()
        {
            IndexGUIs = await _elasticNet.GetElasticSearchIndices(IndexName);
        }

        /// <summary>
        ///     Search in all indices
        /// </summary>
        private void SearchInElastic()
        {
            _elasticNet.TweetMatchSearch(IndexGUIs, SearchInElasticText);
        }

        /// <summary>
        ///     Impor all tweets to all created indices
        /// </summary>
        private async void ImportTweets()
        {
            await _elasticNet.ImportTweetsBulk(TweetsRecovered.ToList());
            RefreshIndices();
        }

        /// <summary>
        ///     Create all Indices show documentation
        /// </summary>
        private void CreateIndices()
        {
            _elasticNet.CreateStandardIndex(IndexName);
            _elasticNet.CreateKStemIndex(IndexName);
            _elasticNet.CreatePorterStemmerIndex(IndexName);
            _elasticNet.CreateSnowballIndex(IndexName);
            _elasticNet.CreateStopWordIndex(IndexName);
            _elasticNet.CreateStopWordSnowballIndex(IndexName);
            _elasticNet.CreateStopWordSnowballIndexDFR(IndexName);
            RefreshIndices();
        }

        /// <summary>
        ///     Create the new window with the results retrievaled
        /// </summary>
        /// <param name="obj"></param>
        private void ShowResultsWindow(string obj)
        {
            var index = IndexGUIs.First(t => t.Name.Equals(obj));
            var window = new ResultsIndexWindow
            {
                DataContext = index,
                Title = index.Name
            };
            window.Show();
        }

        private string _indexName = "your index";

        public string IndexName
        {
            get { return _indexName; }
            set { SetProperty(ref _indexName, value); }
        }

        public DelegateCommand CreateIndecesCommand { get; set; }
        public DelegateCommand ImportTweetsCommand { get; set; }
        public DelegateCommand<string> ShowResultsWindowCommand { get; set; }

        #endregion

        #region elasticConfig

        /// <summary>
        ///     Connect to ElasticSearch
        /// </summary>
        private void Connect()
        {
            var connected = _elasticNet.Connect(User, Pass, Host);
            ConnectText = connected ? "Disconnect" : " Connect";
        }

        private string _host = "188.166.147.155:4444";

        public string Host
        {
            get { return _host; }
            set { SetProperty(ref _host, value); }
        }


        private string _user = "elastic";

        public string User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }


        private string _pass = "mineoMineo";

        public string Pass
        {
            get { return _pass; }
            set { SetProperty(ref _pass, value); }
        }


        private string _connectText = "Connect";

        public string ConnectText
        {
            get { return _connectText; }
            set { SetProperty(ref _connectText, value); }
        }


        public DelegateCommand ConnectCommand { get; set; }

        #endregion

        #region Tweets

        /// <summary>
        ///     Look for tweets with Twitter API
        /// </summary>
        private async void SearchTweets()
        {
            var searchParameter = new SearchTweetsParameters(SearchText)
            {
                SearchType = SearchResultType.Mixed,
                MaximumNumberOfResults = 50
            };
            Auth.SetUserCredentials("Jh0bbj6EMR6AlFPh6VC7C7zSM",
                "wtlp17TnbIboqmbXn7Mn1REAxQdncNV0uynAqiYGQu3FnLH3dl",
                "229170763-jnZpiJ3XIkpy9JzwbMtyt2wOqEG0ZRKAXcGjTWga",
                "DWlRojhGL2OnxRbZMJqZLl1dVCZDDLGrutmiEtz4bbYdV");

            var tweets = await SearchAsync.SearchTweets(searchParameter);
            TweetsRecovered =
                new ObservableCollection<string>(
                    tweets.Select(tweet => MyTweet.FilterCharacters(tweet.FullText)).ToList());
        }

        public DelegateCommand SearchTweetsCommand { get; set; }


        private string _searchText = "your search";

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }


        private ObservableCollection<string> _tweetsRecovered;

        public ObservableCollection<string> TweetsRecovered
        {
            get { return _tweetsRecovered; }
            set { SetProperty(ref _tweetsRecovered, value); }
        }

        #endregion
    }
}