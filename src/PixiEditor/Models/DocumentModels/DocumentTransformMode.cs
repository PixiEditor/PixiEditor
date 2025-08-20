using System.ComponentModel;

namespace PixiEditor.Models.DocumentModels;
internal enum DocumentTransformMode
{
    // Comments show localization in DocumentTransformViewModel.cs, comments are needed for localization key check pipeline
    
    // TRANSFORM_ACTION_DISPLAY_SCALE_NOROTATE_NOSHEAR_NOPERSPECTIVE
    [Description("SCALE_NOROTATE_NOSHEAR_NOPERSPECTIVE")]
    Scale_NoRotate_NoShear_NoPerspective,
    
    // TRANSFORM_ACTION_DISPLAY_SCALE_ROTATE_NOSHEAR_NOPERSPECTIVE
    [Description("SCALE_ROTATE_NOSHEAR_NOPERSPECTIVE")]
    Scale_Rotate_NoShear_NoPerspective,
    
    // TRANSFORM_ACTION_DISPLAY_SCALE_ROTATE_SHEAR_NOPERSPECTIVE
    [Description("SCALE_ROTATE_SHEAR_NOPERSPECTIVE")]
    Scale_Rotate_Shear_NoPerspective,
    
    // TRANSFORM_ACTION_DISPLAY_SCALE_ROTATE_SHEAR_PERSPECTIVE
    [Description("SCALE_ROTATE_SHEAR_PERSPECTIVE")]
    Scale_Rotate_Shear_Perspective
}
