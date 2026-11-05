using UnityEditor;

public static class FinalMasterSetup
{
    public static void ApplyAndBuild()
    {
        InteriorKitGameSetup.Apply();
        ReleaseBuild.Windows();
    }
}
