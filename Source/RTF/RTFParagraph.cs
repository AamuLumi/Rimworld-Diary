using System.Collections.Generic;
using Diary.RTF;

namespace RTFExporter
{
    /// <summary>
    /// The RTF paragraph class, every class need a document to append
    /// </summary>
    public class RTFParagraph
    {
        public List<RTFElement> elements = new List<RTFElement>();
        public RTFParagraphStyle style;

        /// <summary>
        /// The RTF paragraph constructor
        /// <seealso cref="RTFExporter.RTFDocument">
        /// </summary>
        /// <param name="document">The RTF document to append the paragraph</param>
        public RTFParagraph(RTFDocument document)
        {
            style = new RTFParagraphStyle(document);
            document.paragraphs.Add(this);
        }

        /// <summary>
        /// The method to add a text to a paragraph
        /// <seealso cref="RTFExporter.RTFText">
        /// </summary>
        /// <param name="content">The text content</param>
        /// <returns>Return the text instantiated with the content</returns>
        public RTFText AppendText(string content)
        {
            RTFText text = new RTFText(content);
            RTFElement element = new RTFElement(this, text);

            return text;
        }

        /// <summary>
        /// The method to add a text to a paragraph
        /// <seealso cref="RTFExporter.RTFText">
        /// <seealso cref="RTFExporter.RTFTextStyle">
        /// </summary>
        /// <param name="content">The text content</param>
        /// <param name="style">The text styler</param>
        /// <returns>Return the text instantiated with the content and the style</returns>
        public RTFText AppendText(string content, RTFTextStyle style)
        {
            RTFText text = new RTFText(content, style);
            RTFElement element = new RTFElement(this, text);

            return text;
        }

        public RTFImage AppendImage(string url)
        {
            RTFImage img = new RTFImage(url);
            RTFElement element = new RTFElement(this, img);

            return img;
        }
    }
}
