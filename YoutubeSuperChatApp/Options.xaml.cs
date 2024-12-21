using Microsoft.Win32;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Windows.UI.Popups;

namespace YoutubeSuperChatApp
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private List<Tier>? tierList = [];
        private string currency = "";
        private readonly MainWindow parent;
        public Options(MainWindow p)
        {
            InitializeComponent();
            Dictionary<string, double> rates = DataAccess.GetRates();
            string curCurrency = DataAccess.GetOption("HomeCurrency");
            currency = curCurrency;
            foreach (var rate in rates.Keys)
            {
                ComboBoxItem item = new()
                {
                    Content = rate,
                    Tag = rate,
                };
                item.Content = rate;
                HomeCurrency.Items.Add(item);
                if (rate == curCurrency)
                {
                    HomeCurrency.SelectedItem = item;
                }
            }

            string json_tier = DataAccess.GetOption("Tiers");
            if(json_tier != "" )
                tierList = JsonSerializer.Deserialize<List<Tier>>(json_tier);
            textBox.Text = DataAccess.GetOption("KeyLoc");
            dataGrid.ItemsSource = tierList;
            parent = p;
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new()
            {
                Filter = "Key File|*.json",
                Multiselect = false
            };
            fileDialog.ShowDialog();
            if(fileDialog.CheckFileExists)
            {
                textBox.Text = fileDialog.FileName;
            }
        }

        private void SaveBtn_Clicked(object sender, RoutedEventArgs e)
        {
            DataAccess.SetOption(textBox.Text, "KeyLoc");
            string json_tier = "";
            if (tierList != null)
                json_tier = JsonSerializer.Serialize<List<Tier>>(tierList);
            DataAccess.SetOption(json_tier, "Tiers");
            if(parent != null && tierList != null)
                parent.UpdateOptions(textBox.Text, tierList);
            if (parent != null && ((ComboBoxItem)HomeCurrency.SelectedItem).Tag.ToString() != currency)
            {
                DataAccess.SetOption(((ComboBoxItem)HomeCurrency.SelectedItem).Tag.ToString(), "HomeCurrency");
                parent.rates = DataAccess.InitializeRates(true);
            }
            this.Close();
            
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddBtn_Click(sender, e, cPicker);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e, Xceed.Wpf.Toolkit.ColorPicker cPicker)
        {
            if (!double.TryParse(Lbound.Text, out double lval) || !double.TryParse(Ubound.Text, out double uval))
            {
                _ = new MessageDialog("Lower and Upper Bounds must be postive numbers.");

            }
            else if (lval < 0 || uval < 0) {
                _ = new MessageDialog("Lower and Upper Bounds must be postive numbers.");
            }
            else if (TName.Text == "")
            {
                _ = new MessageDialog("Name cannot be blank.");
            }
            else {
                Tier t = new()
                {
                    Name = TName.Text,
                    Low_Val = lval,
                    High_Val = uval,
                    Color = (System.Windows.Media.Color)(cPicker.SelectedColor == null ? System.Windows.Media.Color.FromRgb(0, 0, 0) : cPicker.SelectedColor),
                };
                tierList ??= [];
                tierList.Add(t);
            }
            dataGrid.Items.Refresh();
        }

        private void RmBtn_Click(object sender, RoutedEventArgs e)
        {
            if(tierList != null)
                if (tierList.Count > 0)
                {
                    Tier t = (Tier)dataGrid.SelectedItem;
                    if (t != null)
                        tierList.Remove(t);
                }
            dataGrid.Items.Refresh();
        }
    }
    [DataObject]
    public class Tier
    {
        public string Name { get; set; } = "Default";
        public double Low_Val { get; set; } = 0;
        public double High_Val { get; set; } = int.MaxValue;
        public System.Windows.Media.Color Color { get; set; } = System.Windows.Media.Color.FromRgb(0, 0, 0);
    }
}
