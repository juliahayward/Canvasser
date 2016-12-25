using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Reflection;
using Canvasser.Extensions;
using JuliaHayward.Common.Logging;
using Canvasser.Schema;
using Microsoft.Win32;
using JuliaHayward.Common.Environment;
using AutoUpdaterForWPF;

namespace Canvasser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanvasserDataContext _context;
        private ElectorsVM _electorsVm;
        private StatisticsVM _statsVm;
        private Predicate<Elector> _pdPredicate = (x => x.PD != null);
        private Predicate<Elector> _nationalityPredicate = (x => true);
        private Predicate<Elector> _partyPredicate = (x => true);
        private Predicate<Elector> _votePredicate = (x => true);
        private Predicate<Elector> _searchPredicate = (x => true);
        private bool _isDirty = false;

        private Predicate<Elector> _allPreds;
        private readonly ILogger _logger;
        private int _schemaVersion;

        public MainWindow()
        {
            CheckForUpdates();

            try
            {
                var connStr = ConfigurationManager.ConnectionStrings[1].ConnectionString;

                if (JuliaEnvironment.CurrentEnvironment.IsDebug())
                {
                    var openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog().Value)
                    {
                        connStr = connStr.Replace(@".\Data.sdf", openFileDialog.FileName);
                    }
                }
                _context = new CanvasserDataContext(connStr);
                _context.ObjectTrackingEnabled = true;

                this.DataContext = new MainWindowVM(_context);

                InitializeComponent();

                PopulatePDFilter();

                _logger = new TrelloLogger("**REDACTEDTrelloKey**",
                    "**REDACTEDTrelloAuthKey**");
                App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;


                if (!JuliaEnvironment.CurrentEnvironment.IsDebug())
                {
                    // If Derek's machine, upgrade automatically. Must be done *after*
                    // initialiseCpomponent so that we can update the UI
                    upgrade_Click(null, null);
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModified = fileInfo.LastWriteTime;
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".0.0", "");
                this.Title = "Canvasser (version " + version + ",  " + lastModified.ToString("dd MMM yyyy") + ")"; 

                _allPreds = (x =>
                _pdPredicate(x) && _nationalityPredicate(x) && _partyPredicate(x)
                    && _votePredicate(x) && _searchPredicate(x));


                var updater = new SchemaUpdater(_context);
                _schemaVersion = updater.GetVersion();
                schemaVersionLabel.Text = "Schema: v" + _schemaVersion;

                dataGrid1.ItemsSource = new PartiesVM(_context);

                _electorsVm = new ElectorsVM(_context);
                dgElectors.ItemsSource = _electorsVm;
                Intentions = new List<string>();
                Intentions.Add("");
                Intentions.Add("D");
                Intentions.Add("P");
                Intentions.Add("CON");
                Intentions.Add("CON soft");
                Intentions.Add("LAB");
                Intentions.Add("LAB soft");
                Intentions.Add("LIBDEM");
                Intentions.Add("LIBDEM soft");
                Intentions.Add("UKIP");
                Intentions.Add("GREEN");
                Intentions.Add("ANTI");
                Intentions.Add("NV");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void PopulatePDFilter()
        {
            bool isFirst = true;
            foreach (var pd in _context.PollingDistricts)
            {
                pdFilterComboBox.Items.Add(new ComboBoxItem() { Content = pd.PD + " (" + pd.ShortName + ")", Tag = pd.PD });
                kuPDFilter.Items.Add(new ComboBoxItem() { Content = pd.PD + " (" + pd.ShortName + ")", Tag = pd.PD, IsSelected = isFirst });
                isFirst = false;
            }
        }

        private void CheckForUpdates()
        {
            try
            {
                var autoUpdater = new AutoUpdater();
                var result = autoUpdater.DoUpdate("**REDACTEDCanvasserAutoUpdateUrl**");
                if (result == AutoUpdateResult.UpdateInitiated)
                {
                    this.Close();
                    Environment.Exit(0);
                }
            }
            catch (Exception) { /* don't bother logging */ }
        }
        
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error("Canvasser", e.Exception.Message, e.Exception.StackTrace);
        }

        public List<string> Intentions
        {
            get;
            protected set;
        }

        private MainWindowVM Model
        {
            get { return (MainWindowVM)DataContext; }
        }

        private void addParty_Click(object sender, RoutedEventArgs e)
        {
            var party = new Party();
            party.PartyId = _context.Parties.Max(x => x.PartyId) + 1;
            party.Name = "New Party";
            (dataGrid1.ItemsSource as PartiesVM).Add(party);
        }

        private void deleteParty_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItem != null)
            {
                var party = dataGrid1.SelectedItem as Party;
                (dataGrid1.ItemsSource as PartiesVM).Remove(party);
            }
        }

        private void saveParty_Click(object sender, RoutedEventArgs e)
        {
            (dataGrid1.ItemsSource as PartiesVM).Save();
        }

       
        public ObservableCollection<Elector> FilteredElectors
        {
            get
            {
                if (_electorsVm == null)
                    return new ObservableCollection<Elector>();

                var electors = new ObservableCollection<Elector>(
                    _electorsVm.Where(x => _allPreds(x)).OrderBy(x => x.PD) 
                    .ThenBy(x => x.PN).ThenBy(x => x.PNs));

                status.Text = "Selected " + electors.Count + " electors, " +
                    electors.Select(x => x.Address).Distinct().Count() + " properties";

                return electors;
            }
        }

        private void pdFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string;
            if (tag == "")
                _pdPredicate = (x => x.PD != null);
            else
                _pdPredicate = (x => x.PD == tag);
            dgElectors.ItemsSource = FilteredElectors;
        }

        private void gFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string;
            if (tag == "")
                _pdPredicate = (x => true);
            else if (tag == "G")
                _pdPredicate = (x => x.Markers.Contains("G"));
            else if (tag == "non-G")
                _pdPredicate = (x => !x.Markers.Contains("G"));
            dgElectors.ItemsSource = FilteredElectors;
        }

        private void intFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string;
            if (tag == "")
                _partyPredicate = (x => true);
            else
            {
                if (tag.StartsWith("NOT:"))
                {
                    tag = tag.Replace("NOT:", "");
                    var notTags = tag.Split(new[] { ',' });
                    _partyPredicate = (x => !notTags.Contains(x.Intention));
                }
                else
                {
                    var tags = tag.Split(new[] { ',' });
                    _partyPredicate = (x => tags.Contains(x.Intention));
                }
            }
            dgElectors.ItemsSource = FilteredElectors;
        }

        private void voteFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string;
            if (tag == "")
                _votePredicate = (x => true);
            else if (tag == "Voted2015")
                _votePredicate = (x => x.Voted2015.Value);
            else if (tag == "Postal2016")
                _votePredicate = (x => x.Postal2016.Value);
            else if (tag == "NoPostal2016")
                _votePredicate = (x => !x.Postal2016.Value);
            else if (tag == "Both2015")
                _votePredicate = (x => (x.Postal2015.Value || x.Voted2015.Value));
            else if (tag == "New2016")
                _votePredicate = (x => string.IsNullOrEmpty(x.PD2015));
            dgElectors.ItemsSource = FilteredElectors;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (string.IsNullOrEmpty(text))
                _searchPredicate = (x => true);
            else
                _searchPredicate = (x => x.FirstName.ToLower().Contains(text)
                    || x.Surname.ToLower().Contains(text)
                    || x.Address.ToLower().Contains(text)
                    || x.Address2.ToLower().Contains(text));
            dgElectors.ItemsSource = FilteredElectors;
        }


        private void print_Click(object sender, RoutedEventArgs e)
        {
            var electors = new List<Elector>(FilteredElectors)
                .OrderBy(x => x.PD).ThenBy(x => x.PN).ThenBy(x => x.PNs);

            MessageBox.Show("Printing " + electors.Count() + " people");

            if (printHouseholds.IsChecked.Value)
                PrintElectors(TakeFirstInHousehold(electors));
            else
                PrintElectors(electors);
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            var electors = new List<Elector>(FilteredElectors)
                .OrderBy(x => x.PD).ThenBy(x => x.PN).ThenBy(x => x.PNs);

            MessageBox.Show("Exporting " + electors.Count() + " people");

            Cursor = Cursors.Wait;

            if (printHouseholds.IsChecked.Value)
                ExportElectors(TakeFirstInHousehold(electors));
            else
                ExportElectors(electors);

            Cursor = Cursors.Arrow;
        }

        private void printSelected_Click(object sender, RoutedEventArgs e)
        {
            var electors = new List<Elector>();
            foreach (var obj in dgElectors.SelectedItems)
                electors.Add(obj as Elector);

            var sortedElectors = electors
                .OrderBy(x => x.PD2015).ThenBy(x => x.PN2015).ThenBy(x => x.PNs2015);

            MessageBox.Show("Printing " + electors.Count() + " people");

            if (printHouseholds.IsChecked.Value)
                PrintElectors(TakeFirstInHousehold(sortedElectors));
            else
                PrintElectors(sortedElectors);
        }

        private void exportSelected_Click(object sender, RoutedEventArgs e)
        {
            var electors = new List<Elector>();
            foreach (var obj in dgElectors.SelectedItems)
                electors.Add(obj as Elector);

            var sortedElectors = electors
                .OrderBy(x => x.PD).ThenBy(x => x.PN).ThenBy(x => x.PNs);

            MessageBox.Show("Exporting " + electors.Count() + " people");

            Cursor = Cursors.Wait;

            if (printHouseholds.IsChecked.Value)
                ExportElectors(TakeFirstInHousehold(sortedElectors));
            else
                ExportElectors(sortedElectors);

            Cursor = Cursors.Arrow;
        }

        private List<Elector> TakeFirstInHousehold(IEnumerable<Elector> electors)
        {
            var result = new List<Elector>();
            var lastAddress = "";
            foreach (var elector in electors)
            {
                if (elector.Address != lastAddress)
                {
                    result.Add(elector);
                    lastAddress = elector.Address;
                }
            }
            return result;
        }

        private void PrintElectors(IEnumerable<Elector> electorsToPrint)
        {
            var numberToPrint = electorsToPrint.Count();
            var numPages = (numberToPrint + 33) / 34;

            PrintDialog printDialog = new PrintDialog();
            if ((bool)printDialog.ShowDialog().GetValueOrDefault())
            {
                FlowDocument flowDocument = new FlowDocument();
                flowDocument.ColumnWidth = 700; // 96ths of an inch
                flowDocument.FontFamily = new FontFamily("Arial");
                for (var pageNo = 0; pageNo < numPages; pageNo++)
                {
                    Table table = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    if (pageNo > 0) table.BreakPageBefore = true;
                    table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(200) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(170) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(80) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(88) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(180) });
                    table.RowGroups.Add(new TableRowGroup());
                    var numberInPage = Math.Min(numberToPrint - pageNo * 34, 34);
                    for (int i = 0; i < numberInPage; i++)
                    {
                        var elector = electorsToPrint.ElementAt(pageNo * 34 + i);

                        var tablerow = new TableRow();
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.PD + elector.FullPN)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.FirstName + " " + elector.Surname)) { Margin = new Thickness(2,4,2,4), FontSize = 12}) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.Address)) { Margin = new Thickness(2,4,2,4), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.Annotations)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.Telephone)) { Margin = new Thickness(4), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run("  ")) { Margin = new Thickness(4) }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        table.RowGroups[0].Rows.Add(tablerow);
                    }

                    flowDocument.Blocks.Add(table);

                    Paragraph myParagraph = new Paragraph();
                    myParagraph.Margin = new Thickness(0);
                    var imprintProvider = new ImprintProvider(_context);
                    myParagraph.Inlines.Add(new Run(imprintProvider.Provide(electorsToPrint.First().PD)));
                    flowDocument.Blocks.Add(myParagraph);
                }
         
                DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Title");
            }
        }

        private void ExportElectors(IEnumerable<Elector> electorsToPrint)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Electors"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Excel files (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (!(result.HasValue && result.Value)) return;
            string filename = dlg.FileName;
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                using (var writer = new StreamWriter(fs))
                {
                    writer.WriteLine("Number,Name,Address1,Address2,Postcode");

                    foreach (var elector in electorsToPrint)
                    {
                    var record = string.Format("{0},{1},{2},{3},{4}",
                        elector.PD + elector.FullPN,
                        elector.FirstName + " " + elector.Surname.ToSurnameCase(),
                        elector.Address,
                        elector.Address2,
                        elector.Postcode);

                        writer.WriteLine(record);
                    }
                }
        }

        private void printStats_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if ((bool)printDialog.ShowDialog().GetValueOrDefault())
            {
                FlowDocument flowDocument = new FlowDocument();
                flowDocument.ColumnWidth = 700; // 96ths of an inch
                flowDocument.FontFamily = new FontFamily("Arial");

                Paragraph myParagraph = new Paragraph();
                myParagraph.Margin = new Thickness(0);
                myParagraph.TextAlignment = TextAlignment.Center;
                myParagraph.Inlines.Add(new Run("Canvasser Statistics"));
                flowDocument.Blocks.Add(myParagraph);

                myParagraph = new Paragraph();
                myParagraph.Margin = new Thickness(0);
                myParagraph.TextAlignment = TextAlignment.Center;
                myParagraph.Inlines.Add(new Run("Printed at " + DateTime.Now.ToString("dd MMM yyyy, HH:mm")));
                flowDocument.Blocks.Add(myParagraph);

                Table table = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                table.RowGroups.Add(new TableRowGroup());

                var tablerow = new TableRow();
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("Poll District")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("Total Canvass")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("Derek")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("Con")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("Lab")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("LibDem")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                tablerow.Cells.Add(new TableCell(new Paragraph(new Run("UKIP")) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                table.RowGroups[0].Rows.Add(tablerow);

                foreach (var row in _statsVm)
                {
                    tablerow = new TableRow();
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.Name)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.Total.ToString())) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.DerekS)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.ConS)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.LabS)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.LibdemS)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    tablerow.Cells.Add(new TableCell(new Paragraph(new Run(row.UkipS)) { Margin = new Thickness(4), FontSize = 14 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    table.RowGroups[0].Rows.Add(tablerow);
                }

                flowDocument.Blocks.Add(table);

                myParagraph = new Paragraph();
                myParagraph.Margin = new Thickness(0);
                var imprintProvider = new ImprintProvider(_context);
                myParagraph.Inlines.Add(new Run(imprintProvider.Provide(null)));
                flowDocument.Blocks.Add(myParagraph);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Title");
            }
        }

        private string SanitiseIntention(string party)
        {
            // hard coded to match old options
            party = party.ToUpper();

            if (party == "C") party = "CON";
            if (party == "D") party = "D";
            if (party == "PD") party = "P";
            if (party == "LD") party = "LIBDEM";
            if (party == "SOFT C") party = "Soft CON";
            return party;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            _electorsVm.Save();
            _isDirty = false;

            Cursor = Cursors.Arrow;
        }

        private void savePDs_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            // looks bad but basically context.SaveChanges()
            _electorsVm.Save();
            _isDirty = false;

            Cursor = Cursors.Arrow;
        }

        


        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
                UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            _statsVm = new StatisticsVM(_context);

            dgStatistics.ItemsSource = _statsVm;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isDirty)
            {
                var result =
                  MessageBox.Show("You have unsaved changes. Are you sure you want to leave?",
                      "Canvasser", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                      MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void dgElectors_CurrentCellChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void dgElectors_KeyDown(object sender, KeyEventArgs e)
        {
            var elector = dgElectors.CurrentItem as Elector;
            if (elector == null) return; 

            if (e.Key == Key.D)
                elector.Intention2016 = "D";
            if (e.Key == Key.P)
                elector.Intention2016 = "P";
            if (e.Key == Key.C)
                elector.Intention2016 = "CON";
            if (e.Key == Key.L)
                elector.Intention2016 = "LAB";
            if (e.Key == Key.U)
                elector.Intention2016 = "UKIP";
            if (e.Key == Key.A)
                elector.Intention2016 = "ANTI";
            if (e.Key == Key.N)
                elector.Intention2016 = "NV";
            if (e.Key == Key.Back)
                elector.Intention2016 = "";
            _isDirty = true;
        }

        private void upgrade_Click(object sender, RoutedEventArgs e)
        {
            var updater = new SchemaUpdater(_context);
            updater.Update();
            _schemaVersion = updater.GetVersion();
            schemaVersionLabel.Text = "Schema: v" + _schemaVersion;
        }

        private void import2015Voted_Click(object sender, RoutedEventArgs e)
        {
            // Important - this year Derek noted who DIDNT vote
            var importer = new VotedIn2015Importer(_context, status);
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                var filename = dlg.FileName;
                importer.Import(filename);
                Cursor = Cursors.Arrow;
            }
        }

        private void import2015BVoted_Click(object sender, RoutedEventArgs e)
        {
            var importer = new VotedIn2015ByeImporter(_context, status);
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                var filename = dlg.FileName;
                importer.Import(filename);
                Cursor = Cursors.Arrow;
            }
        }

        private void knockUpList_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This will overwrite any existing knock-up list. Are you sure?",
                "Canvasser", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
                return;

            var sql = @"DELETE FROM TargetVoter";
            _context.ExecuteCommand(sql);
            sql = @"INSERT INTO TargetVoter
                (PD, PN, PNs, FirstName, Surname, Address, Address2, Telephone, Voted, Notes)
                SELECT PD, PN, PNs, FirstName, Surname, Address, Address2, Telephone, 0, ''
                FROM Elector
                WHERE (Voted2012 = 1 OR Voted2013 = 1 OR Voted2014 = 1 OR Voted2015 = 1)
                AND Postal2016 = 0
                AND PD IS NOT NULL
                AND (Intention2012 IN ('D','P') OR Intention2013 IN ('D','P') OR 
                    Intention2014 IN ('D','P') OR Intention2015 IN ('D','P')
                    OR Intention2016 IN ('D','P'))";
            _context.ExecuteCommand(sql);

            Model.TargetPD = "ER";
            Model.TargetCount = _context.TargetVoters.Where(x => x.PD == Model.TargetPD).Count();
            Model.TargetVoted = _context.TargetVoters.Where(x => x.PD == Model.TargetPD && x.Voted).Count();
 

            Model.RefreshTargetVoters();
        }

        private void kuDayFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var tag = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string;
                Model.TargetPD = tag;
                Model.TargetCount = _context.TargetVoters.Where(x => x.PD == Model.TargetPD).Count();
                Model.TargetVoted = _context.TargetVoters.Where(x => x.PD == Model.TargetPD && x.Voted).Count();

            }
            catch (Exception ex)
            {
                // On older schemas this will fail on startup
            }
        }

        private void importNewRegister_Click(object sender, RoutedEventArgs e)
        {
            var importer = new HDC2016RegisterImporter(_context, status);
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                var filename = dlg.FileName;
                importer.Import(filename);
                Cursor = Cursors.Arrow;
            }
        }

        private void sendDataHome_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            this.status.Text = "Uploading...";

            // Copy the data aside
            var userName = System.Environment.UserName;
            var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var newFileName = string.Format("{0}_{1}_Data.sdf", userName, date);
            File.Copy("Data.sdf", newFileName);
            ;

            // Send it
            WebClient myWebClient = new WebClient();
            var uri = new Uri("**REDACTEDCanvasserTestUrl**");
            byte[] response = myWebClient.UploadFile(uri, "POST", newFileName);
            if (response.Length == 0)
                MessageBox.Show("Data successfully sent");
            else
                MessageBox.Show(System.Text.Encoding.Default.GetString(response));

            // Get rid of the copy
            File.Delete(newFileName);

            this.status.Text = "Ready";
            Cursor = Cursors.Arrow;
        }
    }
}
