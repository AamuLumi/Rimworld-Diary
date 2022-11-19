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

namespace Diary
{
    public enum ExportFormat
    {
        Text,
        RTF,
    }
    public enum DefaultMessage
    {
        NoEntryFound,
        Empty,
    }
    public enum LogFilter
    {
        All,
        Events,
        Chats,
    }
    public enum LogWriterFilter
    {
        All,
        Events,
        Chats,
        None,
    }

    public class DiaryTypeTools
    {
        public static string GetFormatName(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Text: return "Diary_Format_Text".Translate();
                case ExportFormat.RTF: return "Diary_Format_RTF".Translate();
                default: return "";
            }
        }

        public static string GetDefaultMessageName(DefaultMessage m)
        {
            switch (m)
            {
                case DefaultMessage.Empty: return "Diary_Empty".Translate();
                case DefaultMessage.NoEntryFound: return "Diary_No_Entry_Found_Message".Translate();
                default: return "";
            }
        }

        public static string GetLogFilterName(LogFilter f)
        {
            switch (f)
            {
                case LogFilter.All: return "Diary_All".Translate();
                case LogFilter.Chats: return "Diary_Chats".Translate();
                case LogFilter.Events: return "Diary_Events".Translate();
                default: return "";
            }
        }

        public static string GetLogWriterFilterName(LogWriterFilter f)
        {
            switch (f)
            {
                case LogWriterFilter.All: return "Diary_All".Translate();
                case LogWriterFilter.Chats: return "Diary_Chats".Translate();
                case LogWriterFilter.Events: return "Diary_Events".Translate();
                case LogWriterFilter.None: return "Diary_None".Translate();
                default: return "";
            }
        }
    }
}
