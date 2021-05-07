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
using Plugin.FirebaseAuth;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Studid.Dialogs
{
    public class AddExamDialog : DialogFragment
    {
        TextInputLayout addexaminputlayout;
        EditText addexamnameText;
        Button submitButton, cancelButton;
        private DateTime date;
        private int cfu = 0;
        private string examname = "";
        MaterialButton buttondate;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.dialog_add_exam, container, false);
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

            addexamnameText = (EditText)view.FindViewById(Resource.Id.dialog_name_editText);
            addexaminputlayout = (TextInputLayout)view.FindViewById(Resource.Id.dialog_name_input_layout);
            Calendar calendar = Calendar.GetInstance(Locale.Italy);
            buttondate = (MaterialButton)view.FindViewById(Resource.Id.date_button);
            buttondate.Click += Buttondate_Click;


            cancelButton = (Button)view.FindViewById(Resource.Id.dialog_cancel_btn);
            cancelButton.Click += CancelButton_Click;

            NumberPicker cfuNumPic = (NumberPicker)view.FindViewById(Resource.Id.cfu_numpic);
            cfuNumPic.MinValue = 0;
            cfuNumPic.MaxValue = 25;
            submitButton = (Button)view.FindViewById(Resource.Id.dialog_ok_btn);

            submitButton.Click += async delegate
            {
                string examName = addexamnameText.Text.ToUpper().Trim();
                if (examName.Equals(""))
                {
                    addexaminputlayout.Error = Resources.GetString(Resource.String.empty_name_field);
                    addexaminputlayout.RequestFocus();
                }
                else if (examName.Length > 20)
                {
                    addexaminputlayout.Error = Resources.GetString(Resource.String.overflow_name_field);
                    addexaminputlayout.RequestFocus();
                }
                else if (await IsSameNameAsync(examName))
                {
                    addexaminputlayout.Error = Resources.GetString(Resource.String.used_name);
                    addexaminputlayout.RequestFocus();
                }
                else
                {
                    ExamModel model = new ExamModel();
                    model.examId = Guid.NewGuid().ToString();
                    model.examName = addexamnameText.Text.ToUpper().Trim();
                    model.date = new Plugin.CloudFirestore.Timestamp(date);
                    model.cfu = cfuNumPic.Value;
                    CrossCloudFirestore.Current
                                 .Instance
                                 .Collection("Users")
                                 .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                 .Collection("Exams")
                                 .Document(model.examId)
                                 .SetAsync(model);
                    this.Dismiss();
                }
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
        async Task<bool> IsSameNameAsync(string examNewName)
        {
            var query = await CrossCloudFirestore.Current
                .Instance
                .Collection("Users")
                .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                .Collection("Exams")
                .GetAsync();
            var examlist = query.ToObjects<ExamModel>();

            foreach (ExamModel exam in examlist)
            {
                if (exam.examName.Equals(examNewName))
                    return true;
            }
            return false;
        }    
    }
}