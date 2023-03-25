using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTFExporter;
using UnityEngine.UI;

namespace Diary.RTF
{
    public class RTFElement
    {
        private RTFText _text;
        private RTFImage _image;

        public RTFImage Image
        {
            get { return _image; }
        }
        public RTFText Text
        {
            get { return _text; }
        }

        public RTFElement(RTFParagraph paragraph, RTFText text)
        {
            _text = text;
            paragraph.elements.Add(this);
        }

        public RTFElement(RTFParagraph paragraph, RTFImage image)
        {
            _image = image;
            paragraph.elements.Add(this);
        }
    }
}
