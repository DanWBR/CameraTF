using Android.Content;
using Android.Util;
using Android.Widget;

namespace MotoDetector
{
    public class BlankView : LinearLayout
    {

        public BlankView(Context context) :
            base(context)
        {
            Initialize();
        }

        public BlankView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public BlankView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        void Initialize()
        {

            this.AddView(Inflate(this.Context, Resource.Layout.BlankLayout, null));

        }

    }
}
