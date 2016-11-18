using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Diagnostics;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace slacklist
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private SlackMembers _currentProfiles = new SlackMembers();
        private Member _currentMember = new Member();
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Navigatedto method.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentMember = e.Parameter as Member;
            if (_currentMember != null)
            {
                ProfilePageTitle.Text = _currentMember.Name;
                UserProfileBox.DataContext = _currentProfiles;
                _currentProfiles.Add(_currentMember);
                var getAuthTask = new Task(AsyncPresence);
                getAuthTask.Start();
            }
            else
            {
                // TODO: add code to clean up broken navigation.
            }
        }

        /// <summary>
        /// Get Presence information per user.
        /// </summary>
        private async void AsyncPresence()
        {
            var client = new HttpClient();

            var uri = new Uri(string.Format(Configurations.PresenceUrl, Configurations.ApiKey, _currentMember.id));
            var response = await client.GetAsync(uri);
            var statusCode = response.StatusCode;
            switch (statusCode)
            {
                // TODO: Error handling for invalid htpp responses.
            }
            response.EnsureSuccessStatusCode();
            var theResponse = await response.Content.ReadAsStringAsync();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(theResponse)))
            {
                var serializer = new DataContractJsonSerializer(typeof(PresenceRoot));
                var presenceObject = serializer.ReadObject(ms) as PresenceRoot;
                // This will allow the application to toggle the presence indicator color.    
                if (presenceObject != null && presenceObject.ok && presenceObject.IsActive())
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        StatusIndicator.Fill = new SolidColorBrush(Color.FromArgb(255, 127, 153, 71));
                    });
                }
                // TODO: Add more code for bad replies or invalid requests.
            }
        }

        /// <summary>
        /// Back Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        /// <summary>
        /// IM Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void IM_Button_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Sorry, Not ready yet!");
            await messageDialog.ShowAsync();
        }

        /// <summary>
        /// Channel Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChannelButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Sorry, Not ready yet!");
            await messageDialog.ShowAsync();
        }
    }
}
