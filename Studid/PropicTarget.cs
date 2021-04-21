using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Firebase.Auth;
using Google.Android.Material.AppBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Studid
{
    class PropicTarget : CustomTarget
    {
        private IMenuItem item;
        public PropicTarget(IMenuItem item)
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

        public static void setProPic(Context context, MaterialToolbar topAppBar)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;

            if (user != null)
            {
                Glide.With(context)
                        .AsBitmap()
                        .Load(user.PhotoUrl)
                        .CenterCrop()
                        .CircleCrop()
                        .Into(new PropicTarget(topAppBar.Menu.GetItem(0)));
            }
            else
            {
                topAppBar.Menu.GetItem(0).SetIcon(Resource.Drawable.ic_account_circle_light);
            }
        }
    }
}