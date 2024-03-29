﻿//!CompilerOption:AddRef:SlimDx.dll

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using FFXIVOverlay.Overlay;
using Color = System.Drawing.Color;
using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using System.Windows.Media;
using Newtonsoft.Json;
//using Logger = ff14bot.Helpers.Logging;
using System.Globalization;
using FFXIVOverlay.Command;
using System.Collections.Generic;
using System.Threading;
using ff14bot.Enums;
using SlimDX.Direct3D9;

namespace FFXIVOverlay
{
   

    public class Plugin : BotPlugin
    {
        public override string Name => "Overlay Mod";
        public override string Description => "Draw custom overlays by config";
        public override string Author => "Ilya";

        public override Version Version => new Version(1, 0, 0);

        public override bool WantButton => true;
        public override string ButtonText => "Reload";


        private RenderForm _renderForm;

        HashSet<uint> skipObjects = new HashSet<uint>();
        HashSet<ff14bot.Enums.GameObjectType> skipGameType = new HashSet<ff14bot.Enums.GameObjectType>();
        ComplexDrawCommand SelfDrawing; // "self"
        ComplexDrawCommand TargetDrawing; // "target"
         
        ComplexDrawCommand AllDrawing; // "all"
        ComplexDrawCommand HostileDrawing; // "hostile"

        Dictionary<ff14bot.Enums.GameObjectType, ComplexDrawCommand> CustomObjectTypesDrawing;
        Dictionary<uint, ComplexDrawCommand> NpcDrawing;

        List<GatherNode> GatherNodes = new List<GatherNode>();

        ComplexDrawCommand customPoints = new ComplexDrawCommand();

        DrawCacheManager cacheDrawManager = new DrawCacheManager();

        private int _hotkeyId1 = 0;
        private int _hotkeyId2 = 0;


        public override void OnPulse()
        {
        }

        public override void OnInitialize()
        {
            //Check if rivatuner is running, if rivatuner is running and isn't blocked from attaching to rebornbuddy it can cause RB to crash
            var rivaTunerRunning = Process.GetProcessesByName("RTSS").Any();
            if (rivaTunerRunning)
            {
                Logging.Write(Colors.Red, @"Rivatuner has been detected running on this machine. If rebornbuddy crashes, add rebornbuddy.exe to rivatuner and disable ""On-Screen Display support""");
            }

            try
            {
                // Check that we can create DirectX 9.0 Object.
                using(var d3d = new Direct3D())
                {
                }
            } 
            catch(Exception ex)
            {
                Logging.Write(Colors.Red, @"DirectX 9.0 problem:\n {0}", ex);
            }
        }

        public override void OnShutdown()
        {
            Task.Run(OnDisableAsync);
        }

        public override void OnEnabled()
        {
            this.resetDrawing();

            // load Config.
            string configPath;
            try
            {
                configPath = JsonSettings.GetSettingsFilePath(Core.Me.Name, "OverlayMod.yaml");
            } 
            catch(Exception)
            {
                configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "OverlayMod.yaml");
            }

            loadConfig(configPath);

            Task.Factory.StartNew(RunRenderForm, TaskCreationOptions.LongRunning);

            _hotkeyId1 = HotKeyManager.RegisterHotKey(Keys.O, KeyModifiers.Shift | KeyModifiers.Alt);
            _hotkeyId2 = HotKeyManager.RegisterHotKey(Keys.I, KeyModifiers.Shift | KeyModifiers.Alt);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
        }

        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (e.Key == Keys.O && e.Modifiers == (KeyModifiers.Shift | KeyModifiers.Alt))
            {
                // add circle on me
                CircleAttack f = new CircleAttack();
                f.FixCenter = true;
                f.Center = Core.Me.Location.Convert();

                f.HeadingOffset = Core.Me.Heading;
                f.Radius = 3;
                f.Color = Color.FromArgb(0x70, 0x70, 0, 0);
                f.PointColor = Color.FromArgb(0x70, 0x70, 0x70, 0);

                customPoints.AddDrawItem(f);
            }

