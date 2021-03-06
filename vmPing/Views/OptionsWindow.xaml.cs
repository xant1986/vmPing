﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public static OptionsWindow openWindow = null;

        public OptionsWindow()
        {
            InitializeComponent();

            PopulateGeneralOptions();
            PopulateEmailAlertOptions();
            PopulateLogOutputOptions();
        }

        private void PopulateGeneralOptions()
        {
            string pingIntervalText;
            int pingIntervalDivisor;
            int pingInterval = ApplicationOptions.PingInterval;
            int pingTimeout = ApplicationOptions.PingTimeout;

            if (ApplicationOptions.PingInterval >= 3600000 && ApplicationOptions.PingInterval % 3600000 == 0)
            {
                pingIntervalText = "Hours";
                pingIntervalDivisor = 3600000;
            }
            else if (ApplicationOptions.PingInterval >= 60000 && ApplicationOptions.PingInterval % 60000 == 0)
            {
                pingIntervalText = "Minutes";
                pingIntervalDivisor = 60000;
            }
            else
            {
                pingIntervalText = "Seconds";
                pingIntervalDivisor = 1000;
            }

            pingInterval /= pingIntervalDivisor;
            pingTimeout /= 1000;

            txtPingInterval.Text = pingInterval.ToString();
            txtPingTimeout.Text = pingTimeout.ToString();
            txtAlertThreshold.Text = ApplicationOptions.AlertThreshold.ToString();
            cboPingInterval.Text = pingIntervalText;
        }

        private void PopulateEmailAlertOptions()
        {
            IsEmailAlertsEnabled.IsChecked = ApplicationOptions.IsEmailAlertEnabled;
            IsSmtpAuthenticationRequired.IsChecked = ApplicationOptions.IsEmailAuthenticationRequired;
            SmtpServer.Text = ApplicationOptions.EmailServer;
            SmtpPort.Text = ApplicationOptions.EmailPort;
            SmtpUsername.Text = ApplicationOptions.EmailUser;
            SmtpPassword.Password = ApplicationOptions.EmailPassword;
            EmailRecipientAddress.Text = ApplicationOptions.EmailRecipient;
            EmailFromAddress.Text = ApplicationOptions.EmailFromAddress;
        }

        private void PopulateLogOutputOptions()
        {
            LogPath.Text = ApplicationOptions.LogPath;
            IsLogOutputEnabled.IsChecked = ApplicationOptions.IsLogOutputEnabled;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (SaveGeneralOptions() == false)
                return;

            if (SaveEmailAlertOptions() == false)
                return;

            if (SaveLogOutputOptions() == false)
                return;

            Close();
        }


        private bool SaveGeneralOptions()
        {
            if (txtPingInterval.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter a valid ping interval.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingInterval.Focus();
                return false;
            }
            else if (txtPingTimeout.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter a valid ping timeout.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingTimeout.Focus();
                return false;
            }
            else if (txtAlertThreshold.Text.Length == 0)
            {
                GeneralTab.Focus();
                MessageBox.Show(
                    "Please enter an alert threshold.",
                    "vmPing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                txtPingTimeout.Focus();
                return false;
            }


            // Ping interval
            int pingInterval;
            int multiplier = 1000;

            switch (cboPingInterval.Text)
            {
                case "Seconds":
                    multiplier = 1000;
                    break;
                case "Minutes":
                    multiplier = 1000 * 60;
                    break;
                case "Hours":
                    multiplier = 1000 * 60 * 60;
                    break;
            }

            if (int.TryParse(txtPingInterval.Text, out pingInterval) && pingInterval > 0 && pingInterval <= 86400)
                pingInterval *= multiplier;
            else
                pingInterval = Constants.PING_INTERVAL;

            ApplicationOptions.PingInterval = pingInterval;

            // Ping timeout
            int pingTimeout;

            if (int.TryParse(txtPingTimeout.Text, out pingTimeout) && pingTimeout > 0 && pingTimeout <= 60)
                pingTimeout *= 1000;
            else
                pingTimeout = Constants.PING_TIMEOUT;

            ApplicationOptions.PingTimeout = pingTimeout;

            // Alert threshold
            int alertThreshold;

            var isThresholdValid = int.TryParse(txtAlertThreshold.Text, out alertThreshold) && alertThreshold > 0 && alertThreshold <= 60;
            if (!isThresholdValid)
                alertThreshold = 1;

            ApplicationOptions.AlertThreshold = alertThreshold;

            return true;
        }


        private bool SaveEmailAlertOptions()
        {
            // Validate input.
            if (IsEmailAlertsEnabled.IsChecked == true)
            {
                var regex = new Regex("^\\d+$");

                if (SmtpServer.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid address for your outgoing mail server.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    SmtpServer.Focus();
                    return false;
                }
                else if (SmtpPort.Text.Length == 0 || !regex.IsMatch(SmtpPort.Text))
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid port number.  The standard is 25.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    SmtpPort.Focus();
                    return false;
                }
                else if (EmailRecipientAddress.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid recipient email address.  This is the address that will receive alerts.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EmailRecipientAddress.Focus();
                    return false;
                }
                else if (EmailFromAddress.Text.Length == 0)
                {
                    EmailAlertsTab.Focus();
                    MessageBox.Show(
                        "Please enter a valid 'from' address.  This address will appear as the sender for any alerts that are sent.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EmailFromAddress.Focus();
                    return false;
                }
                if (IsSmtpAuthenticationRequired.IsChecked == true)
                {
                    ApplicationOptions.IsEmailAuthenticationRequired = true;
                    if (SmtpUsername.Text.Length == 0)
                    {
                        EmailAlertsTab.Focus();
                        MessageBox.Show(
                            "Please enter a valid username for authenticating to your mail server.",
                            "vmPing Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        SmtpUsername.Focus();
                        return false;
                    }
                }

                ApplicationOptions.IsEmailAlertEnabled = true;
                ApplicationOptions.EmailServer = SmtpServer.Text;
                ApplicationOptions.EmailPort = SmtpPort.Text;
                ApplicationOptions.EmailUser = SmtpUsername.Text;
                ApplicationOptions.EmailPassword = SmtpPassword.Password;
                ApplicationOptions.EmailRecipient = EmailRecipientAddress.Text;
                ApplicationOptions.EmailFromAddress = EmailFromAddress.Text;

                return true;
            }
            else
                return true;
        }

        private bool SaveLogOutputOptions()
        {
            if (IsLogOutputEnabled.IsChecked == true)
            {
                if (!Directory.Exists(LogPath.Text))
                {
                    LogOutputTab.Focus();
                    MessageBox.Show(
                        "The specified path does not exist.  Please enter a valid path.",
                        "vmPing Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    LogPath.Focus();
                    return false;
                }

                ApplicationOptions.IsLogOutputEnabled = true;
                ApplicationOptions.LogPath = LogPath.Text;
            }
            else
            {
                ApplicationOptions.IsLogOutputEnabled = false;
            }

            return true;
        }


        private void txtNumericTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.-]+");
            if (regex.IsMatch(e.Text))
                e.Handled = true;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            openWindow = this;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            openWindow = null;
        }

        private void EmailRecipientAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailFromAddress.Text.Length == 0 && EmailRecipientAddress.Text.IndexOf('@') >= 0)
                EmailFromAddress.Text = "vmPing" + EmailRecipientAddress.Text.Substring(EmailRecipientAddress.Text.IndexOf('@'));
        }

        private void IsEmailAlertsEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (IsEmailAlertsEnabled.IsChecked == true && SmtpServer.Text.Length == 0)
                SmtpServer.Focus();
        }

        private void IsSmtpAuthenticationRequired_Click(object sender, RoutedEventArgs e)
        {
            if (IsSmtpAuthenticationRequired.IsChecked == true)
                SmtpUsername.Focus();
        }

        private void BrowseLogPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select a location for the log files.";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                LogPath.Text = dialog.SelectedPath;
        }
    }
}
