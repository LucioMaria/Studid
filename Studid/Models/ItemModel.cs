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
        
        [MapTo("memorized")]
        public bool isMemorized { get; set; }

        [MapTo("picUrl")]
        public String picUrl;

        public bool isPlaying;
        public int CompareTo(object obj)
        {
            var em = obj as ItemModel;
            return itemName.CompareTo(em.itemName);
        }

        public ItemModel() { }
        public ItemModel(string itemId, string itemName,string picUrl)
        {
            this.itemId = itemId;
            this.itemName = itemName;
            this.picUrl = picUrl;
            this.isMemorized = false;
        }
        public ItemModel(string itemId, string itemName)
        {
            this.itemId = itemId;
            this.itemName = itemName;
            this.picUrl = null;
            this.isMemorized = false;
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemModel)
            {
                ItemModel model = (ItemModel)obj;
                return this.itemId.Equals(model.itemId);
            }
            throw new Exception();
        }
    }
}