using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Rect = UnityEngine.Rect;

namespace Diary.UI;

public class GUIDraggableTexture
{
    private Texture2D _currentImageDisplayed;
    private float _currentImageScale;
    private bool _firstLoading;
    private int _imageHeight;
    private UnityWebRequest _imageLoadRequest;
    private bool _imageLoading;
    private Rect _imageRect;
    private int _imageWidth;
    private float _maxImageScale;
    private bool _mustRecomputeOuterRect;
    private Rect _outerRect;
    private float _zoomRatio = 0.2f;

    public GUIDraggableTexture()
    {
        _currentImageScale = 1.0f;

        _imageRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        _mustRecomputeOuterRect = false;
        _firstLoading = true;
    }

    public bool HasImageLoaded()
    {
        return _currentImageDisplayed != null;
    }

    public bool IsLoading()
    {
        return _imageLoading;
    }

    public void LoadTexture(string path)
    {
        _imageLoading = true;
        _imageLoadRequest = UnityWebRequestTexture.GetTexture($"file://{path}");

        _imageLoadRequest.SendWebRequest().completed += delegate
        {
            _currentImageDisplayed = DownloadHandlerTexture.GetContent(_imageLoadRequest);

            _imageWidth = _currentImageDisplayed.width;
            _imageHeight = _currentImageDisplayed.height;

            _maxImageScale = Math.Max(_imageWidth, _imageHeight) / 1500f;
            _zoomRatio = (_maxImageScale - 1.0f) / 10;

            _imageLoading = false;
            if (_firstLoading) _mustRecomputeOuterRect = true;
            _firstLoading = false;
        };
    }

    private void TryFixImageCoordinates()
    {
        if (_imageRect.xMin < 0f)
        {
            var diff = _imageRect.xMin;

            _imageRect.xMin -= diff;
            _imageRect.xMax -= diff;
        }
        else if (_imageRect.xMax > 1.0f)
        {
            var diff = _imageRect.xMax - 1.0f;

            _imageRect.xMin -= diff;
            _imageRect.xMax -= diff;
        }

        if (_imageRect.yMin < 0f)
        {
            var diff = _imageRect.yMin;

            _imageRect.yMin -= diff;
            _imageRect.yMax -= diff;
        }
        else if (_imageRect.yMax > 1.0f)
        {
            var diff = _imageRect.yMax - 1.0f;

            _imageRect.yMin -= diff;
            _imageRect.yMax -= diff;
        }
    }

    private void OnScrollWheel()
    {
        Event.current.Use();

        var xRatio = _outerRect.width / _imageWidth;
        var yRatio = _outerRect.height / _imageHeight;

        if (Event.current.delta.y > 0 && _currentImageScale > 1.00f)
        {
            _imageRect.xMin -= xRatio * _zoomRatio;
            _imageRect.xMax += xRatio * _zoomRatio;
            _imageRect.yMin -= yRatio * _zoomRatio;
            _imageRect.yMax += yRatio * _zoomRatio;
            _currentImageScale -= 0.1f;

            TryFixImageCoordinates();
        }
        else if (Event.current.delta.y < 0 && _currentImageScale < 2.0f)
        {
            if (_imageRect.xMin + xRatio * _zoomRatio > _imageRect.xMax - xRatio * _zoomRatio ||
                _imageRect.yMin + yRatio * _zoomRatio > _imageRect.yMax - yRatio * _zoomRatio) return;

            _imageRect.xMin += xRatio * _zoomRatio;
            _imageRect.xMax -= xRatio * _zoomRatio;
            _imageRect.yMin += yRatio * _zoomRatio;
            _imageRect.yMax -= yRatio * _zoomRatio;
            _currentImageScale += 0.1f;

            TryFixImageCoordinates();
        }
    }

    private void OnDrag()
    {
        var currentCenter = _imageRect.center;

        var xDiff = Event.current.delta.x * -0.001f / _currentImageScale / _currentImageScale;
        var yDiff = Event.current.delta.y * -0.001f / _currentImageScale / _currentImageScale;

        if ((xDiff > 0f && _imageRect.xMax < 1.0f) || (xDiff < 0f && _imageRect.xMin > 0.0f))
            currentCenter.x += xDiff;


        if ((yDiff > 0f && _imageRect.yMax < 1.0f) || (yDiff < 0f && _imageRect.yMin > 0.0f))
            currentCenter.y += yDiff;

        _imageRect.center = currentCenter;

        TryFixImageCoordinates();
    }

    private void ComputeDefaultRects(Rect inRect)
    {
        _outerRect = new Rect(0.0f, inRect.yMin, inRect.width, inRect.height);

        var displayRatio = inRect.width / inRect.height;
        var imageRatio = _imageWidth / (float)_imageHeight;

        if (displayRatio > imageRatio)
        {
            var updateRatio = imageRatio / displayRatio;

            _imageRect = new Rect(0f, (1f - updateRatio) * 0.5f, 1f, updateRatio);
        }
        else
        {
            var updateRatio = displayRatio / imageRatio;

            _imageRect = new Rect(0.5f - updateRatio * 0.5f, 0f, updateRatio, 1f);
        }

        _mustRecomputeOuterRect = false;
    }

    public void Draw(Rect inRect)
    {
        if (HasImageLoaded())
        {
            if (_mustRecomputeOuterRect) ComputeDefaultRects(inRect);


            Widgets.DrawTexturePart(_outerRect, _imageRect, _currentImageDisplayed);
        }

        if (Mouse.IsOver(_outerRect))
        {
            if (Event.current.type == EventType.ScrollWheel) OnScrollWheel();

            if (Input.GetMouseButton(0) && Event.current.type == EventType.MouseDrag) OnDrag();
        }
    }
}