using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorPrototype.Models
{
    internal class DocumentUpdater
    {
        private DocumentViewModel doc;
        private DocumentHelpers helper;
        public DocumentUpdater(DocumentViewModel doc, DocumentHelpers helper)
        {
            this.doc = doc;
            this.helper = helper;
        }

        public void ApplyChangeFromChangeInfo(IChangeInfo? arbitraryInfo)
        {
            if (arbitraryInfo is null)
                return;

            switch (arbitraryInfo)
            {
                case CreateStructureMember_ChangeInfo info:
                    ProcessCreateStructureMember(info);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    ProcessDeleteStructureMember(info);
                    break;
                case StructureMemberName_ChangeInfo info:
                    ProcessUpdateStructureMemberName(info);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    ProcessUpdateStructureMemberIsVisible(info);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    ProcessUpdateStructureMemberOpacity(info);
                    break;
                case MoveStructureMember_ChangeInfo info:
                    ProcessMoveStructureMember(info);
                    break;
                case Size_ChangeInfo info:
                    ProcessSize(info);
                    break;
                case MoveViewport_PassthroughAction info:
                    ProcessMoveViewport(info);
                    break;
                case StructureMemberMask_ChangeInfo info:
                    ProcessStructureMemberMask(info);
                    break;
            }
        }

        private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
        {
            var memberVm = helper.StructureHelper.FindOrThrow(info.GuidValue);
            memberVm.RaisePropertyChanged(nameof(memberVm.HasMask));
        }

        private void ProcessMoveViewport(MoveViewport_PassthroughAction info)
        {
            var oldResolution = doc.RenderResolution;

            helper.State.ViewportCenter = info.Center;
            helper.State.ViewportSize = info.Size;
            helper.State.ViewportAngle = info.Angle;
            helper.State.ViewportRealSize = info.RealSize;

            var newResolution = doc.RenderResolution;

            if (oldResolution != newResolution)
                doc.RaisePropertyChanged(nameof(doc.RenderBitmap));
        }

        private void ProcessSize(Size_ChangeInfo info)
        {
            doc.SurfaceFull.Dispose();
            doc.SurfaceHalf?.Dispose();
            doc.SurfaceQuarter?.Dispose();
            doc.SurfaceEighth?.Dispose();

            doc.SurfaceHalf = null;
            doc.SurfaceQuarter = null;
            doc.SurfaceEighth = null;

            doc.BitmapHalf = null;
            doc.BitmapQuarter = null;
            doc.BitmapEighth = null;

            doc.BitmapFull = CreateBitmap(helper.Tracker.Document.Size);
            doc.SurfaceFull = CreateSKSurface(doc.BitmapFull);

            if (helper.Tracker.Document.Size.X > 512 && helper.Tracker.Document.Size.Y > 512)
            {
                doc.BitmapHalf = CreateBitmap(helper.Tracker.Document.Size / 2);
                doc.SurfaceHalf = CreateSKSurface(doc.BitmapHalf);
            }

            if (helper.Tracker.Document.Size.X > 1024 && helper.Tracker.Document.Size.Y > 1024)
            {
                doc.BitmapQuarter = CreateBitmap(helper.Tracker.Document.Size / 4);
                doc.SurfaceQuarter = CreateSKSurface(doc.BitmapQuarter);
            }

            if (helper.Tracker.Document.Size.X > 2048 && helper.Tracker.Document.Size.Y > 2048)
            {
                doc.BitmapEighth = CreateBitmap(helper.Tracker.Document.Size / 8);
                doc.SurfaceEighth = CreateSKSurface(doc.BitmapEighth);
            }

            doc.RaisePropertyChanged(nameof(doc.BitmapFull));
            doc.RaisePropertyChanged(nameof(doc.BitmapHalf));
            doc.RaisePropertyChanged(nameof(doc.BitmapQuarter));
            doc.RaisePropertyChanged(nameof(doc.BitmapEighth));

            doc.RaisePropertyChanged(nameof(doc.RenderBitmap));
        }

        private WriteableBitmap CreateBitmap(Vector2i size)
        {
            return new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Pbgra32, null);
        }

        private SKSurface CreateSKSurface(WriteableBitmap bitmap)
        {
            return SKSurface.Create(
                new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                bitmap.BackBuffer,
                bitmap.BackBufferStride);
        }

        private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
        {
            var (member, parentFolder) = helper.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);
            var parentFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(parentFolder.GuidValue);

            int index = parentFolder.ReadOnlyChildren.IndexOf(member);

            StructureMemberViewModel memberVM = member switch
            {
                IReadOnlyLayer layer => new LayerViewModel(doc, helper, layer),
                IReadOnlyFolder folder => new FolderViewModel(doc, helper, folder),
                _ => throw new InvalidOperationException("Unsupposed member type")
            };

            parentFolderVM.Children.Insert(index, memberVM);

            if (member is IReadOnlyFolder folder2)
            {
                foreach (IReadOnlyStructureMember child in folder2.ReadOnlyChildren)
                {
                    ProcessCreateStructureMember(new CreateStructureMember_ChangeInfo() { GuidValue = child.GuidValue });
                }
            }
        }

        private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
        {
            var (memberVM, folderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            folderVM.Children.Remove(memberVM);
        }

        private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
        {
            var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.IsVisible));
        }

        private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
        {
            var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.Name));
        }

        private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
        {
            var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
            memberVM.RaisePropertyChanged(nameof(memberVM.Opacity));
        }

        private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
        {
            var (memberVM, curFolderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
            var (member, targetFolder) = helper.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);

            int index = targetFolder.ReadOnlyChildren.IndexOf(member);
            var targetFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(targetFolder.GuidValue);

            curFolderVM.Children.Remove(memberVM);
            targetFolderVM.Children.Insert(index, memberVM);
        }
    }
}
