﻿namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IPenToolbar : IBasicToolbar
{
    public bool AntiAliasing { get; set; }
    public float Hardness { get; set; }
    public float Spacing { get; set; }
}