using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PackageVersionChecker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Timer _Timer = new Timer(1000);

        #region Notify Properties

        public ObservableCollection<WinProduct> Products { get; } = new ObservableCollection<WinProduct>();
        public ObservableCollection<DotNetVersions> DotNetVersions { get; } = new ObservableCollection<DotNetVersions>();

        private string _ProductLoadingText;

        public string ProductLoadingText
        {
            get => _ProductLoadingText;
            set
            {
                _ProductLoadingText = value;
                OnPropertyChanged(nameof(ProductLoadingText));
            }
        }

        private string _LoadingText;

        public string LoadingText
        {
            get => _LoadingText;
            set
            {
                _LoadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        private bool _IsLoadingProduct;

        public bool IsLoadingProduct
        {
            get => _IsLoadingProduct;
            set
            {
                _IsLoadingProduct = value;

                _Timer.Enabled = value;

                OnPropertyChanged(nameof(IsLoadingProduct));
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _Timer.Elapsed += (sender, args) => 
            {
                var tick = string.Concat(Enumerable.Repeat(".", DateTime.Now.Second % 4));

                LoadingText = tick;
            };

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // .NET のバージョンを一覧する
                var versions = DotNetVersion.GetVersions();

                foreach (var version in versions)
                {
                    DotNetVersions.Add(new DotNetVersions(version));
                }

                // Microsoft Visual C Redistributable のパッケージを一覧する
                CheckRedistributablePackage();
            }
            catch (Exception)
            {
                // None
            }
        }

        private async void CheckRedistributablePackage()
        {
            IsLoadingProduct = true;
            ProductLoadingText = Properties.Resources.LoadingText;

            var products = await Task<WinProduct[]>.Run(() => GetWinProduct("%Microsoft Visual C%Redistributable%"));

            foreach (var product in products)
            {
                Products.Add(product);
            }

            IsLoadingProduct = false;
            ProductLoadingText = Properties.Resources.LoadingCompletedText;
        }

        private WinProduct[] GetWinProduct(string likeName)
        {
            var products = new List<WinProduct>();
            using (var searcher = new ManagementObjectSearcher($"SELECT Name, IdentifyingNumber FROM Win32_Product WHERE Name LIKE '{likeName}'"))
            {
                int no = 1;

                foreach (ManagementObject obj in searcher.Get())
                {
                    var name = obj["Name"] as string;
                    var identify = obj["IdentifyingNumber"] as string;

                    var product = new WinProduct()
                    {
                        No = no++,
                        Name = name,
                        IdentifyingNumber = identify,
                    };

                    products.Add(product);

                    Console.WriteLine(name);
                }
            }

            return products.ToArray();
        }

        public void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
