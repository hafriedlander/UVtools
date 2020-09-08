﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using UVtools.Core.Obects;

namespace UVtools.Core.Operations
{
    public sealed class OperationLayerImport : Operation
    {
        public Size FileResolution { get; }
        public uint InsertAfterLayerIndex { get; set; }
        public bool ReplaceStartLayer { get; set; }
        public bool ReplaceSubsequentLayers { get; set; }
        public bool DiscardRemainingLayers { get; set; }

        public List<string> Files { get; } = new List<string>();

        public int Count => Files.Count;

        public override string Title => "Import Layer(s)";

        public override string Description =>
            "Import layer(s) from local file(s) into the model at a selected height.\n" +
            "NOTE: Images must respect file resolution and in greyscale color.";

        public override string ConfirmationText => $"import {Count} layer(s)?";

        public OperationLayerImport(Size fileResolution)
        {
            FileResolution = fileResolution;
        }

        public void Sort()
        {
            Files.Sort((file1, file2) => string.Compare(Path.GetFileNameWithoutExtension(file1), Path.GetFileNameWithoutExtension(file2), StringComparison.Ordinal));
        }

        public override StringTag Validate(params object[] parameters)
        {
            var result = new ConcurrentBag<string>();
            Parallel.ForEach(Files, file =>
            {
                using (Mat mat = CvInvoke.Imread(file, ImreadModes.AnyColor))
                {
                    if (mat.Size != FileResolution)
                    {
                        result.Add(file);
                    }
                }
            });

            if (result.IsEmpty) return null;
            var message = new StringBuilder();
            message.AppendLine($"The following {result.Count} files mismatched the slice resolution of {FileResolution.Width} x {FileResolution.Height}:");
            message.AppendLine();
            uint count = 0;
            foreach (var file in result)
            {
                count++;
                if (count == 20)
                {
                    message.AppendLine("... To many to show ...");
                    break;
                }
                message.AppendLine(Path.GetFileNameWithoutExtension(file));
            }

            return new StringTag(message.ToString(), result);
        }

        public uint CalculateTotalLayers(uint totalLayers)
        {
            if (DiscardRemainingLayers)
            {
                return (uint) (1 + InsertAfterLayerIndex + Files.Count - (ReplaceStartLayer ? 1 : 0));
            }
            if (ReplaceSubsequentLayers)
            {
                uint result = (uint) (1 + InsertAfterLayerIndex + Files.Count - (ReplaceStartLayer ? 1 : 0));
                return result <= totalLayers ? totalLayers : result;
            }

            return (uint)(totalLayers + Files.Count - (ReplaceStartLayer ? 1 : 0));
        }

        public uint StartLayerIndex => InsertAfterLayerIndex + (ReplaceStartLayer ? 0u : 1);
        public uint EndLayerIndex => (uint) (StartLayerIndex + Count - 1);
    }
}
