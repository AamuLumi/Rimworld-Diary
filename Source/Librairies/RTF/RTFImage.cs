namespace RTFExporter
{
    public class RTFImage
    {
        public RTFImageStyle style;
        private string _url;

        public string Url
        {
            get { return _url; }
        }

        public RTFImage(string url)
        {
            style = new RTFImageStyle();
            _url = url;
        }

        public RTFImage(string url, RTFImageStyle style)
        {
            this.style = style;
            _url = url;
        }

        public RTFImage SetStyle()
        {
            style = new RTFImageStyle();
            return this;
        }
    }
}
