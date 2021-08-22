﻿using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Interface;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly PluginConfiguration PluginConfiguration;
        private Vector2 _barsize;

        public abstract uint JobId { get; }

        protected Vector4 ColorBlack => new Vector4(0f, 0f, 0f, 1f);
        
        protected float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected int XOffset => 238;
        protected int YOffset => 490;
        protected int BarHeight => 36;
        protected int BarWidth => 300;
        protected Vector2 BarSize => _barsize;
        
        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
            _barsize = new Vector2(BarWidth, BarHeight);
        }

        protected virtual void DrawHealthBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentHp / actor.MaxHp;

            var cursorPos = new Vector2(CenterX - BarWidth - XOffset, CenterY + YOffset);

            DrawOutlinedText($"{actor.Name.Abbreviate().Truncate(16)}", new Vector2(cursorPos.X + 5, cursorPos.Y -22));
            
            var hp = $"{actor.MaxHp.KiloFormat(),6} | ";
            var hpSize = ImGui.CalcTextSize(hp);
            var percentageSize = ImGui.CalcTextSize("100");
            DrawOutlinedText(hp, new Vector2(cursorPos.X + BarWidth - hpSize.X - percentageSize.X - 5, cursorPos.Y -22));
            DrawOutlinedText($"{(int)(scale * 100),3}", new Vector2(cursorPos.X + BarWidth - percentageSize.X - 5, cursorPos.Y -22));
            
            ImGui.SetCursorPos(cursorPos);
            
            if (ImGui.BeginChild("health_bar", BarSize)) {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF17055D);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(BarWidth * scale, BarHeight), 
                    0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                if (ImGui.IsItemClicked()) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(actor);
                }
                
                ImGui.EndChild();
            }
        }

        protected virtual void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(357, 26);
            var cursorPos = new Vector2(CenterX - 178, CenterY + 496);
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y), 
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
        
        protected virtual void DrawTargetBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara actor)) {
                return;
            }

            var scale = (float) actor.CurrentHp / actor.MaxHp;
            var cursorPos = new Vector2(CenterX + XOffset, CenterY + YOffset);
            ImGui.SetCursorPos(cursorPos);
 
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF13374E);
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + new Vector2(BarWidth * scale, BarHeight), 0xFF4091D6, 0xFF45C7E6, 0xFF45C7E6, 0xFF4091D6);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            
            var percentage = $"{(int) (scale * 100)}";
            var percentageSize = ImGui.CalcTextSize(percentage);
            var maxPercentageSize = ImGui.CalcTextSize("100");
            DrawOutlinedText(percentage, new Vector2(cursorPos.X + 5 + maxPercentageSize.X - percentageSize.X, cursorPos.Y - 22));
            DrawOutlinedText($" | {actor.MaxHp.KiloFormat(),-6}", new Vector2(cursorPos.X + 5 + maxPercentageSize.X, cursorPos.Y - 22));
            
            var name = $"{actor.Name.Abbreviate().Truncate(16)}";
            var nameSize = ImGui.CalcTextSize(name);
            DrawOutlinedText(name, new Vector2(cursorPos.X + BarWidth - nameSize.X - 5, cursorPos.Y - 22));

            DrawTargetOfTargetBar(target.TargetActorID);
        }
        
        protected virtual void DrawTargetOfTargetBar(int targetActorId) {
            Actor target = null;
            
            for (var i = 0; i < 200; i += 2) {
                if (PluginInterface.ClientState.Actors[i]?.ActorId == targetActorId) {
                    target = PluginInterface.ClientState.Actors[i];
                }
            }
            
            if (!(target is Chara actor)) {
                return;
            }

            const int barWidth = 120;
            const int barHeight = 20;
            var barSize = new Vector2(barWidth, barHeight);

            var name = $"{actor.Name.Abbreviate().Truncate(12)}";
            var textSize = ImGui.CalcTextSize(name);

            var cursorPos = new Vector2(CenterX + XOffset + BarWidth + 2, CenterY + YOffset);
            DrawOutlinedText(name, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y - 22));
            ImGui.SetCursorPos(cursorPos);    
            
            if (ImGui.BeginChild("target_bar", barSize)) {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF17055D);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float)barWidth * actor.CurrentHp / actor.MaxHp, barHeight), 
                    0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
                );
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                
                if (ImGui.IsItemClicked()) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(target);
                }
                
                ImGui.EndChild();
            }
        }

        protected void DrawOutlinedText(string text, Vector2 pos) {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
        }
        
        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) {
            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y));
            ImGui.TextColored(color, text);
        }
        
        public void Draw() {
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null) {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            
            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | 
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin) {
                return;
            }

            Draw(true);
            
            ImGui.End();
        }
        
        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible() {
            if (IsVisible) {
                return true;
            }
            
            if (PluginConfiguration.HideHud) {
                return false;
            }

            if (PluginConfiguration.HideCombat && PluginInterface.ClientState.Condition[ConditionFlag.InCombat]) {
                return false;
            }

            var parameterWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);
            
            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null || parameterWidget == null || fadeMiddleWidget == null || !parameterWidget->IsVisible || fadeMiddleWidget->IsVisible;
        }
    }
}