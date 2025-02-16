﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.ComponentModel;
using System.Text;
using UVtools.Core.FileFormats;

namespace UVtools.Core.Operations
{
    [Serializable]
    public class OperationFadeExposureTime : Operation
    {
        #region Members

        private uint _layerCount = 10;
        private decimal _fromExposureTime;
        private decimal _toExposureTime;

        #endregion

        #region Overrides
        public override Enumerations.LayerRangeSelection StartLayerRangeSelection => Enumerations.LayerRangeSelection.Normal;
        public override bool LayerIndexEndEnabled => false;
        public override string Title => "Fade exposure time";

        public override string Description =>
            "Fade the exposure time in increments from a start to a end value on the selected layer range.";

        public override string ConfirmationText =>
            $"fade exposure time model layers {LayerIndexStart} through {LayerIndexEnd} with increments of {IncrementValue}s";

        public override string ProgressTitle =>
            $"Fading exposure time from layers {LayerIndexStart} to {LayerIndexEnd} with increments of {IncrementValue}s";

        public override string ProgressAction => "Faded layers";

        public override string ValidateSpawn()
        {
            if (!SlicerFile.CanUseLayerExposureTime)
            {
                return NotSupportedMessage;
            }

            return null;
        }

        public override string ValidateInternally()
        {
            var sb = new StringBuilder();

            if (_layerCount == 0) sb.AppendLine("The layer count must be higher than 0.");
            if(_fromExposureTime == _toExposureTime) sb.AppendLine("The starting exposure time can't be the same as the ending exposure time.");

            return sb.ToString();
        }

        public override string ToString()
        {
            var result = $"[Layers: {LayerRangeCount} From: {_fromExposureTime}s To: {_toExposureTime}s @ {IncrementValue}s] " + LayerRangeString;
            if (!string.IsNullOrEmpty(ProfileName)) result = $"{ProfileName}: {result}";
            return result;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerIndexStart))
            {
                LayerCount = _layerCount; // Sanitize
                LayerIndexEnd = LayerIndexStart + _layerCount - 1 ; // Sync
                RaisePropertyChanged(nameof(MaximumLayerCount));
                RaisePropertyChanged(nameof(IncrementValue));
            }
            /*else if (e.PropertyName == nameof(LayerIndexEnd))
            {
                LayerCount = LayerRangeCount;
                RaisePropertyChanged(nameof(IncrementValue));
            }*/

            base.OnPropertyChanged(e);
        }

        #endregion

        #region Properties

        public uint LayerCount
        {
            get => _layerCount;
            set
            {
                if (!RaiseAndSetIfChanged(ref _layerCount, Math.Min(value, SlicerFile.LayerCount - LayerIndexStart))) return;
                LayerIndexEnd = LayerIndexStart + _layerCount - 1;
                RaisePropertyChanged(nameof(MaximumLayerCount));
                RaisePropertyChanged(nameof(IncrementValue));
            }
        }

        public uint MaximumLayerCount => Math.Max(LayerCount, SlicerFile.LayerCount - LayerIndexStart);

        public decimal FromExposureTime
        {
            get => _fromExposureTime;
            set
            {
                if(!RaiseAndSetIfChanged(ref _fromExposureTime, Math.Round(value, 2))) return;
                RaisePropertyChanged(nameof(IncrementValue));
            }
        }

        public decimal ToExposureTime
        {
            get => _toExposureTime;
            set
            {
                if(!RaiseAndSetIfChanged(ref _toExposureTime, Math.Round(value, 2))) return;
                RaisePropertyChanged(nameof(IncrementValue));
            }
        }

        public decimal IncrementValue => Math.Round(IncrementValueRaw, 2);
        public decimal IncrementValueRaw => (_toExposureTime - _fromExposureTime) / (LayerRangeCount + 1);

        #endregion

        #region Constructor

        public OperationFadeExposureTime() { }

        public OperationFadeExposureTime(FileFormat slicerFile) : base(slicerFile) { }

        public override void InitWithSlicerFile()
        {
            base.InitWithSlicerFile();
            if (_fromExposureTime <= 0) _fromExposureTime = (decimal)SlicerFile.BottomExposureTime;
            if (_toExposureTime <= 0) _toExposureTime = (decimal)SlicerFile.ExposureTime;

            LayerIndexEnd = LayerIndexStart + _layerCount - 1; // Sync
        }

        #endregion

        #region Equality

        protected bool Equals(OperationFadeExposureTime other)
        {
            return _fromExposureTime == other._fromExposureTime && _toExposureTime == other._toExposureTime && _layerCount == other._layerCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationFadeExposureTime)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_fromExposureTime, _toExposureTime, _layerCount);
        }

        #endregion

        #region Methods

        protected override bool ExecuteInternally(OperationProgress progress)
        {
            LayerIndexEnd = LayerIndexStart + _layerCount - 1; // Sanitize

            var increment = IncrementValueRaw;
            var exposure = _fromExposureTime;
            for (uint layerIndex = LayerIndexStart; layerIndex <= LayerIndexEnd; layerIndex++)
            {
                progress.Token.ThrowIfCancellationRequested();
                exposure += increment;
                SlicerFile[layerIndex].ExposureTime = (float)exposure;
            }

            return !progress.Token.IsCancellationRequested;
        }

        #endregion
    }
}
