// Generated by XenoTemplateTool v1.2 on 2018/3/23 at 22:53
using UnityEditor;
namespace CYM.Template
{
    public static class ExtendedTemplateMenuItems
    {

        [MenuItem("Assets/ScriptTemplates/AutoMono", false, 0)]
        public static void AutoMono()
        {
            TemplateTool.PromptUserForNameAuto();
        }
        [MenuItem("Assets/ScriptTemplates/TDConfig", false, 0)]
        public static void TDConfig()
        {
            TemplateTool.ShowTemplate("TDConfig");
        }
        [MenuItem("Assets/ScriptTemplates/MainUIView", false, 0)]
        public static void MainUIView()
        {
            TemplateTool.ShowTemplate("MainUIView");
        }
        [MenuItem("Assets/ScriptTemplates/ECSComponetData", false, 0)]
        public static void ECSComponetData()
        {
            TemplateTool.ShowTemplate("ECSComponetData");
        }
        [MenuItem("Assets/ScriptTemplates/ECSSystem", false, 0)]
        public static void ECSSystem()
        {
            TemplateTool.ShowTemplate("ECSSystem");
        }
    }

}