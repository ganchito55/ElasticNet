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
        private readonly ElasticClient _client;
        private ObservableCollection<IndexGUI> _indices;
        private bool _isConnected;


        /// <summary>
        ///     Connect to ElasticSearch host
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="host"></param>
        public ElasticNetWrapper(string user, string pass, string host)
        {
            Uri uri;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                uri = new Uri("http://" + host);
            else
                uri = new Uri("http://" + user + ":" + pass + "@" + host);
            _client = new ElasticClient(uri);
        }

        /// <summary>
        ///     Get status
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            _isConnected = _client.CatHealth().IsValid;
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
                    Query = new MatchQuery {Field = "tweet", Query = search}
                };
                var response = await _client.SearchAsync<MyTweet>(request);
                if (response != null)
                    index.DocumentsRetrieval = response.Documents.Count;
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
            _client.CreateIndex(name + "-std");
        }

        /// <summary>
        ///     Creates an index with english stemmer
        /// </summary>
        /// <param name="name"></param>
        public void CreateEnglishStemmerIndex(string name)
        {
            _client.CreateIndex(name + "-stem-eng", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.Stemmer("myFilter", j => j.Language("english")));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        /// <summary>
        ///     Creates an index with light english stemmer
        /// </summary>
        /// <param name="name"></param>
        public void CreateLightStemmerIndex(string name)
        {
            _client.CreateIndex(name + "-stem-lig", c =>
            {
                c.Settings(l => l.Analysis(a =>
                {
                    a.TokenFilters(t => t.Stemmer("myFilter", j => j.Language("light_english")));
                    a.Analyzers(an => an.Custom("myAnalizer", def =>
                    {
                        def.Tokenizer("standard");
                        def.Filters("myFilter");
                        return def;
                    }));
                    return a;
                }));
                c.Mappings(md => md.Map<MyTweet>(m => m.AutoMap()));
                return c;
            });
        }

        #endregion
    }
}