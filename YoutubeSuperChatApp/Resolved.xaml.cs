using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static YoutubeSuperChatApp.DataAccess;

namespace YoutubeSuperChatApp
{
    
    public partial class Resolved : Window
    {
        public ObservableCollection<ListViewItemR> ListItemsUnresolved { get; set; } = [];
        public Resolved()
        {
            InitializeComponent();

            List<Record> records = DataAccess.GetRecords(true);
            recordList.DataContext = this;
            foreach (var record in records)
            {
                if (record.Date < DateTime.Now.AddDays(-1))
                    continue;
                double rate = DataAccess.GetRate(record.CurrencyType);
                double converted = record.Amount * rate;
                Color clr = Color.FromRgb(0, 0, 0);
                ListItemsUnresolved.Add(item: new ListViewItemR { Name = record.Name, Amount = double.Round(converted / 1000000, 2).ToString(), Date = ((DateTimeOffset)record.Date).ToString("MM/dd/yyyy hh:mm:ss tt"), Clr = new SolidColorBrush(clr), OrgAmount = record.Amount, OrgDate = record.Date });
                recordList.UpdateLayout();
            }
        }

        private void ButtonUnresolve_Click(object sender, RoutedEventArgs e)
        {
            ListViewItemR r = (ListViewItemR)recordList.SelectedItem;
            DataAccess.SetRecordUnresolved(r.Name, (ulong?)r.OrgAmount, r.OrgDate);
            ListItemsUnresolved.Remove(r);
            recordList.UpdateLayout();
        }

        private void ButtonLastDay_Click(object sender, RoutedEventArgs e)
        {
            List<Record> records = DataAccess.GetRecords(true);
            ListItemsUnresolved.Clear();
            foreach (var record in records)
            {
                if (record.Date < DateTime.Now.AddDays(-1))
                    continue;
                double rate = DataAccess.GetRate(record.CurrencyType);
                double converted = record.Amount * rate;
                Color clr = Color.FromRgb(0, 0, 0);
                ListItemsUnresolved.Add(item: new ListViewItemR { Name = record.Name, Amount = double.Round(converted / 1000000, 2).ToString(), Date = ((DateTimeOffset)record.Date).ToString("MM/dd/yyyy hh:mm:ss tt"), Clr = new SolidColorBrush(clr), OrgAmount = record.Amount, OrgDate = record.Date });
                recordList.UpdateLayout();
            }
        }

        private void ButtonAll_Click(object sender, RoutedEventArgs e)
        {
            List<Record> records = DataAccess.GetRecords(true);
            ListItemsUnresolved.Clear();
            foreach (var record in records)
            {
                double rate = DataAccess.GetRate(record.CurrencyType);
                double converted = record.Amount * rate;
                Color clr = Color.FromRgb(0, 0, 0);
                ListItemsUnresolved.Add(item: new ListViewItemR { Name = record.Name, Amount = double.Round(converted / 1000000, 2).ToString(), Date = ((DateTimeOffset)record.Date).ToString("MM/dd/yyyy hh:mm:ss tt"), Clr = new SolidColorBrush(clr), OrgAmount = record.Amount, OrgDate = record.Date });
                recordList.UpdateLayout();
            }
        }
    }

    public partial class ListViewItemR : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";
        public required System.Windows.Media.Brush Clr { get; set; }
        public string Amount { get; set; } = "0";
        public string Date { get; set; } = DateTimeOffset.Now.ToString();
        public long OrgAmount { get; set; }
        public DateTimeOffset? OrgDate { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
