using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Firebase.Auth;
using Firebase.Firestore;
using Google.Android.Material.TextField;
using Java.IO;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
using Studid.Adapter;
using Studid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Studid.Fragments
{
    public class FragmentExercises : Fragment
    {
        private static String FILE_TYPE = "application/pdf";
        private static String STORAGE_FOLDER = "Exercises";
        private String examname, examId;

        private RecyclerView recyclerView;
        private ItemAdapter adapterItem;
        private ContentLoadingProgressBar progressIndicator;
        private FirebaseUser user;

        List<ItemModel> ItemList = new List<ItemModel>();


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            examname = Arguments.GetString("exam_name");
            examId = Arguments.GetString("exam_id");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_items, container, false);
            progressIndicator = (ContentLoadingProgressBar)view.FindViewById(Resource.Id.progressIndicator);
            recyclerView = (RecyclerView)view.FindViewById(Resource.Id.recicler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this.Context));
            SetupRecyclerView();
            /* ColorDrawable background = new ColorDrawable(Xamarin.Forms.Color.FromHex("#BBDEFB").ToAndroid());
            Drawable deleteIcon = ContextCompat.GetDrawable(Application.Context, Resource.Drawable.deletebin); */
            File storagePath = new File(this.Activity.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments), examname + "/" + STORAGE_FOLDER);
            user = FirebaseAuth.Instance.CurrentUser;
            if (user != null)
            {
                UpdateUI(recyclerView, view);
            }
            ItemTouchHelper.Callback callback = new MyItemTouchHelper(STORAGE_FOLDER, examname);
            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(callback);
            itemTouchHelper.AttachToRecyclerView(recyclerView);

            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        private void SetupRecyclerView()
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(Application.Context));
            adapterItem = new ItemAdapter(Application.Context, recyclerView, ItemList);
            adapterItem.ItemUpdate_NameClick += AdapterItem_ItemUpdate_NameClick;
            adapterItem.Item_CheckClick += AdapterItem_Item_CheckClick;
            adapterItem.Item_SelectClick += AdapterItem_Item_SelectClick;
            recyclerView.SetAdapter(adapterItem);
        }

        private void AdapterItem_Item_SelectClick(object sender, ItemAdapterClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AdapterItem_Item_CheckClick(object sender, ItemAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            ItemModel itemcheck_clicked = this.ItemList[e.Position];
            string itemcheck = itemcheck_clicked.itemName;
            DocumentReference ItemToUpdate = (DocumentReference)await CrossCloudFirestore.Current
                        .Instance
                        .Collection("Exams")
                        .Document(examname)
                        .Collection(STORAGE_FOLDER)
                        .Document(itemcheck)
                        .GetAsync();
            if (itemcheck_clicked.IsMemorized)
            {
                ItemToUpdate.Update("memorized", false);
            }
            else
            {
                ItemToUpdate.Update("memorized", true);
            }
        }

        private void AdapterItem_ItemUpdate_NameClick(object sender, ItemAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            ItemModel itemname_clicked = this.ItemList[e.Position];
            string itemname = itemname_clicked.itemName;
            Dialog nameDialog = new Dialog(Application.Context);
            nameDialog.SetContentView(Resource.Layout.dialog_name_update);
            EditText editText = (EditText)nameDialog.FindViewById(Resource.Id.dialog_name_editText);
            editText.Text = examname;
            /* Non riconosce più android.support.design e quindi ho messo il design che sta in google.design*/
            TextInputLayout textInputLayout = (TextInputLayout)nameDialog.FindViewById(Resource.Id.dialog_name_input_layout);
            nameDialog.Show();
            Android.Widget.Button okButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_ok);
            Android.Widget.Button cancelButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_cancel);
            okButton.Click += async delegate
            {
                string itemNameNew = editText.Text.ToUpper().Trim();
                if (itemNameNew.Equals(""))
                {
                    // textInputLayout.ErrorEnabled = true;
                    textInputLayout.Error = "Please fill the name field";
                    textInputLayout.RequestFocus();
                }
                else if (itemNameNew.Length > 15)
                {
                    // textInputLayout.ErrorEnabled = true;
                    textInputLayout.Error = "Please enter a short name";
                    textInputLayout.RequestFocus();
                }
                else if (IsSameName(itemNameNew))
                {
                    // textInputLayout.ErrorEnabled = true;
                    textInputLayout.Error = "Please change the name field";
                    textInputLayout.RequestFocus();
                }
                else
                {
                    var document = await CrossCloudFirestore.Current
                    .Instance
                    .Collection("Exams")
                    .Document(itemname)
                    .Collection(STORAGE_FOLDER)
                    .GetAsync();
                    if (!(document.IsEmpty))
                    {
                        ItemModel itemmodel = (ItemModel)document.ToObjects<ItemModel>();
                        itemmodel.itemName = itemNameNew;

                        await CrossCloudFirestore.Current
                                    .Instance
                                    .Collection("Users")
                                    .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                    .Collection("Exams")
                                    .Document(itemNameNew)
                                    .SetAsync(itemmodel);

                        await CrossCloudFirestore.Current
                                    .Instance
                                    .Collection("Users")
                                    .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                    .Collection("Exams")
                                    .Document(itemname)
                                    .DeleteAsync();
                    }
                    nameDialog.Dismiss();
                }
            };

            cancelButton.Click += delegate
            {
                nameDialog.Dismiss();
            };
        }

        bool IsSameName(string itemNewName)
        {
            foreach (ItemModel item in ItemList)
            {
                if (item.itemName.Equals(itemNewName))
                    return true;
            }
            return false;
        }

        //launching intent for file opening
        private void fileOpener(File file)
        {
            var uri = Android.Net.Uri.FromFile(file);
            Intent openfile = new Intent(Intent.ActionView);
            openfile.SetDataAndType(uri, FILE_TYPE);
            ContentResolver contentResolver = Activity.ContentResolver;
            contentResolver.TakePersistableUriPermission(uri,
            ActivityFlags.GrantReadUriPermission);
            Intent intent1 = Intent.CreateChooser(openfile, GetString(Resource.String.openfile_chooser));
            StartActivity(intent1);
        }

        //checking if there is connection
        private bool isOnline(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            //should check null because in airplane mode it will be null
            return (netInfo != null && netInfo.IsConnected);
        }

        private void UpdateUI(RecyclerView recyclerView, View view)
        {
            ImageView imageView = (ImageView)view.FindViewById(Resource.Id.empty_recycler_image);
            CrossCloudFirestore.Current
                               .Instance
                               .Collection("Users")
                               .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                               .Collection("Exams")
                               .Document(examname)
                               .Collection(STORAGE_FOLDER)
                               .AddSnapshotListener((snapshot, error) =>
                               {
                                   if (snapshot != null)
                                   {
                                       foreach (var documentChange in snapshot.DocumentChanges)
                                       {
                                           switch (documentChange.Type)
                                           {
                                               case DocumentChangeType.Added:
                                                   ItemList.Add(documentChange.Document.ToObject<ItemModel>());
                                                   break;
                                               case DocumentChangeType.Removed:
                                                   ItemList.Remove(documentChange.Document.ToObject<ItemModel>());
                                                   break;
                                               case DocumentChangeType.Modified:
                                                   var em = documentChange.Document.ToObject<ItemModel>();
                                                   var index = ItemList.FindIndex(x => x.itemName.Equals(em.itemName));
                                                   ItemList.Remove(em);
                                                   ItemList.Insert(index, em);
                                                   break;
                                           }
                                           ItemList.Sort();
                                           adapterItem.NotifyDataSetChanged();
                                           if (adapterItem.ItemCount == 0)
                                           {
                                               imageView.Visibility = ViewStates.Visible;
                                               recyclerView.Visibility = ViewStates.Invisible;
                                           }
                                           else
                                           {
                                               imageView.Visibility = ViewStates.Invisible;
                                               recyclerView.Visibility = ViewStates.Visible;
                                           }
                                       }
                                   }
                               });

        }
    }
}