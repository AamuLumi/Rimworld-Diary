using System;
using System.Collections.Generic;
using System.IO;

namespace RTFExporter
{
  /// <summary>
  /// The RTF parser class, it transform the RTFdocument into a file or string
  /// </summary>
  public class RTFParser
  {
    public static RTFDocument document;
    private static Dictionary<string, int> fontsIndex = new Dictionary<string, int>();

    /// <summary>
    /// Create or rewrite a file with RTF content
    /// <seealso cref="RTExporter.RTFDocument">
    /// </summary>
    /// <param name="path">The folder path with filename</param>
    /// <param name="document">The RTF document to save</param>
    public static void ToFile(string path, RTFDocument document)
    {
      document.SetFile(path);
      document.Save();
      document.Close();
    }

    /// <summary>
    /// Write a content straight to a file
    /// </summary>
    /// <param name="path">The folder path with filename</param>
    /// <param name="document">The file content</param>
    public static void ToFile(string path, string content)
    {
      using(FileStream fs = new FileStream(path, FileMode.Create))
      {
        using(StreamWriter writer = new StreamWriter(fs))
        {
          writer.Write(content);
        }
      }
    }

    /// <summary>
    /// Return a rich text formatted string from a RTF document object
    /// <seealso cref="RTExporter.RTFDocument">
    /// </summary>
    /// <param name="document">The RTF document to be formatted</param>
    /// <returns>A rich text formatted string</returns>
    public static string ToString(RTFDocument document)
    {
      RTFParser.document = document;

      string str = "{\\rtf1\\ansi\\deff0";

      foreach (RTFParagraph paragraph in document.paragraphs)
      {
        foreach (RTFText text in paragraph.text)
        {
          document.colors.Add(text.style.color);

          if (text.style.fontFamily != string.Empty)
          {
            document.fonts.Add(text.style.fontFamily);
          }
        }
      }

      str += FontsParsing();
      str += ColorParsing();

      str += "{\\info {\\author " + document.author + "}";
      DateTime date = DateTime.Now;
      str += "{\\creatim\\yr" + date.Year + "\\mo" + date.Month + "\\dy" + date.Day + "\\hr" + date.Hour + "\\min" + date.Minute + "}";
      str += "{\\version" + document.version + "}";
      str += "{\\edmins0}";
      str += "{\\nofpages1}";
      str += "{\\nofwords0}";
      str += "{\\nofchars0}";
      str += "}";

      str += "{\\keywords ";

      foreach (string keyword in document.keywords)
      {
        str += keyword + " ";
      }

      str += "}";

      switch (document.orientation)
      {
        case Orientation.Landscape:
          str += "\\landscape";
          break;
        case Orientation.Portrait:
          str += "\\portrait";
          break;
      }

      str += "\\paperw" + value(document.width) + "\\paperh" + value(document.height) +
        "\\margl" + value(document.margin.left) + "\\margr" + value(document.margin.right) +
        "\\margt" + value(document.margin.top) + "\\margb" + value(document.margin.bottom) + " ";

      str += ParagraphParsing();

      str += "}";
      return str;
    }

    private static string FontsParsing()
    {
      List<string> fonts = new List<string>();

      foreach (string docFonts in document.fonts)
      {
        var add = true;

        foreach (string font in fonts)
        {
          if (font == docFonts)
          {
            add = false;
            break;
          }
        }

        if (add)
        {
          fonts.Add(docFonts);
        }
      }

      string str = "{\\fonttbl";

      for (int i = 0; i < fonts.Count; i++)
      {
        str += "{\\f" + i + " " + fonts[i] + ";}";
        try
        {
          fontsIndex.Add(fonts[i], i);
        }
        catch
        {
          // Font repeated
        }
      }

      str += "}";

      return str;
    }

    private static string ColorParsing()
    {
      List<Color> colors = new List<Color>();
      int j = 1;

      for (int i = 0; i < document.colors.Count; i++)
      {
        var add = true;

        foreach (Color color in colors)
        {
          if (color.r == document.colors[i].r && color.g == document.colors[i].g && color.b == document.colors[i].b)
          {
            add = false;
            break;
          }
        }

        if (add)
        {
          document.colors[i].index = j;
          j++;

          colors.Add(document.colors[i]);
        }
      }

      string str = "{\\colortbl;";

      for (int i = 0; i < colors.Count; i++)
      {
        str += "\\red" + colors[i].r + "\\green" + colors[i].g + "\\blue" + colors[i].b + ";";
      }

      str += "}";

      return str;
    }

