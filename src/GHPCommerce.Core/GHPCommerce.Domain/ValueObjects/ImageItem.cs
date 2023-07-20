using System.Collections.Generic;

namespace GHPCommerce.Domain.ValueObjects
{
    public class ImageItem : ValueObject
    {
       

        public string Name { get; private set; }
        public byte[] ImageBytes { get; private set; }
        public string ImageUrl { get; private set; }
        public bool MainImage { get; private set; }

        public ImageItem()
        {
            
        }

        public ImageItem(string name, byte[] imageBytes, string imageUrl,bool mainImage)
        {
            Name = name;
            ImageBytes = imageBytes;
            ImageUrl = imageUrl;
            MainImage = mainImage;
        }

        public ImageItem(string imageTitle, byte[] imageBytes,bool mainImage)
        {
            Name = imageTitle;
            ImageBytes = imageBytes;
            MainImage = mainImage;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return ImageBytes;
            yield return ImageUrl;
        }
    }
}