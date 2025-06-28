using Newtonsoft.Json;
using NHotkey;
using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;


namespace HotkeyLauncher
{
    public partial class MainWindow : Window
    {
        private List<HotkeySetting> hotkeySettings = new List<HotkeySetting>();
        private string settingsPath = "hotkeys.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            RegisterAllHotkeys();
        }

        private void AddHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            var keyConverter = new KeyConverter();
            Key key;

            try
            {
                key = (Key)keyConverter.ConvertFromString(KeyTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Некорректная клавиша.");
                return;
            }

            ModifierKeys modifiers = 0;
            if (CtrlCheckBox.IsChecked == true) modifiers |= ModifierKeys.Control;
            if (AltCheckBox.IsChecked == true) modifiers |= ModifierKeys.Alt;
            if (ShiftCheckBox.IsChecked == true) modifiers |= ModifierKeys.Shift;

            string browser = ((ComboBoxItem)BrowserComboBox.SelectedItem).Content.ToString().ToLower();

            var setting = new HotkeySetting
            {
                Name = $"{modifiers}+{key}",
                PathsOrUrls = PathTextBox.Text.Split(',')
                                               .Select(p => p.Trim())
                                               .Where(p => !string.IsNullOrEmpty(p))
                                               .ToList(),
                Key = key,
                Modifiers = modifiers,
                Browser = browser
            };


            hotkeySettings.Add(setting);
            HotkeysListBox.Items.Add(setting.Name);

            RegisterHotkey(setting);
            SaveSettings();
        }

        private void RegisterHotkey(HotkeySetting setting)
        {
            HotkeyManager.Current.AddOrReplace(setting.Name, setting.Key, setting.Modifiers, (s, e) =>
            {
                try
                {
                    var urls = setting.PathsOrUrls
                        .Where(url => url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var programs = setting.PathsOrUrls
                        .Where(path => !path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (urls.Any())
                    {
                        switch (setting.Browser?.ToLower())
                        {
                            case "firefox":
                                {
                                    string args = string.Join(" ", urls.Select(u => $"-new-tab \"{u}\""));
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = @"C:\Program Files\Mozilla Firefox\firefox.exe",
                                        Arguments = args,
                                        UseShellExecute = false
                                    });
                                    break;
                                }
                            case "chrome":
                                {   
                                    string args = string.Join(" ", urls.Select(u => $"\"{u}\""));
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                                        Arguments = args,
                                        UseShellExecute = false
                                    });
                                    break;
                                }
                            case "edge":
                                {  
                                    string args = string.Join(" ", urls.Select(u => $"\"{u}\""));
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                                        Arguments = args,
                                        UseShellExecute = false
                                    });
                                    break;
                                }
                            default:
                                {
                                    foreach (var url in urls)
                                    {
                                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                                    }
                                    break;
                                }
                        }
                    }

                 
                    foreach (var app in programs)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(app) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка запуска приложения: {app}\n{ex.Message}");
                        }
                    }

                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке горячей клавиши:\n{ex.Message}");
                }
            });
        }



        private void RegisterAllHotkeys()
        {
            foreach (var setting in hotkeySettings)
            {
                RegisterHotkey(setting);
                HotkeysListBox.Items.Add(setting.Name);
            }
        }

        private void SaveSettings()
        {
            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(hotkeySettings, Formatting.Indented));
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsPath))
            {
                hotkeySettings = JsonConvert.DeserializeObject<List<HotkeySetting>>(File.ReadAllText(settingsPath));
            }
        }
        private void RemoveHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (HotkeysListBox.SelectedItem is string selectedName)
            {
                var setting = hotkeySettings.Find(h => h.Name == selectedName);
                if (setting != null)
                {
                    hotkeySettings.Remove(setting);

                    HotkeyManager.Current.Remove(selectedName);

                    HotkeysListBox.Items.Remove(selectedName);

                    SaveSettings();
                }
            }
            else
            {
                MessageBox.Show("Выберите комбинацию для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Выберите файл для запуска";
            dlg.Filter = "Все файлы (*.*)|*.*";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(PathTextBox.Text))
                {
                    PathTextBox.Text = dlg.FileName;
                }
                else
                {
                    PathTextBox.Text += "," + dlg.FileName;
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }


    }
}
