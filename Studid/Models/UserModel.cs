using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using Plugin.CloudFirestore.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Studid.Models
{
    class UserModel : IComparable
    {
        [MapTo("userName")]
        public string userName { get; set; }
        
        
        public int CompareTo(object obj)
        {
            var em = obj as UserModel;
            return userName.CompareTo(em.userName);
        }

        public override bool Equals(object obj)
        {
            if (obj is UserModel)
            {
                UserModel model = (UserModel)obj;
                return this.userName.Equals(model.userName);
            }
            throw new Exception();
        }
    }
}