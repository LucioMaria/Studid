using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.IO;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Studid.Dialogs.AddItemDialog.OnInputSelected;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;


namespace Studid.Dialogs
{
    public class TakeRecDialog : DialogFragment
    {

        private static string LOG_TAG = "AudioRecordTest";
        private const int REQUEST_RECORD_AUDIO_PERMISSION = 200;
        private File record;
        private AddItemDialog.OnInputSelected onInputSelected;
        private EditText filenameET;
        private TextInputLayout textInputLayout;

        private AppCompatImageButton recordButton = null;
        private MediaRecorder recorder = null;

        private MaterialButton saveButton = null;

        // Requesting permission to RECORD_AUDIO
        private bool permissionToRecordAccepted = false;
        bool mStartRecording = true;
        private string[] permissions = { Manifest.Permission.RecordAudio };
        private string examId;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            examId = Arguments.GetString("exam_id");
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.dialog_take_rec, container, false);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            RequestPermissions(permissions, REQUEST_RECORD_AUDIO_PERMISSION);

            record = new File(Activity.GetExternalCacheDirs()[0], "audiorecordtemp.mp3");
            if (record.Exists())
                record.Delete();

            filenameET = (EditText)view.FindViewById(Resource.Id.dialog_trec_editText);
            textInputLayout =(TextInputLayout) view.FindViewById(Resource.Id.dialog_trec_input_layout);
            recordButton =(AppCompatImageButton) view.FindViewById(Resource.Id.rec);
            recordButton.Click += delegate {
                onRecord(mStartRecording);
                if (mStartRecording)
                {
                    recordButton.SetBackgroundResource(Resource.Drawable.button_roundshape_accent);
                }
                else
                {
                    recordButton.SetBackgroundResource(Resource.Color.transparent);
                }
                mStartRecording = !mStartRecording;
            };
            saveButton = (MaterialButton)view.FindViewById(Resource.Id.save);
            //handling ok click
            saveButton.Click += delegate {
                string itemNameNew = filenameET.Text.Trim();
                if (itemNameNew.Equals(""))
                {
                    textInputLayout.Error= Resources.GetString(Resource.String.empty_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (itemNameNew.Length >= 20)
                {
                    textInputLayout.Error = Resources.GetString(Resource.String.overflow_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (!record.Exists())
                {
                    Toast.MakeText(Context, Resource.String.no_recording_found, ToastLength.Short).Show();
                }
                else
                {
                    stopRecording();
                    AddItemDialog.OnInputSelected.nameState result = onInputSelected.sendInput(filenameET.Text, Android.Net.Uri.FromFile(record));
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
                                textInputLayout.Error = Resources.GetString(Resource.String.used_name); ;
                                textInputLayout.RequestFocus();
                            }
                            break;
                    }
                }
            };
            return view;
        }
        private void onRecord(bool start)
        {
            if (start)
            {
                startRecording();
            }
            else
            {
                stopRecording();
            }
        }

        private void startRecording()
        {
            recorder = new MediaRecorder();
            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetOutputFormat(OutputFormat.Default);
            recorder.SetOutputFile(record);
            recorder.SetAudioEncoder(AudioEncoder.AmrNb);

            try
            {
                recorder.Prepare();
            }
            catch (IOException e)
            {
                Log.Error(LOG_TAG, e.ToString());
            }

            recorder.Start();
        }

        private void stopRecording()
        {
            if (recorder != null)
            {
                recorder.Stop();
                recorder.Release();
                recorder = null;
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            if (recorder != null)
            {
                recorder.Release();
                recorder = null;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case REQUEST_RECORD_AUDIO_PERMISSION:
                    permissionToRecordAccepted = grantResults[0] == Permission.Granted;
                    break;
            }
            if (!permissionToRecordAccepted)
            {
                Dialog.Dismiss();
            }
        }
        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            try
            {
                onInputSelected = (AddItemDialog.OnInputSelected) TargetFragment;
            }
            catch (ClassCastException e)
            {
                Log.Error("on attach", "exception" + e.Message);
            }
        }
        public static TakeRecDialog newInstance(string examId)
        {
            TakeRecDialog fragment = new TakeRecDialog();
            Bundle args = new Bundle();
            args.PutString("examId", examId);
            fragment.Arguments = args;
            return fragment;
        }
    }
}