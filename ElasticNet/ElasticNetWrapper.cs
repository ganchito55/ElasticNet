using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace ElasticNet
{
    public class ElasticNetWrapper
    {
        private ElasticClient _client;
        private ObservableCollection<IndexGUI> _indices;
        private bool _isConnected;

        /// <summary>
        /// Connect to ElasticSearch
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="host"></param>
        /// <returns>Status</returns>
        public bool Connect(string user, string pass, string host)
        {
            if (!_isConnected)
            {
                Uri uri;
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                    uri = new Uri("http://" + host);
                else
                    uri = new Uri("http://" + user + ":" + pass + "@" + host);
                _client = new ElasticClient(uri);
                _isConnected = _client.CatHealth().IsValid;
                return _isConnected;
            }

            //Disconnect
            _client = null;
            _isConnected = false;
            return _isConnected;
        }  

        /// <summary>
        ///     Do a match search in all indices
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="search"></param>
        public async void TweetMatchSearch(ObservableCollection<IndexGUI> indices, string search)
        {
            if (!_isConnected) return;

            foreach (var index in indices)
            {
                var request = new SearchRequest(index.Name)
                {
                    Size = 100,
                    Query = new MatchQuery {Field = "tweet", Query = search}
                };
                var response = await _client.SearchAsync<MyTweet>(request);
                index.Results.Clear();
                if (response == null) continue;
                for (int i = 0; i < response.Documents.Count; i++)
                {
                    var result = new ElasticResult
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        Text = response.Documents.ElementAt(i).Msg,
                        Score = response.Hits.ElementAt(i).Score.HasValue ? response.Hits.ElementAt(i).Score.Value : 0
                    };
                    index.Results.Add(result);
                    AnalyzeText(index.Name,result);
                }
            }
        }

        /// <summary>
        ///     Get all indices which name contains the IndexName
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<IndexGUI>> GetElasticSearchIndices(string indexName)
        {
            if (!_isConnected) return null;
            var response = (await _client.CatIndicesAsync())?.Records.Where(t => t.Index.Contains(indexName)).ToList();
            var indices = new ObservableCollection<IndexGUI>();
            if (response != null)
                foreach (var indicesRecord in response)
                    indices.Add(new IndexGUI
                    {
                        Name = indicesRecord.Index,
                        DocumentsNumber = int.Parse(indicesRecord.DocsCount)
                    });
            _indices = indices;
            return indices;
        }

        /// <summary>
        ///     Import all tweets using bulk mode
        /// </summary>
        public async Task ImportTweetsBulk(List<string> tweets)
        {
            if (!_isConnected) return;

            var operations =
                tweets.Select(tweet => new BulkIndexOperation<MyTweet>(new MyTweet(tweet)))
                    .Cast<IBulkOperation>()
                    .ToList();
            var imports = _indices.Select(indexGui => _client.BulkAsync(new BulkRequest(indexGui.Name)
            {
                Operations = operations
            })).Cast<Task>().ToList();

            await Task.WhenAll(imports);
        }

        #region CreateIndex

        /// <summary>
        ///     Creates an standard index
        /// </summary>
        /// <param name="name"></param>
        public void CreateStandardIndex(string name)
        {
            if(!_isConnected) return;
            _client.CreateIndex(name + "-std");
        } 
       
        /// <summary>
        /// Creates an index with the porter stemmer
        /// </summary>
        /// <param name="name"></param>
        public void CreatePorterStemmerIndex(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-porter", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.PorterStem("myFilter",j=>j));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase","myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        /// Creates an index with stop word filter
        /// </summary>
        /// <param name="name"></param>
        public void CreateStopWordIndex(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-stop", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    a.TokenFilters(t => t.Stop("myFilter", j => j.StopWords("_english_"))); 
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase", "myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        /// Creates an index with the KStem system
        /// </summary>
        /// <param name="name"></param>
        public void CreateKStemIndex(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-kstem", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.KStem("myFilter",j=>j));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase", "myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        /// Creates an index using Snowball
        /// </summary>
        /// <param name="name"></param>
        public void CreateSnowballIndex(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-snow", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.Snowball("myFilter", j => j.Language(SnowballLanguage.English)));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase", "myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        /// Creates an index using Snowball more Stop Words filter
        /// </summary>
        /// <param name="name"></param>
        public void CreateStopWordSnowballIndex(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-stop-snow", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    a.TokenFilters(t => t.Snowball("snowFilter", j => j.Language(SnowballLanguage.English)).Stop("stopFilter",d=>d.StopWords("_english_")));  
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase","stopFilter", "snowFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        /// Creates an index using Snowball more Stop Words filter more DFR similarity algorithm
        /// </summary>
        /// <param name="name"></param>
        // ReSharper disable once InconsistentNaming
        public void CreateStopWordSnowballIndexDFR(string name)
        {
            if (!_isConnected) return;

            _client.CreateIndex(name + "-stem-stop-snow-dfr", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    a.TokenFilters(t => t.Snowball("snowFilter", j => j.Language(SnowballLanguage.English)).Stop("stopFilter", d => d.StopWords("_english_")));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("lowercase", "stopFilter", "snowFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweetDFR>(m => m.AutoMap()));
                return c;
            });
        }
        #endregion

        /// <summary>
        /// Retrieves the analized text
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="elasticResult"></param>
        private async void AnalyzeText(string indexName, ElasticResult elasticResult)
        {
            if (!_isConnected) return;
            var result = await _client.AnalyzeAsync(t => t.Index(indexName).Analyzer("myAnalizer").Text(elasticResult.Text));
            elasticResult.TextAnalyzed = string.Join(" ",result.Tokens.Select(t=>t.Token));
        }
    }
}