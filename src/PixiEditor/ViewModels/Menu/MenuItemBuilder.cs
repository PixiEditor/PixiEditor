﻿using System.Collections.Generic;
using Avalonia.Controls;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.UI;

namespace PixiEditor.ViewModels.Menu;

internal abstract class MenuItemBuilder
{
    public abstract void ModifyMenuTree(ICollection<MenuItem> tree);

    protected bool TryFindMenuItem(ICollection<MenuItem> tree, string header, out MenuItem? menuItem)
    {
        foreach (var item in tree)
        {
            if (item.Header is LocalizedString localizedString && localizedString.Key == header)
            {
                menuItem = item;
                return true;
            }
            
            if(Translator.GetKey(item) == header)
            {
                menuItem = item;
                return true;
            }

            if (item.Header is string headerString && headerString == header)
            {
                menuItem = item;
                return true;
            }
        }

        menuItem = null;
        return false;
    }
}
