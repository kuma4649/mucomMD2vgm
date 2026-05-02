using Android.App;
using Android.Content;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;

namespace mucomMD2vgmMAUI;

[Activity(Label = "FilePickerActivity")]
public class FilePickerActivity : Activity
{
    public const int PickFileRequestCode = 1001;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Intent intent = new Intent(Intent.ActionOpenDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("*/*");
        StartActivityForResult(intent, PickFileRequestCode);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        string? uri = null;

        if (requestCode == PickFileRequestCode &&
            resultCode == Result.Ok &&
            data != null)
        {
            uri = data.DataString;
        }

        WeakReferenceMessenger.Default.Send(new FilePickedMessage(uri));

        Finish();
    }
}