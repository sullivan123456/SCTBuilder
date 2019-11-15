﻿using System;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace SCTBuilder
{
    class CycleInfo
    {
        private readonly static DateTime Start1506 = Convert.ToDateTime("05/28/2015");
        public static int AIRAC = 1506;
        public static DateTime CycleStart;
        public static DateTime CycleEnd;

        public static bool GetINIfile()
        ///<summary>
        /// Reads the INI file.  If corrupted, resets the INI file to startup.
        /// Returns a boolean reporting a successful read. ANY error = false.
        /// </summary>
        {
            bool result = true;
            if (File.Exists(FolderMgt.INIfile))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(FolderMgt.INIfile))
                    {
                        string Message = sr.ReadLine();
                        if (VersionInfo.Title != Message)
                        {
                            Message = "INI file version " + Message + " differs from program version " +
                                VersionInfo.Title;
                            MessageBoxButtons buttons = MessageBoxButtons.OK;
                            MessageBoxIcon icon = MessageBoxIcon.Warning;
                            string caption = VersionInfo.Title;
                            MessageBox.Show(Message, caption, buttons, icon);
                        }
                        AIRAC = Convert.ToInt16(sr.ReadLine());
                        CycleStart = Convert.ToDateTime(sr.ReadLine());
                        CycleEnd = Convert.ToDateTime(sr.ReadLine());
                        FolderMgt.DataFolder = sr.ReadLine();
                        // Test for valid Data file folder
                        if (!Directory.Exists(FolderMgt.DataFolder))
                        {
                            FolderMgt.DataFolder = string.Empty;
                            result = false;
                        }
                        FolderMgt.OutputFolder = sr.ReadLine();
                        if (!Directory.Exists(FolderMgt.OutputFolder))
                        {
                            FolderMgt.OutputFolder = string.Empty;
                        }
                        InfoSection.SponsorARTCC = sr.ReadLine();
                        InfoSection.DefaultAirport = Conversions.RevICOA(sr.ReadLine());
                        InfoSection.FacilityEngineer = sr.ReadLine();
                        if (InfoSection.FacilityEngineer.ToString().Length == 0)
                            result = false;
                        InfoSection.AsstFacilityEngineer = sr.ReadLine();
                    }
                    if (!result)
                    {
                        string Message = "Invalid data found in initialization file." +
                            Environment.NewLine + "You will be prompted to obtain and download data file";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                        MessageBox.Show(Message, VersionInfo.Title, buttons, icon);
                        ResetCycleInfo();
                        result = false;
                    }
                }
                catch
                {
                    string Message = "Invalid data found in initialization file." +
                        Environment.NewLine + "You will be prompted to obtain and download data file";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                    MessageBox.Show(Message, VersionInfo.Title, buttons, icon);
                    ResetCycleInfo();
                    result = false;
                }
            }
            else
            // No INI file exists
            {
                ResetCycleInfo();
                string Message = "No initialization file found." +
                    Environment.NewLine + "You will be prompted to obtain and download data file";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                MessageBox.Show(Message, VersionInfo.Title, buttons, icon);
                result = false;
            }
            return result;

        }
        private static void ResetCycleInfo()
        {
            AIRAC = 1503;
            CycleStart = Convert.ToDateTime("1/1/1900");
            CycleEnd = Convert.ToDateTime("1/2/1900");
            FolderMgt.OutputFolder = string.Empty;
            FolderMgt.DataFolder = string.Empty;
            InfoSection.SponsorARTCC = string.Empty;
            InfoSection.DefaultAirport = string.Empty;
            InfoSection.FacilityEngineer = "Facilities Engineer name";
            InfoSection.AsstFacilityEngineer = "Ass't  Engineer name";
        }

        public static int FindAIRAC(DateTime Cycledate, bool SetCycleInfo = false)
        /// <summary>
        /// Return the AIRAC info from any given date
        /// And populates the AIRAC cycle information
        /// At 1506 cycles became 28 days (was 34).
        /// </summary>
        {
            DateTime WorkingDate = Start1506;
            int CycleInterval = 28;
            int CycleYear = WorkingDate.Year;
            int iCounter = 0;
            // Must know if future or past AIRAC
            while (WorkingDate < Cycledate)
            {
                WorkingDate = WorkingDate.AddDays(CycleInterval);
                if (CycleYear != WorkingDate.Year)
                {
                    iCounter = 1;
                    CycleYear = WorkingDate.Year;
                }
                else
                {
                    iCounter += 1;
                }
            }
            if (SetCycleInfo)
            {
                AIRAC = CalcAIRAC(Convert.ToInt32(Convert.ToString(CycleYear).Substring(2, 2)), (iCounter));
                CycleStart = WorkingDate;
                CycleEnd = WorkingDate.AddDays(CycleInterval);
            }
            // Console.WriteLine("FindAIRAC result: " + CalcAIRAC(Convert.ToInt32(Convert.ToString(CycleYear).Substring(2, 2)), (iCounter)));
            return CalcAIRAC(Convert.ToInt32(Convert.ToString(CycleYear).Substring(2, 2)), (iCounter));
        }
        public static DateTime FindCycle(int reqAIRAC, bool SetCycleInfo = false)
            // Returns the beginning cycle date for a given AIRAC
        {
            DateTime WorkingDate = Start1506;
            int CycleInterval = 28;
            int CycleYear = WorkingDate.Year;
            int iCounter = 1;
            int WorkingAIRAC = CalcAIRAC(CycleYear, iCounter);

            while (WorkingAIRAC < reqAIRAC)
            {
                WorkingDate = WorkingDate.AddDays(CycleInterval);
                if (CycleYear != WorkingDate.Year)
                {
                    iCounter = 1;
                    CycleYear = WorkingDate.Year;
                    WorkingAIRAC = CalcAIRAC(WorkingDate.Year, iCounter);
                }
                else
                {
                    iCounter += 1;
                    WorkingAIRAC = CalcAIRAC(WorkingDate.Year, iCounter);
                }
            }
            if (SetCycleInfo)
            {
                AIRAC = WorkingAIRAC;
                CycleStart = WorkingDate;
                CycleEnd = WorkingDate.AddDays(CycleInterval);
            }
            Console.WriteLine("FindCycle result: " + Convert.ToDateTime(WorkingDate));
            return WorkingDate;
        }
        private static int CalcAIRAC(int aYear, int aCounter)
        {
            return Convert.ToInt32((Extensions.Right(aYear.ToString(), 2)) + aCounter.ToString("D2").ToString());
        }
        public static string BuildCycleText()
        {
            string Message =
                "AIRAC Cycle: " + CycleInfo.AIRAC + Environment.NewLine +
                "Cycle Start:    " + CycleInfo.CycleStart.ToString("dd MMM yyyy") + Environment.NewLine +
                "Cycle End:     " + CycleInfo.CycleEnd.ToString("dd MMM yyyy");
            if (CycleInfo.CycleEnd < DateTime.Today)
            {
                Message = Message + Environment.NewLine + "*** Outdated Cycle Data ***";
            }
            // Console.WriteLine(Message + Environment.NewLine);
            return Message;
        }
    }
}
