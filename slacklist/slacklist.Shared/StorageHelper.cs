using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace slacklist
{
    internal class StorageHelper
    {
        /// <summary>
        /// Convert a Slack User List to a serialized object for storage. Then Store it.
        /// </summary>
        /// <param name="theList">A list of users.</param>
        /// <returns></returns>
        public async Task WriteJsonAsync(SlackMembers theList)
        {
            var serializer = new DataContractJsonSerializer(typeof(SlackMembers));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                          Configurations.LocalStorageFileName,
                          CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(stream, theList);
            }
        }

        /// <summary>
        /// Return an object from the local storage.
        /// </summary>
        /// <returns>A user list.</returns>
        public async Task<SlackMembers> DeserializeJsonAsync()
        {
            // TODO: This won't work in the current async framework. An alternative is required.
            var jsonSerializer = new DataContractJsonSerializer(typeof(SlackMembers));
            var myStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(Configurations.LocalStorageFileName);
            var theReturn = ((SlackMembers)jsonSerializer.ReadObject(myStream));
            return theReturn;
        }
    }
}
