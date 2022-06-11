using Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mopup.Platforms.Android.Gestures
{
    internal class RgGestureDetectorListener : GestureDetector.SimpleOnGestureListener
    {
        public event EventHandler<MotionEvent>? Clicked;


        //OnSingleTap is not called as opposed to it being called on the original RG.Plugin.Popup
        public override bool OnSingleTapUp(MotionEvent? e)
        {
            if (e != null) Clicked?.Invoke(this, e);

            return false;
        }

        ////This method is called //It works so... :)
        //public override bool OnDown(MotionEvent e)
        //{
        //    if (e != null) Clicked?.Invoke(this, e);
        //    return base.OnDown(e);
        //}
        //public override bool OnSingleTapConfirmed(MotionEvent e)
        //{
        //    return base.OnSingleTapConfirmed(e);
        //}
    }
}
