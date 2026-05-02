
#if ANDROID
using Android.Content;
using Google.Android.Material.Color.Utilities;
using static Android.Icu.Text.CaseMap;
#endif
using CommunityToolkit.Mvvm.Messaging;
using Core;
using System.Text;

namespace mucomMD2vgmMAUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            // 17 行の空データを作成
            var emptyRows = new List<PartRow>();
            for (int i = 0; i < 17; i++)
            {
                emptyRows.Add(new PartRow());
            }

            PartsCollection.ItemsSource = emptyRows;

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            WeakReferenceMessenger.Default.Register<FilePickedMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                    ReadFileFromUri(m.Value);
                else
                    LogEditor.Text += "Canceled\n";
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<FilePickedMessage>(this);
        }

        private void OnOpenFileClicked(object sender, EventArgs e)
        {

#if ANDROID
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(FilePickerActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
#endif

        }


        private async void ReadFileFromUri(string uriString)
        {
#if ANDROID
            try
            {
                // SJIS を使えるようにする（必須）
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var context = Android.App.Application.Context;
                var uri = Android.Net.Uri.Parse(uriString);

                using var stream = context.ContentResolver.OpenInputStream(uri);
                if (stream == null)
                {
                    LogEditor.Text += "ファイルを開けませんでした\n";
                    return;
                }

                // ★ Shift_JIS で読む
                using var reader = new StreamReader(stream, Encoding.GetEncoding("shift_jis"));
                string text = await reader.ReadToEndAsync();


                LogEditor.Text += $"--- File Content ---\n{text}\n";
            }
            catch (Exception ex)
            {
                LogEditor.Text += $"読み込みエラー: {ex.Message}\n";
            }
#endif
        }

        private void StartCompile()
        {
            LogEditor.Text += "start compile thread";

            //Action dmy = UpdateTitle;
            //string stPath = System.Windows.Forms.Application.StartupPath;
            int rendSecond;
            //if (!int.TryParse(tstbMaxRendering.Text, out int rendSecond))
            {
                rendSecond = 600;
            }

            //for (int i = 1; i < args.Length; i++)
            //{
            //    string arg = args[i];
            //    if (!File.Exists(arg))
            //    {
            //        continue;
            //    }


            //    title = Path.GetFileName(arg);
            //    this.Invoke(dmy);

            //    Core.Log.Write(string.Format("  compile at [{0}]", args[i]));

            //    msgBox.clear();

            //    string desfn = Path.ChangeExtension(arg, Properties.Resources.ExtensionVGM);
            //    if (tsbToVGZ.Checked)
            //    {
            //        desfn = Path.ChangeExtension(arg, Properties.Resources.ExtensionVGZ);
            //    }

            //    Core.Log.Write("Call mucomMD2vgm core");

            Mmd2vgmArgs mArgs = new()
            {
                srcFn = "",
                desFn = "",
                stPath = "",
                Disp = null,
                isLoopEx = false,
                rendSecond = 600
            };
            MucomMD2vgm mv = new MucomMD2vgm(mArgs);
            //    if (mv.Start() != 0)
            //    {
            //        isSuccess = false;
            //        break;
            //    }

            //    Core.Log.Write("Return mucomMD2vgm core");
            //}

            //Core.Log.Write("Disp Result");

            //dmy = FinishedCompile;
            //this.Invoke(dmy);

            //Core.Log.Write("end compile thread");
            //Core.Log.Close();
        }

    }
}