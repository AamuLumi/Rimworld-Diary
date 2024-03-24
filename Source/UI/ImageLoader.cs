using Verse;

namespace Diary
{
    [StaticConstructorOnStartup]
    public static class ImageLoader
    {
        public static GUIDraggableTexture CreateDraggableTexture()
        {
            return new GUIDraggableTexture();
        }
    }
}