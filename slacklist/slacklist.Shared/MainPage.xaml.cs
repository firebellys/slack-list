using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace slacklist
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string _theResponse;
        private SlackMembers _currentUsers = new SlackMembers();
        private SlackMembers _originalCurrentUsers = new SlackMembers();
        //private Windows.Storage.ApplicationDataContainer _localSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            //_localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            UserListBox.DataContext = _currentUsers;

            // Set up the initial list starting with the HTTP Call.
            var getAuthTask = new Task(AsyncHttpCall);
            getAuthTask.Start();
        }

        /// <summary>
        /// Main Request Thread
        /// </summary>
        async void AsyncHttpCall()
        {
            //Get the Internet connection profile
            try
            {
                // TODO: Section to detect over the limit/data issues. Create events for roaming or other issues. 
                var _localStorageHelper = new StorageHelper();
                var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (internetConnectionProfile.GetNetworkConnectivityLevel() < NetworkConnectivityLevel.InternetAccess)
                {
                    // If the internet is down, check local storage for the last use, if not found, displace a message.
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        NoConnectionText.Visibility = Visibility.Collapsed;
                        MainProgressRing.IsActive = true;
                        MainProgressRing.Visibility = Visibility.Visible;
                        UserListBox.Visibility = Visibility.Collapsed;
                        var messageDialog = new MessageDialog("Sorry, no network detected.");
                        // TODO: Move this out of the async thread.
                        messageDialog.ShowAsync();
                    });
                    // TODO: This needs to be moved to the storage helper.
                    var jsonSerializer = new DataContractJsonSerializer(typeof(SlackMembers));
                    var myStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(Configurations.LocalStorageFileName);
                    _originalCurrentUsers.AddRangeWithClear(((SlackMembers)jsonSerializer.ReadObject(myStream)));

                    // If there is no internet and the application has no previously saved items, display a small message.
                    if (_originalCurrentUsers.Count == 0)
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            NoConnectionText.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        // If we have no internet, but there is previous data, load that data.
                        _currentUsers = _originalCurrentUsers;
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            // TODO: Have the last update pull from the local store, this will require a new saving method.
                            MainProgressRing.IsActive = false;
                            MainProgressRing.Visibility = Visibility.Collapsed;
                            UserListBox.Visibility = Visibility.Visible;
                        });
                    }
                }
                else
                {
                    // If we have a network connection, begin to call the web api.
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainProgressRing.IsActive = true;
                        MainProgressRing.Visibility = Visibility.Visible;
                        UserListBox.Visibility = Visibility.Collapsed;
                        NoConnectionText.Visibility = Visibility.Collapsed;
                    });

                    //Create Client 
                    var client = new HttpClient();

                    var uri = new Uri(string.Format(Configurations.UserListUrl, Configurations.ApiKey));
                    var response = await client.GetAsync(uri);
                    var statusCode = response.StatusCode;
                    switch (statusCode)
                    {
                        // TODO: Error handling for invalid htpp responses.
                    }
                    response.EnsureSuccessStatusCode();
                    _theResponse = await response.Content.ReadAsStringAsync();
                    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(_theResponse)))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(UserRoot));
                        var theList = serializer.ReadObject(ms) as UserRoot;
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (theList != null && theList.ok)
                            {
                                theList.members.LastUpdated = DateTime.Now;
                                // Stamp the last time a user updated the list live.
                                LastUpdatedText.Text = theList.members.LastUpdated.ToString("MMMM, MM dd, yyyy hh: mm:ss");
                                // TODO: This section needs to be cleaned up. We are wasting space on multiple instances of a similar object.
                                _currentUsers.AddRangeWithClear(theList.members);
                                _originalCurrentUsers.AddRangeWithClear(theList.members);
                                MainProgressRing.IsActive = false;
                                MainProgressRing.Visibility = Visibility.Collapsed;
                                UserListBox.Visibility = Visibility.Visible;
                                // Save to local storage, and ignore the thread.
                                // TODO: This is not recommended. Move this call outside the async method to meet standards.
                                _localStorageHelper.WriteJsonAsync(theList.members);
                            }
                            else
                            {
                                // TODO: Add more code for bad replies or invalid requests.
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Find a fix for the await in the exception thread.
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    MainProgressRing.IsActive = false;
                    MainProgressRing.Visibility = Visibility.Collapsed;
                    UserListBox.Visibility = Visibility.Collapsed;
                    var messageDialog = new MessageDialog("Sorry, an error occured! " + ex.Message);
                    NoConnectionText.Visibility = Visibility.Visible;
                    messageDialog.ShowAsync();
                });
            }
        }

        /// <summary>
        /// Refresh the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // TODO: This should launch the Hub, basically become a hamburger button and open up the Win10 and WP10 sidebar/hub.
            var getAuthTask = new Task(AsyncHttpCall);
            getAuthTask.Start();
        }

        /// <summary>
        /// Open the search panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSearchBox(!IsSearchOpen());
        }

        /// <summary>
        /// Open and close the search feature.
        /// </summary>
        /// <param name="openBox"></param>
        private void ToggleSearchBox(bool openBox)
        {
            if (openBox)
            {
                BottomGrid.RowDefinitions[0].Height = new GridLength(51);
            }
            else
            {
                _currentUsers.AddRangeWithClear(_originalCurrentUsers);
                BottomGrid.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        /// <summary>
        /// Is the search feature really open?
        /// </summary>
        /// <returns></returns>
        private bool IsSearchOpen()
        {
            if (BottomGrid.RowDefinitions[0].Height.Value > 0)
            {
                return true; ;
            }
            return false;
        }

        /// <summary>
        /// Get list item click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserListBox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleSearchBox(false);
            Frame.Navigate(typeof(ProfilePage), _currentUsers[UserListBox.SelectedIndex]);
        }

        /// <summary>
        /// Simple manual filter. This could be moved to the class itself eventually.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var result = from o in _originalCurrentUsers
                         where o.Name.ToLower().Contains(SearchBox.Text.ToLower())
                         select o;
            _currentUsers.AddRangeWithClear(result);
        }
    }
}