    private static string ParagraphParsing()
    {
      string str = "";

      foreach (RTFParagraph paragraph in document.paragraphs)
      {
        str += "\\pard";
        str += "\\sb" + paragraph.style.spaceBefore;
        str += "\\sa" + paragraph.style.spaceAfter;

        switch (paragraph.style.alignment)
        {
          case Alignment.Left:
            str += "\\ql";
            break;
          case Alignment.Right:
            str += "\\qr";
            break;
          case Alignment.Center:
            str += "\\qc";
            break;
          case Alignment.Justified:
            str += "\\qj";
            break;
        }

        str += "\\fi" + value(paragraph.style.indent.firstLine);
        str += "\\li" + value(paragraph.style.indent.left);
        str += "\\ri" + value(paragraph.style.indent.right);

        foreach (RTFText text in paragraph.text)
        {
          str += "\\plain ";

          if (text.style.italic)
          {
            str += "\\i ";
          }
          if (text.style.bold)
          {
            str += "\\b ";
          }
          if (text.style.smallCaps)
          {
            str += "\\scaps ";
          }
          if (text.style.allCaps)
          {
            str += "\\caps ";
          }
          if (text.style.strikeThrough)
          {
            str += "\\strike ";
          }
          if (text.style.outline)
          {
            str += "\\outl ";
          }

          switch (text.style.underline)
          {
            case Underline.Dash:
              str += "\\uldash ";
              break;
            case Underline.DotDash:
              str += "\\uldashd ";
              break;
            case Underline.Dotted:
              str += "\\uld ";
              break;
            case Underline.Double:
              str += "\\uldb ";
              break;
            case Underline.Thick:
              str += "\\ulth ";
              break;
            case Underline.Basic:
              str += "\\ul ";
              break;
            case Underline.Wave:
              str += "\\ulwave ";
              break;
            case Underline.WordsOnly:
              str += "\\ulw ";
              break;
          }

          str += "\\fs" + (2 * text.style.fontSize) + " ";
          str += "\\f" + fontsIndex[text.style.fontFamily] + " ";
          str += "\\cf" + text.style.color.index + " ";

          text.content = text.content.Replace("\n", "\\line ");
          text.content = text.content.Replace("\t", "\\tab ");
          text.content = text.content.Replace("<i>", "\\i ");
          text.content = text.content.Replace("</i>", "\\i0 ");
          text.content = text.content.Replace("<b>", "\\b ");
          text.content = text.content.Replace("</b>", "\\b0 ");
          text.content = text.content.Replace("???", "\\'80");
          text.content = text.content.Replace("???", "\\'82");
          text.content = text.content.Replace("??", "\\'83");
          text.content = text.content.Replace("???", "\\'84");
          text.content = text.content.Replace("???", "\\'85");
          text.content = text.content.Replace("???", "\\'86");
          text.content = text.content.Replace("???", "\\'87");
          text.content = text.content.Replace("??", "\\'88");
          text.content = text.content.Replace("???", "\\'89");
          text.content = text.content.Replace("??", "\\'8A");
          text.content = text.content.Replace("???", "\\'8B");
          text.content = text.content.Replace("??", "\\'8C");
          text.content = text.content.Replace("??", "\\'8E");
          text.content = text.content.Replace("???", "\\'91");
          text.content = text.content.Replace("???", "\\'92");
          text.content = text.content.Replace("???", "\\'93");
          text.content = text.content.Replace("???", "\\'94");
          text.content = text.content.Replace("???", "\\'95");
          text.content = text.content.Replace("???", "\\'96");
          text.content = text.content.Replace("???", "\\'97");
          text.content = text.content.Replace("??", "\\'98");
          text.content = text.content.Replace("???", "\\'99");
          text.content = text.content.Replace("??", "\\'9A");
          text.content = text.content.Replace("???", "\\'9B");
          text.content = text.content.Replace("??", "\\'9C");
          text.content = text.content.Replace("??", "\\'9E");
          text.content = text.content.Replace("??", "\\'9F");
          text.content = text.content.Replace("??", "\\'A1");
          text.content = text.content.Replace("??", "\\'A2");
          text.content = text.content.Replace("??", "\\'A3");
          text.content = text.content.Replace("??", "\\'A4");
          text.content = text.content.Replace("??", "\\'A5");
          text.content = text.content.Replace("??", "\\'A6");
          text.content = text.content.Replace("??", "\\'A7");
          text.content = text.content.Replace("??", "\\'A8");
          text.content = text.content.Replace("??", "\\'A9");
          text.content = text.content.Replace("??", "\\'AA");
          text.content = text.content.Replace("??", "\\'AB");
          text.content = text.content.Replace("??", "\\'AC");
          text.content = text.content.Replace("??", "\\'AE");
          text.content = text.content.Replace("??", "\\'AF");
          text.content = text.content.Replace("??", "\\'B0");
          text.content = text.content.Replace("??", "\\'B1");
          text.content = text.content.Replace("??", "\\'B2");
          text.content = text.content.Replace("??", "\\'B3");
          text.content = text.content.Replace("??", "\\'B4");
          text.content = text.content.Replace("??", "\\'B5");
          text.content = text.content.Replace("??", "\\'B6");
          text.content = text.content.Replace("??", "\\'B7");
          text.content = text.content.Replace("??", "\\'B8");
          text.content = text.content.Replace("??", "\\'B9");
          text.content = text.content.Replace("??", "\\'BA");
          text.content = text.content.Replace("??", "\\'BB");
          text.content = text.content.Replace("??", "\\'BC");
          text.content = text.content.Replace("??", "\\'BD");
          text.content = text.content.Replace("??", "\\'BE");
          text.content = text.content.Replace("??", "\\'BF");
          text.content = text.content.Replace("??", "\\'C0");
          text.content = text.content.Replace("??", "\\'C1");
          text.content = text.content.Replace("??", "\\'C2");
          text.content = text.content.Replace("??", "\\'C3");
          text.content = text.content.Replace("??", "\\'C4");
          text.content = text.content.Replace("??", "\\'C5");
          text.content = text.content.Replace("??", "\\'C6");
          text.content = text.content.Replace("??", "\\'C7");
          text.content = text.content.Replace("??", "\\'C8");
          text.content = text.content.Replace("??", "\\'C9");
          text.content = text.content.Replace("??", "\\'CA");
          text.content = text.content.Replace("??", "\\'CB");
          text.content = text.content.Replace("??", "\\'CC");
          text.content = text.content.Replace("??", "\\'CD");
          text.content = text.content.Replace("??", "\\'CE");
          text.content = text.content.Replace("??", "\\'CF");
          text.content = text.content.Replace("??", "\\'D0");
          text.content = text.content.Replace("??", "\\'D1");
          text.content = text.content.Replace("??", "\\'D2");
          text.content = text.content.Replace("??", "\\'D3");
          text.content = text.content.Replace("??", "\\'D4");
          text.content = text.content.Replace("??", "\\'D5");
          text.content = text.content.Replace("??", "\\'D6");
          text.content = text.content.Replace("??", "\\'D7");
          text.content = text.content.Replace("??", "\\'D8");
          text.content = text.content.Replace("??", "\\'D9");
          text.content = text.content.Replace("??", "\\'DA");
          text.content = text.content.Replace("??", "\\'DB");
          text.content = text.content.Replace("??", "\\'DC");
          text.content = text.content.Replace("??", "\\'DD");
          text.content = text.content.Replace("??", "\\'DE");
          text.content = text.content.Replace("??", "\\'DF");
          text.content = text.content.Replace("??", "\\'E0");
          text.content = text.content.Replace("??", "\\'E1");
          text.content = text.content.Replace("??", "\\'E2");
          text.content = text.content.Replace("??", "\\'E3");
          text.content = text.content.Replace("??", "\\'E4");
          text.content = text.content.Replace("??", "\\'E5");
          text.content = text.content.Replace("??", "\\'E6");
          text.content = text.content.Replace("??", "\\'E7");
          text.content = text.content.Replace("??", "\\'E8");
          text.content = text.content.Replace("??", "\\'E9");
          text.content = text.content.Replace("??", "\\'EA");
          text.content = text.content.Replace("??", "\\'EB");
          text.content = text.content.Replace("??", "\\'EC");
          text.content = text.content.Replace("??", "\\'ED");
          text.content = text.content.Replace("??", "\\'EE");
          text.content = text.content.Replace("??", "\\'EF");
          text.content = text.content.Replace("??", "\\'F0");
          text.content = text.content.Replace("??", "\\'F1");
          text.content = text.content.Replace("??", "\\'F2");
          text.content = text.content.Replace("??", "\\'F3");
          text.content = text.content.Replace("??", "\\'F4");
          text.content = text.content.Replace("??", "\\'F5");
          text.content = text.content.Replace("??", "\\'F6");
          text.content = text.content.Replace("??", "\\'F7");
          text.content = text.content.Replace("??", "\\'F8");
          text.content = text.content.Replace("??", "\\'F9");
          text.content = text.content.Replace("??", "\\'FA");
          text.content = text.content.Replace("??", "\\'FB");
          text.content = text.content.Replace("??", "\\'FC");
          text.content = text.content.Replace("??", "\\'FD");
          text.content = text.content.Replace("??", "\\'FE");
          text.content = text.content.Replace("??", "\\'FF");

          str += text.content;
        }

        str += "\\par ";
      }

      return str;
    }

    private static int value(float i)
    {
      float result = 0;

      switch (document.units)
      {
        case Units.Inch:
          result = i * 1440;
          break;
        case Units.Millimeters:
          result = (i / 25.4f) * 1440;
          break;
        case Units.Centimeters:
          result = (i / 2.54f) * 1440;
          break;
      }

      return (int) Math.Ceiling(result);
    }
  }
}
