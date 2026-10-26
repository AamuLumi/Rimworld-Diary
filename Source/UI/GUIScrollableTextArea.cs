using HarmonyLib;
using UnityEngine;
using UnityStubs.Gui;
using UnityStubs.Internal;

namespace Diary.UI;

public static class GUIScrollableTextArea
{
    private static bool _shouldCreateScrollableArea;
    private static float _scrollY;
    private static float _contentHeight;
    private static int _lastCaret = -1;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(GUI), "DrawTextAreaContent"),
            new HarmonyMethod(typeof(GUIScrollableTextArea), nameof(DrawTextAreaContentPrefix)));
        harmony.Patch(AccessTools.Method(typeof(GUI), "TextIndexAtMouse"),
            new HarmonyMethod(typeof(GUIScrollableTextArea), nameof(TextIndexAtMousePrefix)));
    }

    public static string Draw(Rect rect, string controlName, string text, ref float scrollY, GUIStyle style)
    {
        const float scrollbarWidth = 16f;
        var textRect = new Rect(rect.x, rect.y, rect.width - scrollbarWidth - 4f, rect.height);
        var scrollbarRect = new Rect(rect.xMax - scrollbarWidth, rect.y, scrollbarWidth, rect.height);

        var e = Event.current;
        // Update scroll value if we are scrolling on it
        if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
        {
            scrollY += e.delta.y * 40f;
            e.Use();
        }

        _shouldCreateScrollableArea = true;
        _scrollY = scrollY;
        GUI.SetNextControlName(controlName);
        var result = GUI.TextArea(textRect, text, style);
        _shouldCreateScrollableArea = false;
        scrollY = _scrollY;

        var maxScroll = Mathf.Max(0f, _contentHeight - textRect.height);
        var scrollbarValue = GUI.VerticalScrollbar(scrollbarRect, scrollY, textRect.height, 0f,
            Mathf.Max(_contentHeight, textRect.height));

        // Update the scroll value at next update tick
        scrollY = Mathf.Clamp(scrollbarValue, 0f, maxScroll);

        return result;
    }

    private static void TextIndexAtMousePrefix(ref float ____textScrollY)
    {
        if (_shouldCreateScrollableArea) ____textScrollY = _scrollY;
    }

    private static bool DrawTextAreaContentPrefix(Rect rect, string text, int fontIndex, bool focused,
        GUIStyle style, Color textColor, IGuiBackend be, int caret, int selA, int selB, int compA, int compB,
        ref float ____textScrollY, bool ____fieldRichText, List<Rect> ____selectionRects)
    {
        // Enable patch only when we want a scrollable area. Else, go back to initial method
        if (!_shouldCreateScrollableArea) return true;

        // Original code from GUI.TextFieldTextRect
        var p = style?.padding;
        var area = p != null
            ? new Rect(rect.x + p.left, rect.y + p.top, Mathf.Max(0f, rect.width - p.horizontal),
                Mathf.Max(0f, rect.height - p.vertical))
            : new Rect(rect.x + 4f, rect.y + 2f, Mathf.Max(0f, rect.width - 8f), Mathf.Max(0f, rect.height - 4f));

        // Original code from GUI.TextAreaWrapWidth
        var wrapWidth = style != null && !style.wordWrap ? -1f : area.width;

        // Edited code to support scroll
        var caretPos = be.TextCursorPos(fontIndex, 0f, FontStyle.Normal, text, wrapWidth, caret, out var lineHeight,
            ____fieldRichText);
        var endPos = be.TextCursorPos(fontIndex, 0f, FontStyle.Normal, text, wrapWidth, text.Length, out var endLineH,
            ____fieldRichText);

        // By "real", I mean the entire area including the hidden part
        var realTextAreaHeight = endPos.y + endLineH;

        // "rect" is the entire rect drawn by the GUI incl. padding etc.
        // "area" is the intern rect of the visible text
        var verticalPadding = rect.height - area.height;

        // Update _contentHeight with real text area height
        _contentHeight = realTextAreaHeight + verticalPadding;

        if (focused && caret != _lastCaret)
        {
            var endOfCurrentLine = caretPos.y + lineHeight;

            // If the end of current line is lower than the visible, scroll to the end of current line
            if (endOfCurrentLine - _scrollY > area.height) _scrollY = endOfCurrentLine - area.height;
            // If the beginning of current line is higher than the visible, scroll to the beginning of current line
            if (caretPos.y - _scrollY < 0f) _scrollY = caretPos.y;

            _lastCaret = caret;
        }

        // Compute new scroll position
        // Max value is the real area height - 1 visible area
        _scrollY = Mathf.Clamp(_scrollY, 0f, Mathf.Max(0f, realTextAreaHeight - area.height));
        ____textScrollY = _scrollY;
        var sy = _scrollY;

        // Initial code from original method
        be.PushClipRect(area);
        if (focused && selB > selA)
        {
            be.TextSelectionRects(fontIndex, 0f, FontStyle.Normal, text, wrapWidth, selA, selB, ____selectionRects,
                ____fieldRichText);
            foreach (var selectionRect in ____selectionRects)
                be.FillRect(new Rect(area.x + selectionRect.x, area.y + selectionRect.y - sy, selectionRect.width,
                    selectionRect.height), new Color(0.3f, 0.45f, 0.75f, 0.55f));
        }

        be.DrawText(new Rect(area.x, area.y - sy, area.width, area.height + sy), text, fontIndex, 0f,
            FontStyle.Normal, TextAnchor.UpperLeft, wrapWidth > 0f, textColor, new Rect(), ____fieldRichText);

        if (compB > compA)
        {
            be.TextSelectionRects(fontIndex, 0f, FontStyle.Normal, text, wrapWidth, compA, compB, ____selectionRects,
                ____fieldRichText);
            foreach (var selectionRect in ____selectionRects)
                be.FillRect(new Rect(area.x + selectionRect.x,
                    area.y + selectionRect.y + selectionRect.height - 1f - sy, selectionRect.width, 1f), textColor);
        }

        if (focused)
        {
            be.FillRect(new Rect(area.x + caretPos.x, area.y + caretPos.y - sy, 1.5f, lineHeight),
                new Color(0.92f, 0.92f, 0.92f, 0.9f));
            ImeComposition.SetCandidatePos(
                be.GuiToScreenPoint(new Vector2(area.x + caretPos.x, area.y + caretPos.y + lineHeight - sy)));
        }

        be.PopClipRect();

        return false;
    }
}