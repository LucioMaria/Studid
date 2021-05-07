using System;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Java.Text;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Studid.Models;
using Android.Content;
using Bumptech.Glide;

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
            holder.itemId = item.itemId;
            holder.textView.Text = item.itemName;
            holder.check.Checked = item.isMemorized;
            if (item.isPlaying)
            {
                holder.selectItem.SetImageResource(Resource.Drawable.ic_baseline_stop_24);
            }
            else
            {
                holder.selectItem.SetImageResource(Resource.Drawable.ic_baseline_arrow_forward_ios_24);
            }
            if (holder.check.Checked)
            {
                holder.check.Text = context.Resources.GetString(Resource.String.memorized);
            }
            else
            {
                holder.check.Text = "";
            }
            if (item.picUrl != null)
            {
                holder.ItemPicture.Visibility= ViewStates.Visible;
                Glide.With(context).Load(item.picUrl).Into(holder.ItemPicture);
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
        public CheckedTextView check;
        public ImageButton selectItem { get; set; }
        public ImageView ItemPicture;

        public string itemId;


        public ItemViewHolder(View itemView, Action<ItemAdapterClickEventArgs> clickListener,
                            Action<ItemAdapterClickEventArgs> longClickListener, Action<ItemAdapterClickEventArgs> nameClickListener, Action<ItemAdapterClickEventArgs> checkClickListener, Action<ItemAdapterClickEventArgs> selectClickListener) : base(itemView)
        {
            textView = (TextView)itemView.FindViewById(Resource.Id.textView);
            check = (CheckedTextView)itemView.FindViewById(Resource.Id.check);
            selectItem = (ImageButton)itemView.FindViewById(Resource.Id.select_arrow);
            ItemPicture = (ImageView)itemView.FindViewById(Resource.Id.item_pic);
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