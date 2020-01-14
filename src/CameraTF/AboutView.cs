using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Android.Graphics;
using System.IO;
using MotoDetector;
using DWSIMSimulator_Android.Code;

namespace MotoDetector
{
    public class AboutView : LinearLayout
    {

        public AboutView(Context context) :
            base(context)
        {
            Initialize();
        }

        public AboutView(Context context, IAttributeSet attrs) :
                base(context, attrs)
        {
            Initialize();
        }

        public AboutView(Context context, IAttributeSet attrs, int defStyle) :
                base(context, attrs, defStyle)
        {
            Initialize();
        }

        protected void Initialize()
        {

            this.AddView(Inflate(this.Context, Resource.Layout.about, null));

            TextView txt1 = this.FindViewById<TextView>(Resource.Id.textViewAppName);
            txt1.Text = Resources.GetString(Resource.String.app_name);

            TextView txt2 = this.FindViewById<TextView>(Resource.Id.textViewAppVersion);
            txt2.Text = "Versão " + this.Context.PackageManager.GetPackageInfo(this.Context.PackageName, 0).VersionName;

            TextView txt3 = this.FindViewById<TextView>(Resource.Id.textViewAppCopyright);
            txt3.Text = "Copyright (c) 2020 Daniel Medeiros";

            string uuid;

            ISharedPreferences prefs = (this.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            uuid = prefs.GetString("UUID", "");

            Shared.CreateAndAddButtonRow(this, Resource.Id.linearLayoutAbout, "Política de Privacidade", (arg1, arg2, arg3) =>
            {
                string text;
                using (StreamReader sr = new StreamReader(this.Context.Assets.Open("privacypolicy.txt")))
                {
                    text = sr.ReadToEnd();
                }
                var alert = new AlertDialog.Builder(this.Context);
                alert.SetMessage(text);
                alert.SetTitle("Política de Privacidade");
                alert.SetCancelable(false);
                alert.SetPositiveButton("OK", (sender, e) => { });
                alert.Create().Show();
            });

            Shared.CreateAndAddButtonRow(this, Resource.Id.linearLayoutAbout, "Histórico de Versões", (arg1, arg2, arg3) =>
            {
                string text;
                using (StreamReader sr = new StreamReader(this.Context.Assets.Open("whatsnew.txt")))
                {
                    text = sr.ReadToEnd();
                }
                var alert = new AlertDialog.Builder(this.Context);
                alert.SetMessage(text);
                alert.SetTitle("Histórico de Versões");
                alert.SetCancelable(false);
                alert.SetPositiveButton("OK", (sender, e) => { });
                alert.Create().Show();
            });
        }
    }
}

