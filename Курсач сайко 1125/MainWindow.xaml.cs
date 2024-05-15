using BD;
using Microsoft.VisualBasic.ApplicationServices;
using MySqlConnector;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Курсач_сайко_1125;
using Microsoft.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Diagnostics;

namespace CollegeAdmissionAutomation
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string searchText = "";

        private readonly Login login;
        private Zap selectedzap;
        private ObservableCollection<Zap> zaps;

        public MainWindowViewModel ViewModel { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));



        public ObservableCollection<Zap> Zaps
        {
            get => zaps; set
            {
                zaps = value;
                Signal(nameof(Zaps));
            }
        }

        public MainWindow(Login login)
        {
            InitializeComponent();
            FillStyles();
            this.login = login;
            DataContext = this;

        }
        private void FillStyles()
        {
            Zaps = new ObservableCollection<Zap>();

            try
            {
                using var connection = DB.Instance.Database.GetDbConnection() as MySqlConnection;
                connection.Open();

                using var command = new MySqlCommand("SELECT * FROM Zap", connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Zaps.Add(new Zap
                    {
                        Id = reader.GetString("Id"),
                        Name = reader.GetString("Name"),
                        Gpa = reader.GetDecimal("Gpa"),
                        Spec = reader.GetDecimal("Spec")
                    });
                }

                if (Zaps.Any())
                {
                    selectedzap = Zaps.First();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while fetching data from the database: {ex.Message}");
            }
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            LogIn logIn = new LogIn();
            logIn.Show();
            this.Close();
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text as string;

            if (searchTerm != null)
            {
                searchTerm = searchTerm.Trim();
            }
            else
            {
                searchTerm = string.Empty;
            }

            try
            {
                ViewModel.SearchApplicants(searchTerm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"При поиске кандидатов произошла ошибка: {ex.Message}", "ААААШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.CancelSearchCommand.Execute(null);
            }
        }
        public class Zap : INotifyPropertyChanged
        {
            

            public string Id
            {
                get =>  Id;
                set
                {
                    Id = value;
                    OnPropertyChanged();
                }
            }
            public string Name
            {
                get => Name;
                set
                {
                    Name = value;
                    OnPropertyChanged();
                }
            }
            public decimal Gpa
            {
                get => Gpa;
                set
                {
                    Gpa = value;
                    OnPropertyChanged();
                }
            }
            public decimal Spec
            {
                get => Spec;
                set
                {
                    Spec = value;
                    OnPropertyChanged();
                }
            }
            private Zap selectedApplicant;
            public Zap SelectedApplicant
            {
                get => selectedApplicant;
                set
                {
                    selectedApplicant = value;
                    OnPropertyChanged();
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class MainWindowViewModel
        {
            private ObservableCollection<Zap> zaps;
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public ObservableCollection<Zap> Zaps
            {
                get => zaps;
                set
                {
                    zaps = value;
                    OnPropertyChanged();
                }
            }
            public ICommand SearchCommand { get; private set; }
            public ICommand CancelSearchCommand { get; private set; }

            private IEnumerable<Zap> _filteredApplicants;
            public IEnumerable<Zap> FilteredApplicants
            {
                get => _filteredApplicants;
                set
                {
                    _filteredApplicants = value;
                    OnPropertyChanged();
                }
            }
            public MainWindowViewModel(IEnumerable<Zap> zaps)
            {
                zaps = new ObservableCollection<Zap>(zaps);
                FilteredApplicants = zaps;

                SearchCommand = new RelayCommand(SearchApplicants, _ => true);
                CancelSearchCommand = new RelayCommand(CancelSearch, _ => true);
            }
            private void SearchApplicants(object searchTerm)
            {
                if (searchTerm is string term)
                {
                    FilteredApplicants = zaps.Where(a => a.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
                }
            }
            private void CancelSearch(object _)
            {
                FilteredApplicants = zaps;
            }

            internal void SearchApplicants(string searchTerm)
            {
                throw new NotImplementedException();
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}
