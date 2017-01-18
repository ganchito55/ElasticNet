using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace ElasticNet
{
    // ReSharper disable once InconsistentNaming
    public class IndexGUI : BindableBase
    {

        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }


        private int _documentsNumber;

        public int DocumentsNumber
        {
            get { return _documentsNumber; }
            set { SetProperty(ref _documentsNumber, value); }
        }


        private ObservableCollection<ElasticResult> _results = new ObservableCollection<ElasticResult>();

        public ObservableCollection<ElasticResult> Results
        {
            get { return _results; }
            set { SetProperty(ref _results, value); }
        }
    }

    public class ElasticResult
    {
        public string Text { get; set; }
        public double? Score { get; set; }
    }



}