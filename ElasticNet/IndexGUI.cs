using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Prism.Mvvm;

namespace ElasticNet
{
    // ReSharper disable once InconsistentNaming
    public class IndexGUI : BindableBase
    {
        private int _documentsNumber;
        private string _name;


        private ObservableCollection<ElasticResult> _results = new ObservableCollection<ElasticResult>();

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public int DocumentsNumber
        {
            get { return _documentsNumber; }
            set { SetProperty(ref _documentsNumber, value); }
        }

        public ObservableCollection<ElasticResult> Results
        {
            get { return _results; }
            set { SetProperty(ref _results, value); }
        }



        private Metrics _metrics;

        public Metrics Metrics
        {
            get { return this._metrics; }
            set { SetProperty(ref _metrics, value); }
        }

        public void ComputeMetrics()
        {
            Metrics = Metrics.GenerateMetrics(Results);
        }
    }

    public class ElasticResult
    {
        public string Text { get; set; }
        public double Score { get; set; }
        public string TextAnalyzed { get; set; }
    }

    public class Metrics
    {
        public double Precision { get; set; } = -1;
        public double Precision5 { get; set; } = -1;
        public double Recall { get; set; } = -1;
        public double Noise { get; set; } = -1;
        public double Silence { get; set; } = -1;

        public static Metrics GenerateMetrics(ObservableCollection<ElasticResult> retrievaled )
        {
            var totalDocuments = ((ViewModel) Application.Current.MainWindow.DataContext).TweetsRecovered;
            if(totalDocuments==null) return new Metrics();
            var relevants = totalDocuments.Where(d => d.IsRelevant).ToList();
            var relevantsRetrievaled = new List<string>();
            int relevantsRetrievaled5 = 0; 

            for (var index = 0; index < retrievaled.Count; index++)
            {
                ElasticResult elasticResult = retrievaled[index];
                for (int i = 0; i < relevants.Count; i++)
                {
                    if (elasticResult.Text.Equals(relevants.ElementAt(i).Msg))
                    {
                        if (index < 5) relevantsRetrievaled5++;
                        relevantsRetrievaled.Add(elasticResult.Text);
                        break;
                    }
                }
            }

            double precision = -1, recall = -1, noise = -1;
            double silence = -1,precision5=-1;
            if (relevantsRetrievaled.Count != 0)
            {
                precision = retrievaled.Count / (double) relevantsRetrievaled.Count; 
            }
            if (relevants.Count != 0)
            {
                recall = relevantsRetrievaled.Count / (double)relevants.Count;
                silence = (relevants.Count - relevantsRetrievaled.Count) / (double)relevants.Count;
            }
            if (retrievaled.Count != 0)
            {
                noise = (retrievaled.Count - relevantsRetrievaled.Count) / (double)retrievaled.Count; 
            }

            if (retrievaled.Count > 5)
            {
                precision5 = relevantsRetrievaled5 / (double) 5;
            }
            else
            {
                precision5 = relevantsRetrievaled5 / (double) retrievaled.Count;
            } 
            return new Metrics() {Precision = precision,Recall = recall,Noise = noise,Precision5 = precision5,Silence = silence};
        }
    }
}