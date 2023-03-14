using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld.Planet;
using UnityEngine.Networking;
using System.Windows;
using Rect = UnityEngine.Rect;
using Verse.Noise;
using RTFExporter;

namespace Diary
{
    public class GUIDraggableTexture
    {
        private float zoomRatio = 0.2f;
        private float currentImageScale;
        private Rect imageRect;
        private int imageWidth;
        private int imageHeight;
        private Texture2D currentImageDisplayed;
        private UnityWebRequest imageLoadRequest;
        private bool imageLoading;
        private Rect outerRect;
        private Rect initialOuterRect;
        private bool mustRecomputeOuterRect;
        private bool firstLoading;
        private float maxImageScale;

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

                Log.Message(maxImageScale.ToString());

                imageLoading = false;
                if (firstLoading)
                {
                    mustRecomputeOuterRect = true;
                }
                firstLoading = false;
            };
        }

        private void TryFixImageCoordinates()
        {
            if (imageRect.xMin < 0f)
            {
                float diff = imageRect.xMin;

                imageRect.xMin -= diff;
                imageRect.xMax -= diff;
            }
            else if (imageRect.xMax > 1.0f)
            {
                float diff = imageRect.xMax - 1.0f;

                imageRect.xMin -= diff;
                imageRect.xMax -= diff;
            }

            if (imageRect.yMin < 0f)
            {
                float diff = imageRect.yMin;

                imageRect.yMin -= diff;
                imageRect.yMax -= diff;
            }
            else if (imageRect.yMax > 1.0f)
            {
                float diff = imageRect.yMax - 1.0f;

                imageRect.yMin -= diff;
                imageRect.yMax -= diff;
            }
        }

        private void OnScrollWheel()
        {
            Event.current.Use();

            float xRatio = outerRect.width / (float)imageWidth;
            float yRatio = outerRect.height / (float)imageHeight;

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
                if (imageRect.xMin + xRatio * zoomRatio > imageRect.xMax - xRatio * zoomRatio || imageRect.yMin + yRatio * zoomRatio > imageRect.yMax - yRatio * zoomRatio)
                {
                    return;
                }

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

            float xDiff = Event.current.delta.x * -0.001f / currentImageScale / currentImageScale;
            float yDiff = Event.current.delta.y * -0.001f / currentImageScale / currentImageScale;

            if ((xDiff > 0f && imageRect.xMax < 1.0f) || (xDiff < 0f && imageRect.xMin > 0.0f))
            {
                currentCenter.x += xDiff;
            }


            if ((yDiff > 0f && imageRect.yMax < 1.0f) || (yDiff < 0f && imageRect.yMin > 0.0f))
            {
                currentCenter.y += yDiff;
            }

            imageRect.center = currentCenter;

            TryFixImageCoordinates();
        }

        private void ComputeDefaultRects(Rect inRect)
        {
            outerRect = new Rect(0.0f, inRect.yMin, inRect.width, inRect.height);
            initialOuterRect = new Rect(0.0f, inRect.yMin, inRect.width, inRect.height);

            float displayRatio = inRect.width / inRect.height;
            float imageRatio = (float)imageWidth / (float)imageHeight;

            if (displayRatio > imageRatio)
            {
                float updateRatio = imageRatio / displayRatio;

                imageRect = new Rect(0f, (1f - updateRatio) * 0.5f, 1f, updateRatio);
            }
            else
            {
                float updateRatio = displayRatio / imageRatio;

                imageRect = new Rect(0.5f - updateRatio * 0.5f, 0f, updateRatio, 1f);
            }

            mustRecomputeOuterRect = false;
        }

        public void Draw(Rect inRect)
        {
            if (HasImageLoaded())
            {
                if (mustRecomputeOuterRect)
                {
                    ComputeDefaultRects(inRect);
                }


                Widgets.DrawTexturePart(outerRect, imageRect, currentImageDisplayed);
            }
            if (Mouse.IsOver(outerRect))
            {
                if (Event.current.type == EventType.ScrollWheel)
                {
                    OnScrollWheel();
                }

                if (Input.GetMouseButton(0) && Event.current.type == EventType.MouseDrag)
                {
                    OnDrag();
                }
            }
        }
    }
}
