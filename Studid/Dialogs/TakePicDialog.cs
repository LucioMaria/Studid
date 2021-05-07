using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.IO;
using Java.Lang;
using Java.Text;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Studid.Dialogs.AddItemDialog.OnInputSelected;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;


namespace Studid.Dialogs
{
    public class TakePicDialog : DialogFragment
    {
        const int REQUEST_IMAGE_CAPTURE = 1;

        private AddItemDialog.OnInputSelected onInputSelected;
        private EditText filenameET;
        private TextInputLayout textInputLayout;
        private MaterialButton saveButton = null;
        private string examId;

        private File photoFile = null;
        Android.Net.Uri photoURI;

        private AppCompatImageView picView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments != null)
            {
                examId = Arguments.GetString("examId");
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.dialog_take_pic, container, false);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            
            picView = (AppCompatImageView) view.FindViewById(Resource.Id.dialog_tpic_imageview);
            textInputLayout = (TextInputLayout) view.FindViewById(Resource.Id.dialog_tpic_text_layout);
            filenameET =(EditText) view.FindViewById(Resource.Id.dialog_tpic_editText);

            saveButton =(MaterialButton) view.FindViewById(Resource.Id.save);
            saveButton.Click += delegate
            {
                string itemNameNew = filenameET.Text.Trim();
                if (itemNameNew.Equals(""))
                {
                    textInputLayout.Error = Context.Resources.GetString(Resource.String.empty_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (itemNameNew.Length >= 20)
                {
                    textInputLayout.Error = Context.Resources.GetString(Resource.String.overflow_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (!photoFile.Exists())
                {
                    Toast.MakeText(Context, Resource.String.no_picture_found, ToastLength.Short).Show();
                }
                else
                {
                    AddItemDialog.OnInputSelected.nameState result = onInputSelected.sendInput(filenameET.Text, photoURI);
                    switch (result)
                    {
                        case nameState.OK:
                            Dialog.Dismiss();
                            break;
                        case nameState.H_ERROR:
                            Dialog.Dismiss();
                            break;
                        case nameState.USED:
                            {
                                textInputLayout.Error = Context.Resources.GetString(Resource.String.used_name);
                                textInputLayout.RequestFocus();
                            }
                            break;
                    }
                }
            };
           // DispatchTakePictureIntent();
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
        private void DispatchTakePictureIntent()
        {
            Intent takePictureIntent = new Intent(MediaStore.ActionImageCapture);
            // Ensure that there's a camera activity to handle the intent
            if (takePictureIntent.ResolveActivity(Activity.PackageManager) != null)
            {
                // Create the File where the photo should go
                try
                {
                    photoFile = CreateImageFile();
                }
                catch (IOException ex)
                {
                    // Error occurred while creating the File
                }
                // Continue only if the File was successfully created
                if (photoFile != null)
                {
                    photoURI = FileProvider.GetUriForFile(Context,
                            "com.companyname.studid.provider",
                            photoFile);
                    takePictureIntent.PutExtra(MediaStore.ExtraOutput, photoURI);
                    StartActivityForResult(takePictureIntent, REQUEST_IMAGE_CAPTURE);
                }
            }
        }

        private File CreateImageFile()
        {
            // Create an image file name
            string timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Date());
            string imageFileName = "JPEG_" + timeStamp + "_";
            File storageDir = Activity.GetExternalCacheDirs()[0];
            File image = File.CreateTempFile(
                    imageFileName,  /* prefix */
                    ".jpg",         /* suffix */
                    storageDir      /* directory */
            );
            return image;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == REQUEST_IMAGE_CAPTURE && resultCode==-1)
            {
                Glide.With(Context)
                        .Load(photoFile.Path)
                        .FitCenter()
                        .Into(picView);
            }
            else if (resultCode== 0)
            {
                Dialog.Dismiss();
            }
        }
        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            try
            {
                onInputSelected = (AddItemDialog.OnInputSelected)TargetFragment;
            }
            catch (ClassCastException e)
            {
                Log.Error("on attach", "exception" + e.Message);
            }
        }
        public static TakePicDialog newInstance(string examId)
        {
            TakePicDialog fragment = new TakePicDialog();
            Bundle args = new Bundle();
            args.PutString("examId", examId);
            fragment.Arguments= args;
            return fragment;
        }
    }
}