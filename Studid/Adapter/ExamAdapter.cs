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
    class ExamAdapter : RecyclerView.Adapter
    {
    public Context context;
    public RecyclerView RecyclerView;
    public event EventHandler<ExamAdapterClickEventArgs> ItemClick;
    public event EventHandler<ExamAdapterClickEventArgs> ItemLongClick;
    public event EventHandler<ExamAdapterClickEventArgs> Exam_NameClick;
    public event EventHandler<ExamAdapterClickEventArgs> Exam_DateClick;
    public event EventHandler<ExamAdapterClickEventArgs> Exam_CfuClick;
    List<ExamModel> Items;
    public Button cfuButton;
    public ImageButton selectButton;
    public TextView examNameTV, examDateTV;

    /* public FirebaseFirestore GetDatabase()
    {
        var app = FirebaseApp.InitializeApp(this.context);
        FirebaseFirestore database;
        if (app == null)
        {
            var options = new FirebaseOptions.Builder()
            .SetProjectId("fir-project-16446")
            .SetApplicationId("fir-project-16446")
            .SetApiKey("AIzaSyB4tFXBV6P6AiHCZmsjNNEWlF_9eXncoQg")
            .SetDatabaseUrl("https://fir-project-16446.firebaseio.com")
            .SetStorageBucket("fir-project-16446.appspot.com")
            .Build();

            app = FirebaseApp.InitializeApp(this.context, options);
            database = FirebaseFirestore.GetInstance(app);
        }
        else
        {
            database = FirebaseFirestore.GetInstance(app);
        }
        return database;
    } */



    public ExamAdapter(Context context, RecyclerView recyclerView, List<ExamModel> Exams)
    {
        this.context = context;
        RecyclerView = recyclerView;
        Items = Exams;
    }

    // Create new views (invoked by the layout manager)
    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {

        //Setup your layout here
        View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.examrow, parent, false);
        var vh = new ExamAdapterViewHolder(itemview, OnClick, OnLongClick, OnExam_NameClick, OnExam_DateClick, OnExam_CfuClick);
        return vh;
    }

    // Replace the contents of a view (invoked by the layout manager)
    public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
    {
        var exam = Items[position];
        var holder = viewHolder as ExamAdapterViewHolder;
        //holder.TextView.Text = items[position];
        holder.examNameTV.Text = exam.examName;
        DateFormat df = new SimpleDateFormat("dd/MM/yy", Locale.Italy);
        Date mydate = new Date(exam.date.ToDateTime().ToLongDateString());
        string fdate = df.Format(mydate);
        holder.examDateTV.Text = fdate;
        String cfuString = Items[position].cfu + "";
        holder.cfuButton.Text = cfuString;
        // holder.examNameTV.Click += ItemView_Click;

    }

    /* private void ItemView_Click(object sender, EventArgs e)
     {

        int position = this.recyclerView.GetChildAdapterPosition((View)sender);
        ExamModel examname_clicked = this.Items[position];
        string examname = examname_clicked.get_exam_name();
        DocumentReference docRef = GetDatabase().Collection("exams").Document(examname);
        docRef.Update("examname", "Update");
    } */

    public override int ItemCount => Items.Count;

    void OnClick(ExamAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
    void OnLongClick(ExamAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    void OnExam_NameClick(ExamAdapterClickEventArgs args) => Exam_NameClick?.Invoke(this, args);
    void OnExam_DateClick(ExamAdapterClickEventArgs args) => Exam_DateClick?.Invoke(this, args);
    void OnExam_CfuClick(ExamAdapterClickEventArgs args) => Exam_CfuClick?.Invoke(this, args);




    }

public class ExamAdapterViewHolder : RecyclerView.ViewHolder
{
    //public TextView TextView { get; set; }
    public TextView examNameTV { get; set; }
    public TextView examDateTV { get; set; }
    public Button cfuButton { get; set; }
    public ImageButton selectButton { get; set; }
    
    



    public ExamAdapterViewHolder(View itemView, Action<ExamAdapterClickEventArgs> clickListener,
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
        
        itemView.Click += (sender, e) => clickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        itemView.LongClick += (sender, e) => longClickListener(new ExamAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        

    }
        

        /*  private void ExamNameTV_Click(object sender, ExamAdapterClickEventArgs e)
          {

              ExamModel examname_clicked = this.Items[e.Position];
              string examname = examname_clicked.get_exam_name();
              DocumentReference docRef = GetDatabase().Collection("exams").Document(examname);
              docRef.Update("examname", "Update");
          } */
    }

    public class ExamAdapterClickEventArgs : EventArgs
    {
    public View View { get; set; }
    public int Position { get; set; }
    }
}