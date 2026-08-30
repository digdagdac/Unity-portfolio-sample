using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Muloro.Portfolio.EditorTools
{
    public static class PortfolioDemoValidator
    {
        private const string CombatScenePath =
            "Assets/MuloroCombatDemo/Scenes/PortfolioCombatDemo.unity";

        public static void Validate()
        {
            var report = new StringBuilder();
            report.AppendLine("[VALIDATE] scene=" + CombatScenePath);

            Scene scene = EditorSceneManager.OpenScene(CombatScenePath, OpenSceneMode.Single);
            report.AppendLine("[VALIDATE] loaded=" + scene.isLoaded);

            GameObject[] roots = scene.GetRootGameObjects();
            report.AppendLine("[VALIDATE] rootCount=" + roots.Length);

            var all = new List<GameObject>();
            foreach (GameObject root in roots)
            {
                all.Add(root);
                all.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
            }
            all = all.Distinct().ToList();
            report.AppendLine("[VALIDATE] totalGameObjects=" + all.Count);

            int missingScripts = 0;
            foreach (GameObject go in all)
            {
                Component[] comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null)
                    {
                        missingScripts++;
                        report.AppendLine("[VALIDATE] MISSING SCRIPT on: " + go.name);
                    }
                }
            }
            report.AppendLine("[VALIDATE] missingScriptCount=" + missingScripts);

            var players = Object.FindObjectsByType<PortfolioOfflinePlayer>(FindObjectsSortMode.None);
            var bosses = Object.FindObjectsByType<PortfolioOfflineBoss>(FindObjectsSortMode.None);
            var boots = Object.FindObjectsByType<PortfolioSinglePlayerBootstrap>(FindObjectsSortMode.None);
            report.AppendLine("[VALIDATE] player=" + players.Length);
            report.AppendLine("[VALIDATE] boss=" + bosses.Length);
            report.AppendLine("[VALIDATE] bootstrap=" + boots.Length);

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            report.AppendLine("[VALIDATE] cameras=" + cameras.Length);
            report.AppendLine("[VALIDATE] mainCamera=" + (Camera.main != null));

            int animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None).Length;
            report.AppendLine("[VALIDATE] animators=" + animators);

            bool inBuild = EditorBuildSettings.scenes.Any(s => s.path == CombatScenePath && s.enabled);
            report.AppendLine("[VALIDATE] inBuildSettingsEnabled=" + inBuild);

            bool pass = scene.isLoaded
                        && missingScripts == 0
                        && players.Length == 1
                        && bosses.Length == 1
                        && boots.Length == 1
                        && Camera.main != null;
            report.AppendLine("[VALIDATE] RESULT=" + (pass ? "PASS" : "FAIL"));

            Debug.Log(report.ToString());
            EditorApplication.Exit(pass ? 0 : 1);
        }
    }
}
