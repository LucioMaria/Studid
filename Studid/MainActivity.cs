using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Android.Material.BottomNavigation;
using Studid.Fragments;
using Java.Util;
using Google.Android.Material.AppBar;
using Studid.Dialogs;
using Google.Android.Material.TextView;
using Java.Text;
using Android.Net;
using AndroidX.Fragment.App;
using Fragment = AndroidX.Fragment.App.Fragment;
using Firebase.Auth;
using Bumptech.Glide;
using Android.Graphics;
using Android.Graphics.Drawables;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;

namespace Studid
{
    [Activity(Label = "MainActivity")]
    class MainActivity : AppCompatActivity
    {
        private BottomNavigationView navigation;
        private FragmentFlashcards fragmentFlashcards = new FragmentFlashcards();
        private FragmentRecordings fragmentRecordings = new FragmentRecordings();
        private FragmentCmaps fragmentCmaps = new FragmentCmaps();
        private FragmentExercises fragmentExercise = new FragmentExercises();
        private String examName = "", examId;
        Date examDate;
        private int examCfu;
        String fileType = "application/pdf";

        Fragment selectedFragment = null;
        protected void onCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                examName = extras.GetString("exam_name");
                examId = extras.GetString("exam_id");
                examCfu = extras.GetInt("exam_cfu");
                examDate = new Date(extras.GetLong("exam_date"));
                Bundle bundle = new Bundle();
                bundle.PutString("exam_name", examName);
                bundle.PutString("exam_id", examId);
                fragmentFlashcards.Arguments = bundle;
                fragmentRecordings.Arguments = bundle;
                fragmentCmaps.Arguments = bundle;
                fragmentExercise.Arguments = bundle;
                selectedFragment = fragmentFlashcards;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container, fragmentFlashcards).Commit();
            }
            MaterialToolbar topAppBar = (MaterialToolbar)FindViewById(Resource.Id.topAppBar);
            SetSupportActionBar(topAppBar);
            PropicTarget.setProPic(this, topAppBar);
            ActionBar.Title = examName;                   // appbar.Title lo prende
            ActionBar.SetDisplayHomeAsUpEnabled(true);    // appbar.setDisplayHomeAsUpEnabled non lo prende
            ImageButton addbtn = (ImageButton)FindViewById(Resource.Id.add_button_m);
            addbtn.Click += Addbtn_Click;
            MaterialTextView examDetailsTv = (MaterialTextView)FindViewById(Resource.Id.exam_detail_tv);
            DateFormat format = new SimpleDateFormat("dd/MMM/yy", Locale.Italy);
            examDetailsTv.Text = "Date: " + format.Format(examDate) + "\nCfu: " + examCfu;
            //handling the navigation between fragments
            navigation = (BottomNavigationView)FindViewById(Resource.Id.bottom_navigation);
            navigation.SetOnNavigationItemSelectedListener((BottomNavigationView.IOnNavigationItemSelectedListener)this);
            // OnNavigationItemSelected((IMenuItem)this); se si lascia questo si mette OnNavigationItemSelcted nell'OnCreate
        }   //chiusura OnCreate
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_app_bar, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (isOnline(Application.Context))
            {
                AddItemDialog addItemDialog = AddItemDialog.newInstance(fileType);
                addItemDialog.SetTargetFragment(selectedFragment, 1);
                addItemDialog.Show(SupportFragmentManager, "add_dialog");
            }
            else
            {
                Toast.MakeText(Application.Context, "you need to be online", ToastLength.Short).Show();
            }
        }

        public void onWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                if (FirebaseAuth.Instance.CurrentUser == null)
                {
                    this.Finish();
                }
            }
        }
        private bool isOnline(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            //should check null because in airplane mode it will be null
            return (netInfo != null && netInfo.IsConnected);
        }
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.botmenu_recordings:
                    selectedFragment = fragmentRecordings;
                    fileType = "audio/*";
                    break;
                case Resource.Id.botmenu_cmaps:
                    selectedFragment = fragmentCmaps;
                    fileType = "image/*";
                    break;
                case Resource.Id.botmenu_exercise:
                    selectedFragment = fragmentExercise;
                    fileType = "application/pdf";
                    break;
                default:
                    selectedFragment = fragmentFlashcards;
                    fileType = "application/pdf";
                    break;
            }
            if (selectedFragment != null)
            {
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragment_container, selectedFragment).Commit();
            }
            return true;
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
    }
}
