using RTFExporter;

namespace DiaryMod.RTF
{
    public class RTFElement
    {
        public RTFElement(RTFParagraph paragraph, RTFText text)
        {
            Text = text;
            paragraph.elements.Add(this);
        }

        public RTFElement(RTFParagraph paragraph, RTFImage image)
        {
            Image = image;
            paragraph.elements.Add(this);
        }

        public RTFImage Image { get; }

        public RTFText Text { get; }
    }
}