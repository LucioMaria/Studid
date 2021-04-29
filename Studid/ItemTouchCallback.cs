using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Firebase;
using Firebase.Firestore;
using Firebase.Storage;
using Studid.Adapter;
using Plugin.CloudFirestore;
using Android.Graphics;
using AndroidX.Core.Content;
using Android.Graphics.Drawables;
using static Android.Content.Res.Resources;
using Xamarin.Forms.Platform.Android;
using Firebase.Auth;
using Google.Android.Material.Dialog;
using Android.Net;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Studid
{
    public class ItemTouchCallback : ItemTouchHelper.SimpleCallback
    {

        private string storage_folder;
        private string examId;
        private Context context;

        private StorageReference storageRef;
        public ItemTouchCallback(string storage_folder, string examId, Context context ): base(0,ItemTouchHelper.Left)
        {
            storageRef = FirebaseStorage.Instance.Reference;
            this.storage_folder = storage_folder;
            this.examId = examId;
            this.context = context;
        }
        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            return MakeMovementFlags(0, ItemTouchHelper.Left);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return true;
        }
        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int swipedir)
        {
            var holder = viewHolder as ItemViewHolder;
            if (isOnline(context))
            {
                FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
                AlertDialog.Builder alertDialog = new AlertDialog.Builder(context);

                alertDialog.SetTitle(Resource.String.dialog_cancel_title);
                alertDialog.SetMessage(Resource.String.dialog_cancel_message);
                alertDialog.SetPositiveButton("Ok", async delegate
                {
                    storageRef.Child(user.Uid + "/" + examId + "/" + storage_folder + "/" + holder.itemId)
                                        .Delete();
                    await CrossCloudFirestore.Current
                         .Instance
                         .Collection("Users")
                         .Document(user.Uid)
                         .Collection("Exams")
                         .Document(examId)
                         .Collection(storage_folder)
                         .Document(holder.itemId)
                         .DeleteAsync();
                });
                alertDialog.SetNegativeButton("Cancel", delegate
                {
                    alertDialog.Dispose();
                });
                alertDialog.Show();
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(Application.Context);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
            }
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            View itemView = viewHolder.ItemView;
            ColorDrawable background = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.colorPrimarylight)));
            Drawable deleteIcon = ContextCompat.GetDrawable(context, Resource.Drawable.deletebin);
            int backgroundCornerOffset = 40; //so background is behind the rounded corners of itemView
            int backgroundHeightOffset = 30;
            int backgroundRightOffset = 30;
            int iconMargin = (itemView.Height - deleteIcon.IntrinsicHeight) / 2;
            int iconTop = itemView.Top + (itemView.Height - deleteIcon.IntrinsicHeight) / 2;
            int iconBottom = iconTop + deleteIcon.IntrinsicHeight;

            if (dX < 0)
            { // Swiping to the left
                int iconLeft = itemView.Right - iconMargin - deleteIcon.IntrinsicWidth;
                int iconRight = itemView.Right - iconMargin;
                deleteIcon.SetBounds(iconLeft, iconTop, iconRight, iconBottom);

                background.SetBounds(itemView.Right + ((int)dX) - backgroundCornerOffset,
                        itemView.Top + backgroundHeightOffset,
                        itemView.Right + backgroundRightOffset,
                        itemView.Bottom - backgroundHeightOffset);
            }
            else
            { // view is unSwiped
                background.SetBounds(0, 0, 0, 0);
            }
            background.Draw(c);
            deleteIcon.Draw(c);
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