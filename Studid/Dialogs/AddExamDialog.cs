using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using Xamarin.Forms.PlatformConfiguration;
using Firebase.Firestore;
using Firebase;
using Studid.Models;
using Plugin.CloudFirestore;
using System.Threading.Tasks;
using Google.Android.Material.TextField;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Android.App;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;
using Android.Text.Format;
using Java.Text;

namespace Studid.Dialogs
{
    public class AddExamDialog : DialogFragment
{
    EditText addexamnameText;
    // TextInputLayout addexamdateText;
    Button submitButton, cancelButton;
    private DateTime date;
    // private NumberPicker numberpicker;
    private int cfu = 0;
    private string examname = "";
    MaterialButton buttondate;



    public override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Create your fragment here
    }



    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        // Use this to return your custom view for this Fragment

        View view = inflater.Inflate(Resource.Layout.addexam, container, false);
        addexamnameText = (EditText)view.FindViewById(Resource.Id.dialog_name_editText);
        Calendar calendar = Calendar.GetInstance(Locale.Italy);
        buttondate = (MaterialButton)view.FindViewById(Resource.Id.date_button);
        buttondate.Click += Buttondate_Click;


        cancelButton = (Button)view.FindViewById(Resource.Id.dialog_cancel_btn);
        cancelButton.Click += CancelButton_Click;

        NumberPicker cfuNumPic = (NumberPicker)view.FindViewById(Resource.Id.cfu_numpic);
        cfuNumPic.MinValue = 0;
        cfuNumPic.MaxValue = 25;
        /* cfuNumPic.ValueChanged += (s, e) =>
        {
            NumberPicker cfunumpic = e.Picker;
            int oldvalue = e.OldVal;
            int newvalue = e.NewVal;
            onNumPicChange(cfunumpic, oldvalue, newvalue);
        };
        void onNumPicChange(NumberPicker cfunumpic, int oldvalue, int newvalue)
        {
            this.cfu = newvalue;
        }
        */


        // addexamdateText = (TextInputLayout)view.FindViewById(Resource.Id.dialog_name_input_layout);
        submitButton = (Button)view.FindViewById(Resource.Id.dialog_ok_btn);

        submitButton.Click += async (sender, e) =>
        {
            ExamModel model = new ExamModel();
            model.examName = addexamnameText.Text.ToUpper().Trim();
            model.date = new Plugin.CloudFirestore.Timestamp(date);
            model.cfu = cfuNumPic.Value;
            await CrossCloudFirestore.Current
                         .Instance
                         .Collection("exams")
                         .Document(model.examName)
                         .SetAsync(model);
            this.Dismiss();
        };
        return view;
    }



    private void CancelButton_Click(object sender, EventArgs e)
    {
        this.Dismiss();
    }

    void Buttondate_Click(object sender, EventArgs e)
    {
        Dialog calendardialog = new Dialog(this.Context);
        Calendar calendar = Calendar.GetInstance(Locale.Italy);
        calendardialog.SetTitle("please choose a title");
        calendardialog.SetContentView(Resource.Layout.dialog_exam_date);
        CalendarView calendarview = (CalendarView)calendardialog.FindViewById(Resource.Id.calendarView);
        calendarview.MinDate = calendar.TimeInMillis;
        calendarview.DateChange += (s, e) =>
        {
            CalendarView view = e.View;
            int day = e.DayOfMonth;
            int month = e.Month + 1;
            int year = e.Year;
            onselecteddatechange(view, year, month, day);
        };
        async void onselecteddatechange(CalendarView view, int year, int month, int dayofmonth)
        {
            date = new DateTime(year, month, dayofmonth);
            buttondate.Text = date.ToString("dd/MM/yy");
            calendardialog.Dismiss();
        }
        Button buttoncancel = (Button)calendardialog.FindViewById(Resource.Id.date_cancel);
        buttoncancel.Click += buttoncancel_click;
        void buttoncancel_click(object sender, EventArgs e)
        {
            calendardialog.Dismiss();
        }
        calendardialog.Show();
    }
}
}