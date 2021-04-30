using ClassLibrary1;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private V5MainCollection Data = new V5MainCollection();
        private Custom custom = new Custom();

        /* 1. Service Methods */
        public MainWindow()
        {
            InitializeComponent();
            InitializeDataContext();
            Resources["CustomElem"] = custom;
        }

        private void UpdateSaveFlag()
        {
            if (Data.AddChangesAfterSave)
            {
                SaveFlag.Foreground = Brushes.Yellow;
                SaveFlag.Text = "There are unsaved changes";
            }
            else
            {
                SaveFlag.Foreground = Brushes.Green;
                SaveFlag.Text = "All changes saved";
            }
        }

        private bool WantSaveData()
        {
            return (MessageBox.Show("Do you want to save changes?", "", MessageBoxButton.YesNo)
                == MessageBoxResult.Yes);
        }

        private void SaveData()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Serialized Data|*.sd|All|*.*";
            dialog.FilterIndex = 2;

            if ((bool)dialog.ShowDialog())
            {
                Data.Save(dialog.FileName);
            }
        }

        private void InitializeDataContext()
        {
            DataContext = Data;
            UpdateSaveFlag();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            if (Data.AddChangesAfterSave && WantSaveData())
            {
                SaveData();
            }
        }

        private void Filter_DataOnGrid(object sender, FilterEventArgs args)
        {
            args.Accepted = args.Item is V5DataOnGrid;
        }

        private void Filter_DataCollection(object sender, FilterEventArgs args)
        {
            args.Accepted = args.Item is V5DataCollection;
        }


        /* 2. Menu->File Methods */
        private void New(object sender, RoutedEventArgs e)
        {
            if (Data.AddChangesAfterSave && WantSaveData())
            {
                SaveData();
            }
            Data = new V5MainCollection();
            InitializeDataContext();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            if (Data.AddChangesAfterSave && WantSaveData())
            {
                SaveData();
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Serialization data|*.sd|All|*.*";
            dialog.FilterIndex = 2;

            if ((bool)dialog.ShowDialog())
            {
                Data = new V5MainCollection();
                try
                {
                    Data.Load(dialog.FileName);
                }
                catch (Exception exeption)
                {
                    MessageBox.Show(
                    exeption.Message,
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                }
                InitializeDataContext();
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveData();
            UpdateSaveFlag();
        }

        /* 3. Menu->Edit Methods */
        private void AddDefaults(object sender, RoutedEventArgs e)
        {
            Data.AddDefaults();
            UpdateSaveFlag();
        }

        private void AddDefaultV5DataCollection(object sender, RoutedEventArgs e)
        {
            Data.AddOneDefaultColection(3);
            UpdateSaveFlag();
        }

        private void AddDefaultV5DataOnGrid(object sender, RoutedEventArgs e)
        {
            Data.AddOneDefaultGrid(new Grid2D(1, 1, 1.0f, 1.0f));
            UpdateSaveFlag();
        }

        private void AddElementFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Serialization data|*.sd|All|*.*";
            dialog.FilterIndex = 2;

            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    Data.AddOneFileGrid(dialog.FileName);
                }
                catch (Exception exeption)
                {
                    MessageBox.Show(
                    exeption.Message,
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                }
                UpdateSaveFlag();
            }
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            var select_data = (sender as MenuItem).DataContext;
            if (select_data != null)
            {
                Data.RemoveElement(select_data as V5Data);
                UpdateSaveFlag();
            }
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (Data.AddChangesAfterSave && WantSaveData())
            {
                SaveData();
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Serialization data|*.sd|All|*.*";
            dialog.FilterIndex = 2;

            if ((bool)dialog.ShowDialog())
            {
                Data = new V5MainCollection();
                try
                {
                    Data.Load(dialog.FileName);
                }
                catch (Exception exeption)
                {
                    MessageBox.Show(
                    exeption.Message,
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                }
                InitializeDataContext();
            }
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveData();
            UpdateSaveFlag();
        }

        private void SaveCanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Data.AddChangesAfterSave;
        }

        private void AddCustomExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            V5DataOnGrid new_grid = new V5DataOnGrid(custom.ServiceInfo, new DateTime(1970, 1, 1), custom.Grid);
            Data.AddOneCustomGrid(new_grid);
            UpdateSaveFlag();
        }

        private void AddCustomCanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = custom.IsValid() && listBox_DataCollection != null && Data.IsValidServiceInfo(custom.ServiceInfo);
        }

        private void RemoveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var select_data = listBox_Main.SelectedItem;
            if (select_data != null)
            {
                Data.RemoveElement(select_data as V5Data);
                UpdateSaveFlag();
            }
        }

        private void RemoveCanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && listBox_Main.SelectedItem != null;
        }

        private void AddCustom(object sender, RoutedEventArgs e)
        {
            if (custom.IsValid() && listBox_DataCollection != null && Data.IsValidServiceInfo(custom.ServiceInfo))
            {
                V5DataOnGrid new_grid = new V5DataOnGrid(custom.ServiceInfo, new DateTime(1970, 1, 1), custom.Grid);
                Data.AddOneCustomGrid(new_grid);
                UpdateSaveFlag();
            }
            else
            {
                MessageBox.Show(
                    "You do not live according to the laws of thieves, correct yourself, fill in the fields.",
                    "Warning, guy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }

    public class Custom : IDataErrorInfo, INotifyPropertyChanged
    {
        public string ServiceInfo { get; set; } = string.Empty;
        public string NodesNumberX { get; set; }
        public string NodesNumberY { get; set; }
        public string StepSizeX { get; set; }
        public string StepSizeY { get; set; }


        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "ServiceInfo":
                        if (ServiceInfo == string.Empty)
                        {
                            error = "Error";
                        }
                        break;
                    case "NodesNumberX":
                        if (!int.TryParse(NodesNumberX, out _) || !int.TryParse(NodesNumberY, out _) || int.Parse(NodesNumberY) >= int.Parse(NodesNumberX))
                        {
                            error = "Error";
                        }
                        break;
                    case "NodesNumberY":
                        if (!int.TryParse(NodesNumberY, out _) || int.Parse(NodesNumberY) <= 2)
                        {
                            error = "Error";
                        }
                        else
                        {
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NodesNumberX"));
                        }
                        break;
                    case "StepSizeX":
                        if (!float.TryParse(StepSizeX, out _))
                        {
                            error = "Error";
                        }
                        break;
                    case "StepSizeY":
                        if (!float.TryParse(StepSizeY, out _))
                        {
                            error = "Error";
                        }
                        break;
                }
                return error;
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsValid()
        {
            return this[nameof(ServiceInfo)] == string.Empty && this[nameof(NodesNumberX)] == string.Empty &&
            this[nameof(NodesNumberY)] == string.Empty && this[nameof(StepSizeX)] == string.Empty && this[nameof(StepSizeY)] == string.Empty;
        }

        public Grid2D Grid => new Grid2D(int.Parse(NodesNumberX), int.Parse(NodesNumberY), float.Parse(StepSizeX), float.Parse(StepSizeY));
    }

    public class WindowCommands
    {
        public static RoutedCommand AddCustom { get; set; }

        static WindowCommands()
        {
            AddCustom = new RoutedCommand("AddCustom", typeof(MainWindow));
        }
    }
}