            if (e.Key == Keys.I && e.Modifiers == (KeyModifiers.Shift | KeyModifiers.Alt))
            {
                customPoints = new ComplexDrawCommand();
            }
        }

        private void RunRenderForm()
        {
            OverlayManager.Drawing += Drawing;

            IntPtr targetWindow = Core.Memory.Process.MainWindowHandle;
            _renderForm = new RenderForm(targetWindow);

            Application.Run(_renderForm);
        }

        public override void OnDisabled()
        {
            Task.Run(OnDisableAsync);
        }

        private async Task OnDisableAsync()
        {
            HotKeyManager.UnregisterHotKey(_hotkeyId1);
            HotKeyManager.UnregisterHotKey(_hotkeyId2);

            OverlayManager.Drawing -= Drawing;

            this.resetDrawing();

            if (_renderForm == null)
                return;

            await _renderForm.ShutdownAsync();
        }


        public override void OnButtonPress()
        {
            this.resetDrawing();
            string configPath = JsonSettings.GetSettingsFilePath(Core.Me.Name, "OverlayMod.yaml");
            loadConfig(configPath);
        }

        //private void DrawCircleAttack(DrawingContext ctx, GameObject obj)
        //{
        //    ctx.DrawCircleWithPoint(obj.Location, obj.Heading, 11.0f, Color.FromArgb(100, Color.Red), Color.FromArgb(100, Color.Red));
        //}

        //private void DrawSideAttack(DrawingContext ctx, GameObject obj)
        //{
        //    ctx.DrawSideAttackAgroLine(obj.Location, obj.Heading, 5.0f, 50.0f, Color.FromArgb(100, Color.Blue));
        //}

        //private void DrawSliceAttack(DrawingContext ctx, GameObject obj)
        //{
        //    ctx.DrawAgroLine(obj.Location, obj.Heading + (float)Math.PI / 2, 5.0f, 100.0f, Color.FromArgb(100, Color.Yellow), Color.FromArgb(100, Color.Yellow));
        //}

        private void Drawing(DrawingContext ctx)
        {
            if (QuestLogManager.InCutscene)
                return;

            //Gameobject list is threadstatic, always need to pulse otherwise new objects wont show up
            GameObjectManager.Update();

            if (SelfDrawing != null)
            {
                SelfDrawing.Drawing(ctx, Core.Me);
            }

            if (TargetDrawing != null && Core.Me.CurrentTarget != null)
            {
                TargetDrawing.Drawing(ctx, Core.Me.CurrentTarget);
            }

            foreach (GameObject obj in GameObjectManager.GameObjects)
            {
                if (!obj.IsValid)
                    continue;

                if (obj.IsMe)
                    continue;

                if (skipObjects.Contains(obj.NpcId))
                    continue;

                if (NpcDrawing.ContainsKey(obj.NpcId))
                {
                    IDrawCommand d = NpcDrawing[obj.NpcId];
                    if (d != null) {
                        d.Drawing(ctx, obj);
                    }

                    continue;
                }

                if (skipGameType.Contains(obj.Type))
                    continue;

                if (CustomObjectTypesDrawing.ContainsKey(obj.Type))
                {
                    IDrawCommand d = CustomObjectTypesDrawing[obj.Type];
                    if (d != null)
                    {
                        d.Drawing(ctx, obj);
                    }

                    continue;
                }

                var gameCharacter = obj as Character;
                if (gameCharacter != null && HostileDrawing != null)
                {
                    if (gameCharacter.StatusFlags.HasFlag(ff14bot.Enums.StatusFlags.Hostile))
                    {
                        HostileDrawing.Drawing(ctx, obj);
                        continue;
                    }
                }

                if (AllDrawing != null)
                {
                    AllDrawing.Drawing(ctx, obj);
                }
            }
        
            foreach(GatherNode g in this.GatherNodes)
            {
                g.Drawing(ctx, null);
            }

            customPoints.Drawing(ctx, null);

            cacheDrawManager.Draw(ctx);
        }


        public void loadConfig(string configPath)
        {
            // Just to be sure.
            this.resetDrawing();

            YamLikeConfig.ConfigParser parser = new YamLikeConfig.ConfigParser();

            parser.FileErrorLog += (sender, e) => {
                Logging.Write(Colors.Red, string.Format("[{0}] Config error. File {1}, Message: {2}", Name, e.File, e.Message));
            };

            if (!parser.parse(configPath))
            {
                Logging.Write(Colors.Red, string.Format("[{0}] Config error. Can't parse.", configPath));
                return;
            }

            var docs = parser.Result;
            foreach (var doc in docs)
            {
                parseConfigDocument(doc);
            }
        }

        public void parseConfigDocument(YamLikeConfig.Document doc)
        {
            if (doc["skip"] == "1")
                return;

            if (doc.Commands == null || doc.Commands.Count == 0)
                return;

            foreach(var cmd in doc.Commands)
            {
                parseTopLevelCommand(cmd);
            }
        }

        public void parseTopLevelCommand(YamLikeConfig.Command cmd)
        {
            string cmdName = cmd.Name;
            if (string.IsNullOrWhiteSpace(cmdName))
                return;

            if (cmd["skip"] == "1")
                return;

            bool hasSkipCommand = cmd.SubCommand != null && cmd.SubCommand.Any(c1 => c1.Name != null && c1.Name.ToLower() == "skip");
            if (hasSkipCommand)
                return;

            cmdName = cmdName.ToLower();

            switch(cmdName)
            {
                case "self":
                    SelfDrawing.Init(cmd);
                    //fillComplexDrawCommand(ref SelfDrawing, cmd);
                    return;
                case "target":
                    TargetDrawing.Init(cmd);
                    //fillComplexDrawCommand(ref TargetDrawing, cmd);
                    return;
                case "hostile":
                    HostileDrawing.Init(cmd);
                    //fillComplexDrawCommand(ref HostileDrawing, cmd);
                    return;
                case "all":
                    AllDrawing.Init(cmd);
                    //fillComplexDrawCommand(ref AllDrawing, cmd);
                    return;
                case "gametype":
                    fillGameTypeCommand(cmd);
                    return;
                case "npc":
                    fillNpcCommand(cmd);
                    return;
                case "gather":
                    GatherNode gather = CommandDrawingFactoryEx.create(cmd) as GatherNode;
                    if (gather != null)
                    {
                        GatherNodes.Add(gather);
                    }
                    //fillGatherMap(cmd);
                    return;
            }

        }

        //public void fillComplexDrawCommand(ref ComplexDrawCommand complex, YamLikeConfig.Command cmd)
        //{
        //    if (cmd.SubCommand == null || cmd.SubCommand.Count == 0)
        //        return;

        //    foreach(var c in cmd.SubCommand)
        //    {
        //        IDrawCommand drawingCmd = CommandDrawingFactory.create(c);
        //        if (drawingCmd == null)
        //            continue;

        //        if (complex == null)
        //        {
        //            complex = new ComplexDrawCommand();
        //        }

        //        complex.AddDrawItem(drawingCmd);
        //    }
        //}
        public void fillGameTypeCommand(YamLikeConfig.Command gameTypeCommand)
        {
            uint gameTypeInt = 0;
            if (!uint.TryParse(gameTypeCommand["type"], out gameTypeInt))
                return;

            if (gameTypeCommand["hide"] == "1")
            {
                skipGameType.Add((ff14bot.Enums.GameObjectType)gameTypeInt);
                return;
            }

            ComplexDrawCommand complex = null;

            if (gameTypeCommand.SubCommand == null)
            {
                CustomObjectTypesDrawing[(ff14bot.Enums.GameObjectType)gameTypeInt] = complex;
                return;
            }

            foreach (var c in gameTypeCommand.SubCommand)
            {
                IDrawCommand drawingCmd = CommandDrawingFactoryEx.create(c);
                if (drawingCmd == null)
                    continue;

                if (complex == null)
                {
                    complex = new ComplexDrawCommand();
                }

                complex.AddDrawItem(drawingCmd);
            }

            CustomObjectTypesDrawing[(ff14bot.Enums.GameObjectType)gameTypeInt] = complex;
        }

        public void fillNpcCommand(YamLikeConfig.Command npcCommand)
        {
            if (npcCommand.UnnamedParams == null)
                return;

            List<uint> npcIdList = new List<uint>();

            foreach(var npcIdStr in npcCommand.UnnamedParams)
            {
                uint npcId = 0;
                if (!uint.TryParse(npcIdStr, out npcId))
                    continue;

                npcIdList.Add(npcId);
            }

            if (npcIdList.Count == 0)
                return;

            if (npcCommand["hide"] == "1")
            {
                foreach(uint npcId in npcIdList)
                {
                    skipObjects.Add(npcId);
                }
                
                return;
            }

            ComplexDrawCommand complex = null;

            if (npcCommand.SubCommand != null)
            {
                foreach (var c in npcCommand.SubCommand)
                {
                    IDrawCommand drawingCmd = CommandDrawingFactoryEx.create(c);
                    if (drawingCmd == null)
                        continue;

                    if (complex == null)
                    {
                        complex = new ComplexDrawCommand();
                    }

                    complex.AddDrawItem(drawingCmd);
                }
            }

            foreach (uint npcId in npcIdList)
            {
                NpcDrawing[npcId] = complex;
            }
        }

        //public void fillGatherMap(YamLikeConfig.Command docCmd)
        //{
        //    if (docCmd.SubCommand == null || docCmd.SubCommand.Count == 0)
        //    {
        //        return;
        //    }

        //    if (!docCmd.has("zone"))
        //        return;

        //    int mapId = 0;
        //    if (!docCmd.tryGet("zone", out mapId))
        //    {
        //        return;
        //    }

        //    GatherNode node = new GatherNode();
        //    node.zoneId = mapId;

        //    foreach (var c in docCmd.SubCommand)
        //    {
        //        IDrawCommand drawingCmd = CommandDrawingFactory.create(c);
        //        if (drawingCmd == null)
        //            continue;

        //        node.AddDrawItem(drawingCmd);
        //    }

        //    this.GatherNodes.Add(node);
        //}

        void resetDrawing()
        {
            this.SelfDrawing = new ComplexDrawCommand();
            this.TargetDrawing = new ComplexDrawCommand();
            this.AllDrawing = new ComplexDrawCommand();
            this.HostileDrawing = new ComplexDrawCommand();

            this.skipObjects = new HashSet<uint>();
            this.skipGameType = new HashSet<ff14bot.Enums.GameObjectType>();

            this.CustomObjectTypesDrawing = new Dictionary<ff14bot.Enums.GameObjectType, ComplexDrawCommand>();
            this.NpcDrawing = new Dictionary<uint, ComplexDrawCommand>();
            this.GatherNodes = new List<GatherNode>();
            this.cacheDrawManager = new DrawCacheManager();

            CommandDrawingFactoryEx.drawCacheManager = this.cacheDrawManager;
        }

    }
}
 