using System.Collections.Generic;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

namespace PixiEditor.Helpers;

#nullable enable
internal class ChangeInfoListOptimizer
{
    public static List<IChangeInfo> Optimize(List<IChangeInfo?> input)
    {
        List<IChangeInfo> output = new();
        bool selectionInfoOccured = false;
        // discard all Selection_ChangeInfos apart from the last one
        for (int i = input.Count - 1; i >= 0; i--)
        {
            IChangeInfo? info = input[i];
            if (info is null)
                continue;
            if (info is Selection_ChangeInfo && !selectionInfoOccured)
                selectionInfoOccured = true;
            else if (info is Selection_ChangeInfo)
                continue;
            output.Add(info);
        }

        output.Reverse();
        return output;
    }
}
