using System;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Content;
using Java.Text;
using Java.Util;
using AndroidX.RecyclerView.Widget;
using Studid.Models;
using AndroidX.CardView.Widget;
using Android.Graphics;
using AndroidX.Core.Content;
using Android.Util;

namespace Studid.Adapter
{
    class ExamAdapter : RecyclerView.Adapter
    {
        public Context context;
        public RecyclerView RecyclerView;
        public event EventHandler<ExamAdapterClickEventArgs> ExamSelectClick;
        public event EventHandler<ExamAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<ExamAdapterClickEventArgs> ExamNameClick;
        public event EventHandler<ExamAdapterClickEventArgs> ExamDateClick;
        public event EventHandler<ExamAdapterClickEventArgs> ExamCfuClick;
        public List<ExamModel> ExamList;
        public ExamAdapter(Context context, RecyclerView recyclerView, List<ExamModel> Exams)
        {
            this.context = context;
            RecyclerView = recyclerView;
            ExamList = Exams;
        }
        public ExamAdapter(Context context, RecyclerView recyclerView)
        {
            this.context = context;
            RecyclerView = recyclerView;
            ExamList = new List<ExamModel>();
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.exam_row, parent, false);
            var vh = new ExamViewHolder(itemview, OnExamSelectClick, OnLongClick, OnExamNameClick, OnExamDateClick, OnExamCfuClick);
            return vh;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var exam = ExamList[position];
            var holder = viewHolder as ExamViewHolder;
            holder.examId = exam.examId;
            holder.examNameTV.Text = exam.examName;
            DateFormat df = new SimpleDateFormat("dd/MM/yy", Locale.Italy);
            Date examDate = new Date(exam.date.ToDateTime().ToLongDateString());

            holder.examDateTV.Text = df.Format(examDate);
            var result = exam.date.ToDateTime()-DateTime.Today;
            if (result.TotalDays >= 0 && result.Days <= 2)
            {
                holder.examDateTV.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.colorAccent)));
            }
            else if (result.Days >= 15)
            {
                holder.examDateTV.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.colorTextPrimary)));
            }
            else if (result.Days > 2)
            {
                holder.examDateTV.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.colorPrimaryDark)));
            }
            else if (result.TotalDays < 0)
            {
                holder.examDateTV.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.colorTextSecondary)));
            }
            holder.cfuButton.Text = ExamList[position].cfu + "";
        }

        public override int ItemCount => ExamList.Count;

        void OnExamSelectClick(ExamAdapterClickEventArgs args) => ExamSelectClick?.Invoke(this, args);
        void OnLongClick(ExamAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void OnExamNameClick(ExamAdapterClickEventArgs args) => ExamNameClick?.Invoke(this, args);
        void OnExamDateClick(ExamAdapterClickEventArgs args) => ExamDateClick?.Invoke(this, args);
        void OnExamCfuClick(ExamAdapterClickEventArgs args) => ExamCfuClick?.Invoke(this, args);
        }

    public class ExamViewHolder : RecyclerView.ViewHolder
    {
        public TextView examNameTV { get; set; }
        public TextView examDateTV { get; set; }
        public Button cfuButton { get; set; }
        public ImageButton selectButton { get; set; }

        public string examId;

    public ExamViewHolder(View itemView, Action<ExamAdapterClickEventArgs> selectClickListener,
                            Action<ExamAdapterClickEventArgs> longClickListener, Action<ExamAdapterClickEventArgs> nameClickListener, Action<ExamAdapterClickEventArgs> dateClickListener, Action<ExamAdapterClickEventArgs> cfuClickListener) : base(itemView)
        {
            //TextView = v;
            int position = AdapterPosition;
            examNameTV = (TextView)itemView.FindViewById(Resource.Id.exam_name_tv);
            examDateTV = (TextView)itemView.FindViewById(Resource.Id.exam_date_tv);
            cfuButton = (Button)itemView.FindViewById(Resource.Id.exam_cfu_button);
            selectButton = (ImageButton)itemView.FindViewById(Resource.Id.select_arrow);
            examNameTV.Click += (sender, e) => nameClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            examDateTV.Click += (sender, e) => dateClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            cfuButton.Click += (sender, e) => cfuClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            selectButton.Click += (sender, e) => selectClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class ExamAdapterClickEventArgs : EventArgs
    {
    public View View { get; set; }
    public int Position { get; set; }
    }
}