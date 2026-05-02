using System;
using System.Collections.Generic;
using System.IO;
using FutureHeroQuest.SceneFlow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace FutureHeroQuest.EditorTools
{
    public static class FhqOutlineSceneBuilder
    {
        private const string SceneDir = "Assets/Scenes/Outline";
        private const string MaterialDir = "Assets/Materials/Outline";

        private const string Scene00Path = SceneDir + "/Scene00_Prologue_SafeHouse.unity";
        private const string Scene01Path = SceneDir + "/Scene01_CollapsedCorridor.unity";
        private const string Scene02Path = SceneDir + "/Scene02_MountainTunnel.unity";
        private const string Scene03Path = SceneDir + "/Scene03_ArchiveCrane.unity";
        private const string Scene04Path = SceneDir + "/Scene04_FinalRewrite.unity";

        private static readonly string[] OutlineScenePaths =
        {
            Scene00Path,
            Scene01Path,
            Scene02Path,
            Scene03Path,
            Scene04Path
        };

        [MenuItem("FHQ/Build Outline Scene Set")]
        public static void BuildAllOutlineScenes()
        {
            EnsureFolders();

            BuildPrologueSafeHouse();
            BuildCollapsedCorridor();
            BuildMountainTunnel();
            BuildArchiveCrane();
            BuildFinalRewrite();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(Scene00Path, OpenSceneMode.Single);
            Debug.Log("[FHQ] Built outline scene set.");
        }

        [MenuItem("FHQ/Validate Outline Scene Set")]
        public static void ValidateOutlineScenes()
        {
            var specs = new[]
            {
                new SceneSpec(Scene00Path, "Scene00_Prologue_SafeHouse", new[] { "Anchor_Exit_CorridorDoor" }, 1),
                new SceneSpec(Scene01Path, "Scene01_CollapsedCorridor", new[] { "Anchor_Entry_CorridorDoor", "Anchor_Exit_TunnelMouth" }, 1),
                new SceneSpec(Scene02Path, "Scene02_MountainTunnel", new[] { "Anchor_Entry_TunnelMouth", "Anchor_Exit_ArchiveIronDoor" }, 1),
                new SceneSpec(Scene03Path, "Scene03_ArchiveCrane", new[] { "Anchor_Entry_ArchiveIronDoor", "Anchor_Exit_CorePassage" }, 1),
                new SceneSpec(Scene04Path, "Scene04_FinalRewrite", new[] { "Anchor_Entry_CorePassage" }, 0)
            };

            bool passed = true;
            var snapshots = new Dictionary<string, SceneSnapshot>(StringComparer.OrdinalIgnoreCase);
            foreach (SceneSpec spec in specs)
            {
                if (!File.Exists(spec.Path))
                {
                    Debug.LogError($"[FHQ] Outline scene missing: {spec.Path}");
                    passed = false;
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(spec.Path, OpenSceneMode.Single);
                passed &= ValidateScene(scene, spec);
                snapshots[spec.Path] = CaptureSceneSnapshot(scene, spec.Path);
            }

            passed &= ValidateTransitionLinks(snapshots);

            if (!passed)
                throw new InvalidOperationException("[FHQ] Outline scene validation failed.");

            Debug.Log("[FHQ] Outline scene validation passed.");
        }

        [MenuItem("FHQ/Apply Outline Build Settings")]
        public static void ApplyOutlineBuildSettings()
        {
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in OutlineScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    Debug.LogError($"[FHQ] Cannot apply Outline Build Settings; missing scene: {scenePath}");
                    throw new FileNotFoundException(scenePath);
                }

                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("[FHQ] Applied Outline Build Settings. Re-run FHQ/Build Scene order before release builds.");
        }

        private static void BuildPrologueSafeHouse()
        {
            BuildScene(
                "Scene00_Prologue_SafeHouse",
                Scene00Path,
                new Color(0.07f, 0.07f, 0.08f),
                ctx =>
                {
                    CreatePlayer(ctx.Root, new Vector3(0f, 1f, -2.6f), Quaternion.identity);
                    CreateRoom(ctx.Root, "SafeHouseRoom", Vector3.zero, new Vector3(12f, 0.24f, 8f), ctx.MatFloorWarm, ctx.MatWallWarm);

                    CreateCube(ctx.Root, "OldOfficeDesk", new Vector3(-2.6f, 0.55f, -0.7f), new Vector3(2.3f, 0.42f, 1.1f), ctx.MatWood, true);
                    CreateCube(ctx.Root, "SupplyCrates", new Vector3(3.5f, 0.42f, -1.7f), new Vector3(1.4f, 0.84f, 1.0f), ctx.MatRubble, true);
                    CreateCube(ctx.Root, "TeaShelf", new Vector3(4.8f, 1.0f, 1.7f), new Vector3(0.42f, 2.0f, 1.4f), ctx.MatWood, true);
                    CreateDoorFrame(ctx.Root, "CorridorDoorFrame", new Vector3(0f, 1f, 3.7f), Quaternion.identity, ctx.MatAnchor);

                    var splitRoot = new GameObject("PastFutureConcept_AfterDmail");
                    splitRoot.transform.SetParent(ctx.Root, false);
                    CreateCube(splitRoot.transform, "PastWarmPanel", new Vector3(-3.3f, 0.08f, 1.6f), new Vector3(3.5f, 0.1f, 1.8f), ctx.MatPast, false);
                    CreateCube(splitRoot.transform, "FutureCoolPanel", new Vector3(3.3f, 0.08f, 1.6f), new Vector3(3.5f, 0.1f, 1.8f), ctx.MatFuture, false);
                    CreateWorldLabel(splitRoot.transform, "PastFutureLabel", "PAST / FUTURE LINK ESTABLISHED", new Vector3(0f, 1.1f, 1.6f), Color.white, 0.085f);
                    splitRoot.SetActive(false);

                    var phoneRoot = new GameObject("DmailToken_AfterInteract");
                    phoneRoot.transform.SetParent(ctx.Root, false);
                    CreateCube(phoneRoot.transform, "SignalPhone", new Vector3(-2.6f, 1.05f, -0.7f), new Vector3(0.38f, 0.08f, 0.62f), ctx.MatSignal, false);
                    CreateCube(phoneRoot.transform, "SignalBeam", new Vector3(-2.6f, 1.75f, -0.7f), new Vector3(0.18f, 1.1f, 0.18f), ctx.MatSignal, false);
                    CreateWorldLabel(phoneRoot.transform, "DmailLabel", "DMAIL TOKEN", new Vector3(-2.6f, 2.45f, -0.7f), ctx.SignalText, 0.08f);
                    phoneRoot.SetActive(false);

                    GameObject exitHint = CreateWorldLabel(ctx.Root, "ExitHint_AfterDmail", "The same doorway becomes the corridor entrance.", new Vector3(0f, 2.6f, 2.9f), Color.white, 0.075f);
                    exitHint.SetActive(false);

                    GameObject prompt = CreateWorldLabel(ctx.Root, "DmailPrompt", "E: Take the signal phone", new Vector3(-2.6f, 1.85f, -0.7f), Color.white, 0.075f);
                    prompt.SetActive(false);
                    GameObject interactor = CreateCube(ctx.Root, "DmailInteractor", new Vector3(-2.6f, 0.12f, -0.7f), new Vector3(1.4f, 0.2f, 1.2f), ctx.MatSignal, false);
                    CreateAnchor(ctx.Root, "Anchor_Exit_CorridorDoor", "Exit toward collapsed corridor", new Vector3(0f, 1f, 3.2f), Quaternion.identity);
                    GameObject toCollapsedCorridor = CreateBoundary(
                        ctx.Root,
                        "Boundary_ToCollapsedCorridor",
                        new Vector3(0f, 0.8f, 3.15f),
                        new Vector3(2.4f, 1.6f, 0.5f),
                        ctx.MatExit,
                        "Scene01_CollapsedCorridor",
                        Scene01Path,
                        "Anchor_Exit_CorridorDoor",
                        "Anchor_Entry_CorridorDoor");
                    toCollapsedCorridor.SetActive(false);

                    ConfigureInteractable(
                        interactor.AddComponent<OutlineInteractable>(),
                        prompt,
                        new[] { splitRoot, phoneRoot, exitHint, toCollapsedCorridor },
                        Array.Empty<GameObject>(),
                        "Prologue DMAIL token revealed.");

                    CreateWorldLabel(ctx.Root, "SceneGoal", "Prologue: safe house, signal token, two time views", new Vector3(0f, 2.55f, -3.35f), Color.white, 0.08f);
                });
        }

        private static void BuildCollapsedCorridor()
        {
            BuildScene(
                "Scene01_CollapsedCorridor",
                Scene01Path,
                new Color(0.06f, 0.065f, 0.07f),
                ctx =>
                {
                    CreatePlayer(ctx.Root, new Vector3(0f, 1f, -4.4f), Quaternion.identity);
                    CreateCorridor(ctx.Root, "CollapsedExperimentCorridor", new Vector3(0f, 0f, 0f), 8.5f, 12.5f, ctx.MatFloorCool, ctx.MatWallCool);
                    CreateDoorFrame(ctx.Root, "EntryDoorFrame_MatchesSafeHouse", new Vector3(0f, 1f, -4.85f), Quaternion.identity, ctx.MatAnchor);
                    CreateDoorFrame(ctx.Root, "TunnelMouthFrame", new Vector3(0f, 1f, 5.0f), Quaternion.identity, ctx.MatAnchor);

                    CreateCube(ctx.Root, "PastRepairWorkbench", new Vector3(-3.0f, 0.5f, -0.8f), new Vector3(1.5f, 0.9f, 1.0f), ctx.MatPast, true);
                    CreateCube(ctx.Root, "FutureBrokenSlab_A", new Vector3(-0.55f, 0.22f, 1.8f), new Vector3(2.0f, 0.35f, 1.4f), ctx.MatRubble, true).transform.rotation = Quaternion.Euler(0f, 0f, 9f);
                    CreateCube(ctx.Root, "FutureBrokenSlab_B", new Vector3(0.7f, 0.26f, 2.7f), new Vector3(1.8f, 0.35f, 1.2f), ctx.MatRubble, true).transform.rotation = Quaternion.Euler(0f, 0f, -11f);

                    var futurePassage = new GameObject("FuturePassage_AfterPastRepair");
                    futurePassage.transform.SetParent(ctx.Root, false);
                    CreateCube(futurePassage.transform, "RepairedWalkway", new Vector3(0f, 0.08f, 2.25f), new Vector3(2.5f, 0.18f, 3.2f), ctx.MatExit, true);
                    CreateWorldLabel(futurePassage.transform, "RepairedWalkwayLabel", "FUTURE PATH RESTORED", new Vector3(0f, 1.2f, 2.25f), ctx.SignalText, 0.075f);
                    futurePassage.SetActive(false);

                    GameObject corridorBlocker = CreateCube(ctx.Root, "CollapsedCorridorBlocker_DeactivatesOnRepair", new Vector3(0f, 0.9f, 2.25f), new Vector3(7.0f, 1.8f, 3.2f), ctx.MatBlocker, true);
                    var rubbleVisual = new GameObject("RubbleVisual_DeactivatesOnRepair");
                    rubbleVisual.transform.SetParent(ctx.Root, false);
                    CreateCube(rubbleVisual.transform, "RubbleChunk_1", new Vector3(-0.9f, 0.55f, 2.0f), new Vector3(0.8f, 0.8f, 0.8f), ctx.MatRubble, false);
                    CreateCube(rubbleVisual.transform, "RubbleChunk_2", new Vector3(0.6f, 0.5f, 2.55f), new Vector3(0.9f, 0.7f, 0.7f), ctx.MatRubble, false);
                    CreateCube(rubbleVisual.transform, "RubbleChunk_3", new Vector3(0.0f, 0.75f, 1.45f), new Vector3(1.0f, 0.65f, 0.75f), ctx.MatRubble, false);

                    GameObject repairPrompt = CreateWorldLabel(ctx.Root, "RepairPrompt", "E: repair past support", new Vector3(-3.0f, 1.65f, -0.8f), Color.white, 0.075f);
                    repairPrompt.SetActive(false);
                    CreateAnchor(ctx.Root, "Anchor_Entry_CorridorDoor", "Entry from safe house", new Vector3(0f, 1f, -4.45f), Quaternion.identity);
                    CreateAnchor(ctx.Root, "Anchor_Exit_TunnelMouth", "Exit toward mountain tunnel", new Vector3(0f, 1f, 4.45f), Quaternion.identity);
                    GameObject toMountainTunnel = CreateBoundary(
                        ctx.Root,
                        "Boundary_ToMountainTunnel",
                        new Vector3(0f, 0.8f, 4.55f),
                        new Vector3(2.5f, 1.6f, 0.5f),
                        ctx.MatExit,
                        "Scene02_MountainTunnel",
                        Scene02Path,
                        "Anchor_Exit_TunnelMouth",
                        "Anchor_Entry_TunnelMouth");
                    toMountainTunnel.SetActive(false);

                    GameObject repairInteractor = CreateCube(ctx.Root, "PastSupportRepairInteractor", new Vector3(-3.0f, 0.12f, -0.8f), new Vector3(1.6f, 0.2f, 1.4f), ctx.MatPast, false);
                    ConfigureInteractable(
                        repairInteractor.AddComponent<OutlineInteractable>(),
                        repairPrompt,
                        new[] { futurePassage, toMountainTunnel },
                        new[] { corridorBlocker, rubbleVisual },
                        "Collapsed corridor future path restored.");

                    CreateWorldLabel(ctx.Root, "SceneGoal", "L1: past support change opens the future corridor", new Vector3(0f, 2.65f, -5.65f), Color.white, 0.08f);
                });
        }

        private static void BuildMountainTunnel()
        {
            BuildScene(
                "Scene02_MountainTunnel",
                Scene02Path,
                new Color(0.045f, 0.05f, 0.055f),
                ctx =>
                {
                    CreatePlayer(ctx.Root, new Vector3(0f, 1f, -5.0f), Quaternion.identity);
                    CreateTunnel(ctx.Root, ctx);

                    CreateDoorFrame(ctx.Root, "TunnelEntryMouth_MatchesL1", new Vector3(0f, 1f, -5.25f), Quaternion.identity, ctx.MatAnchor);
                    CreateDoorFrame(ctx.Root, "ArchiveIronDoorFrame", new Vector3(0f, 1f, 5.65f), Quaternion.identity, ctx.MatAnchor);

                    CreateCube(ctx.Root, "PastWiringNode_N7", new Vector3(-2.8f, 0.45f, -0.3f), new Vector3(1.1f, 0.9f, 1.1f), ctx.MatPast, true);
                    CreateWorldLabel(ctx.Root, "WiringNodeLabel", "N7 QUAKELINE", new Vector3(-2.8f, 1.25f, -0.3f), ctx.SignalText, 0.065f);

                    GameObject obstacleBlocker = CreateCube(ctx.Root, "FutureBossRouteBlocker_DeactivatesOnCollapse", new Vector3(0f, 0.85f, 2.6f), new Vector3(6.6f, 1.7f, 2.4f), ctx.MatBlocker, true);

                    var collapseRoot = new GameObject("FutureCollapse_AfterWiring");
                    collapseRoot.transform.SetParent(ctx.Root, false);
                    CreateCube(collapseRoot.transform, "FallenRoof_1", new Vector3(-0.55f, 0.45f, 2.0f), new Vector3(1.2f, 0.65f, 0.8f), ctx.MatRubble, true);
                    CreateCube(collapseRoot.transform, "FallenRoof_2", new Vector3(0.65f, 0.4f, 2.75f), new Vector3(1.25f, 0.7f, 0.8f), ctx.MatRubble, true);
                    CreateCube(collapseRoot.transform, "CrawlGap", new Vector3(0f, 0.12f, 2.55f), new Vector3(1.4f, 0.18f, 2.2f), ctx.MatExit, true);
                    CreateWorldLabel(collapseRoot.transform, "CollapseLabel", "COLLAPSE REROUTES THE PURSUIT", new Vector3(0f, 1.35f, 2.45f), Color.white, 0.067f);
                    collapseRoot.SetActive(false);

                    var chaseShadow = new GameObject("PursuitShadow_AfterCollapse");
                    chaseShadow.transform.SetParent(ctx.Root, false);
                    CreateCube(chaseShadow.transform, "ShadowTrail_A", new Vector3(2.95f, 0.12f, -1.6f), new Vector3(0.3f, 0.08f, 3.8f), ctx.MatDanger, false);
                    CreateCube(chaseShadow.transform, "ShadowTrail_B", new Vector3(2.15f, 0.12f, 2.8f), new Vector3(0.3f, 0.08f, 3.6f), ctx.MatDanger, false);
                    CreateWorldLabel(chaseShadow.transform, "ShadowLabel", "boss line forced into rubble", new Vector3(2.8f, 1.15f, 0.8f), ctx.DangerText, 0.06f);
                    chaseShadow.SetActive(false);

                    GameObject wirePrompt = CreateWorldLabel(ctx.Root, "WirePrompt", "E: connect collapse node", new Vector3(-2.8f, 1.7f, -0.3f), Color.white, 0.075f);
                    wirePrompt.SetActive(false);
                    CreateAnchor(ctx.Root, "Anchor_Entry_TunnelMouth", "Entry from collapsed corridor", new Vector3(0f, 1f, -5.0f), Quaternion.identity);
                    CreateAnchor(ctx.Root, "Anchor_Exit_ArchiveIronDoor", "Exit toward archive iron door", new Vector3(0f, 1f, 5.1f), Quaternion.identity);
                    GameObject toArchiveCrane = CreateBoundary(
                        ctx.Root,
                        "Boundary_ToArchiveCrane",
                        new Vector3(0f, 0.8f, 5.2f),
                        new Vector3(2.3f, 1.6f, 0.5f),
                        ctx.MatExit,
                        "Scene03_ArchiveCrane",
                        Scene03Path,
                        "Anchor_Exit_ArchiveIronDoor",
                        "Anchor_Entry_ArchiveIronDoor");
                    toArchiveCrane.SetActive(false);

                    GameObject wireInteractor = CreateCube(ctx.Root, "TunnelWiringInteractor", new Vector3(-2.8f, 0.12f, -0.3f), new Vector3(1.5f, 0.2f, 1.5f), ctx.MatPast, false);
                    ConfigureInteractable(
                        wireInteractor.AddComponent<OutlineInteractable>(),
                        wirePrompt,
                        new[] { collapseRoot, chaseShadow, toArchiveCrane },
                        new[] { obstacleBlocker },
                        "Tunnel collapse node connected.");

                    CreateWorldLabel(ctx.Root, "SceneGoal", "L2: wired node creates a controlled future collapse", new Vector3(0f, 2.65f, -6.0f), Color.white, 0.08f);
                });
        }

        private static void BuildArchiveCrane()
        {
            BuildScene(
                "Scene03_ArchiveCrane",
                Scene03Path,
                new Color(0.055f, 0.052f, 0.047f),
                ctx =>
                {
                    CreatePlayer(ctx.Root, new Vector3(0f, 1f, -5.1f), Quaternion.identity);
                    GameObject archiveRoom = CreateRoom(ctx.Root, "ArchiveRoom", new Vector3(0f, 0f, -2.2f), new Vector3(10f, 0.24f, 7f), ctx.MatFloorWarm, ctx.MatWallWarm);
                    GameObject craneYard = CreateRoom(ctx.Root, "CraneYard", new Vector3(0f, 0f, 3.5f), new Vector3(10f, 0.18f, 5f), ctx.MatFloorCool, ctx.MatWallCool);
                    DestroyChild(archiveRoom.transform, "NorthWall");
                    DestroyChild(craneYard.transform, "SouthWall");
                    CreateDoorFrame(ctx.Root, "ArchiveIronDoor_MatchesTunnel", new Vector3(0f, 1f, -5.55f), Quaternion.identity, ctx.MatAnchor);
                    CreateDoorFrame(ctx.Root, "CorePassageFrame", new Vector3(0f, 1f, 5.75f), Quaternion.identity, ctx.MatAnchor);

                    for (int i = 0; i < 4; i++)
                        CreateCube(ctx.Root, $"ArchiveShelf_{i + 1}", new Vector3(-3.8f + i * 2.4f, 0.9f, -2.2f), new Vector3(1.2f, 1.8f, 0.55f), ctx.MatWood, true);

                    var craneInstructionRoot = new GameObject("CraneInstruction_AfterArchiveRead");
                    craneInstructionRoot.transform.SetParent(ctx.Root, false);
                    CreateWorldLabel(craneInstructionRoot.transform, "CraneInstructionLabel", "TARGET: DROP PREFAB TUNNEL SEGMENT", new Vector3(0f, 1.45f, 1.0f), ctx.SignalText, 0.07f);
                    CreateCube(craneInstructionRoot.transform, "TargetPaint", new Vector3(0f, 0.08f, 3.95f), new Vector3(2.4f, 0.12f, 1.6f), ctx.MatSignal, false);
                    craneInstructionRoot.SetActive(false);

                    CreateCrane(ctx.Root, ctx);

                    GameObject mountainBlocker = CreateCube(ctx.Root, "FutureMountainBlocker_DeactivatesOnCraneRelease", new Vector3(0f, 0.95f, 4.9f), new Vector3(3.0f, 1.9f, 1.4f), ctx.MatBlocker, true);

                    var futurePassage = new GameObject("FutureCorePassage_AfterCraneRelease");
                    futurePassage.transform.SetParent(ctx.Root, false);
                    CreateCube(futurePassage.transform, "PrefabricatedTunnelSegment", new Vector3(0f, 0.35f, 4.65f), new Vector3(2.2f, 0.7f, 2.2f), ctx.MatExit, true);
                    CreateCube(futurePassage.transform, "CorePassageFloor", new Vector3(0f, 0.08f, 5.35f), new Vector3(2.2f, 0.18f, 1.4f), ctx.MatExit, true);
                    CreateWorldLabel(futurePassage.transform, "CorePassageLabel", "CORE LAB PATH OPEN", new Vector3(0f, 1.45f, 4.9f), Color.white, 0.07f);
                    futurePassage.SetActive(false);

                    GameObject readPrompt = CreateWorldLabel(ctx.Root, "ArchiveReadPrompt", "E: read core access file", new Vector3(-1.2f, 1.65f, -3.2f), Color.white, 0.075f);
                    readPrompt.SetActive(false);
                    GameObject readInteractor = CreateCube(ctx.Root, "ArchiveFileInteractor", new Vector3(-1.2f, 0.12f, -3.2f), new Vector3(1.4f, 0.2f, 1.0f), ctx.MatSignal, false);

                    GameObject craneRelease = CreateCube(ctx.Root, "CraneReleaseInteractor_AfterArchiveRead", new Vector3(2.6f, 0.12f, 2.4f), new Vector3(1.4f, 0.2f, 1.2f), ctx.MatPast, false);
                    GameObject releasePrompt = CreateWorldLabel(craneRelease.transform, "CraneReleasePrompt", "E: release tunnel segment", new Vector3(2.6f, 1.55f, 2.4f), Color.white, 0.075f);
                    releasePrompt.SetActive(false);
                    CreateAnchor(ctx.Root, "Anchor_Entry_ArchiveIronDoor", "Entry from mountain tunnel", new Vector3(0f, 1f, -5.1f), Quaternion.identity);
                    CreateAnchor(ctx.Root, "Anchor_Exit_CorePassage", "Exit toward final rewrite", new Vector3(0f, 1f, 5.25f), Quaternion.identity);
                    GameObject toFinalRewrite = CreateBoundary(
                        ctx.Root,
                        "Boundary_ToFinalRewrite",
                        new Vector3(0f, 0.8f, 5.35f),
                        new Vector3(2.3f, 1.6f, 0.5f),
                        ctx.MatExit,
                        "Scene04_FinalRewrite",
                        Scene04Path,
                        "Anchor_Exit_CorePassage",
                        "Anchor_Entry_CorePassage");
                    toFinalRewrite.SetActive(false);

                    ConfigureInteractable(
                        craneRelease.AddComponent<OutlineInteractable>(),
                        releasePrompt,
                        new[] { futurePassage, toFinalRewrite },
                        new[] { mountainBlocker },
                        "Crane released tunnel segment.");
                    craneRelease.SetActive(false);

                    ConfigureInteractable(
                        readInteractor.AddComponent<OutlineInteractable>(),
                        readPrompt,
                        new[] { craneInstructionRoot, craneRelease },
                        Array.Empty<GameObject>(),
                        "Archive file read; crane release enabled.");

                    CreateWorldLabel(ctx.Root, "SceneGoal", "L3: archive clue enables crane tunnel placement", new Vector3(0f, 2.65f, -6.0f), Color.white, 0.08f);
                });
        }

        private static void BuildFinalRewrite()
        {
            BuildScene(
                "Scene04_FinalRewrite",
                Scene04Path,
                new Color(0.06f, 0.065f, 0.07f),
                ctx =>
                {
                    CreatePlayer(ctx.Root, new Vector3(0f, 1f, -4.8f), Quaternion.identity);
                    CreateRoom(ctx.Root, "FinalRewriteRoom", Vector3.zero, new Vector3(12f, 0.24f, 11f), ctx.MatFloorCool, ctx.MatWallCool);
                    CreateDoorFrame(ctx.Root, "CorePassageEntry_MatchesL3", new Vector3(0f, 1f, -5.15f), Quaternion.identity, ctx.MatAnchor);

                    var ruinRoot = new GameObject("RuinTimeline_BeforeCorrection");
                    ruinRoot.transform.SetParent(ctx.Root, false);
                    CreateCube(ruinRoot.transform, "CollapsedDesk", new Vector3(-2.8f, 0.35f, -0.3f), new Vector3(2.2f, 0.45f, 1.2f), ctx.MatRubble, true).transform.rotation = Quaternion.Euler(0f, 0f, 12f);
                    CreateCube(ruinRoot.transform, "BrokenWall", new Vector3(2.8f, 1.0f, 0.7f), new Vector3(2.2f, 2.0f, 0.35f), ctx.MatBlocker, true).transform.rotation = Quaternion.Euler(0f, 20f, 0f);
                    CreateWorldLabel(ruinRoot.transform, "RuinLabel", "RUINED CAMPUS TIMELINE", new Vector3(0f, 1.8f, 1.5f), ctx.DangerText, 0.08f);

                    var cleanRoot = new GameObject("CleanCampusTimeline_AfterCorrection");
                    cleanRoot.transform.SetParent(ctx.Root, false);
                    CreateCube(cleanRoot.transform, "CleanHallFloor", new Vector3(0f, 0.08f, 1.2f), new Vector3(8f, 0.16f, 4f), ctx.MatExit, true);
                    CreateCube(cleanRoot.transform, "NoticeBoard", new Vector3(-3.6f, 1.1f, 1.2f), new Vector3(0.35f, 1.6f, 2.4f), ctx.MatWood, true);
                    CreateCube(cleanRoot.transform, "SunPatch", new Vector3(2.4f, 0.13f, 0.6f), new Vector3(2.4f, 0.08f, 1.4f), ctx.MatSignal, false);
                    CreateWorldLabel(cleanRoot.transform, "CleanLabel", "NORMAL CAMPUS RESTORED", new Vector3(0f, 1.9f, 1.5f), Color.white, 0.085f);
                    cleanRoot.SetActive(false);

                    GameObject completion = CreateWorldLabel(ctx.Root, "CompletionBanner_AfterCorrection", "FINAL REWRITE COMPLETE", new Vector3(0f, 2.7f, -1.5f), ctx.SignalText, 0.1f);
                    completion.SetActive(false);

                    CreateCube(ctx.Root, "WarningPaperDesk", new Vector3(0f, 0.55f, -1.1f), new Vector3(2.3f, 0.4f, 1.1f), ctx.MatWood, true);
                    CreateCube(ctx.Root, "WarningPaper", new Vector3(0f, 0.82f, -1.1f), new Vector3(0.85f, 0.05f, 0.55f), ctx.MatSignal, false);
                    GameObject prompt = CreateWorldLabel(ctx.Root, "WarningPaperPrompt", "E: correct the parameters", new Vector3(0f, 1.55f, -1.1f), Color.white, 0.075f);
                    prompt.SetActive(false);
                    GameObject interactor = CreateCube(ctx.Root, "WarningPaperInteractor", new Vector3(0f, 0.12f, -1.1f), new Vector3(1.5f, 0.2f, 1.2f), ctx.MatSignal, false);
                    ConfigureInteractable(
                        interactor.AddComponent<OutlineInteractable>(),
                        prompt,
                        new[] { cleanRoot, completion },
                        new[] { ruinRoot },
                        "Final timeline rewritten.");

                    CreateAnchor(ctx.Root, "Anchor_Entry_CorePassage", "Entry from archive crane passage", new Vector3(0f, 1f, -4.8f), Quaternion.identity);
                    CreateWorldLabel(ctx.Root, "SceneGoal", "Finale: warning paper rewrites ruin into campus", new Vector3(0f, 2.65f, -5.7f), Color.white, 0.08f);
                });
        }

        private static void BuildScene(string sceneName, string path, Color cameraColor, Action<SceneContext> buildContent)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = sceneName;

            RenderSettings.ambientLight = new Color(0.55f, 0.56f, 0.58f);

            var root = new GameObject(sceneName + "_Root").transform;
            CreateCamera(cameraColor);
            CreateLights();
            CreateFlowController();
            CreateEventSystem();

            var ctx = new SceneContext(root);
            buildContent(ctx);

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = false;
            cam.fieldOfView = 72f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.transform.position = new Vector3(0f, 2.4f, -7.2f);
            go.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        }

        private static void CreateLights()
        {
            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            sun.transform.rotation = Quaternion.Euler(54f, -35f, 0f);

            CreatePointLight("Warm Fill", new Vector3(-5f, 4f, -2f), new Color(1f, 0.72f, 0.46f), 16f, 0.9f);
            CreatePointLight("Cool Fill", new Vector3(5f, 4f, 2f), new Color(0.42f, 0.72f, 1f), 16f, 0.9f);
        }

        private static void CreatePointLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        private static void CreateFlowController()
        {
            var go = new GameObject(nameof(OutlineSceneFlowController));
            go.AddComponent<OutlineSceneFlowController>();
        }

        private static void CreatePlayer(Transform parent, Vector3 position, Quaternion rotation)
        {
            Material mat = CreateMaterial("Outline_Player_Mat", new Color(0.95f, 0.95f, 0.98f));
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "OutlineLocalPlayer";
            player.transform.SetParent(parent, false);
            player.transform.SetPositionAndRotation(position, rotation);
            player.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            player.GetComponent<Renderer>().sharedMaterial = mat;

            var primitiveCollider = player.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null) UnityEngine.Object.DestroyImmediate(primitiveCollider);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            player.AddComponent<OutlineLocalPlayerController>();

            var spawn = new GameObject("OutlineSpawnPoint");
            spawn.transform.SetParent(parent, false);
            spawn.transform.SetPositionAndRotation(position, rotation);

            var serialized = new SerializedObject(player.GetComponent<OutlineLocalPlayerController>());
            serialized.FindProperty("respawnPoint").objectReferenceValue = spawn.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateRoom(Transform parent, string name, Vector3 center, Vector3 floorScale, Material floor, Material wall)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            CreateCube(root.transform, "Floor", center + new Vector3(0f, -0.08f, 0f), floorScale, floor, true);
            float halfX = floorScale.x * 0.5f;
            float halfZ = floorScale.z * 0.5f;
            CreateCube(root.transform, "NorthWall", center + new Vector3(0f, 1.0f, halfZ + 0.12f), new Vector3(floorScale.x + 0.25f, 2.0f, 0.25f), wall, true);
            CreateCube(root.transform, "SouthWall", center + new Vector3(0f, 1.0f, -halfZ - 0.12f), new Vector3(floorScale.x + 0.25f, 2.0f, 0.25f), wall, true);
            CreateCube(root.transform, "WestWall", center + new Vector3(-halfX - 0.12f, 1.0f, 0f), new Vector3(0.25f, 2.0f, floorScale.z + 0.25f), wall, true);
            CreateCube(root.transform, "EastWall", center + new Vector3(halfX + 0.12f, 1.0f, 0f), new Vector3(0.25f, 2.0f, floorScale.z + 0.25f), wall, true);
            return root;
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static void CreateCorridor(Transform parent, string name, Vector3 center, float width, float length, Material floor, Material wall)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            CreateCube(root.transform, "CorridorFloor", center + new Vector3(0f, -0.08f, 0f), new Vector3(width, 0.2f, length), floor, true);
            CreateCube(root.transform, "LeftWall", center + new Vector3(-width * 0.5f - 0.1f, 1.0f, 0f), new Vector3(0.25f, 2.0f, length), wall, true);
            CreateCube(root.transform, "RightWall", center + new Vector3(width * 0.5f + 0.1f, 1.0f, 0f), new Vector3(0.25f, 2.0f, length), wall, true);
        }

        private static void CreateTunnel(Transform parent, SceneContext ctx)
        {
            CreateCube(parent, "TunnelFloor", new Vector3(0f, -0.08f, 0f), new Vector3(7.4f, 0.2f, 12.5f), ctx.MatFloorCool, true);
            CreateCube(parent, "TunnelLeftWall", new Vector3(-3.85f, 1.0f, 0f), new Vector3(0.35f, 2.0f, 12.5f), ctx.MatWallCool, true);
            CreateCube(parent, "TunnelRightWall", new Vector3(3.85f, 1.0f, 0f), new Vector3(0.35f, 2.0f, 12.5f), ctx.MatWallCool, true);
            CreateCube(parent, "TunnelCeilingBand_A", new Vector3(0f, 2.1f, -2.8f), new Vector3(7.2f, 0.24f, 1.2f), ctx.MatWallCool, false);
            CreateCube(parent, "TunnelCeilingBand_B", new Vector3(0f, 2.1f, 1.2f), new Vector3(7.2f, 0.24f, 1.2f), ctx.MatWallCool, false);
            CreateCube(parent, "WetTrackLeft", new Vector3(-1.2f, 0.03f, 0f), new Vector3(0.25f, 0.08f, 11.5f), ctx.MatSignal, false);
            CreateCube(parent, "WetTrackRight", new Vector3(1.2f, 0.03f, 0f), new Vector3(0.25f, 0.08f, 11.5f), ctx.MatSignal, false);
        }

        private static void CreateDoorFrame(Transform parent, string name, Vector3 position, Quaternion rotation, Material mat)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, rotation);
            CreateCube(root.transform, "FrameTop", position + new Vector3(0f, 1.15f, 0f), new Vector3(2.5f, 0.25f, 0.25f), mat, false);
            CreateCube(root.transform, "FrameLeft", position + new Vector3(-1.15f, 0.2f, 0f), new Vector3(0.25f, 1.9f, 0.25f), mat, false);
            CreateCube(root.transform, "FrameRight", position + new Vector3(1.15f, 0.2f, 0f), new Vector3(0.25f, 1.9f, 0.25f), mat, false);
        }

        private static void CreateCrane(Transform parent, SceneContext ctx)
        {
            CreateCube(parent, "CraneTower", new Vector3(2.6f, 1.3f, 2.4f), new Vector3(0.35f, 2.6f, 0.35f), ctx.MatPast, false);
            CreateCube(parent, "CraneArm", new Vector3(0.9f, 2.55f, 3.0f), new Vector3(3.8f, 0.18f, 0.18f), ctx.MatPast, false);
            CreateCube(parent, "CraneCable", new Vector3(-0.7f, 1.55f, 3.0f), new Vector3(0.08f, 1.8f, 0.08f), ctx.MatPast, false);
            CreateCube(parent, "SuspendedPrefabTunnel", new Vector3(-0.7f, 0.6f, 3.0f), new Vector3(1.7f, 0.45f, 1.1f), ctx.MatSignal, false);
        }

        private static void CreateAnchor(Transform parent, string id, string note, Vector3 position, Quaternion rotation)
        {
            var anchor = new GameObject(id);
            anchor.transform.SetParent(parent, false);
            anchor.transform.SetPositionAndRotation(position, rotation);
            var component = anchor.AddComponent<LevelTransitionAnchor>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("anchorId").stringValue = id;
            serialized.FindProperty("note").stringValue = note;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateBoundary(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            string nextSceneName,
            string nextScenePath,
            string sourceAnchorId,
            string targetAnchorId)
        {
            GameObject trigger = CreateCube(parent, name, position, scale, material, true);
            var collider = trigger.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            GameObject prompt = CreateWorldLabel(parent, name + "_Prompt", "E: continue", position + new Vector3(0f, 1.2f, 0f), Color.white, 0.075f);
            prompt.SetActive(false);

            var transition = trigger.AddComponent<LevelBoundaryTransition>();
            var serialized = new SerializedObject(transition);
            serialized.FindProperty("nextSceneName").stringValue = nextSceneName;
            serialized.FindProperty("nextScenePath").stringValue = nextScenePath;
            serialized.FindProperty("sourceAnchorId").stringValue = sourceAnchorId;
            serialized.FindProperty("targetAnchorId").stringValue = targetAnchorId;
            serialized.FindProperty("requireInteract").boolValue = true;
            serialized.FindProperty("interactKey").intValue = (int)KeyCode.E;
            serialized.FindProperty("promptUI").objectReferenceValue = prompt;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return trigger;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (!keepCollider)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            }

            return go;
        }

        private static GameObject CreateWorldLabel(Transform parent, string name, string text, Vector3 position, Color color, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(62f, 0f, 0f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            mesh.fontSize = 96;
            mesh.characterSize = size * 64f / 96f;
            mesh.color = color;
            return go;
        }

        private static void ConfigureInteractable(
            OutlineInteractable interactable,
            GameObject prompt,
            GameObject[] activateAfterUse,
            GameObject[] deactivateAfterUse,
            string successMessage)
        {
            var serialized = new SerializedObject(interactable);
            serialized.FindProperty("interactKey").intValue = (int)KeyCode.E;
            serialized.FindProperty("interactRadius").floatValue = 1.8f;
            serialized.FindProperty("oneShot").boolValue = true;
            serialized.FindProperty("promptUI").objectReferenceValue = prompt;
            SetObjectArray(serialized.FindProperty("activateAfterUse"), activateAfterUse);
            SetObjectArray(serialized.FindProperty("deactivateAfterUse"), deactivateAfterUse);
            serialized.FindProperty("successMessage").stringValue = successMessage;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty property, GameObject[] objects)
        {
            property.arraySize = objects?.Length ?? 0;
            if (objects == null) return;
            for (int i = 0; i < objects.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = MaterialDir + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(SceneDir);
            Directory.CreateDirectory(MaterialDir);
        }

        private static bool ValidateScene(Scene scene, SceneSpec spec)
        {
            bool passed = true;
            if (!string.Equals(scene.name, spec.Name, StringComparison.Ordinal))
            {
                Debug.LogError($"[FHQ] Scene name mismatch for {spec.Path}: expected {spec.Name}, got {scene.name}");
                passed = false;
            }

            if (FindSceneObjects<OutlineLocalPlayerController>(scene).Length != 1)
            {
                Debug.LogError($"[FHQ] {scene.name}: expected exactly one OutlineLocalPlayerController.");
                passed = false;
            }

            if (FindSceneObjects<OutlineSceneFlowController>(scene).Length != 1)
            {
                Debug.LogError($"[FHQ] {scene.name}: expected exactly one OutlineSceneFlowController.");
                passed = false;
            }

            LevelBoundaryTransition[] transitions = FindSceneObjects<LevelBoundaryTransition>(scene);
            if (transitions.Length != spec.ExpectedTransitionCount)
            {
                Debug.LogError($"[FHQ] {scene.name}: expected {spec.ExpectedTransitionCount} boundary transition(s), found {transitions.Length}.");
                passed = false;
            }

            foreach (LevelBoundaryTransition transition in transitions)
            {
                if (transition.gameObject.activeSelf)
                {
                    Debug.LogError($"[FHQ] {scene.name}: transition {transition.name} should start inactive and be enabled by the scene objective.");
                    passed = false;
                }
            }

            LevelTransitionAnchor[] anchors = FindSceneObjects<LevelTransitionAnchor>(scene);
            foreach (string anchorId in spec.AnchorIds)
            {
                bool found = false;
                foreach (LevelTransitionAnchor anchor in anchors)
                {
                    if (string.Equals(anchor.AnchorId, anchorId, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"[FHQ] {scene.name}: missing transition anchor {anchorId}.");
                    passed = false;
                }
            }

            Debug.Log($"[FHQ] Validated {scene.name}: anchors={anchors.Length}, transitions={transitions.Length}.");
            return passed;
        }

        private static SceneSnapshot CaptureSceneSnapshot(Scene scene, string path)
        {
            var snapshot = new SceneSnapshot(path, scene.name);

            foreach (LevelTransitionAnchor anchor in FindSceneObjects<LevelTransitionAnchor>(scene))
                snapshot.AnchorIds.Add(anchor.AnchorId);

            foreach (LevelBoundaryTransition transition in FindSceneObjects<LevelBoundaryTransition>(scene))
            {
                snapshot.Transitions.Add(new TransitionSnapshot(
                    transition.name,
                    transition.NextScenePath,
                    transition.NextSceneName,
                    transition.SourceAnchorId,
                    transition.TargetAnchorId));
            }

            return snapshot;
        }

        private static bool ValidateTransitionLinks(Dictionary<string, SceneSnapshot> snapshots)
        {
            bool passed = true;
            foreach (SceneSnapshot sourceScene in new List<SceneSnapshot>(snapshots.Values))
            {
                foreach (TransitionSnapshot transition in sourceScene.Transitions)
                {
                    if (string.IsNullOrWhiteSpace(transition.NextScenePath) || !File.Exists(transition.NextScenePath))
                    {
                        Debug.LogError($"[FHQ] {sourceScene.Name}/{transition.Name}: missing next scene path {transition.NextScenePath}");
                        passed = false;
                        continue;
                    }

                    if (!snapshots.TryGetValue(transition.NextScenePath, out SceneSnapshot targetScene))
                    {
                        Scene opened = EditorSceneManager.OpenScene(transition.NextScenePath, OpenSceneMode.Single);
                        targetScene = CaptureSceneSnapshot(opened, transition.NextScenePath);
                        snapshots[transition.NextScenePath] = targetScene;
                    }

                    if (!string.IsNullOrWhiteSpace(transition.NextSceneName)
                        && !string.Equals(targetScene.Name, transition.NextSceneName, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogError($"[FHQ] {sourceScene.Name}/{transition.Name}: next scene name mismatch. Expected {transition.NextSceneName}, got {targetScene.Name}.");
                        passed = false;
                    }

                    if (!sourceScene.AnchorIds.Contains(transition.SourceAnchorId))
                    {
                        Debug.LogError($"[FHQ] {sourceScene.Name}/{transition.Name}: source anchor missing: {transition.SourceAnchorId}");
                        passed = false;
                    }

                    if (!targetScene.AnchorIds.Contains(transition.TargetAnchorId))
                    {
                        Debug.LogError($"[FHQ] {sourceScene.Name}/{transition.Name}: target scene {targetScene.Name} missing anchor {transition.TargetAnchorId}");
                        passed = false;
                    }

                    Debug.Log($"[FHQ] Linked {sourceScene.Name}/{transition.Name}: {transition.SourceAnchorId} -> {targetScene.Name}/{transition.TargetAnchorId}");
                }
            }

            return passed;
        }

        private static T[] FindSceneObjects<T>(Scene scene) where T : Component
        {
            var all = Resources.FindObjectsOfTypeAll<T>();
            var matches = new System.Collections.Generic.List<T>();
            foreach (T item in all)
            {
                if (item != null && item.gameObject.scene == scene)
                    matches.Add(item);
            }

            return matches.ToArray();
        }

        private readonly struct SceneSpec
        {
            public readonly string Path;
            public readonly string Name;
            public readonly string[] AnchorIds;
            public readonly int ExpectedTransitionCount;

            public SceneSpec(string path, string name, string[] anchorIds, int expectedTransitionCount)
            {
                Path = path;
                Name = name;
                AnchorIds = anchorIds;
                ExpectedTransitionCount = expectedTransitionCount;
            }
        }

        private sealed class SceneSnapshot
        {
            public readonly string Path;
            public readonly string Name;
            public readonly HashSet<string> AnchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly List<TransitionSnapshot> Transitions = new List<TransitionSnapshot>();

            public SceneSnapshot(string path, string name)
            {
                Path = path;
                Name = name;
            }
        }

        private readonly struct TransitionSnapshot
        {
            public readonly string Name;
            public readonly string NextScenePath;
            public readonly string NextSceneName;
            public readonly string SourceAnchorId;
            public readonly string TargetAnchorId;

            public TransitionSnapshot(
                string name,
                string nextScenePath,
                string nextSceneName,
                string sourceAnchorId,
                string targetAnchorId)
            {
                Name = name;
                NextScenePath = nextScenePath;
                NextSceneName = nextSceneName;
                SourceAnchorId = sourceAnchorId;
                TargetAnchorId = targetAnchorId;
            }
        }

        private sealed class SceneContext
        {
            public readonly Transform Root;
            public readonly Material MatFloorWarm;
            public readonly Material MatFloorCool;
            public readonly Material MatWallWarm;
            public readonly Material MatWallCool;
            public readonly Material MatPast;
            public readonly Material MatFuture;
            public readonly Material MatSignal;
            public readonly Material MatExit;
            public readonly Material MatBlocker;
            public readonly Material MatRubble;
            public readonly Material MatWood;
            public readonly Material MatAnchor;
            public readonly Material MatDanger;
            public readonly Color SignalText = new Color(0.45f, 0.95f, 1f);
            public readonly Color DangerText = new Color(1f, 0.38f, 0.28f);

            public SceneContext(Transform root)
            {
                Root = root;
                MatFloorWarm = CreateMaterial("Outline_FloorWarm_Mat", new Color(0.46f, 0.36f, 0.24f));
                MatFloorCool = CreateMaterial("Outline_FloorCool_Mat", new Color(0.17f, 0.25f, 0.31f));
                MatWallWarm = CreateMaterial("Outline_WallWarm_Mat", new Color(0.62f, 0.50f, 0.34f));
                MatWallCool = CreateMaterial("Outline_WallCool_Mat", new Color(0.26f, 0.36f, 0.43f));
                MatPast = CreateMaterial("Outline_PastAction_Mat", new Color(1f, 0.72f, 0.25f));
                MatFuture = CreateMaterial("Outline_FutureView_Mat", new Color(0.18f, 0.55f, 0.95f));
                MatSignal = CreateMaterial("Outline_SignalCyan_Mat", new Color(0.1f, 0.85f, 0.95f));
                MatExit = CreateMaterial("Outline_ExitGreen_Mat", new Color(0.16f, 0.82f, 0.40f));
                MatBlocker = CreateMaterial("Outline_BlockerRed_Mat", new Color(0.78f, 0.18f, 0.14f));
                MatRubble = CreateMaterial("Outline_Rubble_Mat", new Color(0.28f, 0.27f, 0.25f));
                MatWood = CreateMaterial("Outline_Wood_Mat", new Color(0.38f, 0.24f, 0.13f));
                MatAnchor = CreateMaterial("Outline_AnchorFrame_Mat", new Color(0.72f, 0.78f, 0.82f));
                MatDanger = CreateMaterial("Outline_DangerTrail_Mat", new Color(1f, 0.16f, 0.12f));
            }
        }
    }
}
