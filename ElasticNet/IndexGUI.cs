using System;
using Prism.Mvvm;

namespace ElasticNet
{
    public class IndexGUI : BindableBase
    {

        private String _name;

        public String Name 
        {
            get { return this._name; }
            set { SetProperty(ref _name, value); }
        }


        private int _documentsNumber;

        public int DocumentsNumber
        {
            get { return this._documentsNumber; }
            set { SetProperty(ref _documentsNumber, value); }
        }


        private int _documentsRetrieval;

        public int DocumentsRetrieval
        {
            get { return this._documentsRetrieval; }
            set { SetProperty(ref _documentsRetrieval, value); }
        }
    }
}