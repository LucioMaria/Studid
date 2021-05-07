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
using Google.Android.Material.Dialog;

namespace Studid
{
    [Activity(Label = "ItemChooserActivity")]
    public class ItemChooserActivity : AppCompatActivity
    {
        private MaterialToolbar topAppBar;
        private BottomNavigationView navigation;
        private FragmentFlashcards fragmentFlashcards = new FragmentFlashcards();
        private FragmentRecordings fragmentRecordings = new FragmentRecordings();
        private FragmentCmaps fragmentCmaps = new FragmentCmaps();
        private FragmentExercises fragmentExercise = new FragmentExercises();

        private ImageButton takeRec, takePic;
        private String examName = "", examId;
        private Date examDate;
        private int examCfu;
        private String fileType = "application/pdf";

        private Fragment selectedFragment = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_item_chooser);

            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                examName = extras.GetString("exam_name");
                examId = extras.GetString("exam_id");
                examCfu = extras.GetInt("exam_cfu");
                examDate = new Date(extras.GetString("exam_date"));
                Bundle bundle = new Bundle();
                bundle.PutString("exam_id", examId);
                fragmentFlashcards.Arguments = bundle;
                fragmentRecordings.Arguments = bundle;
                fragmentCmaps.Arguments = bundle;
                fragmentExercise.Arguments = bundle;
                selectedFragment = fragmentFlashcards;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.item_fragment_container, fragmentFlashcards).Commit();
            }
            takeRec = (ImageButton)FindViewById(Resource.Id.record_button_m);
            takePic = (ImageButton)FindViewById(Resource.Id.picture_button_m);

            topAppBar = (MaterialToolbar)FindViewById(Resource.Id.topAppBar);
            SetSupportActionBar(topAppBar);
            SupportActionBar.Title = examName;                   
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true); 
            ImageButton addbtn = (ImageButton)FindViewById(Resource.Id.add_button_i);
            addbtn.Click += Addbtn_Click;
            takeRec = (ImageButton)FindViewById(Resource.Id.record_button_m);
            takeRec.Click += TakeRec_Click;
            takePic = (ImageButton)FindViewById(Resource.Id.picture_button_m);
            takePic.Click += TakePic_Click;
            MaterialTextView examDetailsTv = (MaterialTextView)FindViewById(Resource.Id.exam_detail_tv);
            DateFormat format = new SimpleDateFormat("dd/MMM/yy", Locale.Italy);
            examDetailsTv.Text = "Date: " + format.Format(examDate) + "\nCfu: " + examCfu;
            navigation = (BottomNavigationView)FindViewById(Resource.Id.bottom_navigation_i);
            navigation.NavigationItemSelected += Navigation_NavigationItemSelected;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_app_bar, menu);
            PropicTarget.setProPic(this, topAppBar);
            return base.OnCreateOptionsMenu(menu);
        }
        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (isOnline(this))
            {
                AddItemDialog addItemDialog = AddItemDialog.newInstance(fileType);
                addItemDialog.SetTargetFragment(selectedFragment, 1);
                addItemDialog.Show(SupportFragmentManager, "add_dialog");
            }
        }

        private void TakePic_Click(object sender, EventArgs e)
        {
            if (isOnline(this))
            {
                TakePicDialog takePicDialog = TakePicDialog.newInstance(examId);
                takePicDialog.SetTargetFragment(selectedFragment, 1);
                takePicDialog.Show(SupportFragmentManager, "take_pic");
            }
            else
            {
                AndroidX.AppCompat.App.AlertDialog.Builder alert = new MaterialAlertDialogBuilder(this);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
            }
        }

        private void TakeRec_Click(object sender, EventArgs e)
        {
            if (isOnline(this))
            {
                TakeRecDialog takeRecDialog = TakeRecDialog.newInstance(examId);
                takeRecDialog.SetTargetFragment(selectedFragment, 1);
                takeRecDialog.Show(SupportFragmentManager, "take_rec");
            }
            else
            {
                AndroidX.AppCompat.App.AlertDialog.Builder alert = new MaterialAlertDialogBuilder(this);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                if (FirebaseAuth.Instance.CurrentUser == null)
                {
                   Finish();
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
        private void Navigation_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)            
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.botmenu_recordings:
                    selectedFragment = fragmentRecordings;
                    takeRec.Visibility= ViewStates.Visible;
                    takePic.Visibility= ViewStates.Gone;
                    fileType = "audio/*";
                    break;
                case Resource.Id.botmenu_cmaps:
                    selectedFragment = fragmentCmaps;
                    takeRec.Visibility = ViewStates.Gone;
                    takePic.Visibility = ViewStates.Visible;
                    fileType = "image/*";
                    break;
                case Resource.Id.botmenu_exercise:
                    selectedFragment = fragmentExercise;
                    takeRec.Visibility = ViewStates.Gone;
                    takePic.Visibility = ViewStates.Gone;
                    fileType = "application/pdf";
                    break;
                default:
                    selectedFragment = fragmentFlashcards;
                    takeRec.Visibility = ViewStates.Gone;
                    takePic.Visibility = ViewStates.Gone;
                    fileType = "application/pdf";
                    break;
            }
            if (selectedFragment != null)
            {
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.item_fragment_container, selectedFragment).Commit();
            }
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if  (item.ItemId==Resource.Id.action_login)
            {
                Intent intent= new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
                return true;
            }
            else if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }

}