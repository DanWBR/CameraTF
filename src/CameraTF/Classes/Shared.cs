using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using MotoDetector;

namespace DWSIMSimulator_Android.Code
{

    class ArrayAdapterWithHint : ArrayAdapter<String>
    {

        public ArrayAdapterWithHint(Context ct, int i1, int i2, List<String> values) : base(ct, i1, i2, values)
        {


        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View v = base.GetView(position, convertView, parent);
            if (position == Count)
            {
                ((TextView)v.FindViewById(Android.Resource.Id.Text1)).Text = "";
                ((TextView)v.FindViewById(Android.Resource.Id.Text1)).Hint = GetItem(Count);

            }

            return v;

        }

        public override int Count
        {
            get
            {
                return base.Count - 1;
            }
        }

    }

    public static class Shared
    {

        public static void MessageBox(Activity activity, string title, string message, Action okhandler)
        {
            activity.RunOnUiThread(() =>
                    {
                        using (var alert = new AlertDialog.Builder(activity))
                        {
                            alert.SetMessage(message);
                            alert.SetIcon(Resource.Drawable.icon);
                            alert.SetTitle(title);
                            alert.SetPositiveButton("OK", (s0, e0) => { okhandler.Invoke(); });
                            alert.Create().Show();
                        }
                    });

        }

