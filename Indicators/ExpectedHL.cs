#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class ExpectedHL : Indicator
    {
        private List<DailyPivotData> dailyHistory = new List<DailyPivotData>();
        private Dictionary<string, double> currentProbabilities = new Dictionary<string, double>();
        
        private string[] levels = { "S3", "S2", "S1", "R1", "R2", "R3" };

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = "Expected High/Low Pivot Probabilities";
                Name                                        = "ExpectedHL";
                Calculate                                   = Calculate.OnPriceChange;
                IsOverlay                                   = true;
                DisplayInDataBox                            = false;
                DrawOnPricePanel                            = true;
                IsSuspendedWhileInactive                    = true;

                LookbackDays                                = 500;
                UseConditionalOnHit                         = true;
                HitLabelFontSize                            = 11;
                LeftLabelFontSize                           = 10;
            }
        }

        [NinjaScriptProperty]
        [Display(Name="Lookback Days", GroupName="Parameters", Order=1)]
        public int LookbackDays { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Use Conditional (On Hit Only)", GroupName="Parameters", Order=2)]
