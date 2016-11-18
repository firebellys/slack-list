using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Xml.Serialization;

namespace slacklist
{
    class Users
    {

    }


    /// <summary>
    /// Observables to deal with data at runtime.
    /// </summary>
    public class SlackMembers : ObservableCollection<Member>
    {
        /// <summary>
        /// Allows the collection to fire update events even when being passed an entire array.
        /// </summary>
        /// <param name="collection">A collection of members.</param>
        public void AddRange(IEnumerable<Member> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Allows the collection to fire update events even when being passed an entire array.
        /// Clears before use.
        /// </summary>
        /// <param name="collection">A collection of members.</param>
        public void AddRangeWithClear(IEnumerable<Member> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        /// <summary>
        /// Store the time we last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    public class Profile
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string title { get; set; }
        public string skype { get; set; }
        public string phone { get; set; }
        public string image_24 { get; set; }
        public string image_32 { get; set; }
        public string image_48 { get; set; }
        public string image_72 { get; set; }
        public string image_192 { get; set; }
        public string image_original { get; set; }
        public string real_name { get; set; }
        public string real_name_normalized { get; set; }
        public string email { get; set; }
    }

    public class Member
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool deleted { get; set; }
        public object status { get; set; }
        public string color { get; set; }
        public string real_name { get; set; }
        public string tz { get; set; }
        public string tz_label { get; set; }
        public int tz_offset { get; set; }
        public Profile profile { get; set; }
        public bool is_admin { get; set; }
        public bool is_owner { get; set; }
        public bool is_primary_owner { get; set; }
        public bool is_restricted { get; set; }
        public bool is_ultra_restricted { get; set; }
        public bool is_bot { get; set; }
        public bool has_files { get; set; }
        public bool has_2fa { get; set; }

        public string Title => profile.title;
        public string Skype => profile.skype;
        public string Phone => profile.phone;
        public string Email => profile.email;
        public string Name => profile.real_name_normalized;
        public string Color => "#" + color;
        public string Image => profile.image_72;
    }

    
    public class UserRoot
    {
        public bool ok { get; set; }
        public SlackMembers members { get; set; }
    }

    public class PresenceRoot
    {
        public bool ok { get; set; }
        public string presence { get; set; }

        public bool IsActive()
        {
            return presence == "active";
        }
        public string Color()
        {
            return presence == "active" ? "#7f9947" : "#443142";
        }
    }
}
