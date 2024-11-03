using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Lurasidone.Trench;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using Window = System.Windows.Window;

namespace Lurasidone
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, (Action<string> SetValue, Func<string> GetValue)> _dataBindings;
        private byte[]? _file;
        private List<Datapomp>? _structs;

        public MainWindow()
        {
            InitializeComponent();
            InitializeValues();

            _dataBindings = new Dictionary<string, (Action<string>, Func<string>)>
            {
                { "Keys", (value => KeysCollectedTextBox.Text = value, () => KeysCollectedTextBox.Text) },
                { "MapLayer", (value => MapLayerTextBox.Text = value, () => MapLayerTextBox.Text) },
                { "EmeraldKeys", (value => EmeraldKeysTextBox.Text = value, () => EmeraldKeysTextBox.Text) },
                { "School", (value => SchoolDoneButton.Content = value, () => SchoolDoneButton.Content.ToString() ?? "False") },
                { "Sewer", (value => SewerDoneButton.Content = value, () => SewerDoneButton.Content.ToString() ?? "False") },
                { "Hospital", (value => HospitalDoneButton.Content = value, () => HospitalDoneButton.Content.ToString() ?? "False") },
                { "Library", (value => LibraryDoneButton.Content = value, () => LibraryDoneButton.Content.ToString() ?? "False") },
                { "DNA", (value => DNADoneButton.Content = value, () => DNADoneButton.Content.ToString() ?? "False") },
                { "Building", (value => BuildingDoneButton.Content = value, () => BuildingDoneButton.Content.ToString() ?? "False") },
                { "Symbol", (value => SymbolDoneButton.Content = value, () => SymbolDoneButton.Content.ToString() ?? "False") },
                { "TPSUnlocked", (value => TPSUnlockedButton.Content = value, () => TPSUnlockedButton.Content.ToString() ?? "False") },
                { "WonGame", (value => WonGameButton.Content = value, () => WonGameButton.Content.ToString() ?? "False") }
            };
        }

        private void InitializeValues()
        {
            KeysCollectedTextBox.Text = "0";
            MapLayerTextBox.Text = "1";
            EmeraldKeysTextBox.Text = "0";

            SchoolDoneButton.Content = "False";
            SewerDoneButton.Content = "False";
            HospitalDoneButton.Content = "False";
            DNADoneButton.Content = "False";
            BuildingDoneButton.Content = "False";
            SymbolDoneButton.Content = "False";
            TPSUnlockedButton.Content = "False";
            WonGameButton.Content = "False";
        }

        private void ToggleBoolean(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string content)
            {
                button.Content = content == "True" ? "False" : "True";
            }
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                HandleFile(openFileDialog.FileName);
            }
        }

        private void HandleFile(string filePath)
        {
            _file = File.ReadAllBytes(filePath);
            var hammer = new ByteHammer(_file);
            _structs = hammer.GetStructs();

            FillData(_structs);
        }

        private void FillData(List<Datapomp> structs)
        {
            foreach (var strct in structs)
            {
                if (_dataBindings.TryGetValue(strct.Name, out var binding))
                {
                    binding.SetValue(strct.Value?.ToString() ?? string.Empty);
                }
            }
        }

        private void SaveData(List<Datapomp>? structs)
        {
            if (structs is null)
                return;

            foreach (var strct in structs)
            {
                if (_dataBindings.TryGetValue(strct.Name, out var binding))
                {
                    strct.Value = strct.Name switch
                    {
                        "Keys" or "MapLayer" or "EmeraldKeys" => ReadInt(binding.GetValue()),
                        _ => ReadBool(binding.GetValue())
                    };
                }
            }
        }

        private int ReadInt(string number)
        {
            return int.TryParse(number, out var result) ? result : 0;
        }

        private bool ReadBool(string boolean)
        {
            return bool.TryParse(boolean, out var result) && result;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Save files (*.save)|*.save|All files (*.*)|*.*",
                DefaultExt = ".save",
                FileName = "prefs.save"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                SaveData(_structs);

                if (_structs is null || _file is null)
                    return;

                var overwriteResult = ByteHammer.OverwriteBytes(_file, _structs);
                File.WriteAllBytes(filePath, overwriteResult);
            }
        }
    }
}
