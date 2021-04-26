using Android.App;
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

namespace Studid.Dialogs
{
    public class AddItemDialog : DialogFragment
    {
        Button actionSearch, actionOk, actionCancel;
        private TextView fileuriTV;
        private EditText filenameET;
        private Intent searchfile;
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
            View view = inflater.Inflate(Resource.Layout.dialog_file, container, false);

            fileuriTV = (TextView)view.FindViewById(Resource.Id.dialog_uri_tv);
            filenameET = (EditText)view.FindViewById(Resource.Id.dialog_name_editText);

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
            if (requestCode == 21 && resultCode == (int)Result.Ok) // non so s'è corretto col cast int, senza non funziona
            {
                fileUri = data.Data;
                fileuriTV.Text = (fileUri.Path + "");
                ContentResolver contentResolver = Activity.ContentResolver;
                contentResolver.TakePersistableUriPermission(fileUri,
                        ActivityFlags.GrantReadUriPermission);
                Android.Database.ICursor returnCursor = contentResolver.Query(fileUri,
                        null, null, null, null);
                int nameIndex = returnCursor.GetColumnIndex(OpenableColumns.DisplayName);
                returnCursor.MoveToFirst();
                if (filenameET.Text + "".Trim() == "")
                    filenameET.Text = returnCursor.GetString(nameIndex);
            }
        }

        private void ActionCancel_Click(object sender, EventArgs e)
        {
            Dialog.Dismiss();
        }

        private void ActionOk_Click(object sender, EventArgs e)
        {
            if (filenameET.Text + "".Trim() == "")
            {
                Toast.MakeText(this.Context, Resource.String.empty_name_field, ToastLength.Short).Show();
            }
            else if (fileuriTV.Text + "" == "")
            {
                Toast.MakeText(this.Context, Resource.String.empty_file_field, ToastLength.Short).Show();
            }
            else
            {
                onInputSelected.sendInput(filenameET.Text + "", fileUri);
                try
                {
                    Dialog.Dismiss();
                }
                catch (NullPointerException) { }

            }
        }

        private void ActionSearch_Click(object sender, EventArgs e)
        {
            searchfile = new Intent(Intent.ActionOpenDocument);
            searchfile.SetType(argValue);
            searchfile.AddFlags(ActivityFlags.GrantReadUriPermission);
            searchfile.AddFlags(ActivityFlags.GrantWriteUriPermission);
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
            void sendInput(System.String filename, Android.Net.Uri fileuri);
        }
    }
}