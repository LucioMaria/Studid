using Android.App;
using Android.Net;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.Fragment.App;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;
using Java.Lang;
using String = System.String;
using Android.Provider;
using Android.Util;
using Xamarin.Essentials;
using Google.Android.Material.TextField;
using Java.IO;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Studid.Dialogs
{
    public class AddItemDialog : DialogFragment
    {
        Button actionSearch, actionOk, actionCancel;
        private TextView itemUriTV;
        private EditText itemNameET;
        TextInputLayout addItemInputLayout;
        private Intent searchfile;
        private string filePath;
        private Android.Net.Uri fileUri;
        private String argValue;
        private OnInputSelected onInputSelected;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            try
            {
                argValue = Arguments.GetString("ArgKey");
            }
            catch (System.Exception e)
            {
                _ = e.StackTrace;
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.dialog_add_item, container, false);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));


            addItemInputLayout = (TextInputLayout)view.FindViewById(Resource.Id.dialog_name_input_layout);
            itemUriTV = (TextView)view.FindViewById(Resource.Id.dialog_uri_tv);
            itemNameET = (EditText)view.FindViewById(Resource.Id.dialog_name_editText);
            actionSearch = (Button)view.FindViewById(Resource.Id.dialog_search_btn);
            actionOk = (Button)view.FindViewById(Resource.Id.dialog_ok_btn);
            actionCancel = (Button)view.FindViewById(Resource.Id.dialog_cancel_btn);
            actionSearch.Click += ActionSearch_Click;
            actionOk.Click += ActionOk_Click;
            actionCancel.Click += ActionCancel_Click;
            return view;
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 21 && resultCode == (int)Result.Ok)
            {
                fileUri = data.Data;
                itemUriTV.Text = fileUri.Path;
                ContentResolver contentResolver = this.Activity.ContentResolver;
                contentResolver.TakePersistableUriPermission(fileUri,
                        ActivityFlags.GrantReadUriPermission);
                Android.Database.ICursor returnCursor = contentResolver.Query(fileUri,
                        null, null, null, null);
                int nameIndex = returnCursor.GetColumnIndex(OpenableColumns.DisplayName);
                returnCursor.MoveToFirst();
                if (itemNameET.Text.Trim() == "")
                    itemNameET.Text = returnCursor.GetString(nameIndex);
            }
        }

        private void ActionCancel_Click(object sender, EventArgs e)
        {
            Dialog.Dismiss();
        }

        private void ActionOk_Click(object sender, EventArgs e)
        {
            var itemName = itemNameET.Text.Trim();
            if (itemName == "")
            {
                addItemInputLayout.Error = Resources.GetString(Resource.String.empty_name_field);
                addItemInputLayout.RequestFocus();
            }
            else if(itemName.Length>=20) 
            {
                addItemInputLayout.Error = Resources.GetString(Resource.String.overflow_name_field);
                addItemInputLayout.RequestFocus();
            }
            else if (itemUriTV.Text + "" == "")
            {
                Toast.MakeText(this.Context, Resource.String.empty_file_field, ToastLength.Short).Show();
            }
            else
            {
                var inputResoult = onInputSelected.sendInput(itemNameET.Text + "", fileUri);
                switch (inputResoult)
                {
                    case OnInputSelected.nameState.OK:
                        Dialog.Dismiss();
                        break;
                    case OnInputSelected.nameState.H_ERROR:
                        Dialog.Dismiss();
                        break;
                    case OnInputSelected.nameState.USED:
                        {
                            addItemInputLayout.Error = Resources.GetString(Resource.String.used_name);
                            addItemInputLayout.RequestFocus();
                        }
                        break;
                }
            }
        }

        private async void ActionSearch_Click(object sender, EventArgs e)
        {
            Intent searchfile = new Intent(Intent.ActionOpenDocument);
            searchfile.SetType(argValue);
            searchfile.AddFlags(ActivityFlags.GrantReadUriPermission);
            searchfile.AddFlags(ActivityFlags.GrantPersistableUriPermission);
            StartActivityForResult(searchfile, 21);
        }
        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            try
            {
                onInputSelected = (OnInputSelected)TargetFragment;
            }
            catch (ClassCastException e)
            {
                Log.Error("onAttach", "exception" + e.Message);
            }
        }
        public static AddItemDialog newInstance(String Arg)
        {
            AddItemDialog addItemDialog = new AddItemDialog();
            Bundle args = new Bundle();
            args.PutString("ArgKey", Arg);
            addItemDialog.Arguments = args;
            return addItemDialog;
        }

        //interface to override for getting the uri and the file name
        public interface OnInputSelected
        {
            enum nameState {OK=0, USED = 1, H_ERROR=-1};
            nameState sendInput(System.String filename, Android.Net.Uri fileuri);
        }
    }
}