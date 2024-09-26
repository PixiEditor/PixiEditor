﻿using ChunkyImageLib.DataHolders;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface ITransformableExecutor : IExecutorFeature
{
    public bool IsTransforming { get; }
    public void OnTransformMoved(ShapeCorners corners); 
    public void OnTransformApplied();
    public void OnLineOverlayMoved(VecD start, VecD end);
    public void OnSelectedObjectNudged(VecI distance);
}