#region Using declarations
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.AtmStrategy;
#endregion

namespace NinjaTrader.NinjaScript.AddOns
{
    public class QuickMouseEntryATM : AddOnBase
    {
        private QuantityUpDown quantitySelector;
        private AtmStrategySelector atmSelector;
        private ToolBar toolBar;
        private Window currentWindow;
        private ChartControl currentChart;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Independent per-chart mouse entry: Alt+B Buy / Alt+S Sell with selected ATM";
                Name        = "QuickMouseEntryATM";
                IsEnabled   = true;
            }
        }

        protected override void OnWindowCreated(Window window)
        {
            if (window == null) return;

            currentWindow = window;
            currentWindow.KeyDown += OnGlobalKeyDown;

            CreateToolbar(window);
        }

        private void CreateToolbar(Window window)
        {
            quantitySelector = new QuantityUpDown { Value = 1, Minimum = 1 };
            atmSelector = new AtmStrategySelector();
            atmSelector.Id = Guid.NewGuid().ToString("N");

            toolBar = new ToolBar();
            toolBar.Items.Add(new Label { Content = "Qty:" });
            toolBar.Items.Add(quantitySelector);
            toolBar.Items.Add(new Label { Content = "ATM:" });
            toolBar.Items.Add(atmSelector);

            // Note: Toolbar insertion into chart may need additional visual tree code
        }

        private void OnGlobalKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0) return;
            if (currentWindow == null || !currentWindow.IsActive) return;

            currentChart = GetActiveChartControl(currentWindow);
            if (currentChart == null) return;

            double priceLevel = currentChart.ActiveChartScale.GetValueByY((float)currentChart.MouseDownPoint.Y);

            if (e.Key == Key.B)
            {
                PlaceEntryOrder(true, priceLevel);
                e.Handled = true;
            }
            else if (e.Key == Key.S)
            {
                PlaceEntryOrder(false, priceLevel);
                e.Handled = true;
            }
        }

        private void PlaceEntryOrder(bool isBuy, double priceLevel)
        {
            if (priceLevel <= 0 || currentChart?.Instrument == null) return;

            OrderAction action = isBuy ? OrderAction.Buy : OrderAction.Sell;
            OrderType orderType = (priceLevel > currentChart.Close[0]) ? OrderType.StopMarket : OrderType.Limit;

            string atmName = atmSelector?.SelectedAtmStrategy?.Name;

            try
            {
                Order order = Account.SubmitOrder(
                    currentChart.Instrument.FullName,
                    action,
                    orderType,
                    quantitySelector?.Value ?? 1,
                    0,
                    priceLevel,
                    Guid.NewGuid().ToString(),
                    "QuickMouseEntry"
                );

                if (order != null && !string.IsNullOrEmpty(atmName))
                {
                    AtmStrategy.StartAtmStrategy(atmName, order);
                    Print($"[{currentChart.Instrument.FullName}] {action} {orderType} @ {priceLevel} | Qty: {quantitySelector?.Value} | ATM: {atmName}");
                }
            }
            catch (Exception ex)
            {
                Print("Order error: " + ex.Message);
            }
        }

        private ChartControl GetActiveChartControl(Window window)
        {
            // Refine this if needed
            return window.FindVisualChild<ChartControl>() ?? null;
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Default ATM Template", GroupName = "Settings", Order = 1)]
        public string DefaultAtmTemplate { get; set; } = "";

        [NinjaScriptProperty]
        [Display(Name = "Default Quantity", GroupName = "Settings", Order = 2)]
        [Range(1, 10000)]
        public int DefaultQuantity { get; set; } = 1;
        #endregion
    }
}