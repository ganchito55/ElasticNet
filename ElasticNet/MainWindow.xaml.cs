using Nest;
using System;

namespace ElasticNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            var node = new Uri("http://elastic:mineoMineo@188.166.147.155:4444");
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);

            var response = client.CatIndices();
        }
    }
}
