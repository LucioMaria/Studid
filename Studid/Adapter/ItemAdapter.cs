﻿using System;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Content;
using Java.Text;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Studid.Models;

namespace Studid.Adapter
{
    class ItemAdapter : RecyclerView.Adapter
    {
        public Context context;
        public RecyclerView RecyclerView;
        public event EventHandler<ItemAdapterClickEventArgs> ItemClick;
        public event EventHandler<ItemAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<ItemAdapterClickEventArgs> ItemUpdate_NameClick;
        public event EventHandler<ItemAdapterClickEventArgs> Item_CheckClick;
        public event EventHandler<ItemAdapterClickEventArgs> Item_SelectClick;
        public List<ItemModel> ItemList;
        public TextView textView, chekedText;
        public CheckBox check;
        public ImageButton selectItem;

        public ItemAdapter(Context context, RecyclerView recyclerView, List<ItemModel> ExamItems)
        {
            this.context = context;
            RecyclerView = recyclerView;
            ItemList = ExamItems;
        }
        public ItemAdapter(Context context, RecyclerView recyclerView)
        {
            this.context = context;
            RecyclerView = recyclerView;
            ItemList = new List<ItemModel>();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_row_layout, parent, false);
            var vh = new ItemViewHolder(itemview, OnClick, OnLongClick, OnItemNameClick, OnItemCheckClick, OnItemSelectClick);
            return vh;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = ItemList[position];
            var holder = viewHolder as ItemViewHolder;
            holder.textView.Text = item.itemName;
            holder.check.Checked = item.isMemorized;
            if (holder.check.Checked)
            {
                holder.chekedText.Visibility = ViewStates.Visible;
            }
            else
            {
                holder.chekedText.Visibility = ViewStates.Invisible;
            }
        }

        public override int ItemCount => ItemList.Count;

        void OnClick(ItemAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ItemAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void OnItemNameClick(ItemAdapterClickEventArgs args) => ItemUpdate_NameClick?.Invoke(this, args);
        void OnItemCheckClick(ItemAdapterClickEventArgs args) => Item_CheckClick?.Invoke(this, args);
        void OnItemSelectClick(ItemAdapterClickEventArgs args) => Item_SelectClick?.Invoke(this, args);

    }

    public class ItemViewHolder : RecyclerView.ViewHolder
    {
        public TextView textView { get; set; }
        public TextView chekedText { get; set; }
        public CheckBox check { get; set; }
        public ImageButton selectItem { get; set; }


        public ItemViewHolder(View itemView, Action<ItemAdapterClickEventArgs> clickListener,
                            Action<ItemAdapterClickEventArgs> longClickListener, Action<ItemAdapterClickEventArgs> nameClickListener, Action<ItemAdapterClickEventArgs> checkClickListener, Action<ItemAdapterClickEventArgs> selectClickListener) : base(itemView)
        {
            textView = (TextView)itemView.FindViewById(Resource.Id.textView);
            check = (CheckBox)itemView.FindViewById(Resource.Id.check);
            chekedText = (TextView)itemView.FindViewById(Resource.Id.checktext);
            selectItem = (ImageButton)itemView.FindViewById(Resource.Id.select_arrow);
            textView.Click += (sender, e) => nameClickListener(new ItemAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            check.Click += (sender, e) => checkClickListener(new ItemAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            selectItem.Click += (sender, e) => selectClickListener(new ItemAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.Click += (sender, e) => clickListener(new ItemAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ItemAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class ItemAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}