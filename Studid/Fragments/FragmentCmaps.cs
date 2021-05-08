using Android.App;
using Android.Content;
using Android.Gms.Tasks;
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
using Firebase.Storage;
using Google.Android.Material.TextField;
using Java.IO;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
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
    public class FragmentCmaps : Fragment, AddItemDialog.OnInputSelected, IOnProgressListener, IOnSuccessListener, IOnFailureListener
    {
        private static readonly String FILE_TYPE = "image/*";
        private static readonly String STORAGE_FOLDER = "Cmaps";

        private String examId;
        private RecyclerView recyclerView;
        private ItemAdapter itemAdapter;
        private ContentLoadingProgressBar progressIndicator;
        private FirebaseUser user;
        private File storagePath, fileToOpen;

        private StorageReference storageRef;
        private StorageReference storagefolder;
        String itemId, filename;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            examId = Arguments.GetString("exam_id");
            storageRef = FirebaseStorage.Instance.Reference;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_items, container, false);
            progressIndicator = (ContentLoadingProgressBar)view.FindViewById(Resource.Id.progressIndicator);
            recyclerView = (RecyclerView)view.FindViewById(Resource.Id.recicler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this.Context));
            SetupRecyclerView();

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
            ItemTouchHelper.Callback callback = new ItemTouchCallback(STORAGE_FOLDER, examId, this.Context);
            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(callback);
            itemTouchHelper.AttachToRecyclerView(recyclerView);
        }

        private void AdapterItem_Item_SelectClick(object sender, ItemAdapterClickEventArgs e)
        {
            storagePath = new File(this.Activity.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments), examId + "/" + STORAGE_FOLDER);
            if (!storagePath.Exists())
            {
                storagePath.Mkdirs();
            }
            fileToOpen = new File(storagePath, itemAdapter.ItemList[e.Position].itemId);
            if (fileToOpen.Exists())
            {
                FileOpener(fileToOpen);
            }
            else if (IsOnline(this.Activity))
            {
                StorageReference FileRef = storageRef.Child(user.Uid + "/" + examId + "/" + STORAGE_FOLDER + "/" + itemAdapter.ItemList[e.Position].itemId);
                FileRef.GetFile(fileToOpen).AddOnProgressListener(this).AddOnSuccessListener(this).AddOnFailureListener(this);
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this.Context);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
            }
        }

        private async void AdapterItem_Item_CheckClick(object sender, ItemAdapterClickEventArgs e)
        {
            FirebaseUser user = FirebaseAuth.Instance.CurrentUser;
            ItemModel itemcheck_clicked = itemAdapter.ItemList[e.Position];
            var doctoupdate = CrossCloudFirestore.Current
                        .Instance
                        .Collection("Users")
                        .Document(CrossFirebaseAuth.Current.Instance.CurrentUser.Uid)
                        .Collection("Exams")
                        .Document(examId)
                        .Collection(STORAGE_FOLDER)
                        .Document(itemcheck_clicked.itemId);
            if (itemcheck_clicked.isMemorized)
            {
                await doctoupdate.UpdateAsync("memorized", false);
            }
            else
            {
                await doctoupdate.UpdateAsync("memorized", true);
            }
        }


        private void AdapterItem_ItemUpdate_NameClick(object sender, ItemAdapterClickEventArgs e)
        {
            ItemModel itemname_clicked = itemAdapter.ItemList[e.Position];
            string itemname = itemname_clicked.itemName;
            Dialog nameDialog = new Dialog(this.Context);
            nameDialog.SetContentView(Resource.Layout.dialog_name_update);
            EditText editText = (EditText)nameDialog.FindViewById(Resource.Id.dialog_name_editText);
            editText.Text = itemname;
            TextInputLayout textInputLayout = (TextInputLayout)nameDialog.FindViewById(Resource.Id.dialog_name_input_layout);
            nameDialog.Show();
            Android.Widget.Button okButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_ok);
            Android.Widget.Button cancelButton = (Android.Widget.Button)nameDialog.FindViewById(Resource.Id.name_cancel);
            okButton.Click += async delegate
            {
                string itemNameNew = editText.Text.ToUpper().Trim();
                if (itemNameNew.Equals(""))
                {
                    textInputLayout.Error = Resources.GetString(Resource.String.empty_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (itemNameNew.Length > 20)
                {
                    textInputLayout.Error = Resources.GetString(Resource.String.overflow_name_field);
                    textInputLayout.RequestFocus();
                }
                else if (IsSameName(itemNameNew))
                {
                    textInputLayout.Error = Resources.GetString(Resource.String.used_name);
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
        public AddItemDialog.OnInputSelected.nameState sendInput(string filename, Android.Net.Uri fileuri)
        {
            if (IsOnline(this.Context))
            {
                if (user != null)
                {
                    if (!IsSameName(filename))
                    {
                        this.filename = filename;
                        itemId = Guid.NewGuid().ToString();
                        storagefolder = storageRef.Child(user.Uid + "/" + examId + "/" + STORAGE_FOLDER + "/" + itemId);
                        storagefolder
                            .PutFile(fileuri)
                            .AddOnProgressListener(this)
                            .AddOnSuccessListener(this)
                            .AddOnFailureListener(this);
                        return AddItemDialog.OnInputSelected.nameState.OK;
                    }
                    else
                    {
                        return AddItemDialog.OnInputSelected.nameState.USED;
                    }

                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this.Context);
                    alert.SetTitle(Resource.String.login_title)
                            .SetMessage(Resource.String.alert_login)
                            .Show();
                    return AddItemDialog.OnInputSelected.nameState.H_ERROR;
                }
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this.Context);
                alert.SetTitle(Resource.String.connection_title)
                        .SetMessage(Resource.String.connection_message)
                        .Show();
                return AddItemDialog.OnInputSelected.nameState.H_ERROR;
            }
        }

        //launching intent for file opening
        private void FileOpener(File file)
        {
            Android.Net.Uri uri = FileProvider.GetUriForFile(this.Context, this.Context.PackageName + ".provider", file);
            Intent openfile = new Intent(Intent.ActionView);
            openfile.SetDataAndType(uri, FILE_TYPE);
            openfile.SetFlags(ActivityFlags.GrantReadUriPermission);
            Intent intent1 = Intent.CreateChooser(openfile, Resources.GetString(Resource.String.openfile_chooser));
            StartActivity(intent1);
        }

        //checking if there is connection
        private bool IsOnline(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            //should check null because in airplane mode it will be null
            return (netInfo != null && netInfo.IsConnected);
        }

        public async void OnSuccess(Java.Lang.Object result)
        {
            progressIndicator.Hide();
            if (result is UploadTask.TaskSnapshot)
            {
                var durl = await storagefolder.GetDownloadUrlAsync();
                await CrossCloudFirestore.Current.Instance
                .Collection("Users").Document(user.Uid)
                .Collection("Exams").Document(examId)
                .Collection(STORAGE_FOLDER).Document(itemId)
                .SetAsync(new ItemModel(itemId, filename, durl.ToString()));
            }
            if (result is FileDownloadTask.TaskSnapshot)
                FileOpener(fileToOpen);
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(this.Context, "faliure", ToastLength.Short).Show();
        }

        public void snapshot(Java.Lang.Object p0)
        {
            if (p0 is UploadTask.TaskSnapshot)
            {
                var taskSnapshot = p0 as UploadTask.TaskSnapshot;
                progressIndicator.Show();
                progressIndicator.Max = (int)taskSnapshot.TotalByteCount;
                progressIndicator.Progress = (int)taskSnapshot.BytesTransferred;
            }
            else if (p0 is FileDownloadTask.TaskSnapshot)
            {
                var taskSnapshot = p0 as FileDownloadTask.TaskSnapshot;
                progressIndicator.Show();
                progressIndicator.Max = (int)taskSnapshot.TotalByteCount;
                progressIndicator.Progress = (int)taskSnapshot.BytesTransferred;
            }
        }
    }
}