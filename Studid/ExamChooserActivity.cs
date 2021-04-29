using Android.Util;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using System;
using Studid.Models;
using System.Collections.Generic;
using Studid.Adapter;
using Studid.Fragments;
using Firebase.Firestore;
using Android.Gms.Tasks;
using Firebase;
using Java.Lang;
using Xamarin.Forms;
using Android.Views;
using Google.Android.Material.AppBar;
using System.Threading.Tasks;
using Firebase.Database;
using System.IO;
using Plugin.CloudFirestore;
using Google.Android.Material.TextField;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Java.Util;
using Studid.Dialogs;
using Android.Gms.Auth.Api.SignIn;
using Android.Content;
using Bumptech.Glide.Request.Target;
using Android.Graphics.Drawables;
using Bumptech.Glide.Request.Transition;
using Bumptech.Glide;
using Firebase.Auth;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Android.Net;
using Plugin.FirebaseAuth;
using Android.Graphics;
using AndroidX.Core.Content;

namespace Studid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class ExamChooserActivity : AppCompatActivity
    {
        private ImageView imageView;
        ImageView addButton;
        RecyclerView rv;
        ExamAdapter adapter;
        MaterialToolbar topAppBar;
        private GoogleSignInClient mGoogleSignInClient;
        AddExamDialog addExamDialog;
        private bool isLoginAlerted = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_exam_chooser);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            rv = (RecyclerView)FindViewById(Resource.Id.recicler_view_exams);
            imageView = (ImageView)FindViewById(Resource.Id.empty_recycler_image);
            addButton = (ImageView)FindViewById(Resource.Id.add_button_exam);
            topAppBar = FindViewById<MaterialToolbar>(Resource.Id.topAppBar);
            SetSupportActionBar(topAppBar);
            addButton.Click += AddButton_Click;
            SetupRecyclerView();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_app_bar, menu);
            PropicTarget.setProPic(this,topAppBar);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_login)
            {
                LoginDialog loginDialog = new LoginDialog();
                var transaction = SupportFragmentManager.BeginTransaction();
                loginDialog.Show(transaction, "login_dialog");
                return base.OnOptionsItemSelected(item);
            }
            return false;
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            if (FirebaseAuth.Instance.CurrentUser == null)
            {
                alertLogin();
            }
            else {
                addExamDialog = new AddExamDialog();
                var transaction = SupportFragmentManager.BeginTransaction();
                addExamDialog.Show(transaction, "add exam");
            }
        }
        private void SetupRecyclerView()
        {
            rv.SetLayoutManager(new LinearLayoutManager(this));
            adapter = new ExamAdapter(this.ApplicationContext, rv);
            adapter.ExamSelectClick += ExamSelectClick;
            adapter.ExamNameClick += ExamNameTVClick;
            adapter.ExamDateClick += ExamDateClick;
            adapter.ExamCfuClick += ExamCfuClick;
            rv.SetAdapter(adapter);
            ItemTouchHelper.SimpleCallback callback = new ExamTouchCallback(this);
            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(callback);
            itemTouchHelper.AttachToRecyclerView(rv);
        }
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus); 
            if (hasFocus)
            {
                PropicTarget.setProPic(this, topAppBar);
                FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
                if (user != null)
                {
                    isLoginAlerted = false;
                    CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(user.Uid)
                        .Collection("Exams")
                        .AddSnapshotListener((snapshot, error) =>
                        {
                            if (snapshot != null)
                            {
                                foreach (var documentChange in snapshot.DocumentChanges)
                                {
                                    var newExam = documentChange.Document.ToObject<ExamModel>();
                                    var i = adapter.ExamList.FindIndex(x => x.examId.Equals(newExam.examId));
                                    switch (documentChange.Type)
                                    {                                      
                                        case DocumentChangeType.Added:
                                            if (i == -1)
                                            {
                                                adapter.ExamList.Add(newExam);
                                                adapter.NotifyItemInserted(adapter.ItemCount - 1);
                                            }
                                            else
                                                adapter.NotifyDataSetChanged();
                                            break;
                                        case DocumentChangeType.Removed:
                                            adapter.ExamList.Remove(newExam);
                                            adapter.NotifyItemRemoved(i);
                                            break;
                                        case DocumentChangeType.Modified:
                                            adapter.ExamList[i] = newExam;
                                            adapter.NotifyItemChanged(i);
                                            break;
                                    }
                                }
                                if (adapter.ItemCount == 0)
                                {
                                    imageView.Visibility = ViewStates.Visible;
                                    rv.Visibility = ViewStates.Invisible;
                                }
                                else
                                {
                                    imageView.Visibility = ViewStates.Invisible;
                                    rv.Visibility = ViewStates.Visible;
                                }
                            }
                        });
                }
                else 
                { 
                    if (!isLoginAlerted)
                    {
                        isLoginAlerted = true;
                        alertLogin();
                    }
                    imageView.Visibility = ViewStates.Visible;
                    rv.Visibility = ViewStates.Invisible;
                }
            }
        }
        private void ExamSelectClick(object sender, ExamAdapterClickEventArgs e)
        {
            if (FirebaseAuth.Instance.CurrentUser != null)
            {
                Intent intentExamName = new Intent(this,typeof(ItemChooserActivity));
                intentExamName.PutExtra("exam_name", adapter.ExamList[e.Position].examName);
                intentExamName.PutExtra("exam_id", adapter.ExamList[e.Position].examId);
                intentExamName.PutExtra("exam_date", adapter.ExamList[e.Position].date.ToDateTime().ToLongDateString());
                intentExamName.PutExtra("exam_cfu", adapter.ExamList[e.Position].cfu);
                StartActivity(intentExamName);
                Log.Info("Onclick", "Click");
            }
        }
        private void ExamCfuClick(object sender, ExamAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examname_clicked = adapter.ExamList[e.Position];
                string examId = examname_clicked.examId;
                Dialog cfuDialog = new Dialog(this);
                cfuDialog.SetContentView(Resource.Layout.dialog_exam_cfu);
                NumberPicker numberPicker = (NumberPicker)cfuDialog.FindViewById(Resource.Id.cfu_numpic);
                numberPicker.MinValue = 0;
                numberPicker.MaxValue = 25;
                numberPicker.Value = adapter.ExamList[e.Position].cfu;
                Android.Widget.Button okButton = (Android.Widget.Button)cfuDialog.FindViewById(Resource.Id.name_ok);
                Android.Widget.Button cancelButton = (Android.Widget.Button)cfuDialog.FindViewById(Resource.Id.cfu_cancel);
                cancelButton.Click += delegate
                {
                    cfuDialog.Dismiss();
                };
                okButton.Click += async delegate
                {
                    await CrossCloudFirestore.Current
                              .Instance
                              .Collection("Users")
                              .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                              .Collection("Exams")
                              .Document(examId)
                              .UpdateAsync("cfu", numberPicker.Value);
                    cfuDialog.Dismiss();
                };
                cfuDialog.Show();
            }
        }
        private void ExamDateClick(object sender, ExamAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examdate_clicked = adapter.ExamList[e.Position];
                string examId = examdate_clicked.examId;
                Plugin.CloudFirestore.Timestamp examdate = examdate_clicked.date;
                Dialog calendardialog = new Dialog(this);
                Calendar calendar = Calendar.GetInstance(Locale.Italy);
                calendardialog.SetContentView(Resource.Layout.dialog_exam_date);
                calendardialog.Show();
                CalendarView calendarview = (CalendarView)calendardialog.FindViewById(Resource.Id.calendarView);
                calendarview.MinDate = calendar.TimeInMillis;
                Android.Widget.Button cancelButton = (Android.Widget.Button)calendardialog.FindViewById(Resource.Id.date_cancel);
                cancelButton.Click += delegate
                {
                    calendardialog.Dismiss();
                };
                calendarview.DateChange += (s, e) =>
                {
                    CalendarView view = e.View;
                    int day = e.DayOfMonth;
                    int month = e.Month + 1;
                    int year = e.Year;
                    onselecteddatechange(view, year, month, day);
                };
                async void onselecteddatechange(CalendarView view, int year, int month, int dayofmonth)
                {
                    Plugin.CloudFirestore.Timestamp timestamp = new Plugin.CloudFirestore.Timestamp(new DateTime(year, month, dayofmonth));
                    await CrossCloudFirestore.Current
                             .Instance
                             .Collection("Users")
                             .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                             .Collection("Exams")
                             .Document(examId)
                             .UpdateAsync("date", timestamp);
                    calendardialog.Dismiss();
                }
            }
        }
        private void ExamNameTVClick(object sender, ExamAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examname_clicked = adapter.ExamList[e.Position];
                string examId = examname_clicked.examId;
                Dialog nameDialog = new Dialog(this);
                nameDialog.SetContentView(Resource.Layout.dialog_name_update);
                EditText editText = (EditText)nameDialog.FindViewById(Resource.Id.dialog_name_editText);
                editText.Text = examname_clicked.examName;
                TextInputLayout textInputLayout = (TextInputLayout)nameDialog.FindViewById(Resource.Id.dialog_name_input_layout);
                nameDialog.Show();
                Android.Widget.Button okButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_ok);
                Android.Widget.Button cancelButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_cancel);
                okButton.Click += async delegate
                {
                    string examNameNew = editText.Text.ToUpper().Trim();
                    if (examNameNew.Equals(""))
                    {
                        textInputLayout.Error = Resources.GetString(Resource.String.empty_name_field);
                        textInputLayout.RequestFocus();
                    }
                    else if (examNameNew.Length > 15)
                    {
                        textInputLayout.Error = Resources.GetString(Resource.String.overflow_name_field);
                        textInputLayout.RequestFocus();
                    }
                    else if (IsSameName(examNameNew))
                    {
                        textInputLayout.Error = Resources.GetString(Resource.String.used_name);
                        textInputLayout.RequestFocus();
                    }
                    else
                    {
                        await CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                        .Collection("Exams")
                        .Document(examId)
                        .UpdateAsync("examName", examNameNew);
                        nameDialog.Dismiss();
                    }
                };
                cancelButton.Click += delegate
                {
                    nameDialog.Dismiss();
                };
            }           
        }
        bool IsSameName(string examNewName)
        {
            foreach (ExamModel exam in adapter.ExamList)
            {
                if (exam.examName.Equals(examNewName))
                    return true;
            }
            return false;
        }
        private void alertLogin()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(Resource.String.login_title)
                    .SetMessage(Resource.String.alert_login);
            alert.SetPositiveButton("Ok", (senderAlert, args)=>
            {
                LoginDialog loginDialog = new LoginDialog();
                loginDialog.Show(SupportFragmentManager.BeginTransaction(), "login_dialog");
                alert.Dispose();
            });
            alert.Show();           
        }
    }
}