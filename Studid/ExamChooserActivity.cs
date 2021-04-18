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

namespace Studid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class ExamChooserActivity : AppCompatActivity
    {
        private ImageView imageView;
        ImageView addButton;
        RecyclerView rv;
        ExamAdapter adapter;
        // FirestoreDb database;
        MaterialToolbar topAppBar;
        IOnCompleteListener OnCompleteListener;
        private GoogleSignInClient mGoogleSignInClient;
        List<ExamModel> ExamList = new List<ExamModel>();
        AddExamDialog addExamFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_exam_chooser);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            // string path = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, "google-services-studid.json");
            // System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            Forms.SetFlags("SwipeView_Experimental");
            rv = (RecyclerView)FindViewById(Resource.Id.recicler_view_exams);
            imageView = (ImageView)FindViewById(Resource.Id.empty_recycler_image);
            addButton = (ImageView)FindViewById(Resource.Id.add_button_exam);
            // addButton = (ImageView)FindViewById(Resource.Id.addButton)
            topAppBar = FindViewById<MaterialToolbar>(Resource.Id.topAppBar);
            SetSupportActionBar(topAppBar);
            // topAppBar.Click += TopAppBar_Click;
            addButton.Click += AddButton_Click;
            SetupRecyclerView();
            FetchandListen();
            // database = FirestoreDb.Create("fir-project-16446");
            // FirebaseApp.InitializeApp(this);
        }

        /* Non serve
         private void TopAppBar_Click(object sender, EventArgs e)
        {
            addExamFragment = new AddExamFragment();
            var transaction = SupportFragmentManager.BeginTransaction();
            addExamFragment.Show(transaction, "add exam");
        }
        */


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_app_bar, menu);
            setProPic();
            return base.OnCreateOptionsMenu(menu);
        }

        //per lanciare il login
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
            addExamFragment = new AddExamDialog();
            var transaction = SupportFragmentManager.BeginTransaction();
            addExamFragment.Show(transaction, "add exam");
        }

        /* public FirestoreDb GetDatabase()
         {
             return FirestoreDb.Create("fir-project-16446");
         } */

        void FetchandListen()
        {
            /* Google.Cloud.Firestore.CollectionReference exams = database.Collection("exams");
            // Google.Cloud.Firestore.Query query = database.Collection("exams");

            FirestoreChangeListener listener = exams.Listen(snapshot =>
            {
                foreach (Google.Cloud.Firestore.DocumentSnapshot documentSnapshot in snapshot.Documents)
                {
                    ExamList.Add(documentSnapshot.ConvertTo<ExamModel>());
                }
            }); */
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user != null)
            {
                flag = false;
                CrossCloudFirestore.Current
                   .Instance
                   .Collection("Users")
                   .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                   .Collection("exams")
                   .AddSnapshotListener((snapshot, error) =>
                   {
                       if (snapshot != null)
                       {
                           foreach (var documentChange in snapshot.DocumentChanges)
                           {
                               switch (documentChange.Type)
                               {
                                   case DocumentChangeType.Added:
                                       ExamList.Add(documentChange.Document.ToObject<ExamModel>());
                                       break;
                                   case DocumentChangeType.Removed:
                                       ExamList.Remove(documentChange.Document.ToObject<ExamModel>());
                                       break;
                                   case DocumentChangeType.Modified:
                                       var em = documentChange.Document.ToObject<ExamModel>();
                                       var index = ExamList.FindIndex(x => x.examName.Equals(em.examName));
                                       ExamList.Remove(em);
                                       ExamList.Insert(index, em);
                                       break;
                               }
                               ExamList.Sort();
                               adapter.NotifyDataSetChanged();
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
                       }
                   });
            } 
            else if (!flag)
            {
                flag = true;
                alertLogin();
            }
            else
            {
                imageView.Visibility = ViewStates.Visible;
                rv.Visibility = ViewStates.Invisible;
            }
        }

        private void SetupRecyclerView()
        {
            rv.SetLayoutManager(new LinearLayoutManager(this));
            adapter = new ExamAdapter(this.ApplicationContext, rv, ExamList);
            adapter.ItemClick += Adapter_ItemClick;
            adapter.Exam_NameClick += ExamNameTV_Click;
            adapter.Exam_DateClick += Exam_DateClick;
            adapter.Exam_CfuClick += Exam_CfuClick;
            rv.SetAdapter(adapter);
            //ItemTouchHelper.Callback callback = new MyExamTouchHelper();
            //ItemTouchHelper itemTouchHelper = new ItemTouchHelper(callback);
            //itemTouchHelper.AttachToRecyclerView(rv);
        }

        

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus); 
            if (hasFocus)
            {
                // IMenuItem item = topAppBar.Menu.GetItem(0);
                setProPic();
                FetchandListen();
                
            }
        }

        private void Adapter_ItemClick(object sender, ExamAdapterClickEventArgs e)
        {
            // da cambiare con Plugin Cross Current
            if (mGoogleSignInClient != null)
            {
                Intent intentExamName = new Intent(this, typeof(NavigationActivity));
                intentExamName.PutExtra("exam_name", this.ExamList[e.Position].examName);
                intentExamName.PutExtra("exam_id", this.ExamList[e.Position].examId);
                intentExamName.PutExtra("exam_date", this.ExamList[e.Position].date.ToString());
                intentExamName.PutExtra("exam_cfu", this.ExamList[e.Position].cfu);
                StartActivity(intentExamName);
                Log.Info("Onclick", "Click");
            }
        }

        private void Exam_CfuClick(object sender, ExamAdapterClickEventArgs e)
        {
            
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examname_clicked = this.ExamList[e.Position];
                string examname = examname_clicked.examName;
                Dialog cfuDialog = new Dialog(this);
                cfuDialog.SetContentView(Resource.Layout.dialog_exam_cfu);
                NumberPicker numberPicker = (NumberPicker)cfuDialog.FindViewById(Resource.Id.cfu_numpic);
                numberPicker.MinValue = 0;
                numberPicker.MaxValue = 25;
                numberPicker.Value = this.ExamList[e.Position].cfu;
                // numberPicker numberPickerupdate = numberPicker.Value;
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
                              .Collection("exams")
                              .Document(examname)
                              .UpdateAsync("cfu", numberPicker.Value);
                    cfuDialog.Dismiss();
                    adapter.NotifyDataSetChanged();
                };
                cfuDialog.Show();
            }
        }

        private void Exam_DateClick(object sender, ExamAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examdate_clicked = this.ExamList[e.Position];
                string examname = examdate_clicked.examName;
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
                             .Collection("exams")
                             .Document(examname)
                             .UpdateAsync("date", timestamp);
                    calendardialog.Dismiss();
                    adapter.NotifyDataSetChanged();
                }
            }
        }







        private void ExamNameTV_Click(object sender, ExamAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            if (user == null)
            {
                alertLogin();
            }
            else
            {
                ExamModel examname_clicked = this.ExamList[e.Position];
                string examname = examname_clicked.examName;
                Dialog nameDialog = new Dialog(this);
                nameDialog.SetContentView(Resource.Layout.dialog_name_update);
                EditText editText = (EditText)nameDialog.FindViewById(Resource.Id.dialog_name_editText);
                editText.Text = examname;
                /* Non riconosce più android.support.design e quindi ho messo il design che sta in google.design*/
                TextInputLayout textInputLayout = (TextInputLayout)nameDialog.FindViewById(Resource.Id.dialog_name_input_layout);
                nameDialog.Show();
                Android.Widget.Button okButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_ok);
                Android.Widget.Button cancelButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_cancel);
                okButton.Click += async delegate
                {
                    string examNameNew = editText.Text.ToUpper().Trim();
                    if (examNameNew.Equals(""))
                    {
                                    // textInputLayout.ErrorEnabled = true;
                                    textInputLayout.Error = "Please fill the name field";
                        textInputLayout.RequestFocus();
                    }
                    else if (examNameNew.Length > 15)
                    {
                        // textInputLayout.ErrorEnabled = true;
                        textInputLayout.Error = "Please enter a short name";
                        textInputLayout.RequestFocus();
                    }
                    else if (IsSameName(examNameNew))
                    {
                        // textInputLayout.ErrorEnabled = true;
                        textInputLayout.Error = "Please change the name field";
                        textInputLayout.RequestFocus();
                    }
                    else
                    {
                        var document = await CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                        .Collection("exams")
                        .Document(examname)
                        .GetAsync();
                        if (document.Exists)
                        {
                            ExamModel exammodel = document.ToObject<ExamModel>();
                            exammodel.examName = examNameNew;

                            await CrossCloudFirestore.Current
                                        .Instance
                                        .Collection("Users")
                                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                        .Collection("exams")
                                        .Document(examNameNew)
                                        .SetAsync(exammodel);

                            await CrossCloudFirestore.Current
                                        .Instance
                                        .Collection("Users")
                                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                        .Collection("exams")
                                        .Document(examname)
                                        .DeleteAsync();



                                        /*    Google.Cloud.Firestore.CollectionReference exams = database.Collection("exams");
                                              Google.Cloud.Firestore.DocumentReference examDoc = exams.Document(examname);
                                              Google.Cloud.Firestore.DocumentSnapshot snapshot = await examDoc.GetSnapshotAsync();
                                              if (snapshot.Exists)
                                              {
                                                  await exams.Document(examNameNew).SetAsync(snapshot);
                                                  await examDoc.DeleteAsync();
                                              }
                                         */
                                }
                                    nameDialog.Dismiss();
                            }
                        };

                    cancelButton.Click += delegate
                    {
                       nameDialog.Dismiss();
                    };
            }   
            bool IsSameName(string examNewName)
            {
                foreach (ExamModel exam in ExamList)
                {
                    if (exam.examName.Equals(examNewName))
                        return true;
                }
                return false;

                /* for (int j = 0; j < ExamList.Count ; j++)
                {
                    if (ExamList.examName.Equals(examNewName))
                        return true;
                } */
                // return false;  
            }     
        }

        private void setProPic()
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;

            if (user != null)
            {
                Glide.With(this)
                        .AsBitmap()
                        .Load(user.PhotoUrl)
                        .CenterCrop()
                        .CircleCrop()
                        .Into(new MyTarget(topAppBar.Menu.GetItem(0)));
            }
            else
            {
                topAppBar.Menu.GetItem(0).SetIcon(Resource.Drawable.ic_account_circle_light);
            }
        }

        private bool isOnline(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            //should check null because in airplane mode it will be null
            return (netInfo != null && netInfo.IsConnected);
        }

        private bool flag = false;
        

        private void alertLogin()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(Resource.String.login_title)
                    .SetMessage(Resource.String.alert_login)
                    .Show();
            alert.SetNeutralButton("Ok", delegate {
                alert.Dispose();
                LoginDialog loginDialog = new LoginDialog();
                var transaction = SupportFragmentManager.BeginTransaction();
                loginDialog.Show(transaction, "dialog_login");
            });
            alert.Show();
        }
    }

    class MyTarget : CustomTarget
    {
        private IMenuItem item;
        public MyTarget(IMenuItem item)
        {
            this.item = item;
        }
        public override void OnLoadCleared(Drawable p0)
        {

        }

        public override void OnResourceReady(Java.Lang.Object resource, ITransition transition)
        {
            item.SetIcon(new BitmapDrawable(resource as Bitmap));
        }
    }
}