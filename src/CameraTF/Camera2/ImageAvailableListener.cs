using Android.Media;
using Java.IO;
using Java.Lang;
using Java.Nio;
using Microsoft.AppCenter.Crashes;

namespace MotoDetector.Listeners
{
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        public ImageAvailableListener(CameraController2 fragment, File file)
        {
            if (fragment == null)
                throw new System.ArgumentNullException("fragment");
            if (file == null)
                throw new System.ArgumentNullException("file");

            owner = fragment;
            this.file = file;
        }

        private readonly File file;
        private readonly CameraController2 owner;

        //public File File { get; private set; }
        //public Camera2BasicFragment Owner { get; private set; }

        public void OnImageAvailable(ImageReader reader)
        {
            owner.mBackgroundHandler.Post(new ImageSaver(reader.AcquireNextImage(), file));
        }

        // Saves a JPEG {@link Image} into the specified {@link File}.
        private class ImageSaver : Java.Lang.Object, IRunnable
        {
            // The JPEG image
            private Image mImage;

            // The file we save the image into.
            private File mFile;

            public ImageSaver(Image image, File file)
            {
                if (image == null)
                    throw new System.ArgumentNullException("image");
                if (file == null)
                    throw new System.ArgumentNullException("file");

                mImage = image;
                mFile = file;
            }

            public void Run()
            {
                var buffer = mImage.GetPlanes()[0].Buffer;
                var bytes = new byte[buffer.Remaining()];
                buffer.Get(bytes);
                using (var output = new FileOutputStream(mFile))
                {
                    try
                    {
                        output.Write(bytes);
                    }
                    catch (IOException e)
                    {
                        Crashes.TrackError(e);
                        e.PrintStackTrace();
                    }
                    finally
                    {
                        mImage.Close();
                    }
                }
            }
        }
    }
}