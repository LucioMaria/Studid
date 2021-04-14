using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CloudFirestore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Studid.Models
{
    class ItemModel : IComparable
    {
        [MapTo("itemId")]
        public string itemId { get; set; }

        [MapTo("itemName")]
        public string itemName { get; set; }
        
        [MapTo("isMemorized")]
        public bool IsMemorized { get; set; }
        public int CompareTo(object obj)
        {
            var em = obj as ItemModel;
            return itemName.CompareTo(em.itemName);
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemModel)
            {
                ItemModel model = (ItemModel)obj;
                return this.itemName.Equals(model.itemName);
            }
            throw new Exception();
        }
    }
}