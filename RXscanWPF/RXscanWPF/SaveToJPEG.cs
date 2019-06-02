using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RXscanWPF
{
    public class SaveToJPEG
    {
        Bitmap sourceImage;
        String targetImagePath;

        public SaveToJPEG(Bitmap sourceBitmap, String targetPath)
        {
            sourceImage = sourceBitmap;
            targetImagePath = Utils.GetNextFileName(targetPath+"\\NewJPEG.jpeg");
        }

        public void Convert()
        {
            ImageCodecInfo myImageCodecInfo;
            System.Drawing.Imaging.Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;

            // Get an ImageCodecInfo object that represents the JPEG codec.
            myImageCodecInfo = GetEncoderInfo("image/jpeg");

            // Create an Encoder object based on the GUID

            // for the Quality parameter category.
            myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            myEncoderParameters = new EncoderParameters(1);

            // Save the bitmap as a JPEG file with quality level 50.
            myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            Debug.WriteLine("Save Path: " + targetImagePath);
            sourceImage.Save(targetImagePath, myImageCodecInfo, myEncoderParameters);

            // Release source image so it may be deleted
            sourceImage.Dispose();
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();

            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }        
    }
}

