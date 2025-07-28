using System.ComponentModel;

namespace PixiEditor.Models.DocumentModels;
internal enum DocumentTransformMode
{
    [Description("SCALE_NOROTATE_NOSHEAR_NOPERSPECTIVE")]
    Scale_NoRotate_NoShear_NoPerspective,
    [Description("SCALE_ROTATE_NOSHEAR_NOPERSPECTIVE")]
    Scale_Rotate_NoShear_NoPerspective,
    [Description("SCALE_ROTATE_SHEAR_NOPERSPECTIVE")]
    Scale_Rotate_Shear_NoPerspective,
    [Description("SCALE_ROTATE_SHEAR_PERSPECTIVE")]
    Scale_Rotate_Shear_Perspective
}
