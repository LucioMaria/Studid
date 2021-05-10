using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Plugin.FirebaseAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;
using Bumptech.Glide;
using Android.Net;
using AndroidX.AppCompat.App;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Studid.Dialogs
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : AppCompatActivity
    {
        private SignInButton signInButton;
        private Button logOutButton;
        private ImageButton closeBtn;
        private GoogleSignInClient mGoogleSignInClient;
        private TextView usernameTv;
        private static int RC_SIGN_IN = 300;
        private ImageView proPicView;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);
            signInButton = (SignInButton)FindViewById(Resource.Id.sign_in_button);
            logOutButton = (Button)FindViewById(Resource.Id.logout_button);
            usernameTv = (TextView)FindViewById(Resource.Id.username);
            proPicView = (ImageView)FindViewById(Resource.Id.profile_picture);

            closeBtn = (ImageButton)FindViewById(Resource.Id.dismiss);
            closeBtn.Click += LoginDialogDismiss_Click;
            logOutButton.Click += LogOutButton_Click;
            GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
               .RequestIdToken(Resources.GetString(Resource.String.server_client_id))
               .RequestEmail()
               .RequestProfile()
               .Build();
            mGoogleSignInClient = GoogleSignIn.GetClient(this, gso);
            updateUI();
            signInButton.Click += SignInButton_Click;
        }
        private void SignInButton_Click(object sender, EventArgs e)
        {
            if (isOnline(this))
                StartActivityForResult(mGoogleSignInClient.SignInIntent, RC_SIGN_IN);
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
            }
        }

        private void LogOutButton_Click(object sender, EventArgs e)
        {
            mGoogleSignInClient.SignOut();
            CrossFirebaseAuth.Current.Instance.SignOut();
            updateUI();
        }

        private void LoginDialogDismiss_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(ExamChooserActivity));
            StartActivity(intent);
        }
        private void updateUI()
        {
            var account = CrossFirebaseAuth.Current.Instance.CurrentUser;
            if (account != null)
            {
                closeBtn.Visibility = ViewStates.Visible;
                signInButton.Visibility = ViewStates.Invisible;
                logOutButton.Visibility = ViewStates.Visible;
                usernameTv.Text = account.DisplayName;
                Glide.With(this)
                        .Load(account.PhotoUrl.AbsoluteUri)
                        .Placeholder(Resource.Drawable.ic_account_circle_light)
                        .CenterCrop()
                        //.CircleCrop()
                        .Into(proPicView);
            }
            else
            {
                closeBtn.Visibility = ViewStates.Invisible;
                signInButton.Visibility = ViewStates.Visible;
                logOutButton.Visibility = ViewStates.Invisible;
                usernameTv.Text = Resources.GetString(Resource.String.alert_login);
                proPicView.SetImageResource(Resource.Drawable.ic_account_circle_light);
            }
        }
        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == RC_SIGN_IN)
            {
                var result = GoogleSignIn.GetSignedInAccountFromIntent(data);
                if (result.IsSuccessful)
                {
                    var account = result.Result.JavaCast<GoogleSignInAccount>();
                    //google sign in was successful, authenticate with firebase
                    Log.Info("googlesignin", "firebaseauthwithgoogle:" + account.Id);
                    var credential = CrossFirebaseAuth.Current.GoogleAuthProvider.GetCredential(account.IdToken, null);
                    var firebaseresult = await CrossFirebaseAuth.Current.Instance.SignInWithCredentialAsync(credential);
                    updateUI();
                    Toast.MakeText(ApplicationContext, Resource.String.welcome_message, ToastLength.Short).Show();
                    Intent intent = new Intent(this, typeof(ExamChooserActivity));
                    StartActivity(intent);
                }
                else
                {  //Google Sign In failed, update UI appropriately
                    Log.Info("googlesignin", "fail");
                    updateUI();
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
    }
}