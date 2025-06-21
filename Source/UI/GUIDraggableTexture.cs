﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Rect = UnityEngine.Rect;

namespace DiaryMod
{
    public class GUIDraggableTexture
    {
        private Texture2D currentImageDisplayed;
        private float currentImageScale;
        private bool firstLoading;
        private int imageHeight;
        private bool imageLoading;
        private UnityWebRequest imageLoadRequest;
        private Rect imageRect;
        private int imageWidth;
        private Rect initialOuterRect;
        private float maxImageScale;
        private bool mustRecomputeOuterRect;
        private Rect outerRect;
        private float zoomRatio = 0.2f;

        public GUIDraggableTexture()
        {
            currentImageScale = 1.0f;

            imageRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            mustRecomputeOuterRect = false;
            firstLoading = true;
        }

        public bool HasImageLoaded()
        {
            return currentImageDisplayed != null;
        }

        public bool IsLoading()
        {
            return imageLoading;
        }

        public void LoadTexture(string path)
        {
            imageLoading = true;
            imageLoadRequest = UnityWebRequestTexture.GetTexture($"file://{path}");

            imageLoadRequest.SendWebRequest().completed += delegate
            {
                currentImageDisplayed = DownloadHandlerTexture.GetContent(imageLoadRequest);

                imageWidth = currentImageDisplayed.width;
                imageHeight = currentImageDisplayed.height;

                maxImageScale = Math.Max(imageWidth, imageHeight) / 1500f;
                zoomRatio = (maxImageScale - 1.0f) / 10;

                imageLoading = false;
                if (firstLoading) mustRecomputeOuterRect = true;
                firstLoading = false;
            };
        }

        private void TryFixImageCoordinates()
        {
            if (imageRect.xMin < 0f)
            {
                var diff = imageRect.xMin;

                imageRect.xMin -= diff;
                imageRect.xMax -= diff;
            }
            else if (imageRect.xMax > 1.0f)
            {
                var diff = imageRect.xMax - 1.0f;

                imageRect.xMin -= diff;
                imageRect.xMax -= diff;
            }

            if (imageRect.yMin < 0f)
            {
                var diff = imageRect.yMin;

                imageRect.yMin -= diff;
                imageRect.yMax -= diff;
            }
            else if (imageRect.yMax > 1.0f)
            {
                var diff = imageRect.yMax - 1.0f;

                imageRect.yMin -= diff;
                imageRect.yMax -= diff;
            }
        }

        private void OnScrollWheel()
        {
            Event.current.Use();

            var xRatio = outerRect.width / imageWidth;
            var yRatio = outerRect.height / imageHeight;

            if (Event.current.delta.y > 0 && currentImageScale > 1.00f)
            {
                imageRect.xMin -= xRatio * zoomRatio;
                imageRect.xMax += xRatio * zoomRatio;
                imageRect.yMin -= yRatio * zoomRatio;
                imageRect.yMax += yRatio * zoomRatio;
                currentImageScale -= 0.1f;

                TryFixImageCoordinates();
            }
            else if (Event.current.delta.y < 0 && currentImageScale < 2.0f)
            {
                if (imageRect.xMin + xRatio * zoomRatio > imageRect.xMax - xRatio * zoomRatio ||
                    imageRect.yMin + yRatio * zoomRatio > imageRect.yMax - yRatio * zoomRatio) return;

                imageRect.xMin += xRatio * zoomRatio;
                imageRect.xMax -= xRatio * zoomRatio;
                imageRect.yMin += yRatio * zoomRatio;
                imageRect.yMax -= yRatio * zoomRatio;
                currentImageScale += 0.1f;

                TryFixImageCoordinates();
            }
        }

        private void OnDrag()
        {
            var currentCenter = imageRect.center;

            var xDiff = Event.current.delta.x * -0.001f / currentImageScale / currentImageScale;
            var yDiff = Event.current.delta.y * -0.001f / currentImageScale / currentImageScale;

            if ((xDiff > 0f && imageRect.xMax < 1.0f) || (xDiff < 0f && imageRect.xMin > 0.0f))
                currentCenter.x += xDiff;


            if ((yDiff > 0f && imageRect.yMax < 1.0f) || (yDiff < 0f && imageRect.yMin > 0.0f))
                currentCenter.y += yDiff;

            imageRect.center = currentCenter;

            TryFixImageCoordinates();
        }

        private void ComputeDefaultRects(Rect inRect)
        {
            outerRect = new Rect(0.0f, inRect.yMin, inRect.width, inRect.height);
            initialOuterRect = new Rect(0.0f, inRect.yMin, inRect.width, inRect.height);

            var displayRatio = inRect.width / inRect.height;
            var imageRatio = imageWidth / (float)imageHeight;

            if (displayRatio > imageRatio)
            {
                var updateRatio = imageRatio / displayRatio;

                imageRect = new Rect(0f, (1f - updateRatio) * 0.5f, 1f, updateRatio);
            }
            else
            {
                var updateRatio = displayRatio / imageRatio;

                imageRect = new Rect(0.5f - updateRatio * 0.5f, 0f, updateRatio, 1f);
            }

            mustRecomputeOuterRect = false;
        }

        public void Draw(Rect inRect)
        {
            if (HasImageLoaded())
            {
                if (mustRecomputeOuterRect) ComputeDefaultRects(inRect);


                Widgets.DrawTexturePart(outerRect, imageRect, currentImageDisplayed);
            }

            if (Mouse.IsOver(outerRect))
            {
                if (Event.current.type == EventType.ScrollWheel) OnScrollWheel();

                if (Input.GetMouseButton(0) && Event.current.type == EventType.MouseDrag) OnDrag();
            }
        }
    }
}