using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Firebase.Auth;
using Firebase.Firestore;
using Google.Android.Material.TextField;
using Java.IO;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
using Plugin.FirebaseStorage;
using Studid.Adapter;
using Studid.Dialogs;
using Studid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Studid.Fragments
{
    public class FragmentFlashcards : Fragment, AddItemDialog.OnInputSelected
    {
        private static System.String FILE_TYPE = "application/pdf";
        private static System.String STORAGE_FOLDER = "Flashcards";
        private String examname, examId;
        private RecyclerView recyclerView;
        private ItemAdapter itemAdapter;
        private ContentLoadingProgressBar progressIndicator;
        private FirebaseUser user;
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
            File storagePath = new File(this.Activity.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments), examname + "/" + STORAGE_FOLDER);
            user = FirebaseAuth.Instance.CurrentUser;
            if (user != null)
            {
                ImageView imageView = (ImageView)view.FindViewById(Resource.Id.empty_recycler_image);
                CrossCloudFirestore.Current
                                   .Instance
                                   .Collection("Users")
                                   .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                                   .Collection("Exams")
                                   .Document(examId)
                                   .Collection(STORAGE_FOLDER)
                                   .AddSnapshotListener((snapshot, error) =>
                                   {
                                       if (snapshot != null)
                                       {
                                           foreach (var documentChange in snapshot.DocumentChanges)
                                           {
                                               var newItem = documentChange.Document.ToObject<ItemModel>();
                                               var i = itemAdapter.ItemList.FindIndex(x => x.Equals(newItem));
                                               switch (documentChange.Type)
                                               {
                                                   case DocumentChangeType.Added:
                                                       if (i == -1)
                                                       {
                                                           itemAdapter.ItemList.Add(newItem);
                                                           itemAdapter.NotifyItemInserted(itemAdapter.ItemCount - 1);
                                                       }
                                                       else
                                                           itemAdapter.NotifyDataSetChanged();
                                                       break;
                                                   case DocumentChangeType.Removed:
                                                       itemAdapter.ItemList.Remove(newItem);
                                                       itemAdapter.NotifyItemRemoved(i);
                                                       break;
                                                   case DocumentChangeType.Modified:
                                                       itemAdapter.ItemList[i] = newItem;
                                                       itemAdapter.NotifyItemChanged(i);
                                                       break;
                                               }
                                           }
                                           if (itemAdapter.ItemCount == 0)
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
                                   });
            }
            //ItemTouchHelper.Callback callback = new MyItemTouchHelper(STORAGE_FOLDER, examname);
            //ItemTouchHelper itemTouchHelper = new ItemTouchHelper(callback);
            //itemTouchHelper.AttachToRecyclerView(recyclerView);
            return view;
        }

        private void SetupRecyclerView()
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(Application.Context));
            itemAdapter = new ItemAdapter(Application.Context, recyclerView);
            itemAdapter.ItemUpdate_NameClick += AdapterItem_ItemUpdate_NameClick;
            itemAdapter.Item_CheckClick += AdapterItem_Item_CheckClick;
            itemAdapter.Item_SelectClick += AdapterItem_Item_SelectClick;
            recyclerView.SetAdapter(itemAdapter);
        }

        private void AdapterItem_Item_SelectClick(object sender, ItemAdapterClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AdapterItem_Item_CheckClick(object sender, ItemAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            ItemModel itemcheck_clicked = itemAdapter.ItemList[e.Position];
            if (itemcheck_clicked.isMemorized)
            {
                await CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                        .Collection("Exams")
                        .Document(examId)
                        .Collection(STORAGE_FOLDER)
                        .Document(itemcheck_clicked.itemId)
                        .UpdateAsync("isMemorized", false);
            }
            else
            {
                await CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                        .Collection("Exams")
                        .Document(examId)
                        .Collection(STORAGE_FOLDER)
                        .Document(itemcheck_clicked.itemId)
                        .UpdateAsync("isMemorized", true);
            }
        }


        private void AdapterItem_ItemUpdate_NameClick(object sender, ItemAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            ItemModel itemname_clicked = itemAdapter.ItemList[e.Position];
            string itemname = itemname_clicked.itemName;
            Dialog nameDialog = new Dialog(this.Context);
            nameDialog.SetContentView(Resource.Layout.dialog_name_update);
            EditText editText = (EditText)nameDialog.FindViewById(Resource.Id.dialog_name_editText);
            editText.Text = itemname;
            /* Non riconosce più android.support.design e quindi ho messo il design che sta in google.design*/
            TextInputLayout textInputLayout = (TextInputLayout)nameDialog.FindViewById(Resource.Id.dialog_name_input_layout);
            nameDialog.Show();
            Android.Widget.Button okButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_ok);
            Android.Widget.Button cancelButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_cancel);
            okButton.Click += async delegate
            {// need to put srting in strings.xml
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
                    await CrossCloudFirestore.Current
                    .Instance
                    .Collection("Users")
                    .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                    .Collection("Exams")
                    .Document(examId)
                    .Collection(STORAGE_FOLDER)
                    .Document(itemname_clicked.itemId)
                    .UpdateAsync("itemName", itemNameNew);
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
            foreach (ItemModel item in itemAdapter.ItemList)
            {
                if (item.itemName.Equals(itemNewName))
                    return true;
            }
            return false;
        }
        public async void sendInput(string filename, string fileuri)
        {
            if (user != null && isOnline(this.Context))
            {
                String itemId = Guid.NewGuid().ToString();
                var storageReference = CrossFirebaseStorage.Current.Instance.RootReference.Child(user.Uid + "/" + examId + "/" + STORAGE_FOLDER + "/" + itemId);
                var uploadProgress = new Progress<IUploadState>();
                uploadProgress.ProgressChanged += async (sender, e) =>
                {
                    progressIndicator.Show();
                    progressIndicator.Max = (int)e.TotalByteCount;
                    progressIndicator.Progress = (int)e.BytesTransferred;
                    var progress = e.TotalByteCount > 0 ? 100.0 * e.BytesTransferred / e.TotalByteCount : 0;
                    if (progress == 100)
                    {
                        await CrossCloudFirestore.Current.Instance
                            .Collection("Users").Document(user.Uid)
                            .Collection("Exams").Document(examId)
                            .Collection(STORAGE_FOLDER).Document(itemId)
                            .SetAsync(new ItemModel(itemId, filename));
                        progressIndicator.Hide();
                    }
                };
                await storageReference.PutFileAsync(fileuri, progress: uploadProgress);
            } else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this.Context);
                alert.SetTitle("Login and connection reqired")
                        .SetMessage("you need to be online and logged in to perform this action");
                alert.Show();
            }
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
    }
}