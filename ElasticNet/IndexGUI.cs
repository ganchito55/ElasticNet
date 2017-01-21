using System.Collections.ObjectModel;
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
    }

    public class ElasticResult
    {
        public string Text { get; set; }
        public double Score { get; set; }
        public string TextAnalyzed { get; set; }
    }
}