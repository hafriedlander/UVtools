﻿using System;
using Avalonia.Markup.Xaml;
using UVtools.Core;
using UVtools.Core.Layers;
using UVtools.Core.Operations;
using UVtools.WPF.Windows;

namespace UVtools.WPF.Controls.Tools
{
    public class ToolLayerRemoveControl : ToolControl
    {
        public OperationLayerRemove Operation => BaseOperation as OperationLayerRemove;

        public uint ExtraLayers => (uint)Math.Max(0, (int)Operation.LayerIndexEnd - Operation.LayerIndexStart + 1);

        public string InfoLayersStr
        {
            get
            {
                uint extraLayers = ExtraLayers;
                return $"Layers: {SlicerFile.LayerCount} → {SlicerFile.LayerCount - extraLayers} (- {extraLayers})";
            }
        }

        public string InfoHeightsStr
        {
            get
            {
                float extraHeight = Layer.RoundHeight(ExtraLayers * SlicerFile.LayerHeight);
                return $"Height: {SlicerFile.PrintHeight}mm → {Layer.RoundHeight(App.SlicerFile.PrintHeight - extraHeight)}mm (- {extraHeight}mm)";
            }
        }

        public ToolLayerRemoveControl()
        {
            BaseOperation = new OperationLayerRemove(SlicerFile);
            if (!ValidateSpawn()) return;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Callback(ToolWindow.Callbacks callback)
        {
            switch (callback)
            {
                case ToolWindow.Callbacks.Init:
                case ToolWindow.Callbacks.Loaded:
                    Operation.PropertyChanged += (sender, args) =>
                    {
                        RaisePropertyChanged(nameof(InfoLayersStr));
                        RaisePropertyChanged(nameof(InfoHeightsStr));
                    };
                    break;
            }
        }
    }
}
