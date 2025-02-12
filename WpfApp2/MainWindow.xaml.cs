using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            if (!IsRunAsAdmin())
            {
                RestartAsAdmin();
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();
        }

        private bool IsRunAsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);

                if (key != null)
                {
                    RegistryKey systemKey = key.OpenSubKey("System", true) ?? key.CreateSubKey("System");

                    systemKey.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
                    systemKey.Close();

                    MessageBox.Show("Диспетчер задач отключен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка доступа к реестру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);

                if (key != null && key.GetValue("DisableTaskMgr") != null)
                {
                    key.DeleteValue("DisableTaskMgr");
                    key.Close();
                    MessageBox.Show("Диспетчер задач включен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Параметр не найден, диспетчер задач уже включен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"SOFTWARE\Microsoft\PolicyManager\default\Start\HideRestart";

                // Открываем реестр с доступом к 64-битной ветке
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                                  .OpenSubKey(path, true) ??
                                                  Registry.LocalMachine.CreateSubKey(path))
                {
                    if (key != null)
                    {
                        key.SetValue("Value", 1, RegistryValueKind.DWord);
                        MessageBox.Show("Перезапуск скрыт!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ключ реестра не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"SOFTWARE\Microsoft\PolicyManager\default\Start\HideRestart";

                // Открываем реестр с доступом к 64-битной ветке
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                                  .OpenSubKey(path, true) ??
                                                  Registry.LocalMachine.CreateSubKey(path))
                {
                    if (key != null)
                    {
                        key.SetValue("Value", 0, RegistryValueKind.DWord);
                        MessageBox.Show("Перезапуск скрыт!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ключ реестра не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // Проверяем наличие интернет-соединения
            if (!IsInternetAvailable())
            {
                MessageBox.Show("Отсутствует интернет-соединение. Проверьте подключение и попробуйте снова.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string folderPath = @"C:\Windows\System32\drivers\etc";
            try
            {
                // Путь к файлу hosts
                string filePath = @"C:\Windows\System32\drivers\etc\hosts";

                // Удаление файла, если он существует
                if (File.Exists(filePath))
                {
                    // Изменяем владельца файла на текущего пользователя
                    ChangeOwner(filePath);

                    // Добавляем полный доступ для текущего пользователя
                    AddFullAccess(filePath);

                    // Удаляем файл
                    File.Delete(filePath);
                }

                // Загрузка данных с GitHub
                string url = "https://raw.githubusercontent.com/bibonuwu/Bibon/main/blocked_sites.txt"; // Укажите ваш URL к файлу на GitHub
                string content = await DownloadBlockedSitesAsync(url);

                // Создание нового файла и запись в него
                File.WriteAllText(filePath, content + Environment.NewLine);

                // Очистка DNS-кэша
                FlushDNS();



                // Отключение Microsoft Store
                DisableMicrosoftStore();

                // Установка атрибута "Только для чтения"


                if (Directory.Exists(folderPath))
                {
                    FileInfo dirInfo = new FileInfo(filePath);
                    dirInfo.Attributes |= FileAttributes.Hidden; // Устанавливаем атрибут скрытой папки
                }
                else
                {
                    MessageBox.Show("Папка не найдена.");
                }

                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                    dirInfo.Attributes |= FileAttributes.Hidden; // Устанавливаем атрибут скрытой папки
                }
                else
                {
                    MessageBox.Show("Папка не найдена.");
                }
                // Установка защиты от удаления
                ProtectFileFromDeletion(filePath);
                MessageBox.Show("Операция выполнена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        // Метод для изменения владельца файла
        private void ChangeOwner(string filePath)
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c takeown /f \"{filePath}\" /a")
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };
            Process.Start(psi)?.WaitForExit();
        }

        // Метод для добавления полного доступа
        private void AddFullAccess(string filePath)
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            var psi = new ProcessStartInfo("cmd.exe", $"/c icacls \"{filePath}\" /grant \"{userName}\":F")
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };
            Process.Start(psi)?.WaitForExit();
        }
        private async Task<string> DownloadBlockedSitesAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
                    return string.Empty;
                }
            }
        }
        private void FlushDNS()
        {

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();

            }
            catch (Exception ex)
            {
            }
        }
        private void DisableMicrosoftStore()
        {
            try
            {
                // Запуск PowerShell-команды для удаления Microsoft Store
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-AppxPackage -AllUsers *WindowsStore* | Remove-AppxPackage",
                    Verb = "runas", // Запуск от имени администратора
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden // Нормальный стиль окна

                };
                var process = Process.Start(processInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отключить Microsoft Store: " + ex.Message);
            }
        }
        private void ProtectFileFromDeletion(string filePath)
        {
            var commands = new[]
            {
        // Устанавливаем владельца (не обязательно, но повышает безопасность)
        $"/c takeown /f \"{filePath}\"",
        
        // Сбрасываем наследование и удаляем все существующие права
        $"/c icacls \"{filePath}\" /inheritance:r /grant:r *S-1-5-18:(F)", // Полные права для SYSTEM
        
        // Разрешаем чтение для всех пользователей
        $"/c icacls \"{filePath}\" /grant *S-1-1-0:(RX)", // Everyone: Read & Execute
        
        // Запрещаем удаление для всех пользователей
        $"/c icacls \"{filePath}\" /deny *S-1-1-0:(DE,DC,WDAC,WO)" // Deny Delete, DeleteChild, WriteDAC, WriteOwner
    };

            foreach (var cmd in commands)
            {
                var psi = new ProcessStartInfo("cmd.exe", cmd)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi)?.WaitForExit();
            }
        }



        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://www.google.com").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // Проверяем наличие интернет-соединения
            if (!IsInternetAvailable())
            {
                MessageBox.Show("Отсутствует интернет-соединение. Проверьте подключение и попробуйте снова.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string folderPath = @"C:\Windows\System32\drivers\etc";
            string hostsPath = System.IO.Path.Combine(folderPath, "hosts");


            try
            {
                if (File.Exists(hostsPath))
                {
                    // Изменяем владельца файла на текущего пользователя
                    ResetFilePermissions(hostsPath);


                    // Удаляем файл hosts
                    File.Delete(hostsPath);

                    // Включение Microsoft Store
                    EnableMicrosoftStore2();

                    // Снимаем скрытые атрибуты с папки и её содержимого
                    UnhideFolderAndContents(folderPath);

                }
                else
                {
                    UnhideFolderAndContents(folderPath);

                    MessageBox.Show("Файл hosts не найден.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                MessageBox.Show("Файл hosts удален, а Microsoft Store восстановлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении файла hosts: " + ex.Message);
            }

        }

        private void ResetFilePermissions(string filePath)
        {
            var commands = new[]
            {
        // Берем владение файлом
        $"/c takeown /f \"{filePath}\"",
        
        // Сбрасываем все права до исходных
        $"/c icacls \"{filePath}\" /reset",
        
        // Явно даем полные права текущему пользователю
        $"/c icacls \"{filePath}\" /grant %USERNAME%:F"
    };

            foreach (var cmd in commands)
            {
                ExecuteCommand(cmd);
                Thread.Sleep(500); // Даем время на применение прав
            }
        }
        private void ExecuteCommand(string command)
        {
            var psi = new ProcessStartInfo("cmd.exe", command)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas", // Запуск с правами администратора
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                }
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Требуются права администратора!");
            }
        }

        private void EnableMicrosoftStore2()
        {
            try
            {
                // Запуск PowerShell-команды для восстановления Microsoft Store
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-AppXPackage *WindowsStore* -AllUsers | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \\\"$($_.InstallLocation)\\AppXManifest.xml\\\"}",
                    Verb = "runas", // Запуск от имени администратора
                    UseShellExecute = true,
                    CreateNoWindow = true, // Показывать окно
                    WindowStyle = ProcessWindowStyle.Hidden // Нормальный стиль окна
                };
                var process = Process.Start(processInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось включить Microsoft Store: " + ex.Message);
            }
        }
        private void UnhideFolderAndContents(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // Снимаем атрибут "Скрытый" с самой папки
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                dirInfo.Attributes &= ~FileAttributes.Hidden;

                // Снимаем атрибут "Скрытый" с вложенных файлов
                foreach (var file in dirInfo.GetFiles())
                {
                    file.Attributes &= ~FileAttributes.Hidden;
                }

                // Снимаем атрибут "Скрытый" с вложенных папок
                foreach (var subDir in dirInfo.GetDirectories())
                {
                    subDir.Attributes &= ~FileAttributes.Hidden;
                }

            }
            else
            {
                MessageBox.Show("Папка не найдена.");
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем или создаем ключ реестра
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true))
                {
                    if (key != null)
                    {
                        // Создаем параметр DWORD (32 бита) с именем NoControlPanel и значением 1
                        key.SetValue("NoControlPanel", 1, RegistryValueKind.DWord);

                        MessageBox.Show("Панель управления заблокирована!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось открыть ключ реестра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Запустите программу от имени администратора!", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем или создаем ключ реестра
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true))
                {
                    if (key != null)
                    {
                        // Создаем параметр DWORD (32 бита) с именем NoControlPanel и значением 1
                        key.SetValue("NoControlPanel", 0, RegistryValueKind.DWord);

                        MessageBox.Show("Панель управления разблокирована!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось открыть ключ реестра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Запустите программу от имени администратора!", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