        public static Spinner CreateAndAddSpinnerRow(View view, int layoutID, String text, IEnumerable<String> options, int position, Action<object, Android.Widget.AdapterView.ItemSelectedEventArgs, Spinner> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 0.60f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.SetTextColor(Android.Graphics.Color.White);
            txtName.Gravity = GravityFlags.Left;

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(30 * scale), 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            param2.Weight = 0.4f;

            Spinner cbConnection = new Spinner(view.Context, SpinnerMode.Dialog);
            var adapter1 = new ArrayAdapterWithHint(view.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, 0, options.ToList());
            adapter1.Add("touch to select...");
            cbConnection.Adapter = adapter1;
            cbConnection.LayoutParameters = param2;

            if (options.Count() > 0)
            {
                try
                {
                    if (options.ToList()[position] == "")
                    {
                        cbConnection.SetSelection(adapter1.Count);
                    }
                    else
                    {
                        cbConnection.SetSelection(position);
                    }
                }
                catch (Exception)
                {
                    Toast.MakeText(view.Context, "Error: invalid value for " + text + ".", ToastLength.Short);
                }
            }
            else { cbConnection.SetSelection(adapter1.Count); }
            bool loaded = false;

            cbConnection.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {
                if (loaded) { handler.Invoke(sender, e, cbConnection); }
                loaded = true;
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(cbConnection);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return cbConnection;

        }

        public static EditText CreateAndAddTextBoxRow(View view, int layoutID, String numberformat, String text, Double currval, Action<object, Android.Text.TextChangedEventArgs, EditText> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 0.3f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            param2.Weight = 0.7f;

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Right | GravityFlags.Bottom;
            textBox.Text = currval.ToString(numberformat, System.Globalization.CultureInfo.InvariantCulture);
            textBox.InputType = Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                if (handler != null) handler.Invoke(sender, e, textBox);
            };

            LinearLayout.LayoutParams paramimg = new LinearLayout.LayoutParams((int)(fontsize * scale), (int)(fontsize * scale));
            paramimg.LeftMargin = (int)(2 * scale + 0.5f);
            paramimg.RightMargin = (int)(5 * scale + 0.5f);
            paramimg.Gravity = GravityFlags.CenterVertical;
            //paramimg.Weight = 0.1f;

            ImageButton imgClear = new ImageButton(view.Context);
            imgClear.Id = new Random().Next();
            imgClear.SetImageResource(Resource.Drawable.minus);
            imgClear.SetScaleType(ImageView.ScaleType.CenterInside);
            imgClear.SetBackgroundColor(Android.Graphics.Color.Transparent);
            imgClear.LayoutParameters = paramimg;
            imgClear.SetPadding(0, 0, 0, 0);
            imgClear.Click += (sender, e) =>
            {
                textBox.Text = "";
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(textBox);
            ll.AddView(imgClear);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static EditText CreateAndAddFullTextBoxRow(View view, int layoutID, String text, Action<object, Android.Text.TextChangedEventArgs, EditText> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Left | GravityFlags.Bottom;
            textBox.Text = text;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handler.Invoke(sender, e, textBox);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(textBox);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static EditText CreateAndAddFullMultilineTextBoxRow(View view, int layoutID, String text, Action<object, Android.Text.TextChangedEventArgs, EditText> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(false);
            textBox.SetLines(3);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Left | GravityFlags.Bottom;
            textBox.Text = text;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handler.Invoke(sender, e, textBox);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(textBox);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static EditText CreateAndAddStringEditorRow(View view, int layoutID, String text, String currval, Action<object, Android.Text.TextChangedEventArgs, EditText> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 0.5f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            param2.Weight = 0.5f;

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Left | GravityFlags.Bottom;
            textBox.Text = currval;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handler.Invoke(sender, e, textBox);
            };

            LinearLayout.LayoutParams paramimg = new LinearLayout.LayoutParams((int)(fontsize * scale), (int)(fontsize * scale));
            paramimg.LeftMargin = (int)(2 * scale + 0.5f);
            paramimg.RightMargin = (int)(5 * scale + 0.5f);
            paramimg.Gravity = GravityFlags.CenterVertical; ;
            //paramimg.Weight = 0.1f;

            ImageButton imgClear = new ImageButton(view.Context);
            imgClear.Id = new Random().Next();
            imgClear.SetImageResource(Resource.Drawable.minus);
            imgClear.SetScaleType(ImageView.ScaleType.CenterInside);
            imgClear.SetBackgroundColor(Android.Graphics.Color.Transparent);
            imgClear.LayoutParameters = paramimg;
            imgClear.SetPadding(0, 0, 0, 0);
            imgClear.Click += (sender, e) =>
            {
                textBox.Text = "";
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(textBox);
            ll.AddView(imgClear);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static LinearLayout CreateAndAddEditStringAndTwoButtonsRow(View view, int layoutID, String currval,
                                                                      int btn1ID, int btn2ID,
                                                                      Action<object, Android.Text.TextChangedEventArgs, EditText> handleredit,
                                                             Action<object, ImageButton> btn1,
                                                             Action<object, ImageButton> btn2)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            param2.Weight = 0.3f;

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Left | GravityFlags.Bottom;
            textBox.Text = currval;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handleredit.Invoke(sender, e, textBox);
            };

            LinearLayout.LayoutParams paramimg = new LinearLayout.LayoutParams((int)((fontsize + 6) * scale), (int)((fontsize + 6) * scale));
            paramimg.LeftMargin = (int)(2 * scale + 0.5f);
            paramimg.RightMargin = (int)(5 * scale + 0.5f);
            paramimg.Gravity = GravityFlags.CenterVertical; ;
            //paramimg.Weight = 0.1f;

            ImageButton imgClear = new ImageButton(view.Context);
            imgClear.Id = new Random().Next();
            imgClear.SetImageResource(btn1ID);
            imgClear.SetScaleType(ImageView.ScaleType.CenterInside);
            imgClear.SetBackgroundColor(Android.Graphics.Color.Transparent);
            imgClear.LayoutParameters = paramimg;
            imgClear.SetPadding(0, 0, 0, 0);
            imgClear.Click += (sender, e) =>
            {
                btn1.Invoke(sender, imgClear);
            };

            ImageButton imgClear2 = new ImageButton(view.Context);
            imgClear2.Id = new Random().Next();
            imgClear2.SetImageResource(btn2ID);
            imgClear2.SetScaleType(ImageView.ScaleType.CenterInside);
            imgClear2.SetBackgroundColor(Android.Graphics.Color.Transparent);
            imgClear2.LayoutParameters = paramimg;
            imgClear2.SetPadding(0, 0, 0, 0);
            imgClear2.Click += (sender, e) =>
            {
                btn2.Invoke(sender, imgClear2);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(textBox);
            ll.AddView(imgClear);
            ll.AddView(imgClear2);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return ll;

        }

        public static LinearLayout CreateAndAddLabelAndButtonRow(View view, int layoutID, String label,
                                                                      int btn1ID, Action<object, ImageButton> btn1)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(15 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = label;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams paramimg = new LinearLayout.LayoutParams((int)((fontsize + 6) * scale), (int)((fontsize + 6) * scale));
            paramimg.LeftMargin = (int)(2 * scale + 0.5f);
            paramimg.RightMargin = (int)(5 * scale + 0.5f);
            paramimg.TopMargin = (int)(15 * scale + 0.5f);
            paramimg.BottomMargin = (int)(5 * scale + 0.5f);
            paramimg.Gravity = GravityFlags.Top;
            //paramimg.Weight = 0.1f;

            ImageButton imgClear = new ImageButton(view.Context);
            imgClear.Id = new Random().Next();
            imgClear.SetImageResource(btn1ID);
            imgClear.SetScaleType(ImageView.ScaleType.CenterInside);
            imgClear.SetBackgroundColor(Android.Graphics.Color.Transparent);
            imgClear.LayoutParameters = paramimg;
            imgClear.SetPadding(0, 0, 0, 0);
            imgClear.Click += (sender, e) =>
            {
                btn1.Invoke(sender, imgClear);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(imgClear);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return ll;

        }


        public static EditText CreateAndAddDoubleTextBoxRow(View view, int layoutID, String value1, String numberformat, String text, Double currval, Action<object, Android.Text.TextChangedEventArgs, EditText> handler, Action<object, Android.Text.TextChangedEventArgs, EditText> handler2)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0.4f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            //param.Weight = 0.4f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0.3f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            //param2.Weight = 0.3f;

            EditText textBox0 = new EditText(view.Context);
            textBox0.SetSingleLine(true);
            textBox0.LayoutParameters = param2;
            textBox0.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox0.Gravity = GravityFlags.Left | GravityFlags.Bottom;
            textBox0.Text = value1;
            textBox0.SetTextColor(Android.Graphics.Color.White);

            textBox0.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handler.Invoke(sender, e, textBox0);
            };

            EditText textBox = new EditText(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Right | GravityFlags.Bottom;
            textBox.Text = currval.ToString(numberformat);
            textBox.InputType = Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned;
            textBox.SetTextColor(Android.Graphics.Color.White);

            textBox.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                handler2.Invoke(sender, e, textBox);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.WeightSum = 1.0f;
            ll.AddView(txtName);
            ll.AddView(textBox0);
            ll.AddView(textBox);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static TextView CreateAndAddRegularLabelBoxRow(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(15 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static TextView CreateAndAddLabelBoxRow(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(15 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static TextView CreateAndAddDescriptionRow(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14")) - 3.0f;

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(0 * scale + 0.5f);
            param.BottomMargin = (int)(0 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            //txtName.SetTextColor(Android.Graphics.Color.LightGray);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static TextView CreateAndAddDescriptionRow2(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(0 * scale + 0.5f);
            param.BottomMargin = (int)(0 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            //txtName.SetTextColor(Android.Graphics.Color.LightGray);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static TextView CreateAndAddDescriptionRow(View view, int layoutID, String text, String value)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);

            TextView textBox = new TextView(view.Context);
            textBox.SetSingleLine(true);
            textBox.LayoutParameters = param2;
            textBox.SetTextSize(ComplexUnitType.Sp, fontsize - 2);
            textBox.Gravity = GravityFlags.Right | GravityFlags.Bottom;
            textBox.Text = value;
            textBox.SetTextColor(Android.Graphics.Color.White);
            //textBox.SetTypeface(null, Android.Graphics.TypefaceStyle.Italic);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(textBox);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return textBox;

        }

        public static TextView CreateAndAddBoldDescriptionRow2(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static TextView CreateAndAddBoldDescriptionRow(View view, int layoutID, String text)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14")) - 3.0f;

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(0 * scale + 0.5f);
            param.BottomMargin = (int)(0 * scale + 0.5f);
            param.Weight = 1.0f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            //txtName.SetTextColor(Android.Graphics.Color.LightGray);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static CheckBox CreateAndAddCheckBoxRow(View view, int layoutID, String text, Boolean currval, Action<object, CompoundButton.CheckedChangeEventArgs, CheckBox> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 1.0f;

            CheckBox txtName = new CheckBox(view.Context);
            txtName.Text = text;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
            txtName.TextAlignment = TextAlignment.Gravity;
            txtName.Checked = currval;
            txtName.SetTextColor(Android.Graphics.Color.White);

            txtName.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
            {
                handler.Invoke(sender, e, txtName);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            ll.Id = new Random().Next();
            ll.LayoutParameters = param3;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

            return txtName;

        }

        public static Button CreateAndAddButtonRow(View view, int layoutID, String text, Action<object, System.EventArgs, Button> handler)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14")) - 1.0f;

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(40 * scale), 1.0f);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.Weight = 1.0f;

            Button txtName = new Button(view.Context);
            txtName.Text = text;
            txtName.LayoutParameters = param;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);

            TypedValue outvalue = new TypedValue();
            view.Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, outvalue, true);
            txtName.SetBackgroundResource(outvalue.ResourceId);

            txtName.Click += (object sender, EventArgs e) =>
            {
                handler.Invoke(sender, e, txtName);
            };

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            param3.TopMargin = (int)(2 * scale + 0.5f);
            param3.BottomMargin = (int)(2 * scale + 0.5f);
            ll.Id = new Random().Next();
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.LayoutParameters = param3;
            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);
            ll.LayoutParameters = param3;

            return txtName;

        }

        public static void CreateAndAddListRow(View view, int layoutID, String numberformat, String label, String value, String units)
        {

            ISharedPreferences prefs = (view.Context).GetSharedPreferences("prefs.txt", FileCreationMode.Private);
            var fontsize = float.Parse(prefs.GetString("fontsize2", "14"));

            // Get the screen's density scale
            float scale = view.Resources.DisplayMetrics.ScaledDensity;

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param.LeftMargin = (int)(5 * scale + 0.5f);
            param.RightMargin = (int)(5 * scale + 0.5f);
            param.TopMargin = (int)(5 * scale + 0.5f);
            param.BottomMargin = (int)(5 * scale + 0.5f);
            param.Weight = 0.6f;

            TextView txtName = new TextView(view.Context);
            txtName.Text = label;
            txtName.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName.LayoutParameters = param;
            txtName.Gravity = GravityFlags.Left;
            txtName.SetMinWidth((int)(140 * scale));
            txtName.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param2.LeftMargin = (int)(5 * scale + 0.5f);
            param2.RightMargin = (int)(5 * scale + 0.5f);
            param2.TopMargin = (int)(5 * scale + 0.5f);
            param2.BottomMargin = (int)(5 * scale + 0.5f);
            param2.Weight = 0.3f;

            TextView txtName2 = new TextView(view.Context);
            txtName2.Text = value;
            txtName2.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName2.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName2.LayoutParameters = param2;
            txtName2.Gravity = GravityFlags.Right;
            //txtName2.SetMinWidth((int)(300 * scale));
            txtName2.SetTextColor(Android.Graphics.Color.White);

            LinearLayout.LayoutParams param3 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            param3.LeftMargin = (int)(5 * scale + 0.5f);
            param3.RightMargin = (int)(5 * scale + 0.5f);
            param3.TopMargin = (int)(5 * scale + 0.5f);
            param3.BottomMargin = (int)(5 * scale + 0.5f);
            param3.Weight = 0.1f;

            TextView txtName3 = new TextView(view.Context);
            txtName3.Text = units;
            txtName3.SetTextSize(ComplexUnitType.Sp, fontsize);
            txtName3.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            txtName3.LayoutParameters = param3;
            txtName3.Gravity = GravityFlags.Left;
            txtName3.SetMinWidth((int)(70 * scale));
            txtName3.SetTextColor(Android.Graphics.Color.White);

            LinearLayout ll = new LinearLayout(view.Context);
            LinearLayout.LayoutParams param4 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
            //param4.Weight = 1.0f;
            ll.Id = new Random().Next();
            //ll.WeightSum = 1.0f;
            ll.LayoutParameters = param4;
            ll.Orientation = Orientation.Horizontal;
            ll.AddView(txtName);
            ll.AddView(txtName2);
            ll.AddView(txtName3);

            LinearLayout l0 = view.FindViewById<LinearLayout>(layoutID);
            l0.AddView(ll);

        }

        public static void ShareText(Context ctxt, string text, string description)
        {
            String shareBody = text;
            Intent sharingIntent = new Intent(Android.Content.Intent.ActionSend);
            sharingIntent.SetType("text/plain");
            sharingIntent.PutExtra(Android.Content.Intent.ExtraSubject, description);
            sharingIntent.PutExtra(Android.Content.Intent.ExtraText, shareBody);
            ctxt.StartActivity(Intent.CreateChooser(sharingIntent, "Share Results"));
        }

        public static void SaveText(Context ctxt, string text, string filename)
        {
            string path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            string filename2 = System.IO.Path.Combine(path, filename);

            try
            {
                System.IO.File.WriteAllText(filename2, text);
                Toast toast = Toast.MakeText(ctxt, "Text successfully saved to " + filename2, ToastLength.Long);
                toast.Show();
            }
            catch (Exception ex)
            {
                Toast toast = Toast.MakeText(ctxt, "Error: " + ex.Message.ToString(), ToastLength.Long);
                toast.Show();
            }
        }

    }
}