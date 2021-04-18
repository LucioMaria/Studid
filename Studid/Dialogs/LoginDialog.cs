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

namespace Studid.Dialogs
{
    class LoginDialog : DialogFragment
{
    private SignInButton signInButton;
    private Button logOutButton;
    private GoogleSignInClient mGoogleSignInClient;
    private TextView emailtv;
    private static int RC_SIGN_IN = 300;
    private ImageView proPicView;
    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Create your fragment here
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        // Use this to return your custom view for this Fragment

        View view = inflater.Inflate(Resource.Layout.dialog_login, container, false);
        signInButton = (SignInButton)view.FindViewById(Resource.Id.sign_in_button);
        logOutButton = (Button)view.FindViewById(Resource.Id.logout_button);
        emailtv = (TextView)view.FindViewById(Resource.Id.email);
        proPicView = (ImageView)view.FindViewById(Resource.Id.profile_picture);
        view.FindViewById(Resource.Id.dismiss).Click += LoginDialogDismiss_Click;
        logOutButton.Click += LogOutButton_Click;
        GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
           .RequestIdToken("789838138525-lp4kc60n0uqnfntnk8415df52eg93fof.apps.googleusercontent.com")
           .RequestEmail()
           .RequestProfile()
           .Build();
        mGoogleSignInClient = GoogleSignIn.GetClient(this.Activity, gso);
        updateUI();
        signInButton.Click += SignInButton_Click;
        return view;
    }

    private void SignInButton_Click(object sender, EventArgs e)
    {
        StartActivityForResult(mGoogleSignInClient.SignInIntent, RC_SIGN_IN);
    }

    private void LogOutButton_Click(object sender, EventArgs e)
    {
        //GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
        //     .RequestIdToken("1044961019460-ju2v6mvgar92vk9nfee8u4pq37m091q1.apps.googleusercontent.com")
        //     .RequestEmail()
        //     .RequestProfile()
        //     .Build();
        //mGoogleSignInClient = GoogleSignIn.GetClient(this.Activity, gso);

        mGoogleSignInClient.SignOut();
        CrossFirebaseAuth.Current.Instance.SignOut();
        updateUI();
    }

    private void LoginDialogDismiss_Click(object sender, EventArgs e)
    {
        this.Dismiss();
    }
    private void updateUI()
    {
        var account = CrossFirebaseAuth.Current.Instance.CurrentUser;
        if (account != null)
        {
            signInButton.Visibility = ViewStates.Invisible;
            logOutButton.Visibility = ViewStates.Visible;
            emailtv.Text = account.DisplayName;
            Glide
                    .With(this.Context)
                    .Load(account.PhotoUrl.AbsoluteUri)
                    .Placeholder(Resource.Drawable.ic_account_circle_light)
                    .CenterCrop()
                    .CircleCrop()
                    .Into(proPicView);
        }
        else
        {
            signInButton.Visibility = ViewStates.Visible;
            logOutButton.Visibility = ViewStates.Invisible;
            emailtv.Text = "";
            proPicView.SetImageResource(Resource.Drawable.ic_account_circle_light);
        }
    }
    public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
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
                //crasha nella linea sotto
                updateUI();
            }
            else
            {  //Google Sign In failed, update UI appropriately
                Log.Info("googlesignin", "fail");
                updateUI();
            }

        }
    }
}
}