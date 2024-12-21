using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Google.Apis.Services;
using System.Text.Json;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Google;
using static YoutubeSuperChatApp.DataAccess;


namespace YoutubeSuperChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserCredential? credential { get; set; }
        private string keyLoc = "";
        private List<Tier> tierList = [];
        private readonly Timer timer;
        public Dictionary<string, double>? rates;
        public DateTime start = DateTime.Now;
        public ObservableCollection<ListViewItem> ListItems { get; set; } = [];
        public MainWindow()
        {
            InitializeComponent();
            _ = DataAccess.InitializeDatabase();
            rates = DataAccess.InitializeRates();
            DataAccess.ClearChats();
            timer = new Timer(PollAPI, null, Timeout.Infinite, Timeout.Infinite);
            keyLoc = DataAccess.GetOption("KeyLoc");
            if (keyLoc != "")
                credential = this.Auth().Result;
            else
            {
                MessageBox.Show("No key file location was defined. Please go into options and select its location.");
            }
            string json_tier = DataAccess.GetOption("Tiers");
            if (json_tier != "")
                tierList = JsonSerializer.Deserialize<List<Tier>>(json_tier);
            recordList.DataContext = this;
        }

        public void UpdateOptions(string keyloc, List<Tier> tierls)
        {
            keyLoc = keyloc;
            tierList = tierls;
            credential = this.Auth().Result;
        }

        private async Task<UserCredential?> Auth()
        {
            UserCredential cred;
            using (var stream = new FileStream(keyLoc, FileMode.Open, FileAccess.Read))
            {
                cred = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    [YouTubeService.Scope.YoutubeReadonly],
                    "user", CancellationToken.None);
            };
            return cred;
        }

        private void PollAPI(object? state)
        {
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Superchat Reader"
            });
            try
            {
                var superchats = service.SuperChatEvents.List(part: "snippet");

                var re = superchats.Execute().Items;
                foreach (var superchat in re)
                {
                    var name = superchat.Snippet.SupporterDetails.DisplayName;
                    var currencyType = superchat.Snippet.Currency;
                    var amount = superchat.Snippet.AmountMicros;
                    var date = superchat.Snippet.CreatedAtDateTimeOffset;
                    
                    if (date > start)
                        DataAccess.InsertRecord(name, currencyType, amount, date);
                }
                this.Dispatcher.Invoke(() =>
                {
                    int index = recordList.SelectedIndex;
                    ListItems.Clear();
                    List<Record> records = DataAccess.GetRecords();
                    //if (records.Count == 0)
                    //{
                    //    var r = new Record
                    //    {
                    //        Name = "Test",
                    //        Amount = 15000000,
                    //        Date = DateTimeOffset.Now,
                    //        CurrencyType = "EUR"
                    //    };
                    //    records.Add(r);
                    //    DataAccess.InsertRecord(r.Name, r.CurrencyType, (ulong?)r.Amount, r.Date);
                    //}
                    foreach (var record in records)
                    {
                        double rate = DataAccess.GetRate(record.CurrencyType);
                        double converted = record.Amount * rate;
                        Color clr = Color.FromRgb(0, 0, 0);
                        bool tierFound = false;
                        if (tierList != null)
                            foreach (var tier in tierList)
                            {
                                if (tier.Low_Val * 1000000 <= converted && tier.High_Val * 1000000 >= converted)
                                {
                                    tierFound = true;
                                    clr = tier.Color;
                                    break;
                                }
                            }
                        if (tierFound)
                            ListItems.Add(item: new ListViewItem { Name = record.Name, Amount = double.Round(converted/1000000, 2).ToString(), Date = ((DateTimeOffset)record.Date).ToString("MM/dd/yyyy hh:mm:ss tt"), Clr = new SolidColorBrush(clr), OrgAmount = record.Amount, OrgDate = record.Date });
                        if (index < recordList.Items.Count && index > -1)
                        {
                            recordList.SelectedIndex = index;
                        }
                        recordList.UpdateLayout();
                    }
                });
            }
            catch (GoogleApiException)
            {
                MessageBox.Show("Could not connect to the YouTube API. Did you define the key file location in options?");
            }
        }

        private void Options_Clicked(object sender, RoutedEventArgs e)
        {
            Options op = new(this);
            op.ShowDialog();
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tierList.Count > 0)
            {
                timer.Change(0, 10000);
            }
            else
            {
                MessageBox.Show("At least one tier must be define before polling is started.");
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem item = (ListViewItem)recordList.SelectedItem;
            if (item != null)
            {
                DataAccess.SetRecordResolved(item.Name, (ulong?)item.OrgAmount, item.OrgDate);
                ListItems.Remove(item);
                recordList.UpdateLayout();
                recordList.SelectedIndex = -1;
            }
        }

        private void AllBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure? This will purge everything as resolved!", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach(ListViewItem item in recordList.Items)
                {
                    DataAccess.SetRecordResolved(item.Name, (ulong?)item.OrgAmount, item.OrgDate);
                }
                ListItems.Clear();
            }
        }

        private void viewResolved_Clicked(object sender, RoutedEventArgs e)
        {
            Resolved r = new();
            r.ShowDialog();
        }
    }
    public partial class ListViewItem : INotifyPropertyChanged
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